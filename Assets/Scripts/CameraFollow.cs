using UnityEngine;

// Camara ortografica de scroll lateral: solo se desplaza cuando el objetivo
// se acerca a los bordes de la pantalla y nunca muestra mas alla del nivel.
[RequireComponent(typeof(Camera))]
public class CameraFollow : MonoBehaviour
{
    // Objeto a seguir (Rodolfo).
    public Transform target;

    // Raiz de los bloques del nivel: sus limites se calculan a partir de sus renderers.
    public Transform levelRoot;

    // Distancia (en unidades) a los bordes horizontales y verticales a partir de la cual la camara se mueve.
    public float horizontalEdgeMargin = 4.0f;
    public float verticalEdgeMargin = 2.5f;

    // Tiempo aproximado que tarda la camara en alcanzar su destino.
    public float smoothTime = 0.25f;

    private Camera cam;
    private Bounds levelBounds;
    private Vector3 desiredPosition;
    private Vector3 currentVelocity;

    void Start()
    {
        cam = GetComponent<Camera>();
        levelBounds = CalculateLevelBounds();
        desiredPosition = ClampToLevel(transform.position);
        transform.position = desiredPosition;
    }

    void LateUpdate()
    {
        if (target == null)
        {
            return;
        }

        desiredPosition = ClampToLevel(FollowWhenNearEdges(desiredPosition));
        transform.position = Vector3.SmoothDamp(transform.position, desiredPosition, ref currentVelocity, smoothTime);
    }

    public Bounds LevelBounds
    {
        get { return levelBounds; }
    }

    // Mueve el destino de la camara solo lo necesario para que el objetivo
    // quede a "edgeMargin" del borde de la pantalla.
    private Vector3 FollowWhenNearEdges(Vector3 position)
    {
        Vector3 targetPosition = target.position;
        float halfHeight = cam.orthographicSize;
        float halfWidth = halfHeight * cam.aspect;

        float maxOffsetX = Mathf.Max(0, halfWidth - horizontalEdgeMargin);
        float maxOffsetY = Mathf.Max(0, halfHeight - verticalEdgeMargin);

        position.x = Mathf.Clamp(position.x, targetPosition.x - maxOffsetX, targetPosition.x + maxOffsetX);
        position.y = Mathf.Clamp(position.y, targetPosition.y - maxOffsetY, targetPosition.y + maxOffsetY);
        return position;
    }

    // Evita que la vista salga de los limites del nivel.
    private Vector3 ClampToLevel(Vector3 position)
    {
        float halfHeight = cam.orthographicSize;
        float halfWidth = halfHeight * cam.aspect;

        position.x = ClampAxis(position.x, levelBounds.min.x + halfWidth, levelBounds.max.x - halfWidth, levelBounds.center.x);
        position.y = ClampAxis(position.y, levelBounds.min.y + halfHeight, levelBounds.max.y - halfHeight, levelBounds.center.y);
        return position;
    }

    // Si el nivel es mas chico que la vista en un eje, la camara queda centrada en ese eje.
    private float ClampAxis(float value, float min, float max, float center)
    {
        if (min > max)
        {
            return center;
        }
        return Mathf.Clamp(value, min, max);
    }

    private Bounds CalculateLevelBounds()
    {
        Renderer[] renderers = levelRoot.GetComponentsInChildren<Renderer>();
        Bounds bounds = renderers[0].bounds;
        foreach (Renderer levelRenderer in renderers)
        {
            bounds.Encapsulate(levelRenderer.bounds);
        }
        return bounds;
    }
}
