using UnityEngine;

[ExecuteAlways]
public class BillboardFollower : MonoBehaviour
{
    [Tooltip("Cámara objetivo. Si se deja vacío, se usará la cámara principal.")]
    public Camera targetCamera;

    [Header("Offsets")]
    public Vector3 positionOffset = new Vector3(0, 0, 2);
    public Vector3 rotationOffset = Vector3.zero;

    [Header("Velocidad de interpolación")]
    [Range(0f, 20f)] public float positionFollowSpeed = 5f;
    [Range(0f, 20f)] public float rotationFollowSpeed = 5f;

    private void LateUpdate()
    {
        if (targetCamera == null)
        {
            targetCamera = Camera.main;
            if (targetCamera == null) return;
        }

        // --- POSICIÓN ---
        Vector3 targetPosition = targetCamera.transform.position + targetCamera.transform.rotation * positionOffset;
        transform.position = Vector3.Lerp(transform.position, targetPosition, Time.deltaTime * positionFollowSpeed);

        // --- ROTACIÓN ---
        Vector3 directionToCamera = transform.position - targetCamera.transform.position;
        Quaternion targetRotation = Quaternion.LookRotation(directionToCamera);
        targetRotation *= Quaternion.Euler(rotationOffset); // Aplica el offset de rotación

        transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * rotationFollowSpeed);
    }
}
