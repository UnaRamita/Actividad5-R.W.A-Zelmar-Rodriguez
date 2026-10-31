using UnityEngine;

// Camara ortografica de scroll lateral: acompana al objetivo de forma suave,
// lo vuelve a centrar cuando se detiene y nunca muestra mas alla del nivel.
[RequireComponent(typeof(Camera))]
public class CameraFollow : MonoBehaviour
{
    // Objeto a seguir (Rodolfo).
    public Transform target;

    // Raiz de los bloques del nivel: sus limites se calculan a partir de sus renderers.
    public Transform levelRoot;

    // Distancia maxima del objetivo al centro, como fraccion de media pantalla (0 = siempre centrado, 1 = borde).
    // Evita que el objetivo se acerque a los bordes aunque la camara vaya con retraso.
    [Range(0, 1)]
    public float maxScreenOffset = 0.3f;

    // Tiempo aproximado que tarda la camara en alcanzar su destino.
    public float smoothTime = 0.35f;

    private Camera cam;
    private Rigidbody targetBody;
    private Bounds levelBounds;
    private Vector3 desiredPosition;
    private Vector3 currentVelocity;

    void Start()
    {
        cam = GetComponent<Camera>();
        targetBody = target != null ? target.GetComponent<Rigidbody>() : null;
        levelBounds = CalculateLevelBounds();

        desiredPosition = transform.position;
        if (target != null)
        {
            desiredPosition.x = target.position.x;
            desiredPosition.y = target.position.y;
        }
        desiredPosition = ClampToLevel(desiredPosition);
        transform.position = desiredPosition;
    }

    void LateUpdate()
    {
        if (target == null)
        {
            return;
        }

        desiredPosition = ClampToLevel(FollowTarget(desiredPosition));
        Vector3 position = Vector3.SmoothDamp(transform.position, desiredPosition, ref currentVelocity, smoothTime);
        transform.position = ClampToLevel(KeepTargetNearCenter(position));
    }

    public Bounds LevelBounds
    {
        get { return levelBounds; }
    }

    // En X la camara siempre busca centrar al objetivo: al moverse lo acompana y al detenerse termina centrada.
    // En Y solo se recentra cuando el objetivo esta apoyado, para no sacudirse con cada salto.
    private Vector3 FollowTarget(Vector3 position)
    {
        position.x = target.position.x;
        bool isStanding = targetBody == null || Mathf.Abs(targetBody.linearVelocity.y) < 0.1f;
        if (isStanding)
        {
            position.y = target.position.y;
        }
        return position;
    }

    // Limita el retraso del suavizado para que el objetivo no se acerque a los bordes de la pantalla.
    private Vector3 KeepTargetNearCenter(Vector3 position)
    {
        float maxOffsetY = cam.orthographicSize * maxScreenOffset;
        float maxOffsetX = cam.orthographicSize * cam.aspect * maxScreenOffset;

        position.x = Mathf.Clamp(position.x, target.position.x - maxOffsetX, target.position.x + maxOffsetX);
        position.y = Mathf.Clamp(position.y, target.position.y - maxOffsetY, target.position.y + maxOffsetY);
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
