using UnityEngine;
using System.Collections.Generic;

public class ParticleRateController : MonoBehaviour
{
    [SerializeField] private ParticleSystem particleSystem;
    [SerializeField] private Animator animator;
    [SerializeField] private List<string> animatorParameters = new(); // Lista de parámetros
    [SerializeField] private float minRate = 10f;
    [SerializeField] private float maxRate = 100f;
    [SerializeField] private bool invertParameter = false;

    private ParticleSystem.EmissionModule emissionModule;

    void Start()
    {
        if (particleSystem == null) particleSystem = GetComponent<ParticleSystem>();
        if (animator == null) animator = GetComponent<Animator>();

        emissionModule = particleSystem.emission;
    }

    void Update()
    {
        float maxParamValue = 0f;

        foreach (string param in animatorParameters)
        {
            if (animator.HasParameter(param))
            {
                float value = animator.GetFloat(param);
                if (value > maxParamValue)
                    maxParamValue = value;
            }
        }

        if (invertParameter)
            maxParamValue = 1f - maxParamValue;

        float newRate = Mathf.Lerp(minRate, maxRate, maxParamValue);
        emissionModule.rateOverTime = newRate;
    }
}

// Extensión útil para validar si el parámetro existe en el Animator
public static class AnimatorExtensions
{
    public static bool HasParameter(this Animator animator, string paramName)
    {
        foreach (AnimatorControllerParameter param in animator.parameters)
        {
            if (param.name == paramName && param.type == AnimatorControllerParameterType.Float)
                return true;
        }
        return false;
    }
}
