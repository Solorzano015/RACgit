using UnityEngine;
using System.Collections;

[RequireComponent(typeof(Rigidbody))]
public class CharacterDamage : MonoBehaviour
{
    [Header("Estado de Daño y Respawn")]
    public LayerMask damageLayer;
    [Tooltip("Tiempo de inmunidad tras recibir daño o tras el respawn (segundos).")]
    public float damageImmunityDuration = 3f;
    public bool respawnRequested = false;

    [Header("Vidas")]
    public int maxLives = 3;
    [Tooltip("Vidas restantes (se mostrará en el Inspector).")]
    public int currentLives;
    public int CurrentLives { get { return currentLives; } }

    [Header("Push Back")]
    public LayerMask pushBackLayer;
    public float pushBackForce = 10f;

    [Header("Desactivación de Controles")]
    [Tooltip("Tiempo de desactivación de controles si el daño se recibe en el aire.")]
    public float controlDisableDurationAir = 0.2f;
    [Tooltip("Tiempo de desactivación de controles si el daño se recibe en tierra.")]
    public float controlDisableDurationGround = 0.3f;
    private bool controlsEnabled = true;
    private CharacterMovement characterMovement;

    [Header("Estado de Vida")]
    [SerializeField] public bool ALIVE = true;
    [SerializeField] public bool DEAD = false;

    private float stateChangeCooldown = 0.5f;
    private float lastStateChangeTime = 0f;

    [Header("Animator")]
    public Animator animator;

    private Rigidbody rb;
    private bool isDead = false;
    private bool isImmune = false;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        characterMovement = GetComponent<CharacterMovement>();
        if (animator == null)
        {
            animator = GetComponent<Animator>();
        }
        currentLives = maxLives;

        // Initialize ALIVE and DEAD based on currentLives
        ALIVE = currentLives > 0;
        DEAD = !ALIVE;

        Debug.Log("Vidas iniciales: " + currentLives);
    }

    void Update()
    {
        UpdateLifeState();

        if (respawnRequested)
        {
            respawnRequested = false;
            Respawn();
        }
    }

    void OnCollisionEnter(Collision collision)
    {
        // Handle non-damage push-back first
        if ((pushBackLayer.value & (1 << collision.gameObject.layer)) != 0)
        {
            ApplyPushBack(collision.contacts[0].point);
        }

        // Handle damage with push-back
        if (!isDead && !isImmune && ((damageLayer.value & (1 << collision.gameObject.layer)) != 0))
        {
            TakeDamage(collision.contacts[0].point);
        }
    }

    void OnTriggerEnter(Collider other)
    {
        // Handle non-damage push-back first
        if ((pushBackLayer.value & (1 << other.gameObject.layer)) != 0)
        {
            ApplyPushBack(other.ClosestPoint(transform.position));
        }

        // Handle damage with push-back
        if (!isDead && !isImmune && ((damageLayer.value & (1 << other.gameObject.layer)) != 0))
        {
            TakeDamage(other.ClosestPoint(transform.position));
        }
    }

    void ApplyPushBack(Vector3 impactPoint)
    {
        Vector3 pushDirection = (transform.position - impactPoint).normalized;
        rb.AddForce(pushDirection * pushBackForce, ForceMode.Impulse);
    }

    void TakeDamage(Vector3 impactPoint)
    {
        if (isDead || isImmune)
            return;

        // 1. Check grounded state BEFORE push-back
        bool wasGrounded = animator.GetBool("IsGrounded");

        // 2. Apply push-back
        ApplyPushBack(impactPoint);

        // 3. Proceed with damage handling using original grounded state
        if (currentLives > 1)
        {
            currentLives--;
            Debug.Log("Vidas restantes: " + currentLives);
            UpdateLifeState();
            StartCoroutine(HurtRoutine(wasGrounded));
        }
        else
        {
            currentLives = 0;
            Debug.Log("Vidas restantes: " + currentLives);
            UpdateLifeState();
            Die();
        }
    }

    IEnumerator HurtRoutine(bool wasGrounded)
    {
        animator.SetBool("HURT", true);
        isImmune = true;
        DisableControls();
        yield return new WaitForSeconds(0.1f);
        animator.SetBool("HURT", false);
        float remainingImmunity = damageImmunityDuration - 0.1f;

        // Use original grounded state
        float controlDisableDuration = wasGrounded ? controlDisableDurationGround : controlDisableDurationAir;
        float elapsedTime = 0f;

        while (elapsedTime < controlDisableDuration)
        {
            //Debug.Log("Tiempo restante para recuperar control: " + 
            //         (controlDisableDuration - elapsedTime).ToString("F2") + " segundos");
            yield return new WaitForSeconds(0.1f);
            elapsedTime += 0.1f;
        }

        EnableControls();
        yield return new WaitForSeconds(remainingImmunity);
        isImmune = false;
    }

    void Die()
    {
        isDead = true;
        animator.SetBool("DEAD", true);
        DisableControls();
    }

    public void Respawn()
    {
        Debug.Log("Revivir activado");
        currentLives = maxLives;
        Debug.Log("Vidas restauradas a: " + currentLives);
        UpdateLifeState();
        animator.SetBool("DEAD", false);
        animator.SetBool("RESPAWN", true);
        isDead = false;
        isImmune = true;
        EnableControls();
        StartCoroutine(RemoveImmunityAfterDelay());
    }

    IEnumerator RemoveImmunityAfterDelay()
    {
        yield return new WaitForSeconds(damageImmunityDuration);
        isImmune = false;
        animator.SetBool("RESPAWN", false);
    }

    void UpdateLifeState()
    {
        if (Time.time - lastStateChangeTime < stateChangeCooldown)
            return;

        bool wasAlive = ALIVE;
        bool wasDead = DEAD;

        ALIVE = currentLives > 0;
        DEAD = !ALIVE;

        if (ALIVE != wasAlive || DEAD != wasDead)
        {
            lastStateChangeTime = Time.time;
            if (DEAD)
            {
                DisableControls();
            }
            else
            {
                EnableControls();
            }
        }
        //Debug.Log($"ALIVE: {ALIVE}, DEAD: {DEAD}");
    }

    void DisableControls()
    {
        controlsEnabled = false;
        if (characterMovement != null)
        {
            characterMovement.movementEnabled = false;
        }
    }

    void EnableControls()
    {
        controlsEnabled = true;
        if (characterMovement != null)
        {
            characterMovement.movementEnabled = true;
        }
    }
}