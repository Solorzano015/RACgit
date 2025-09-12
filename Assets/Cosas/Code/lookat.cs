using UnityEngine;

public class LookAtTarget : MonoBehaviour
{
    [Header("Target")]
    public Transform target;

    [Header("Rotation Settings")]
    public float rotationSpeed = 5f;

    [Tooltip("Permitir rotación en los ejes:")]
    public bool rotateX = false;
    public bool rotateY = true;
    public bool rotateZ = false;

    void Update()
    {
        if (target == null) return;

        // Dirección hacia el objetivo
        Vector3 direction = target.position - transform.position;
        if (direction == Vector3.zero) return;

        // Crear rotación deseada
        Quaternion targetRotation = Quaternion.LookRotation(direction);

        // Filtrar ejes según configuración
        Vector3 targetEuler = targetRotation.eulerAngles;
        Vector3 currentEuler = transform.rotation.eulerAngles;

        float x = rotateX ? targetEuler.x : currentEuler.x;
        float y = rotateY ? targetEuler.y : currentEuler.y;
        float z = rotateZ ? targetEuler.z : currentEuler.z;

        Quaternion finalRotation = Quaternion.Euler(x, y, z);

        // Interpolación suave
        transform.rotation = Quaternion.Lerp(transform.rotation, finalRotation, rotationSpeed * Time.deltaTime);
    }
}
