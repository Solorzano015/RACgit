using UnityEngine;
using System.Collections;

public class NPCCabezaAtencion : MonoBehaviour
{
    [Header("Configuración de visión")]
    public Transform player;
    public float visionDistance = 5f;
    [Range(0f, 180f)] public float visionAngle = 60f;
    public Vector3 visionDirection = Vector3.forward;

    [Header("Rotación limitada")]
    [Range(0f, 90f)] public float maxYaw = 45f;
    [Range(0f, 90f)] public float maxPitch = 30f;
    public float rotacionVelocidad = 5f;

    [Header("Comportamiento")]
    [Range(0f, 1f)] public float mirarProbabilidad = 0.5f;
    public float tiempoMinReset = 0.5f;
    public float tiempoMaxReset = 2f;

    private Animator animator;
    private bool isLooking = false;
    private bool jugadorEnVision = false;
    private bool rotacionEnProgreso = false;
    private Coroutine rotacionCoroutine;
    private Quaternion initialLocalRotation;

    void Start()
    {
        animator = GetComponent<Animator>();
        initialLocalRotation = transform.localRotation;
    }

    void Update()
    {
        Vector3 toPlayerWorld = player.position - transform.position;
        float distance = toPlayerWorld.magnitude;

        Vector3 visionWorld = transform.TransformDirection(visionDirection.normalized);
        float angleToPlayer = Vector3.Angle(visionWorld, toPlayerWorld.normalized);

        bool estaEnVision = distance <= visionDistance && angleToPlayer <= visionAngle * 0.5f;

        // El jugador acaba de entrar
        if (estaEnVision && !jugadorEnVision)
        {
            Debug.Log("Jugador entró al rango de visión.");
            jugadorEnVision = true;

            // Cancelamos la corrutina si estaba en curso
            if (rotacionCoroutine != null)
            {
                StopCoroutine(rotacionCoroutine);
                rotacionCoroutine = null;
                rotacionEnProgreso = false;
                Debug.Log("Se canceló la rotación de regreso por reingreso.");
            }

            // Evaluamos si debe mirar
            if (!isLooking && Random.value < mirarProbabilidad)
            {
                isLooking = true;
                animator.SetBool("ATTENTION", true);
                Debug.Log("NPC decidió mirar al jugador.");
            }
        }

        // El jugador salió del cono de visión
        if (!estaEnVision && jugadorEnVision)
        {
            jugadorEnVision = false;
            Debug.Log("Jugador salió del rango de visión. Iniciando regreso.");
            if (rotacionCoroutine == null)
            {
                rotacionCoroutine = StartCoroutine(RetornarDespuesDeTiempo());
            }
        }

        // Si está mirando, seguimos rotando la cabeza hacia el jugador
        if (isLooking && jugadorEnVision)
        {
            Quaternion lookRot = Quaternion.LookRotation(toPlayerWorld.normalized, Vector3.up);
            Quaternion targetLocalRot = Quaternion.Inverse(transform.parent.rotation) * lookRot;

            Vector3 euler = NormalizeAngles(targetLocalRot.eulerAngles);
            float limitedPitch = Mathf.Clamp(euler.x, -maxPitch, maxPitch);
            float limitedYaw = Mathf.Clamp(euler.y, -maxYaw, maxYaw);

            Quaternion limitedRotation = Quaternion.Euler(limitedPitch, limitedYaw, 0);
            transform.localRotation = Quaternion.Slerp(transform.localRotation, limitedRotation, Time.deltaTime * rotacionVelocidad);
        }
    }

    IEnumerator RetornarDespuesDeTiempo()
    {
        rotacionEnProgreso = true;
        animator.SetBool("ATTENTION", false);
        isLooking = false;

        float delay = Random.Range(tiempoMinReset, tiempoMaxReset);
        Debug.Log($"Esperando {delay:F2}s antes de volver a la posición original.");
        yield return new WaitForSeconds(delay);

        Debug.Log("Iniciando rotación de regreso.");
        while (Quaternion.Angle(transform.localRotation, initialLocalRotation) > 0.1f)
        {
            transform.localRotation = Quaternion.Slerp(transform.localRotation, initialLocalRotation, Time.deltaTime * rotacionVelocidad);
            yield return null;
        }

        transform.localRotation = initialLocalRotation;
        Debug.Log("Regresó a la posición original.");
        rotacionCoroutine = null;
        rotacionEnProgreso = false;
    }

    private Vector3 NormalizeAngles(Vector3 angles)
    {
        angles.x = NormalizeAngle(angles.x);
        angles.y = NormalizeAngle(angles.y);
        angles.z = NormalizeAngle(angles.z);
        return angles;
    }

    private float NormalizeAngle(float angle)
    {
        if (angle > 180f) angle -= 360f;
        return angle;
    }

    void OnDrawGizmosSelected()
    {
        if (player == null) return;
        Gizmos.color = Color.cyan;
        Vector3 origin = transform.position;
        Vector3 forward = transform.TransformDirection(visionDirection.normalized);
        Quaternion left = Quaternion.AngleAxis(-visionAngle / 2f, Vector3.up);
        Quaternion right = Quaternion.AngleAxis(visionAngle / 2f, Vector3.up);

        Gizmos.DrawLine(origin, origin + left * forward * visionDistance);
        Gizmos.DrawLine(origin, origin + right * forward * visionDistance);
    }
}
