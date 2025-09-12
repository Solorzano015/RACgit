using UnityEngine;

public class CameraCollisionFollow : MonoBehaviour
{
    [Header("Configuración de seguimiento")]
    public Transform target;
    public Vector3 offset = new Vector3(0, 3, -6);
    public float smoothSpeed = 0.125f;

    [Header("Rotación y entrada del mouse")]
    public float rotationSpeed = 5f;
    public bool invertPitch = false;
    public float minPitch = 10f;
    public float maxPitch = 80f;

    [Header("Configuración de zoom")]
    public float minOffsetY = 1f;
    public float maxOffsetY = 5f;
    public float scrollSpeed = 1f;

    [Header("Configuración de Focus")]
    public Vector3 focusOffset = new Vector3(0, 2, -4);
    private bool isFocusing = false;
    private Vector3 originalOffset;

    public float focusMinPitch = 20f;
    public float focusMaxPitch = 60f;

    // Añade estas variables al principio de la clase si aún no existen:
    private float currentMinPitch;
    private float currentMaxPitch;


    [Header("Configuración de transición")]
    public float focusTransitionSpeed = 5f;
    private Vector3 targetOffset; // Usado como offset interpolado

    [Header("Detección de colisiones")]
    public float collisionRadius = 0.2f;
    public float minDistance = 1f;
    public LayerMask collisionLayers;

    private float yaw = 0f;
    private float pitch = 30f;

    void Start()
    {
        targetOffset = offset;
        currentMinPitch = minPitch;
        currentMaxPitch = maxPitch;
    }


    void LateUpdate()
    {
        // Rotación
        yaw += Input.GetAxis("Mouse X") * rotationSpeed;
        float mouseY = Input.GetAxis("Mouse Y") * rotationSpeed;
        pitch += invertPitch ? mouseY : -mouseY;




        // Calcula los valores objetivo
        float targetMinPitch = isFocusing ? focusMinPitch : minPitch;
        float targetMaxPitch = isFocusing ? focusMaxPitch : maxPitch;

        // Suaviza los valores actuales hacia los objetivos
        currentMinPitch = Mathf.Lerp(currentMinPitch, targetMinPitch, Time.deltaTime * focusTransitionSpeed);
        currentMaxPitch = Mathf.Lerp(currentMaxPitch, targetMaxPitch, Time.deltaTime * focusTransitionSpeed);

        // Aplica los límites suavizados al pitch
        pitch = Mathf.Clamp(pitch, currentMinPitch, currentMaxPitch);






        // Transición suave entre offsets
        targetOffset = Vector3.Lerp(targetOffset, offset, Time.deltaTime * focusTransitionSpeed);

        // Cálculo de posición y colisión
        Quaternion rotation = Quaternion.Euler(pitch, yaw, 0);
        Vector3 desiredPosition = target.position + rotation * targetOffset;

        Vector3 direction = desiredPosition - target.position;
        float desiredDistance = direction.magnitude;
        Ray ray = new Ray(target.position, direction.normalized);
        RaycastHit hit;
        if (Physics.SphereCast(ray, collisionRadius, out hit, desiredDistance, collisionLayers))
        {
            float adjustedDistance = Mathf.Clamp(hit.distance, minDistance, desiredDistance);
            desiredPosition = target.position + direction.normalized * adjustedDistance;
        }

        transform.position = Vector3.Lerp(transform.position, desiredPosition, smoothSpeed);
        transform.LookAt(target.position);

        // Zoom (solo fuera de focus)
        if (!isFocusing)
        {
            float scroll = Input.GetAxis("Mouse ScrollWheel");
            if (scroll != 0)
            {
                offset.y = Mathf.Clamp(offset.y - scroll * scrollSpeed, minOffsetY, maxOffsetY);
            }
        }

        // Modo Focus
        if (Input.GetKeyDown(KeyCode.LeftShift))
        {
            originalOffset = offset;
            offset = focusOffset;
            isFocusing = true;
        }
        else if (Input.GetKeyUp(KeyCode.LeftShift) && isFocusing)
        {
            offset = originalOffset;
            isFocusing = false;
        }
    }
}
