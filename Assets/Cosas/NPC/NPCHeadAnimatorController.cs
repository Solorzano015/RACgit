using UnityEngine;

public class NPCHeadAnimatorController : MonoBehaviour
{
    private Animator animator;
    private NPCHeadRotation headRotation;
    [Header("Mecánicas Extra")]
    public float detectionRadius = 3f;
    public float verticalRotationSpeed = 5f; // Nueva variable para la velocidad de rotación vertical
    private bool playerInRadius = false;
    public string closeParameterName = "CLOSE";

    [Header("Referencias (Opcional)")]
    public Transform headTransformToRotate;

    void Start()
    {
        animator = GetComponent<Animator>();
        // Buscamos el componente NPCHeadRotation
        headRotation = GetComponentInChildren<NPCHeadRotation>();
        if (headRotation == null)
        {
            headRotation = GetComponentInParent<NPCHeadRotation>();
        }

        if (headRotation == null)
        {
            Debug.LogError("No se encontró el componente NPCHeadRotation en este GameObject o sus ancestros/descendientes.");
            enabled = false;
            return;
        }

        // Suscribimos el evento de cambio de estado de "mirando"
        headRotation.LookingChanged += OnLookingStateChanged;

        // Si no se asignó una cabeza específica para rotar, usamos el propio transform
        if (headTransformToRotate == null)
        {
            headTransformToRotate = transform;
        }
    }

    void Update()
    {
        // Comprobamos si el jugador está dentro del radio
        if (headRotation.player != null)
        {
            float distanceToPlayer = Vector3.Distance(transform.position, headRotation.player.position);
            bool wasInRadius = playerInRadius;
            playerInRadius = distanceToPlayer <= detectionRadius;

            // Si el jugador entra en el radio
            if (playerInRadius && !wasInRadius)
            {
                animator.SetBool(closeParameterName, true);
                Debug.Log("Jugador entró en el radio cercano.");
            }
            // Si el jugador sale del radio
            else if (!playerInRadius && wasInRadius)
            {
                animator.SetBool(closeParameterName, false);
                Debug.Log("Jugador salió del radio cercano.");
            }

            // Si el jugador está dentro del radio, apuntamos verticalmente
            if (playerInRadius)
            {
                RotateTowardsPlayerVertical();
            }
        }
    }

    void OnLookingStateChanged(bool isLooking)
    {
        animator.SetBool("ATTENTION", isLooking);
    }

    void RotateTowardsPlayerVertical()
    {
        Vector3 targetDirection = headRotation.player.position - headTransformToRotate.position;
        // Proyectamos la dirección al plano horizontal para ignorar la diferencia de altura
        Vector3 targetDirectionHorizontal = Vector3.ProjectOnPlane(targetDirection, Vector3.up).normalized;

        // Si la dirección horizontal no es cero
        if (targetDirectionHorizontal != Vector3.zero)
        {
            Quaternion targetRotation = Quaternion.LookRotation(targetDirectionHorizontal, Vector3.up);
            // Usamos la nueva variable verticalRotationSpeed para controlar la velocidad del Slerp
            headTransformToRotate.rotation = Quaternion.Slerp(headTransformToRotate.rotation, targetRotation, Time.deltaTime * verticalRotationSpeed);
        }
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, detectionRadius);
    }

    private void OnDestroy()
    {
        if (headRotation != null)
        {
            headRotation.LookingChanged -= OnLookingStateChanged;
        }
    }
}