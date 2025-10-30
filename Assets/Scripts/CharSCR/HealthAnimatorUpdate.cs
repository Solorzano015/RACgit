using UnityEngine;

public class HealthAnimatorUpdater : MonoBehaviour
{
    public Animator animator;
    public CharacterDamage characterDamage; // Referencia al script CharacterDamage
    private int lastHealthPercentage; // Último valor enviado al Animator

    void Start()
    {
        if (animator == null) animator = GetComponent<Animator>();
        if (characterDamage == null) characterDamage = GetComponent<CharacterDamage>();

        if (characterDamage != null)
        {
            UpdateAnimatorHealth();
        }
        else
        {
            Debug.LogError("CharacterDamage no asignado en " + gameObject.name);
        }
    }

    void Update()
    {
        if (characterDamage != null)
        {
            int newHealthPercentage = CalculateHealthPercentage();

            // Solo actualiza si el porcentaje ha cambiado
            if (newHealthPercentage != lastHealthPercentage)
            {
                lastHealthPercentage = newHealthPercentage;
                animator.SetInteger("HealthPercentage", lastHealthPercentage);
            }
        }
    }

    // Calcula el porcentaje de vida y lo devuelve como un entero sin decimales
    private int CalculateHealthPercentage()
    {
        if (characterDamage.maxLives <= 0) return 0; // Evita divisiones por 0

        float percentage = ((float)characterDamage.currentLives / characterDamage.maxLives) * 100;
        return Mathf.RoundToInt(percentage); // Redondea al entero más cercano
    }

    // Llama a esta función en Start() para inicializar la salud en el Animator
    private void UpdateAnimatorHealth()
    {
        lastHealthPercentage = CalculateHealthPercentage();
        animator.SetInteger("HealthPercentage", lastHealthPercentage);
    }
}
