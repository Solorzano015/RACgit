using UnityEngine;
using System.Collections;
using UnityEngine.InputSystem;

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

    public bool botonA;
    public bool botonX;
    public bool LTrigger;
    public bool RTrigger;
    public bool botonStart;
    public bool LBumper;
    public bool RBumper;

    public bool walkPressed;
    public bool jumpPressed;
    public bool dashPressed;
    public bool Lpunch;
    public bool Rpunch;

    [Header("Referencias")]
    public Transform cameraTransform;
    public Animator animator;
    public GroundChecker groundChecker;  // Referencia al componente GroundChecker
    public Animator punchAnim;

    [Header("Estado de Movimiento")]
    public bool movementEnabled = true;

    private Rigidbody rb;

    public Vector2 controlAxis;

    public void OnMove(InputValue value)
    {

        controlAxis = value.Get<Vector2>();

    }




    void Start()
    {
        rb = GetComponent<Rigidbody>();
        if (animator == null)
            animator = GetComponent<Animator>();

        if (groundChecker == null)
        {
            Debug.LogError("No se asignó el GroundChecker en " + gameObject.name);
        }

        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;
        walkPressed = true;
        jumpPressed = false;
        dashPressed = false;
    }

    void Update()
    {
        Debug.Log("jumpRESSED" + jumpPressed);
        // Actualiza la animación de "IsGrounded" según el GroundChecker.
        animator.SetBool("IsGrounded", groundChecker.IsGrounded);
        animator.SetFloat("Y", rb.linearVelocity.y);
        if (movementEnabled)
        {
            HandleMovement();
            HandleJump();
            HandlePunch();
        }

        UpdateFallingAndAscendingAnimation();

        if (movementEnabled && dashPressed && canDash && groundChecker.IsGrounded)
        {
            StartCoroutine(DashRoutine());
        }

        if (groundChecker.IsGrounded && rb.linearVelocity.y <= 0)
        {
            hasJumped = false;
        }

        if (Keyboard.current.shiftKey.isPressed)
        {
            walkPressed = false;
        }

        if (Keyboard.current.shiftKey.wasReleasedThisFrame)
        {

            walkPressed = true;

        }

        if (Gamepad.current.leftShoulder.isPressed)
        {
            walkPressed = false;
        }
        if (Gamepad.current.leftShoulder.wasReleasedThisFrame)
        {
            walkPressed = true;
        }


        if (Keyboard.current.ctrlKey.isPressed)
        {
            dashPressed = true;
        }
        if (Keyboard.current.ctrlKey.wasReleasedThisFrame)
        {
            dashPressed = false;
        }
        if (Gamepad.current.rightShoulder.isPressed)
        {
            dashPressed = true;
        }
        if (Gamepad.current.rightShoulder.wasReleasedThisFrame)
        {
            dashPressed = false;
        }

        if (Gamepad.current.leftTrigger.isPressed)
        {
            Lpunch = true;
        }
        if (Gamepad.current.leftTrigger.wasReleasedThisFrame)
        {
            Lpunch = false;
        }

        if (Gamepad.current.rightTrigger.isPressed)
        {
            Rpunch = true;
        }
        if (Gamepad.current.rightTrigger.wasReleasedThisFrame)
        {
            Rpunch = false;
        }
    }

    void HandlePunch()
    {

        if (Lpunch == true)
        {

            punchAnim.SetBool("GolpeIzquierdo", true);

        }

        if (Lpunch == false)
        {

            punchAnim.SetBool("GolpeIzquierdo", false);

        }

        if (Rpunch == true)
        {

            punchAnim.SetBool("GolpeDerecho", true);

        }
        if (Rpunch == false)
        {

            punchAnim.SetBool("GolpeDerecho", false);

        }

    }
    void HandleMovement()
    {
        // Leer input
        float inputX;
        float inputZ;

        if (controlAxis != Vector2.zero)
        {

            // Leer input
            inputX = controlAxis.x;
            inputZ = controlAxis.y;

        }
        else
        {

            // Leer input
            inputX = (Input.GetKey(leftKey) ? -1f : 0f) + (Input.GetKey(rightKey) ? 1f : 0f);
            inputZ = (Input.GetKey(forwardKey) ? 1f : 0f) + (Input.GetKey(backwardKey) ? -1f : 0f);

        }


        //// Leer input
        //inputX = (Input.GetKey(leftKey) ? -1f : 0f) + (Input.GetKey(rightKey) ? 1f : 0f);
        //inputZ = (Input.GetKey(forwardKey) ? 1f : 0f) + (Input.GetKey(backwardKey) ? -1f : 0f);

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


        bool isRunning = walkPressed;
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
        
            if (Keyboard.current.spaceKey.wasPressedThisFrame)
            {
                jumpPressed = true;
            }

            if (Keyboard.current.spaceKey.wasReleasedThisFrame)
            {
                jumpPressed = false;
            }

            if (Gamepad.current.buttonSouth.wasPressedThisFrame)
            {
                jumpPressed = true;
            }
            if (Gamepad.current.buttonSouth.wasReleasedThisFrame)
            {
                jumpPressed = false;
            }


        if (hasJumped == false && jumpPressed==false)
        {
            if (jumpPressed = true && groundChecker.IsGrounded)
            {
                StartCoroutine(JumpRoutine());
                isDoubleJumping = false; // Reiniciamos el estado del doble saltos
                animator.SetBool("IsDoubleJumping", false); // Aseguramos que esté en false al iniciar un salto nuevo
            }

            HandleDoubleJump();
        }

    }


    IEnumerator JumpRoutine()
    {
        yield return new WaitForSeconds(jumpDelay);
        rb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
        hasJumped = true; // Marcamos que el salto se ha hecho
    }


    void HandleDoubleJump()
    {
        if (hasJumped == true && isDoubleJumping==false && jumpPressed == true)
        {
            StartCoroutine(DoubleJumpRoutine());
        }
    }


    IEnumerator DoubleJumpRoutine()
    {
        isDoubleJumping = true;  // Activamos el flag
        animator.SetBool("IsDoubleJumping", true);

        yield return new WaitForSeconds(doubleJumpDelay); // Espera antes de ejecutar el salto
        rb.AddForce(Vector3.up * doubleJumpForce, ForceMode.Impulse);

        // Desactivamos el parámetro en el Animator cuando finalice
        yield return new WaitForSeconds(0.1f); // Pequeño tiempo para que la animación procese el salto
        animator.SetBool("IsDoubleJumping", false);
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
        
        if (dashPressed == true && canDash == true)
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

}






