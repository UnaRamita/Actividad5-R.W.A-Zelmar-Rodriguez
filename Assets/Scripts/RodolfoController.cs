using UnityEngine;
using UnityEngine.InputSystem;

// Espera inputs del jugador y usa la interfaz publica de RodolfoBehaviour
// para mover y hacer saltar a Rodolfo.
[RequireComponent(typeof(RodolfoBehaviour))]
public class RodolfoController : MonoBehaviour
{
    private RodolfoBehaviour rodolfoBehaviour;

    void Start()
    {
        rodolfoBehaviour = GetComponent<RodolfoBehaviour>();
    }

    void Update()
    {
        Keyboard keyboard = Keyboard.current;
        if (keyboard == null)
        {
            return;
        }

        rodolfoBehaviour.SetHorizontalDirection(ReadHorizontalInput(keyboard));

        if (keyboard.spaceKey.wasPressedThisFrame)
        {
            rodolfoBehaviour.Jump();
        }
    }

    void OnDisable()
    {
        // Al quitarle el control al jugador, Rodolfo deja de avanzar.
        if (rodolfoBehaviour != null)
        {
            rodolfoBehaviour.SetHorizontalDirection(0);
        }
    }

    // Flechas izquierda/derecha o teclas A/D. Si se presionan ambas direcciones, se anulan.
    private float ReadHorizontalInput(Keyboard keyboard)
    {
        float direction = 0;
        if (keyboard.leftArrowKey.isPressed || keyboard.aKey.isPressed)
        {
            direction -= 1;
        }
        if (keyboard.rightArrowKey.isPressed || keyboard.dKey.isPressed)
        {
            direction += 1;
        }
        return direction;
    }
}
