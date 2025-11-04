using UnityEngine;

public class ParticleRateController : MonoBehaviour
{
    [Header("Referencias")]
    [SerializeField] private ParticleSystem particleSystem;
    [SerializeField] private MonoBehaviour sourceScript; // Script que contiene walkPressed
    [SerializeField] private string walkPressedFieldName = "walkPressed";
    [SerializeField] private Transform objectToTrack; // Objeto que se mueve (por defecto este mismo)

    [Header("Configuración de emisión")]
    [SerializeField] private float minRate = 0f;
    [SerializeField] private float maxRate = 100f;
    [SerializeField] private float movementThreshold = 0.01f; // Sensibilidad del movimiento
    [SerializeField] private float smoothSpeed = 5f;

    private ParticleSystem.EmissionModule emissionModule;
    private System.Reflection.FieldInfo walkPressedField;
    private System.Reflection.PropertyInfo walkPressedProperty;
    private Vector3 lastPosition;
    private float currentRate;

    void Start()
    {
        if (particleSystem == null)
            particleSystem = GetComponent<ParticleSystem>();

        if (objectToTrack == null)
            objectToTrack = transform;

        emissionModule = particleSystem.emission;
        currentRate = minRate;
        lastPosition = objectToTrack.position;

        if (sourceScript != null)
        {
            var type = sourceScript.GetType();
            walkPressedField = type.GetField(walkPressedFieldName);
            walkPressedProperty = type.GetProperty(walkPressedFieldName);
        }
    }

    void Update()
    {
        // 1️⃣ Obtener el valor de walkPressed
        bool walkPressed = false;
        if (sourceScript != null)
        {
            if (walkPressedField != null)
                walkPressed = (bool)walkPressedField.GetValue(sourceScript);
            else if (walkPressedProperty != null)
                walkPressed = (bool)walkPressedProperty.GetValue(sourceScript);
        }

        // 2️⃣ Detectar si se está moviendo
        float distanceMoved = Vector3.Distance(objectToTrack.position, lastPosition);
        bool isMoving = distanceMoved > movementThreshold;
        lastPosition = objectToTrack.position;

        // 3️⃣ Activar partículas solo si ambas condiciones son verdaderas
        bool shouldActivate = walkPressed && isMoving;
        float targetRate = shouldActivate ? maxRate : minRate;

        // 4️⃣ Transición suave
        currentRate = Mathf.Lerp(currentRate, targetRate, Time.deltaTime * smoothSpeed);
        emissionModule.rateOverTime = currentRate;
    }
}
