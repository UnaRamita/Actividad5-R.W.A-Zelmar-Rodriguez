using UnityEngine;

// Mueve a Rodolfo con fisica (Rigidbody) y expone una interfaz publica
// para desplazarlo horizontalmente, saltar o detenerlo.
[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(BoxCollider))]
public class RodolfoBehaviour : MonoBehaviour
{
    // Velocidad horizontal maxima en unidades por segundo.
    public float moveSpeed = 6.0f;

    // Aceleracion horizontal: evita arranques y frenadas bruscas.
    public float acceleration = 40.0f;

    // Altura maxima del salto en unidades (la base de Rodolfo sube esta distancia).
    public float jumpHeight = 4.0f;

    // Multiplicador de la gravedad del proyecto: mas alto = sube y cae mas rapido.
    public float gravityScale = 2.2f;

    // Superficies sobre las que Rodolfo puede apoyarse para saltar.
    public LayerMask groundLayers = ~0;

    private Rigidbody body;
    private BoxCollider bodyCollider;
    private readonly Collider[] groundHits = new Collider[8];

    // -1 = izquierda, 1 = derecha, 0 = quieto.
    private float horizontalDirection = 0;
    private bool jumpRequested = false;
    private bool frozen = false;
    private float lastJumpTime = -1.0f;
    private const float JumpGroundIgnoreTime = 0.15f;

    public bool IsGrounded { get; private set; }

    void Awake()
    {
        body = GetComponent<Rigidbody>();
        bodyCollider = GetComponent<BoxCollider>();
    }

    void FixedUpdate()
    {
        if (frozen)
        {
            return;
        }

        IsGrounded = CheckGrounded();
        ApplyExtraGravity();
        ApplyHorizontalMovement();
        ApplyJump();
    }

    public void SetHorizontalDirection(float direction)
    {
        horizontalDirection = Mathf.Clamp(direction, -1.0f, 1.0f);
    }

    public void Jump()
    {
        jumpRequested = true;
    }

    // Detiene a Rodolfo en el lugar y deja de responder a nuevas ordenes.
    public void Freeze()
    {
        frozen = true;
        horizontalDirection = 0;
        jumpRequested = false;
        body.linearVelocity = Vector3.zero;
        body.constraints = RigidbodyConstraints.FreezeAll;
    }

    // El Rigidbody ya aplica la gravedad normal; se suma la parte extra para llegar a gravityScale.
    private void ApplyExtraGravity()
    {
        body.AddForce(Physics.gravity * (gravityScale - 1.0f), ForceMode.Acceleration);
    }

    private void ApplyHorizontalMovement()
    {
        Vector3 velocity = body.linearVelocity;
        float targetSpeed = horizontalDirection * moveSpeed;
        velocity.x = Mathf.MoveTowards(velocity.x, targetSpeed, acceleration * Time.fixedDeltaTime);
        velocity.z = 0;
        body.linearVelocity = velocity;
    }

    private void ApplyJump()
    {
        // El pedido de salto se consume siempre: si no esta apoyado, se descarta
        // y asi no se pueden encadenar saltos en el aire.
        if (jumpRequested && IsGrounded)
        {
            Vector3 velocity = body.linearVelocity;
            // Velocidad inicial necesaria para alcanzar jumpHeight con la gravedad aumentada.
            velocity.y = Mathf.Sqrt(2.0f * Mathf.Abs(Physics.gravity.y) * gravityScale * jumpHeight);
            body.linearVelocity = velocity;
            IsGrounded = false;
            lastJumpTime = Time.time;
        }
        jumpRequested = false;
    }

    // Busca superficies solidas en una franja delgada justo debajo de los pies.
    private bool CheckGrounded()
    {
        // Justo despues de saltar los pies siguen tocando el suelo: se ignora el contacto
        // por un instante para que no cuente como apoyo (evita un segundo salto al despegar).
        if (Time.time - lastJumpTime < JumpGroundIgnoreTime)
        {
            return false;
        }

        Bounds bounds = bodyCollider.bounds;
        Vector3 center = new Vector3(bounds.center.x, bounds.min.y - 0.05f, bounds.center.z);
        // Mas angosta que el cuerpo para no confundir el costado de una pared con el suelo.
        Vector3 halfExtents = new Vector3(bounds.extents.x * 0.7f, 0.06f, bounds.extents.z * 0.9f);
        int count = Physics.OverlapBoxNonAlloc(center, halfExtents, groundHits, Quaternion.identity,
            groundLayers, QueryTriggerInteraction.Ignore);

        for (int i = 0; i < count; i++)
        {
            if (groundHits[i].attachedRigidbody != body)
            {
                return true;
            }
        }
        return false;
    }
}
