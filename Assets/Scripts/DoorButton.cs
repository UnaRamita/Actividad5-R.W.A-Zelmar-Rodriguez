using UnityEngine;

// Boton que, al ser pisado por Rodolfo, usa la interfaz publica de DoorBehaviour para abrir la puerta.
[RequireComponent(typeof(BoxCollider))]
public class DoorButton : MonoBehaviour
{
    // Puerta que abre este boton.
    public DoorBehaviour door;

    // Parte visible del boton, que se hunde al presionarlo.
    public Transform visual;
    public float pressedDepth = 0.15f;

    private bool pressed = false;

    public bool IsPressed
    {
        get { return pressed; }
    }

    void Awake()
    {
        GetComponent<BoxCollider>().isTrigger = true;
    }

    void OnTriggerEnter(Collider other)
    {
        if (pressed || other.GetComponentInParent<RodolfoBehaviour>() == null)
        {
            return;
        }

        pressed = true;
        if (visual != null)
        {
            visual.localPosition += Vector3.down * pressedDepth;
        }
        door.Open();
    }
}
