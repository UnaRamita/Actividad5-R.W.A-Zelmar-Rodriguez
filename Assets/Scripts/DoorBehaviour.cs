using UnityEngine;

// Puerta que empieza abierta (escondida arriba), se cierra de golpe cuando entra
// en el campo de vision de la camara y expone una interfaz publica para abrirla.
[RequireComponent(typeof(Rigidbody))]
public class DoorBehaviour : MonoBehaviour
{
    // Cuanto sube la puerta al abrirse (en unidades), respecto de su posicion cerrada en la escena.
    public float openHeight = 13.0f;

    // Velocidades de cierre (rapido, para que se note) y de apertura.
    public float closeSpeed = 20.0f;
    public float openSpeed = 6.0f;

    // Se cierra cuando su posicion queda a la izquierda de esta fraccion de la pantalla (0 = borde izquierdo, 1 = derecho).
    [Range(0, 1)]
    public float closeWhenVisibleAt = 0.85f;

    private Rigidbody body;
    private Vector3 closedPosition;
    private Vector3 openPosition;
    private Vector3 targetPosition;
    private float speed;
    private bool hasClosed = false;
    private bool opened = false;

    public bool IsClosed
    {
        get { return hasClosed && !opened && body.position == closedPosition; }
    }

    public bool IsFullyOpen
    {
        get { return opened && body.position == openPosition; }
    }

    void Awake()
    {
        body = GetComponent<Rigidbody>();
        body.isKinematic = true;

        // La posicion de la escena es la puerta cerrada; arranca abierta.
        closedPosition = transform.position;
        openPosition = closedPosition + Vector3.up * openHeight;
        transform.position = openPosition;
        body.position = openPosition;
        targetPosition = openPosition;
    }

    void Update()
    {
        if (!hasClosed && !opened && IsInCameraView())
        {
            Close();
        }
    }

    void FixedUpdate()
    {
        body.MovePosition(Vector3.MoveTowards(body.position, targetPosition, speed * Time.fixedDeltaTime));
    }

    public void Open()
    {
        opened = true;
        hasClosed = true;
        targetPosition = openPosition;
        speed = openSpeed;
    }

    private void Close()
    {
        hasClosed = true;
        targetPosition = closedPosition;
        speed = closeSpeed;
    }

    // Comprueba si la columna de la puerta ya esta dentro de la pantalla.
    private bool IsInCameraView()
    {
        Camera cam = Camera.main;
        if (cam == null)
        {
            return false;
        }

        Vector3 viewportPoint = cam.WorldToViewportPoint(closedPosition);
        return viewportPoint.x >= 0 && viewportPoint.x <= closeWhenVisibleAt;
    }
}
