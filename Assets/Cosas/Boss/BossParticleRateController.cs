using UnityEngine;

public class BossParticleRateController : MonoBehaviour
{
    [Header("Configuración de Partículas")]
    [Tooltip("Arrastra aquí el sistema de partículas que deseas controlar.")]
    public ParticleSystem targetParticleSystem;

    [Header("Configuración del Animator")]
    [Tooltip("Arrastra aquí el GameObject que contiene el componente Animator.")]
    public Animator targetAnimator;
    [Tooltip("El nombre del parámetro booleano en el Animator que controlará el Rate over Time.")]
    public string booleanParameterName = "IsActive"; // Puedes cambiar este nombre en el Inspector

    [Header("Valores de Rate over Time")]
    [Tooltip("El Rate over Time cuando el parámetro booleano está activo.")]
    public float activeRateOverTime = 10f;
    [Tooltip("El Rate over Time cuando el parámetro booleano está inactivo (generalmente 0).")]
    public float inactiveRateOverTime = 0f;

    private void Update()
    {
        // Asegurarse de que las referencias no sean nulas
        if (targetParticleSystem == null)
        {
            Debug.LogError("Error: El sistema de partículas objetivo no ha sido asignado en el Inspector.");
            return;
        }

        if (targetAnimator == null)
        {
            Debug.LogError("Error: El Animator objetivo no ha sido asignado en el Inspector.");
            return;
        }

        // Obtener el módulo de emisión del sistema de partículas para modificarlo
        var emission = targetParticleSystem.emission;

        // Verificar el estado del parámetro booleano en el Animator
        if (targetAnimator.GetBool(booleanParameterName))
        {
            // Si el booleano está activo, establecer el Rate over Time al valor deseado
            emission.rateOverTime = activeRateOverTime;
        }
        else
        {
            // Si el booleano está inactivo, establecer el Rate over Time a 0
            emission.rateOverTime = inactiveRateOverTime;
        }
    }
}