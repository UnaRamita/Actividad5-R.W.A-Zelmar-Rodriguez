using UnityEngine;

// Marcador de fin de nivel: cuando el cuerpo completo de Rodolfo queda
// contenido dentro del area del marcador, se termina el nivel.
[RequireComponent(typeof(BoxCollider))]
public class LevelEndMarker : MonoBehaviour
{
    // Mensaje de exito que se activa al completar el nivel.
    public GameObject successMessage;

    // Margen (en unidades) que se tolera alrededor del area del marcador.
    // El marcador mide lo mismo que Rodolfo, por lo que sin margen seria imposible encajarlo.
    public float tolerance = 0.15f;

    private BoxCollider area;
    private bool completed = false;

    public bool IsCompleted
    {
        get { return completed; }
    }

    void Awake()
    {
        area = GetComponent<BoxCollider>();
        area.isTrigger = true;
    }

    void Start()
    {
        if (successMessage != null)
        {
            successMessage.SetActive(false);
        }
    }

    void OnTriggerStay(Collider other)
    {
        if (completed)
        {
            return;
        }

        RodolfoBehaviour rodolfo = other.GetComponentInParent<RodolfoBehaviour>();
        if (rodolfo != null && IsFullyInside(other.bounds))
        {
            CompleteLevel(rodolfo);
        }
    }

    // Comprueba en el plano XY que los limites del cuerpo esten dentro del area del marcador.
    public bool IsFullyInside(Bounds bodyBounds)
    {
        Bounds markerBounds = area.bounds;
        return bodyBounds.min.x >= markerBounds.min.x - tolerance
            && bodyBounds.max.x <= markerBounds.max.x + tolerance
            && bodyBounds.min.y >= markerBounds.min.y - tolerance
            && bodyBounds.max.y <= markerBounds.max.y + tolerance;
    }

    private void CompleteLevel(RodolfoBehaviour rodolfo)
    {
        completed = true;

        RodolfoController controller = rodolfo.GetComponent<RodolfoController>();
        if (controller != null)
        {
            controller.enabled = false;
        }
        rodolfo.Freeze();

        if (successMessage != null)
        {
            successMessage.SetActive(true);
        }
    }
}
