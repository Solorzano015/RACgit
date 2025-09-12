using UnityEngine;
using System.Collections;
using UnityEditor;  

public class EnemyMovement : MonoBehaviour
{
    public enum MovementState { Roaming, Chasing }

    [Header("Área de movimiento (Roaming)")]
    public Vector3 areaCenter = Vector3.zero;
    public Vector3 areaSize = new Vector3(10, 0, 10);

    [Header("Combate")]
    public int maxVidas = 3;
    private int vidasActuales;
    public string recibirGolpeParameter = "isHit";

    [Header("Invulnerabilidad")]
    public float tiempoInvulnerabilidad = 0.5f;
    private bool esInvulnerable = false;

    [Header("Hitbox de Golpe")]
    public Vector3 hitboxOffset = Vector3.zero;
    public Vector3 hitboxSize = new Vector3(1f, 1f, 1f);
    public LayerMask golpeLayerMask;
    public float knockbackForce = 10f;

    [Header("Parámetros de Roaming")]
    public float speed = 10f;
    public float maxSpeed = 5f;
    public float stoppingDistance = 1f;
    public float minWaitTime = 1f;
    public float maxWaitTime = 3f;
    public float waitAfterArrival = 2f;

    [Header("Parámetros de Chase")]
    public Transform playerTransform;
    public float playerDetectionRange = 10f;
    public float slowDownDistance = 5f;
    public float stopDistanceToPlayer = 1.5f;
    public float slowedSpeedMultiplier = 0.5f;
    public float chaseSpeedMultiplier = 1.5f;

    [Header("Animator")]
    public string walkingParameter = "isWalking";
    public string idleParameter = "isIdle";
    public string playerDetectedParameter = "playerDetected";
    public string chasingFastParameter = "isChasingFast";
    public string chasingSlowParameter = "isChasingSlow";
    public string nearPlayerParameter = "isNearPlayer";

    private Vector3 targetPosition;
    private Animator animator;
    private Rigidbody rb;
    private MovementState currentState = MovementState.Roaming;
    private Coroutine roamingCoroutine;

    void Start()
    {
        vidasActuales = maxVidas;
        
        Debug.Log("EnemyMovement Script iniciado");
        animator = GetComponent<Animator>();
        rb = GetComponent<Rigidbody>();

        GameObject player = GameObject.FindWithTag("Player");
        if (player != null)
        {
            playerTransform = player.transform;
        }
        else
        {
            Debug.LogWarning("No se encontró ningún GameObject con el tag 'Player'. Asegúrate de asignarlo en el jugador.");
        }

        if(rb == null)
        {
            Debug.LogError("NO HAY RIGIDBODY EN EL ENEMIGO");
            return;
        }

        rb.isKinematic = false;
        rb.useGravity = true;
        rb.constraints = RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationZ;

        roamingCoroutine = StartCoroutine(MoveRoutine());
    }

    void Update()
    {
        if (playerTransform != null)
        {
            float distToPlayer = Vector3.Distance(transform.position, playerTransform.position);

            if (IsInsideArea(playerTransform.position) && distToPlayer <= playerDetectionRange)
            {
                if(currentState != MovementState.Chasing)
                {
                    Debug.Log("Jugador detectado dentro del área. Cambiando a modo Chase.");
                    currentState = MovementState.Chasing;
                    animator.SetBool(playerDetectedParameter, true);
                    if(roamingCoroutine != null)
                        StopCoroutine(roamingCoroutine);
                }
            }
            else
            {
                if(currentState != MovementState.Roaming)
                {
                    Debug.Log("Jugador fuera del área o rango. Cambiando a modo Roaming.");
                    currentState = MovementState.Roaming;
                    animator.SetBool(playerDetectedParameter, false);
                    ResetChaseParameters();
                    roamingCoroutine = StartCoroutine(MoveRoutine());
                }
            }
        }

        if (!IsInsideArea(transform.position) && currentState == MovementState.Chasing)
        {
            Debug.Log("Enemigo fuera del área, regresando a Roaming.");
            currentState = MovementState.Roaming;
            animator.SetBool(playerDetectedParameter, false);
            ResetChaseParameters();
            roamingCoroutine = StartCoroutine(MoveRoutine());
        }
    }


    void FixedUpdate()
    {
        if(currentState == MovementState.Chasing)
        {
            ChasePlayer();
        }

        //DetectarGolpe();

        if (vidasActuales > 0 && !esInvulnerable)
        {
            DetectarGolpe();
        }
    }

    private void ChasePlayer()
    {
        if(playerTransform == null) return;

        Vector3 direction = playerTransform.position - transform.position;
        direction.y = 0;
        float distance = direction.magnitude;
        direction.Normalize();

        if(distance <= stopDistanceToPlayer)
        {
            //Debug.Log("Alcanzó al jugador, deteniéndose.");
            rb.linearVelocity = Vector3.zero;
            ResetChaseParameters();
            animator.SetBool(nearPlayerParameter, true);
            return;
        }

        float currentSpeed = speed * chaseSpeedMultiplier;
        if(distance <= slowDownDistance)
        {
            currentSpeed *= slowedSpeedMultiplier;
            //Debug.Log($"Cerca del jugador. Reduciendo velocidad a {currentSpeed}");
            animator.SetBool(chasingSlowParameter, true);
            animator.SetBool(chasingFastParameter, false);
        }
        else
        {
            animator.SetBool(chasingFastParameter, true);
            animator.SetBool(chasingSlowParameter, false);
        }

        animator.SetBool(nearPlayerParameter, false);

        Vector3 movement = direction * currentSpeed * Time.fixedDeltaTime;
        Vector3 newPosition = rb.position + movement;

        if (IsInsideArea(newPosition))
        {
            rb.MovePosition(newPosition);
            RotateTowards(direction);
            animator.SetBool(walkingParameter, true);
            animator.SetBool(idleParameter, false);
        }
        else
        {
            //Debug.Log("Intento de salida del área bloqueado.");
        }
    }

    private IEnumerator MoveRoutine()
    {
        while (true)
        {
            float waitTime = Random.Range(minWaitTime, maxWaitTime);
           // Debug.Log($"[Roaming] Esperando {waitTime} segundos antes de moverse...");
            rb.linearVelocity = Vector3.zero;
            animator.SetBool(idleParameter, true);
            animator.SetBool(walkingParameter, false);
            yield return new WaitForSeconds(waitTime);

            targetPosition = GetRandomPosition();
            //Debug.Log($"[Roaming] Nuevo destino: {targetPosition}");
            animator.SetBool(walkingParameter, true);
            animator.SetBool(idleParameter, false);

            yield return StartCoroutine(MoveToTarget());
            yield return new WaitForSeconds(waitAfterArrival);
        }
    }

    private IEnumerator MoveToTarget()
    {
        while (true)
        {
            Vector3 direction = targetPosition - transform.position;
            direction.y = 0;
            float distance = direction.magnitude;

            if(distance <= stoppingDistance)
            {
               // Debug.Log("[Roaming] Llegó al destino.");
                rb.linearVelocity = Vector3.zero;
                animator.SetBool(walkingParameter, false);
                animator.SetBool(idleParameter, true);
                break;
            }

            direction.Normalize();
            Vector3 movement = direction * speed * Time.fixedDeltaTime;
            rb.MovePosition(rb.position + movement);
            RotateTowards(direction);
            yield return new WaitForFixedUpdate();
        }
    }

    private Vector3 GetRandomPosition()
    {
        float randomX = Random.Range(-areaSize.x / 2f, areaSize.x / 2f);
        float randomZ = Random.Range(-areaSize.z / 2f, areaSize.z / 2f);
        return areaCenter + new Vector3(randomX, 0, randomZ);
    }

    private bool IsInsideArea(Vector3 position)
    {
        return position.x >= areaCenter.x - areaSize.x / 2 &&
               position.x <= areaCenter.x + areaSize.x / 2 &&
               position.z >= areaCenter.z - areaSize.z / 2 &&
               position.z <= areaCenter.z + areaSize.z / 2;
    }

    private void RotateTowards(Vector3 direction)
    {
        if(direction != Vector3.zero)
        {
            Quaternion targetRotation = Quaternion.LookRotation(direction);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.fixedDeltaTime * 10f);
        }
    }

    private void ResetChaseParameters()
    {
        animator.SetBool(chasingFastParameter, false);
        animator.SetBool(chasingSlowParameter, false);
        animator.SetBool(nearPlayerParameter, false);
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireCube(areaCenter, areaSize);
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.layer == golpeLayerMask)
        {
            Debug.Log("¡Enemigo golpeado!");

            // Determinar la dirección contraria a la que está mirando
            Vector3 knockbackDirection = -transform.forward;
            knockbackDirection.y = 0;
            knockbackDirection.Normalize();

            // Aplicar fuerza de retroceso
            rb.AddForce(knockbackDirection * knockbackForce, ForceMode.Impulse);
        }
    }

    private void DetectarGolpe()
    {
        Vector3 worldCenter = transform.position + transform.TransformDirection(hitboxOffset);
        Collider[] hits = Physics.OverlapBox(worldCenter, hitboxSize * 0.5f, transform.rotation, golpeLayerMask);

        if (hits.Length > 0)
        {
            // ACTIVAMOS INVULNERABILIDAD INMEDIATAMENTE
            esInvulnerable = true;

            Debug.Log("¡Enemigo golpeado desde hitbox!");

            // Animación de daño
            if (animator != null)
            {
                animator.SetTrigger(recibirGolpeParameter);
            }

            // Retroceso
            Vector3 knockbackDirection = -transform.forward;
            knockbackDirection.y = 0;
            knockbackDirection.Normalize();
            rb.AddForce(knockbackDirection * knockbackForce, ForceMode.Impulse);

            // Reducir vida
            vidasActuales--;
            Debug.Log($"Vida restante del enemigo: {vidasActuales}");

            if (vidasActuales <= 0)
            {
                Debug.Log("¡Enemigo derrotado!");
                Destroy(gameObject);
            }
            else
            {
                StartCoroutine(InvulnerabilidadTemporal());
            }
        }
    }


    private IEnumerator InvulnerabilidadTemporal()
    {
        esInvulnerable = true;
        yield return new WaitForSeconds(tiempoInvulnerabilidad);
        esInvulnerable = false;
}

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Matrix4x4 rotationMatrix = Matrix4x4.TRS(transform.position + transform.TransformDirection(hitboxOffset), transform.rotation, Vector3.one);
        Gizmos.matrix = rotationMatrix;
        Gizmos.DrawWireCube(Vector3.zero, hitboxSize);
    }
}
