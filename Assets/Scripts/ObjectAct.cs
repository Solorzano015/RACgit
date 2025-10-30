using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class ParameterRequirement
{
    [Tooltip("Nombre del parámetro en el Animator.")]
    public string parameterName;
    [Tooltip("Valor esperado del parámetro (true/false).")]
    public bool expectedValue = true;
}

[System.Serializable]
public class ActivationElement
{
    [Tooltip("GameObject a activar o desactivar.")]
    public GameObject targetObject;
    [Tooltip("Parámetro principal del Animator que se evaluará.")]
    public string mainParameter;
    [Tooltip("Valor esperado del parámetro principal.")]
    public bool mainParameterExpectedValue = true;
    [Tooltip("Lista de requisitos adicionales (otros parámetros) que deben cumplirse.")]
    public List<ParameterRequirement> additionalRequirements = new List<ParameterRequirement>();
}

public class ObjectAct : MonoBehaviour
{
    [Header("Elementos a activar/desactivar según parámetros del Animator")]
    [Tooltip("Lista de elementos con su GameObject y sus requisitos para activar o desactivar.")]
    public List<ActivationElement> activationElements = new List<ActivationElement>();

    private Animator animator;

    private void Awake()
    {
        animator = GetComponent<Animator>();
        if (animator == null)
        {
            Debug.LogError("No se encontró un Animator en el GameObject. Por favor, agrega uno.");
        }
    }

    private void Update()
    {
        if (animator == null) return;

        foreach (ActivationElement element in activationElements)
        {
            if (element.targetObject == null)
                continue;

            // Verifica el parámetro principal
            bool activate = animator.GetBool(element.mainParameter) == element.mainParameterExpectedValue;

            // Verifica los requisitos adicionales (si existen)
            if (activate && element.additionalRequirements != null)
            {
                foreach (ParameterRequirement req in element.additionalRequirements)
                {
                    if (animator.GetBool(req.parameterName) != req.expectedValue)
                    {
                        activate = false;
                        break;
                    }
                }
            }

            // Activa o desactiva el GameObject según el resultado
            element.targetObject.SetActive(activate);
        }
    }
}
