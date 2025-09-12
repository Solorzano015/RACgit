using UnityEngine;

public class SimpleFollow : MonoBehaviour
{
    [Tooltip("El objeto que se va a seguir.")]
    public Transform target;

    [Tooltip("Velocidad de seguimiento.")]
    public float followSpeed = 5f;

    private void Update()
    {
        if (target == null)
        {
            Debug.LogWarning("No se ha asignado un target en " + gameObject.name);
            return;
        }

        // Mueve el objeto hacia la posición del target con la velocidad especificada
        transform.position = Vector3.Lerp(transform.position, target.position, followSpeed * Time.deltaTime);
    }
}
