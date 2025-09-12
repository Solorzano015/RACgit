using UnityEngine;
using System.Collections;

public class CharacterController3D : MonoBehaviour
{
    [Header("Movimiento")]
    public float walkSpeed = 5f;
    public float runSpeed = 8f;
    public float jumpForce = 5f;
    [Tooltip("Tiempo de retardo antes de aplicar la fuerza del salto (para sincronizar con la animación)")]
    public float jumpDelay = 0.2f;
    [Tooltip("Factor de influencia del input en el aire (0: sin control, 1: mismo control que en tierra)")]
    [Range(0f, 1f)]
    public float airControlFactor = 0.2f;

    [Header("Controles (Remapeables)")]
    public KeyCode forwardKey = KeyCode.W;
    public KeyCode backwardKey = KeyCode.S;
    public KeyCode leftKey = KeyCode.A;
    public KeyCode rightKey = KeyCode.D;
    public KeyCode jumpKey = KeyCode.Space;
    public KeyCode runKey = KeyCode.LeftShift;

    [Header("Referencias")]
    public Transform cameraTransform;
    public Animator animator;
    
    [Header("Ground Check")]
    public Transform groundCheck;
    public float groundDistance = 0.4f;
    [Tooltip("Capa de los objetos que se consideran suelo normal")]
    public LayerMask groundMask;
    [Tooltip("Capa de los objetos que son rampas")]
    public LayerMask rampLayer;
    [Tooltip("Ángulo máximo permitido para considerar que el personaje está en el suelo (en grados)")]
    public float maxSlopeAngle = 45f;

    [Header("Estado de Daño y Respawn")]
    [Tooltip("Capa que causa el daño al tocarla")]
    public LayerMask damageLayer;
    [Tooltip("Tiempo de inmunidad tras recibir daño o tras el respawn (segundos)")]
    public float damageImmunityDuration = 3f;
    [Tooltip("Si se pone en true, se activa el respawn (para ser modificado por otros scripts)")]
    public bool respawnRequested = false;

    [Header("Vidas")]
    [Tooltip("Número máximo de vidas del personaje")]
    public int maxLives = 3;
    [Header("Vidas Restantes (Mostrar en Inspector)")]
    [Tooltip("Vidas restantes del personaje (solo lectura)")]
    public int currentLives; // Se muestra en el Inspector

    // Propiedad pública para acceder a las vidas restantes (para interfaces, etc.)
    public int CurrentLives { get { return currentLives; } }

    [Header("Push Back")]
    [Tooltip("Capa de la que se aplica el empuje al tocarla")]
    public LayerMask pushBackLayer;
    [Tooltip("Fuerza con la que se empuja al personaje al tocar objetos de la capa pushBackLayer")]
    public float pushBackForce = 10f;

    [Header("Desactivación de Controles")]
    [Tooltip("Tiempo de desactivación de controles si se recibe daño en el aire (después de tocar el suelo)")]
    public float controlDisableDurationAir = 0.2f;
    [Tooltip("Tiempo de desactivación de controles si se recibe daño en tierra")]
    public float controlDisableDurationGround = 0.3f;
    private bool controlsEnabled = true;

    private Rigidbody rb;
    private bool isGrounded;
    private bool isDead = false;
    private bool isImmune = false;
    // Variable para saber si el personaje está sobre una rampa
    private bool isOnRamp = false;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        if (animator == null)
            animator = GetComponent<Animator>();

        currentLives = maxLives;
        Debug.Log("Vidas iniciales: " + currentLives);
    }

    void Update()
    {
        // Si el personaje está muerto, se bloquea el input (se permite respawn mediante variable externa).
        if (isDead)
        {
            if (respawnRequested)
            {
                Respawn();
                respawnRequested = false;
            }
            return;
        }

        // Si los controles están desactivados, procesamos el suelo y animaciones para reactivarlos luego.
        if (!controlsEnabled)
        {
            CheckGround();
            UpdateFallingAndAscendingAnimation();
            return;
        }

        CheckGround();
        HandleMovement();
        HandleJump();
        UpdateFallingAndAscendingAnimation();
    }

    // Comprueba si el personaje está en el suelo usando un Raycast.
    // Si el objeto impactado pertenece a rampLayer, se considera en suelo (isGrounded = true) y se marca isOnRamp.
    void CheckGround()
    {
        RaycastHit hit;
        float rayDistance = groundDistance + 0.1f; // margen extra
        if (Physics.Raycast(groundCheck.position, Vector3.down, out hit, rayDistance))
        {
            int hitLayerMask = 1 << hit.collider.gameObject.layer;
            if ((hitLayerMask & rampLayer) != 0)
            {
                isGrounded = true;
                isOnRamp = true;
            }
            else if ((hitLayerMask & groundMask) != 0)
            {
                float slopeAngle = Vector3.Angle(hit.normal, Vector3.up);
                isGrounded = slopeAngle <= maxSlopeAngle;
                isOnRamp = false;
            }
            else
            {
                isGrounded = false;
                isOnRamp = false;
            }
        }
        else
        {
            isGrounded = false;
            isOnRamp = false;
        }
        animator.SetBool("IsGrounded", isGrounded);
    }

    // Maneja el movimiento.
    // En tierra se asigna la velocidad horizontal según el input.
    // En el aire se añade una aceleración incremental sin sobrescribir el momentum inicial.
    void HandleMovement()
    {
        // Captura el input
        float inputX = 0f;
        float inputZ = 0f;
        if (Input.GetKey(forwardKey)) inputZ += 1f;
        if (Input.GetKey(backwardKey)) inputZ -= 1f;
        if (Input.GetKey(leftKey)) inputX -= 1f;
        if (Input.GetKey(rightKey)) inputX += 1f;

        Vector3 inputDir = new Vector3(inputX, 0, inputZ);
        bool isWalking = inputDir.magnitude > 0.1f;
        animator.SetBool("IsWalking", isWalking);

        if (inputDir.magnitude > 1f)
            inputDir.Normalize();

        // Determina la dirección relativa a la cámara (o local si no se asigna)
        Vector3 moveDirection = Vector3.zero;
        if (cameraTransform != null)
        {
            Vector3 camForward = cameraTransform.forward;
            camForward.y = 0;
            camForward.Normalize();
            Vector3 camRight = cameraTransform.right;
            camRight.y = 0;
            camRight.Normalize();
            moveDirection = camForward * inputDir.z + camRight * inputDir.x;
            if (moveDirection.magnitude > 1f)
                moveDirection.Normalize();
        }
        else
        {
            moveDirection = transform.TransformDirection(inputDir);
        }

        bool isRunning = Input.GetKey(runKey);
        float currentSpeed = isRunning ? runSpeed : walkSpeed;
        animator.SetBool("IsRunning", isRunning);

        // En tierra: se establece la velocidad horizontal según el input.
        // En el aire: se añade una aceleración incremental sin sobrescribir el momentum inicial.
        if (isGrounded)
        {
            Vector3 newVelocity = moveDirection * currentSpeed;
            newVelocity.y = rb.linearVelocity.y; // conserva la componente vertical
            rb.linearVelocity = newVelocity;
        }
        else
        {
            if (moveDirection != Vector3.zero)
            {
                Vector3 airAcceleration = moveDirection * airControlFactor * currentSpeed;
                rb.AddForce(airAcceleration, ForceMode.Acceleration);
            }
        }

        // Rotación suave hacia la dirección de movimiento si hay input.
        if (moveDirection != Vector3.zero)
        {
            Quaternion targetRot = Quaternion.LookRotation(moveDirection);
            rb.rotation = Quaternion.Slerp(rb.rotation, targetRot, 10f * Time.deltaTime);
        }
    }

    // Maneja el salto: se ejecuta sólo cuando se pulsa la tecla y el personaje está en el suelo.
    void HandleJump()
    {
        if (Input.GetKeyDown(jumpKey) && isGrounded)
        {
            StartCoroutine(JumpRoutine());
        }
    }

    IEnumerator JumpRoutine()
    {
        // Se omite modificar el parámetro de salto en el Animator.
        yield return new WaitForSeconds(jumpDelay);
        rb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
    }

    // Actualiza los parámetros del Animator para las animaciones de caída y ascenso.
    // Si el personaje está sobre una rampa, no se activa el estado de caída.
    void UpdateFallingAndAscendingAnimation()
    {
        if (!isGrounded)
        {
            float verticalSpeed = rb.linearVelocity.y;
            if (isOnRamp)
                animator.SetBool("IsFalling", false);
            else
                animator.SetBool("IsFalling", verticalSpeed < 0);
            animator.SetBool("IsAscending", verticalSpeed > 0);
        }
        else
        {
            animator.SetBool("IsFalling", false);
            animator.SetBool("IsAscending", false);
        }
    }

    // Se activa al colisionar o al entrar en trigger con un objeto.
    void OnCollisionEnter(Collision collision)
    {
        // Si el objeto pertenece a la capa de push back, se aplica el empuje.
        if ((pushBackLayer.value & (1 << collision.gameObject.layer)) != 0)
        {
            ApplyPushBack(collision.contacts[0].point);
        }

        // Si el objeto pertenece a la capa de daño, se procesa el daño.
        if (!isDead && !isImmune && ((damageLayer.value & (1 << collision.gameObject.layer)) != 0))
        {
            TakeDamage();
        }
    }

    void OnTriggerEnter(Collider other)
    {
        if ((pushBackLayer.value & (1 << other.gameObject.layer)) != 0)
        {
            // Para triggers, se usa la posición del collider.
            ApplyPushBack(other.ClosestPoint(transform.position));
        }

        if (!isDead && !isImmune && ((damageLayer.value & (1 << other.gameObject.layer)) != 0))
        {
            TakeDamage();
        }
    }

    // Aplica una fuerza de empuje al personaje en dirección contraria al punto de impacto.
    void ApplyPushBack(Vector3 impactPoint)
    {
        Vector3 pushDirection = (transform.position - impactPoint).normalized;
        rb.AddForce(pushDirection * pushBackForce, ForceMode.Impulse);
    }

    // Recibe daño y gestiona las vidas.
    void TakeDamage()
    {
        if (isDead || isImmune)
            return;

        if (currentLives > 1)
        {
            currentLives--;
            Debug.Log("Vidas restantes: " + currentLives);
            StartCoroutine(HurtRoutine());
        }
        else
        {
            currentLives = 0;
            Debug.Log("Vidas restantes: " + currentLives);
            Die();
        }
    }

    // Coroutine para la animación de daño (HURT) e inmunidad.
    // Se espera 0.1 segundos para que el parámetro HURT sea detectable y se descuenta ese tiempo de la inmunidad.
    // Además, se desactivan los controles durante un tiempo diferente según si el daño se recibe en el aire o en tierra.
    IEnumerator HurtRoutine()
    {
        animator.SetBool("HURT", true);
        isImmune = true;
        yield return new WaitForSeconds(0.1f);
        animator.SetBool("HURT", false);
        float remainingImmunity = damageImmunityDuration - 0.1f;

        if (!isGrounded)
        {
            controlsEnabled = false;
            // Espera hasta que se toque el suelo.
            while (!isGrounded)
            {
                yield return null;
            }
            // Una vez en suelo, esperar el tiempo de desactivación de controles en aire.
            yield return new WaitForSeconds(controlDisableDurationAir);
            controlsEnabled = true;
        }
        else
        {
            controlsEnabled = false;
            yield return new WaitForSeconds(controlDisableDurationGround);
            controlsEnabled = true;
        }

        yield return new WaitForSeconds(remainingImmunity);
        isImmune = false;
    }

    // Activa la muerte.
    void Die()
    {
        isDead = true;
        animator.SetBool("DEAD", true);
    }

    // Método público para el respawn (puede ser llamado desde otros scripts).
    public void Respawn()
    {
        animator.SetBool("DEAD", false);
        animator.SetBool("RESPAWN", true);
        isDead = false;
        isImmune = true;
        currentLives = maxLives; // Reinicia las vidas
        Debug.Log("Respawn: Vidas reiniciadas a " + currentLives);
        StartCoroutine(RemoveImmunityAfterDelay());
    }

    // Remueve la inmunidad tras un periodo y desactiva el parámetro RESPAWN.
    IEnumerator RemoveImmunityAfterDelay()
    {
        yield return new WaitForSeconds(damageImmunityDuration);
        isImmune = false;
        animator.SetBool("RESPAWN", false);
    }
}
