using UnityEngine;
using System.Collections;

[RequireComponent(typeof(Rigidbody))]
public class CharacterMovement : MonoBehaviour
{
    [Header("Movimiento")]
    
    public float walkSpeed = 5f;
    public float runSpeed = 8f;
    public float jumpForce = 5f;
    public float airControlFactor = 0.2f;

    [Header("Dash")]
    public float dashForce = 10f;
    public float dashCooldown = 2f;
    public float dashDelay = 0.1f;
    public KeyCode dashKey = KeyCode.LeftControl;
    private bool canDash = true;
    public float dashDisableMovementTime = 0.3f; // Tiempo durante el cual el movimiento se desactiva


    [Header("Doble Salto")]
    public float doubleJumpForce = 5f;
    public float doubleJumpDelay = 0f; // Retraso antes de ejecutar el doble salto

    private bool canDoubleJump = false; // Control del doble salto
    private bool hasJumped = false; // Marca si el primer salto fue ejecutado

    private bool isDoubleJumping = false;

    [Tooltip("Tiempo de retardo para sincronizar el salto con la animación.")]
    public float jumpDelay = 0.2f;
    [Tooltip("Factor de control en el aire (0: sin influencia, 1: igual que en tierra).")]
    
    
    [Header("Limitador de Velocidad")]
    public float maxSpeed = 10f; // Velocidad máxima permitida

    [Header("Controles")]
    public KeyCode forwardKey = KeyCode.W;
    public KeyCode backwardKey = KeyCode.S;
    public KeyCode leftKey = KeyCode.A;
    public KeyCode rightKey = KeyCode.D;
    public KeyCode jumpKey = KeyCode.Space;
    public KeyCode runKey = KeyCode.LeftShift;

    [Header("Referencias")]
    public Transform cameraTransform;
    public Animator animator;
    public GroundChecker groundChecker;  // Referencia al componente GroundChecker

    [Header("Estado de Movimiento")]
    public bool movementEnabled = true;

    private Rigidbody rb;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        if(animator == null)
            animator = GetComponent<Animator>();

        if (groundChecker == null)
        {
            Debug.LogError("No se asignó el GroundChecker en " + gameObject.name);
        }
    }

    void Update()
    {
        // Actualiza la animación de "IsGrounded" según el GroundChecker.
        animator.SetBool("IsGrounded", groundChecker.IsGrounded);
        animator.SetFloat("Y", rb.linearVelocity.y);
        if (movementEnabled)
        {
            HandleMovement();
            HandleJump();
        }
        
        UpdateFallingAndAscendingAnimation();

        if (movementEnabled && Input.GetKeyDown(dashKey) && canDash && groundChecker.IsGrounded)
        {
            StartCoroutine(DashRoutine());
        }

        if (groundChecker.IsGrounded && rb.linearVelocity.y <= 0)
        {
            hasJumped = false;
            canDoubleJump = false;
        }


    }

    void HandleMovement()
{
    // Leer input
    float inputX = (Input.GetKey(leftKey) ? -1f : 0f) + (Input.GetKey(rightKey) ? 1f : 0f);
    float inputZ = (Input.GetKey(forwardKey) ? 1f : 0f) + (Input.GetKey(backwardKey) ? -1f : 0f);
    Vector3 inputDir = new Vector3(inputX, 0, inputZ);
    bool isWalking = inputDir.magnitude > 0.1f;
    animator.SetBool("IsWalking", isWalking);

    if (inputDir.magnitude > 1f)
        inputDir.Normalize();

    // Calcular dirección en función de la cámara (o local)
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

    // Invertimos la lógica: ahora se camina al presionar Shift, y se corre por defecto
    bool isRunning = !Input.GetKey(runKey);
    float currentSpeed = isRunning ? runSpeed : walkSpeed;
    animator.SetBool("IsRunning", isRunning);

    if (groundChecker.IsGrounded)
    {
        Vector3 newVelocity = moveDirection * currentSpeed;
        newVelocity.y = rb.linearVelocity.y;
        rb.linearVelocity = newVelocity;
    }
    else if (moveDirection != Vector3.zero)
    {
        Vector3 airAcceleration = moveDirection * airControlFactor * currentSpeed;
        rb.AddForce(airAcceleration, ForceMode.Acceleration);
    }

    Vector3 horizontalVelocity = new Vector3(rb.linearVelocity.x, 0, rb.linearVelocity.z);
    if (horizontalVelocity.magnitude > maxSpeed)
    {
        Vector3 clampedVelocity = horizontalVelocity.normalized * maxSpeed;
        rb.linearVelocity = new Vector3(clampedVelocity.x, rb.linearVelocity.y, clampedVelocity.z);
    }

    if (moveDirection != Vector3.zero)
    {
        Quaternion targetRot = Quaternion.LookRotation(moveDirection);
        rb.rotation = Quaternion.Slerp(rb.rotation, targetRot, 10f * Time.deltaTime);
    }
}


    void HandleJump()
    {
        if (Input.GetKeyDown(jumpKey) && groundChecker.IsGrounded)
        {
            StartCoroutine(JumpRoutine());
            isDoubleJumping = false; // Reiniciamos el estado del doble salto
            animator.SetBool("IsDoubleJumping", false); // Aseguramos que esté en false al iniciar un salto nuevo
        }
        
        HandleDoubleJump(); 
    }

    void HandleDoubleJump()
    {
        if (canDoubleJump && hasJumped && (Input.GetMouseButtonDown(0) ^ Input.GetMouseButtonDown(1)))
        {
            StartCoroutine(DoubleJumpRoutine());
        }
    }


    IEnumerator DoubleJumpRoutine()
    {
        canDoubleJump = false;   // Deshabilitamos el doble salto para evitar múltiples activaciones
        isDoubleJumping = true;  // Activamos el flag
        animator.SetBool("IsDoubleJumping", true);

        yield return new WaitForSeconds(doubleJumpDelay); // Espera antes de ejecutar el salto
        rb.AddForce(Vector3.up * doubleJumpForce, ForceMode.Impulse);

        // Desactivamos el parámetro en el Animator cuando finalice
        yield return new WaitForSeconds(0.1f); // Pequeño tiempo para que la animación procese el salto
        animator.SetBool("IsDoubleJumping", false);
    }

    IEnumerator JumpRoutine()
    {
        yield return new WaitForSeconds(jumpDelay);
        rb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
        hasJumped = true; // Marcamos que el salto se ha hecho
        canDoubleJump = true; 
    }


    void UpdateFallingAndAscendingAnimation()
    {
        float verticalSpeed = rb.linearVelocity.y;
        float sensitivityThreshold = 0.1f; // Sensibilidad ajustada para detectar pequeños cambios
        
        if (!groundChecker.IsGrounded)
        {
            animator.SetBool("IsFalling", verticalSpeed < -sensitivityThreshold);
            animator.SetBool("IsAscending", verticalSpeed > sensitivityThreshold);
        }
        else
        {
            animator.SetBool("IsFalling", false);
            animator.SetBool("IsAscending", false);
        }
    }

    IEnumerator DashRoutine()
    {
        canDash = false; 
        movementEnabled = false; // Desactivar el movimiento
        animator.SetBool("IsDashing", true);

        yield return new WaitForSeconds(dashDelay);

        // Determinamos la dirección opuesta al movimiento actual
        Vector3 dashDirection = transform.forward;
        rb.AddForce(dashDirection * dashForce, ForceMode.Impulse);

        yield return new WaitForSeconds(dashDisableMovementTime); // Espera para reactivar el movimiento
        movementEnabled = true;
        
        yield return new WaitForSeconds(0.1f); // Pequeño tiempo para la animación
        animator.SetBool("IsDashing", false);

        yield return new WaitForSeconds(dashCooldown); // Espera para volver a habilitar el Dash
        canDash = true;
    }

}
