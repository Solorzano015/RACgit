using UnityEngine;
using System.Collections; // Necesario para las corrutinas

public class EnemyPatrol : MonoBehaviour
{
    [Header("Configuración General")]
    [Tooltip("El Transform del jugador al que el enemigo debe perseguir y atacar.")]
    public Transform playerTransform;
    [Tooltip("El componente Animator del enemigo para controlar las animaciones.")]
    public Animator enemyAnimator;

    [Header("Configuración de Patrullaje")]
    [Tooltip("El centro del área de patrullaje en el mundo.")]
    public Vector3 patrolCenter = Vector3.zero;
    [Tooltip("El tamaño del área de patrullaje (cubo).")]
    public Vector3 patrolAreaSize = new Vector3(20f, 1f, 20f);
    [Tooltip("Velocidad de movimiento del enemigo durante el patrullaje y la persecución.")]
    public float moveSpeed = 3f;
    [Tooltip("Velocidad de rotación base del enemigo (usada cuando no persigue).")]
    public float baseRotationSpeed = 2f;
    [Tooltip("Tiempo mínimo que el enemigo esperará en un punto de patrullaje antes de moverse a otro.")]
    public float minWaitTime = 1f;
    [Tooltip("Tiempo máximo que el enemigo esperará en un punto de patrullaje antes de moverse a otro.")]
    public float maxWaitTime = 3f;
    [Tooltip("Tiempo de retraso antes de que el enemigo empiece a moverse (patrullaje o persecución).")]
    public float movementStartDelay = 0.5f;

    [Header("Configuración de Persecución y Ataque Normal")]
    [Tooltip("Distancia a la que el enemigo dejará de perseguir y volverá a patrullar.")]
    public float chaseStopDistance = 25f; // Aunque usamos patrolAreaSize para esto, se mantiene por si se quiere un valor diferente.
    [Tooltip("Rango de ataque del enemigo. Si el jugador está dentro de esta distancia, el enemigo atacará.")]
    public float attackRange = 2f;
    [Tooltip("Tiempo que el enemigo permanece en el estado de ataque normal (duración de la animación de ataque).")]
    public float attackRotationHoldTime = 1f;
    [Tooltip("Velocidad de rotación del enemigo cuando persigue al jugador.")]
    public float chaseRotationSpeed = 5f;
    [Tooltip("El ángulo máximo (en grados) para que el enemigo ataque al jugador (ej. 30 para +/- 15 grados).")]
    public float attackAngleThreshold = 30f;

    [Header("Configuración de Ataque de Rodar")]
    [Tooltip("Distancia mínima del jugador para activar el ataque de rodar.")]
    public float rollAttackMinDistance = 10f;
    [Tooltip("Velocidad del ataque de rodar.")]
    public float rollAttackSpeed = 15f;
    [Tooltip("Tiempo máximo de persecución antes de intentar un ataque de rodar.")]
    public float chaseDurationForRollAttack = 10f;

    [Header("Nombres de Parámetros de Animator")]
    [Tooltip("El nombre del parámetro booleano en el Animator para la animación de caminar (ej. 'IsWalking').")]
    public string walkingParameterName = "IsWalking";
    [Tooltip("El nombre del parámetro booleano en el Animator para el ataque normal (ej. 'IsAttacking').")]
    public string attackingParameterName = "IsAttacking";
    [Tooltip("El nombre del parámetro booleano en el Animator para el ataque de rodar (ej. 'IsRollingAttack').")]
    public string rollAttackParameterName = "IsRollingAttack";

    [Header("Configuración de Desatasco")]
    [Tooltip("Distancia mínima que el enemigo debe moverse por FixedUpdate para no considerarse atascado.")]
    public float stuckThreshold = 0.05f; 
    [Tooltip("Tiempo máximo que el enemigo puede estar atascado antes de intentar desatascarse.")]
    public float maxStuckTime = 1.5f; 

    // Referencias privadas
    private Rigidbody rb;
    private Vector3 targetPosition;
    private float rotationSpeed; 

    // Estados del enemigo
    private bool isPatrolling = false;
    private bool isWaiting = false;
    private bool isChasing = false;
    private bool isAttacking = false; 
    private bool isRollAttacking = false; 
    private bool isApplyingMovementStartDelay = false; 
    private bool ignoreStuckDetectionTemporarily = false; 
    private float ignoreStuckDetectionDuration = 0.2f; // Pequeña duración para ignorar la detección (ajustable)

    // Timers
    private float chaseTimer = 0f; 
    private float stuckTimer = 0f; 
    private Vector3 lastPosition; 

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        if (rb == null)
        {
            Debug.LogError("¡ERROR! Este script requiere un componente Rigidbody en el enemigo.", this);
            enabled = false;
            return;
        }

        rb.useGravity = true;
        rb.freezeRotation = true; 

        if (enemyAnimator == null)
        {
            Debug.LogWarning("Advertencia: No se encontró un Animator asignado. Las animaciones no se controlarán.", this);
        }

        lastPosition = transform.position; 
        // Inicia el patrullaje al comenzar el juego
        StartCoroutine(PatrolRoutine()); 
    }

    void Update()
    {
        if (playerTransform == null)
        {
            // Si no hay playerTransform, forzar al enemigo a patrullar y limpiar todos los estados.
            if (!isPatrolling) // Solo si no estamos ya patrullando
            {
                Debug.LogWarning("Advertencia: No se ha asignado un Player Transform. Forzando patrullaje.");
                StopAllCoroutines(); 
                isChasing = false;
                isAttacking = false;
                isRollAttacking = false;
                isWaiting = false;
                isApplyingMovementStartDelay = false;
                chaseTimer = 0f;
                StartCoroutine(PatrolRoutine());
            }
            return; 
        }

        float distanceToPlayer = Vector3.Distance(transform.position, playerTransform.position);
        Bounds patrolBounds = new Bounds(patrolCenter, patrolAreaSize);
        patrolBounds.center = new Vector3(patrolBounds.center.x, playerTransform.position.y, patrolBounds.center.z);
        patrolBounds.size = new Vector3(patrolBounds.size.x, 1f, patrolBounds.size.z);
        bool playerInDetectionArea = patrolBounds.Contains(playerTransform.position);


        if (playerInDetectionArea)
        {
            // Si el jugador está en el área de detección y NO estamos ya persiguiendo o rodando
            if (!isChasing && !isRollAttacking)
            {
                TransitionToChaseState();
            }

            // Aumentar el timer de persecución solo si estamos persiguiendo y NO estamos en ningún ataque O aplicando el retraso de movimiento
            if (isChasing && !isAttacking && !isRollAttacking && !isApplyingMovementStartDelay)
            {
                chaseTimer += Time.deltaTime;
                Debug.Log($"Persecución activa. chaseTimer: {chaseTimer:F2} segundos.");
            }

            // Lógica para iniciar el ataque de rodar
            if (isChasing && !isAttacking && !isRollAttacking && !isApplyingMovementStartDelay)
            {
                bool triggerRollAttackByDistance = distanceToPlayer > attackRange && distanceToPlayer > rollAttackMinDistance;
                bool triggerRollAttackByTime = chaseTimer >= chaseDurationForRollAttack;

                if (triggerRollAttackByTime)
                {
                    Debug.Log($"CONDICIÓN DE TIEMPO CUMPLIDA: chaseTimer ({chaseTimer:F2}s) >= chaseDurationForRollAttack ({chaseDurationForRollAttack}s)");
                }

                if (triggerRollAttackByDistance || triggerRollAttackByTime)
                {
                    Debug.Log($"Condición de ataque de rodar cumplida. Iniciando ataque de rodar. Distancia: {distanceToPlayer:F2}, Timer: {chaseTimer:F2}");
                    // PerformRollAttack() maneja su propio StopAllCoroutines() y la transición de estados.
                    StartCoroutine(PerformRollAttack());
                    return; 
                }
            }

            // Lógica de ataque normal (solo si estamos persiguiendo, no atacando de rodar, y el jugador está en rango y ángulo)
            if (isChasing && !isAttacking && !isRollAttacking && !isApplyingMovementStartDelay && distanceToPlayer <= attackRange)
            {
                Vector3 directionToPlayer = (playerTransform.position - transform.position).normalized;
                directionToPlayer.y = 0;
                float angleToPlayer = Vector3.Angle(transform.forward, directionToPlayer);

                if (angleToPlayer <= attackAngleThreshold / 2f)
                {
                    if (!isAttacking) 
                    {
                        Debug.Log("Jugador en rango y mirando! Iniciando ataque normal.");
                        StartCoroutine(PerformAttack());
                    }
                }
                else 
                {
                    if (isAttacking) 
                    {
                        Debug.Log("Jugador fuera de ángulo o rango de ataque normal. Deteniendo ataque normal.");
                        ResetAttackState(); 
                    }
                }
            }
            else 
            {
                if (isAttacking) 
                {
                    Debug.Log("Jugador fuera de rango de ataque normal o ángulo incorrecto. Deteniendo ataque normal.");
                    ResetAttackState(); 
                }
            }

            // El objetivo de movimiento ahora es el jugador (solo en XZ) si estamos persiguiendo y NO atacando (normal o rodando)
            if (isChasing && !isAttacking && !isRollAttacking && !isApplyingMovementStartDelay)
            {
                targetPosition = new Vector3(playerTransform.position.x, transform.position.y, playerTransform.position.z);
            }
        }
        else // El jugador está fuera del área de detección/patrullaje
        {
            // Si estábamos persiguiendo/atacando/rodando y el jugador salió del área
            if (isChasing || isAttacking || isRollAttacking) 
            {
                TransitionToPatrolState();
            }
        }

        // Lógica de desatasco (solo si se está moviendo y no en ataques estacionarios o esperando el inicio del movimiento)
        bool currentlyMovingOrChasing = (isPatrolling && !isWaiting && !isApplyingMovementStartDelay) || (isChasing && !isAttacking && !isRollAttacking && !isApplyingMovementStartDelay);

        // --- MODIFICACIÓN: Añadir la condición para ignorar temporalmente la detección de atasco ---
        if (currentlyMovingOrChasing && !ignoreStuckDetectionTemporarily)
        {
            // La velocidad del Rigidbody en XZ es la indicación de movimiento.
            Vector3 flatVelocity = new Vector3(rb.linearVelocity.x, 0, rb.linearVelocity.z);
            if (flatVelocity.sqrMagnitude < stuckThreshold * stuckThreshold)
            {
                stuckTimer += Time.deltaTime;
                if (stuckTimer >= maxStuckTime)
                {
                    Debug.Log($"Enemigo atascado en {transform.position}! Intentando desatascarse. stuckTimer: {stuckTimer:F2}, current flat velocity: {flatVelocity.magnitude:F2}");
                    stuckTimer = 0f;

                    StopAllCoroutines(); 
                    isApplyingMovementStartDelay = false; 

                    if (playerTransform != null && patrolBounds.Contains(playerTransform.position))
                    {
                        Debug.Log("Reanudando persecución después de desatascarse.");
                        isChasing = true;
                        isPatrolling = false;
                        isWaiting = false;
                        isAttacking = false;
                        isRollAttacking = false;
                        StartCoroutine(ApplyMovementDelayRoutine()); 
                    }
                    else 
                    {
                        Debug.Log("Volviendo a patrullar después de desatascarse.");
                        StartCoroutine(PatrolRoutine());
                    }
                    return;
                }
            }
            else
            {
                stuckTimer = 0f; 
            }
        }
        else // Resetear el timer si no estamos en un estado donde pueda atascarse o si estamos en el período de gracia
        {
            stuckTimer = 0f; 
        }
    }


    void FixedUpdate() 
    {
        Vector3 currentVelocity = rb.linearVelocity; 
        Vector3 horizontalMovement = Vector3.zero;

        rotationSpeed = (isChasing && !isRollAttacking) ? chaseRotationSpeed : baseRotationSpeed;

        if (isRollAttacking) 
        {
            horizontalMovement = transform.forward * rollAttackSpeed;

            if (enemyAnimator != null)
            {
                enemyAnimator.SetBool(walkingParameterName, false);
                enemyAnimator.SetBool(attackingParameterName, false);
                enemyAnimator.SetBool(rollAttackParameterName, true); 
            }
        }
        else if ((isPatrolling && !isWaiting && !isApplyingMovementStartDelay) || (isChasing && !isAttacking && !isApplyingMovementStartDelay)) 
        {
            Vector3 directionToTarget = (targetPosition - transform.position);
            directionToTarget.y = 0;
            directionToTarget.Normalize();

            horizontalMovement = directionToTarget * moveSpeed;

            if (directionToTarget != Vector3.zero)
            {
                Quaternion targetRotation = Quaternion.LookRotation(directionToTarget);
                rb.rotation = Quaternion.Slerp(rb.rotation, targetRotation, rotationSpeed * Time.fixedDeltaTime);
            }

            if (enemyAnimator != null)
            {
                enemyAnimator.SetBool(walkingParameterName, true);
                enemyAnimator.SetBool(attackingParameterName, false);
                enemyAnimator.SetBool(rollAttackParameterName, false);
            }

            if (isPatrolling)
            {
                Vector3 flatPos = new Vector3(transform.position.x, 0, transform.position.z);
                Vector3 flatTarget = new Vector3(targetPosition.x, 0, targetPosition.z);
                
                if (Vector3.Distance(flatPos, flatTarget) < 0.5f) 
                {
                    rb.linearVelocity = new Vector3(0, rb.linearVelocity.y, 0); 
                    StartCoroutine(WaitAndFindNewTarget());
                }
            }
        }
        else 
        {
            horizontalMovement = Vector3.zero; 
            
            if (!rb.isKinematic) 
            {
                rb.angularVelocity = Vector3.zero; 
            }

            if (enemyAnimator != null)
            {
                if (!isAttacking && !isRollAttacking)
                {
                    enemyAnimator.SetBool(walkingParameterName, false);
                }
            }
        }

        Vector3 newPosition = transform.position + new Vector3(horizontalMovement.x, 0, horizontalMovement.z) * Time.fixedDeltaTime;
        rb.MovePosition(new Vector3(newPosition.x, transform.position.y + currentVelocity.y * Time.fixedDeltaTime, newPosition.z));
    }


    void OnCollisionEnter(Collision collision)
    {
        if (isRollAttacking) 
        {
            Debug.Log("Ataque de rodar chocó con: " + collision.gameObject.name);
            StopAllCoroutines(); 
            StartCoroutine(StopRollAttackAndResumeRoutine());
        }
    }

    void TransitionToChaseState()
    {
        Debug.Log("Transicionando a estado de persecución.");
        StopAllCoroutines(); 
        isChasing = true;
        isPatrolling = false;
        isWaiting = false;
        isAttacking = false;
        isRollAttacking = false;
        chaseTimer = 0f; 
        rb.isKinematic = false;

        StartCoroutine(ApplyMovementDelayRoutine());
    }

    void TransitionToPatrolState()
    {
        Debug.Log("Transicionando a estado de patrullaje.");
        StopAllCoroutines(); 
        isChasing = false;
        isAttacking = false;
        isRollAttacking = false;
        isApplyingMovementStartDelay = false; 
        chaseTimer = 0f;
        StartCoroutine(PatrolRoutine());
    }

    void ResetAttackState()
    {
        isAttacking = false;
        if (enemyAnimator != null) enemyAnimator.SetBool(attackingParameterName, false);
        rb.isKinematic = false;
    }

    IEnumerator PatrolRoutine()
    {
        if (isPatrolling) yield break; 

        isPatrolling = true;
        Debug.Log("Iniciando rutina de patrullaje.");

        while (true) 
        {
            if (isChasing || isAttacking || isRollAttacking)
            {
                isPatrolling = false; 
                Debug.Log("PatrolRoutine: Estado cambiado, saliendo del patrullaje.");
                yield break;
            }

            targetPosition = GetRandomPointInPatrolArea();
            isWaiting = false; 

            rb.isKinematic = false; 

            StartCoroutine(ApplyMovementDelayRoutine());

            yield return new WaitUntil(() => !isApplyingMovementStartDelay || isChasing || isAttacking || isRollAttacking);

            if (isChasing || isAttacking || isRollAttacking)
            {
                isPatrolling = false;
                Debug.Log("PatrolRoutine: Estado cambiado durante el delay, saliendo del patrullaje.");
                yield break;
            }

            yield return new WaitUntil(() => Vector3.Distance(new Vector3(transform.position.x, 0, transform.position.z), new Vector3(targetPosition.x, 0, targetPosition.z)) < 0.5f || isChasing || isAttacking || isRollAttacking);

            if (isChasing || isAttacking || isRollAttacking)
            {
                isPatrolling = false;
                Debug.Log("PatrolRoutine: Estado cambiado mientras se movía, saliendo del patrullaje.");
                yield break;
            }
            
            Debug.Log("PatrolRoutine: Llegó al punto, iniciando espera.");
            StartCoroutine(WaitAndFindNewTarget()); 
            
            yield return new WaitUntil(() => !isWaiting || isChasing || isAttacking || isRollAttacking);

            if (isChasing || isAttacking || isRollAttacking)
            {
                isPatrolling = false;
                Debug.Log("PatrolRoutine: Estado cambiado durante la espera, saliendo del patrullaje.");
                yield break;
            }

            Debug.Log("PatrolRoutine: Espera terminada, buscando nuevo punto de patrulla.");
        }
    }

    IEnumerator WaitAndFindNewTarget()
    {
        if (isWaiting || isChasing || isAttacking || isRollAttacking) yield break;

        isPatrolling = false;
        isWaiting = true;

        rb.linearVelocity = new Vector3(0, rb.linearVelocity.y, 0);
        if (!rb.isKinematic)
        {
            rb.angularVelocity = Vector3.zero;
        }
        rb.isKinematic = true;

        if (enemyAnimator != null)
        {
            enemyAnimator.SetBool(walkingParameterName, false);
            enemyAnimator.SetBool(attackingParameterName, false);
            enemyAnimator.SetBool(rollAttackParameterName, false);
        }

        float waitTime = Random.Range(minWaitTime, maxWaitTime);
        Debug.Log($"Enemigo esperando por {waitTime:F2} segundos...");
        yield return new WaitForSeconds(waitTime);
        Debug.Log("Enemigo terminó de esperar.");

        rb.isKinematic = false;


        if (playerTransform != null)
        {
            Bounds patrolBounds = new Bounds(patrolCenter, patrolAreaSize);
            patrolBounds.center = new Vector3(patrolBounds.center.x, playerTransform.position.y, patrolBounds.center.z);
            patrolBounds.size = new Vector3(patrolBounds.size.x, 1f, patrolBounds.size.z);

            if (patrolBounds.Contains(playerTransform.position))
            {
                Debug.Log("Jugador cerca después de esperar. Transicionando a persecución.");
                TransitionToChaseState();
                yield break;
            }
        }
        isWaiting = false;

        TransitionToPatrolState();
    }

    IEnumerator ApplyMovementDelayRoutine()
    {
        if (isApplyingMovementStartDelay)
        {
            Debug.Log("ApplyMovementDelayRoutine: Ya hay un delay en progreso, no iniciando otro.");
            yield break;
        }

        isApplyingMovementStartDelay = true;
        
        rb.linearVelocity = new Vector3(0, rb.linearVelocity.y, 0);
        if (!rb.isKinematic) rb.angularVelocity = Vector3.zero;
        if (enemyAnimator != null) enemyAnimator.SetBool(walkingParameterName, false);

        Debug.Log($"Aplicando retraso de movimiento por {movementStartDelay:F2} segundos.");
        yield return new WaitForSeconds(movementStartDelay);
        isApplyingMovementStartDelay = false;
        Debug.Log("Retraso de movimiento terminado.");

        // --- NUEVO: Activar la bandera para ignorar la detección de atasco temporalmente ---
        StartCoroutine(IgnoreStuckDetectionGracePeriod());
    }

    IEnumerator IgnoreStuckDetectionGracePeriod()
    {
        ignoreStuckDetectionTemporarily = true;
        Debug.Log($"Ignorando detección de atasco por {ignoreStuckDetectionDuration:F2} segundos.");
        yield return new WaitForSeconds(ignoreStuckDetectionDuration);
        ignoreStuckDetectionTemporarily = false;
        Debug.Log("Detección de atasco reanudada.");
    }

    IEnumerator PerformAttack()
    {
        if (isAttacking) yield break; 

        isAttacking = true;
        isChasing = false; 
        isPatrolling = false;
        isWaiting = false;
        isRollAttacking = false;
        isApplyingMovementStartDelay = false; 

        StopAllCoroutines(); 

        rb.linearVelocity = new Vector3(0, rb.linearVelocity.y, 0);
        if (!rb.isKinematic) rb.angularVelocity = Vector3.zero;
        rb.isKinematic = true; 

        if (enemyAnimator != null)
        {
            enemyAnimator.SetBool(walkingParameterName, false);
            enemyAnimator.SetBool(attackingParameterName, true);
            enemyAnimator.SetBool(rollAttackParameterName, false);
        }

        if (playerTransform != null)
        {
            Vector3 lookDirection = playerTransform.position - transform.position;
            lookDirection.y = 0;
            if (lookDirection != Vector3.zero)
            {
                Quaternion startRotation = transform.rotation;
                Quaternion targetRot = Quaternion.LookRotation(lookDirection);

                float rotateTimer = 0f;
                float rotateDuration = 0.2f;
                while (rotateTimer < rotateDuration)
                {
                    transform.rotation = Quaternion.Slerp(startRotation, targetRot, rotateTimer / rotateDuration);
                    rotateTimer += Time.deltaTime;
                    yield return null;
                }
                transform.rotation = targetRot;
            }
        }

        yield return new WaitForSeconds(attackRotationHoldTime);

        if (enemyAnimator != null)
        {
            enemyAnimator.SetBool(attackingParameterName, false);
        }

        isAttacking = false;
        rb.isKinematic = false;

        if (playerTransform != null)
        {
            Bounds patrolBounds = new Bounds(patrolCenter, patrolAreaSize);
            patrolBounds.center = new Vector3(patrolBounds.center.x, playerTransform.position.y, patrolBounds.center.z);
            patrolBounds.size = new Vector3(patrolBounds.size.x, 1f, patrolBounds.size.z);

            if (patrolBounds.Contains(playerTransform.position))
            {
                TransitionToChaseState(); 
            }
            else
            {
                TransitionToPatrolState(); 
            }
        }
        else
        {
            TransitionToPatrolState(); 
        }
    }

    IEnumerator PerformRollAttack()
    {
        if (isRollAttacking) yield break;

        isRollAttacking = true;
        isChasing = false;
        isPatrolling = false;
        isAttacking = false;
        isWaiting = false;
        isApplyingMovementStartDelay = false; 

        StopAllCoroutines(); 

        rb.linearVelocity = new Vector3(0, rb.linearVelocity.y, 0);
        if (!rb.isKinematic) rb.angularVelocity = Vector3.zero;
        rb.isKinematic = false; 

        if (enemyAnimator != null)
        {
            enemyAnimator.SetBool(walkingParameterName, false);
            enemyAnimator.SetBool(attackingParameterName, false);
            enemyAnimator.SetBool(rollAttackParameterName, true);
        }

        if (playerTransform != null)
        {
            Vector3 lookDirection = playerTransform.position - transform.position;
            lookDirection.y = 0;
            if (lookDirection != Vector3.zero)
            {
                transform.rotation = Quaternion.LookRotation(lookDirection);
            }
        }
        
        yield return new WaitForSeconds(0.3f); 

        yield break;
    }

    IEnumerator StopRollAttackAndResumeRoutine()
    {
        if (!isRollAttacking) yield break;

        isRollAttacking = false;

        rb.linearVelocity = new Vector3(0, rb.linearVelocity.y, 0);
        if (!rb.isKinematic) 
        {
            rb.angularVelocity = Vector3.zero;
        }
        rb.isKinematic = false;

        if (enemyAnimator != null)
        {
            enemyAnimator.SetBool(rollAttackParameterName, false);
        }

        yield return new WaitForSeconds(0.1f);

        if (playerTransform != null)
        {
            Bounds patrolBounds = new Bounds(patrolCenter, patrolAreaSize);
            patrolBounds.center = new Vector3(patrolBounds.center.x, playerTransform.position.y, patrolBounds.center.z);
            patrolBounds.size = new Vector3(patrolBounds.size.x, 1f, patrolBounds.size.z);

            if (patrolBounds.Contains(playerTransform.position))
            {
                Debug.Log("Ataque de rodar terminado. Jugador en rango, volviendo a persecución. Reiniciando chaseTimer.");
                TransitionToChaseState(); 
            }
            else
            {
                Debug.Log("Ataque de rodar terminado. Jugador fuera de rango, volviendo a patrullar. Reiniciando chaseTimer.");
                TransitionToPatrolState(); 
            }
        }
        else
        {
            Debug.Log("Ataque de rodar terminado. Jugador no encontrado. Volviendo a patrullar. Reiniciando chaseTimer.");
            TransitionToPatrolState(); 
        }
    }

    private Vector3 GetRandomPointInPatrolArea()
    {
        float randomX = Random.Range(patrolCenter.x - patrolAreaSize.x / 2f, patrolCenter.x + patrolAreaSize.x / 2f);
        float randomZ = Random.Range(patrolCenter.z - patrolAreaSize.z / 2f, patrolCenter.z + patrolAreaSize.z / 2f);
        return new Vector3(randomX, transform.position.y, randomZ);
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireCube(patrolCenter, patrolAreaSize);

        if (isPatrolling && !isWaiting)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawSphere(targetPosition, 0.2f); 
        }

        Gizmos.color = Color.magenta;
        Gizmos.DrawWireSphere(transform.position, attackRange); 

        Vector3 forward = transform.forward;
        forward.y = 0; 
        forward.Normalize();

        Quaternion leftArcRotation = Quaternion.AngleAxis(-attackAngleThreshold / 2f, Vector3.up);
        Quaternion rightArcRotation = Quaternion.AngleAxis(attackAngleThreshold / 2f, Vector3.up);

        Vector3 leftArcDirection = leftArcRotation * forward;
        Vector3 rightArcDirection = rightArcRotation * forward;

        Gizmos.DrawRay(transform.position, leftArcDirection * attackRange);
        Gizmos.DrawRay(transform.position, rightArcDirection * attackRange);

        // Puedes dibujar un arco más visual si lo deseas, aunque es un poco más complejo
        // UnityEditor.Handles.color = Color.blue;
        // UnityEditor.Handles.DrawSolidArc(transform.position, Vector3.up, leftArcDirection, attackAngleThreshold, attackRange);
    }
}