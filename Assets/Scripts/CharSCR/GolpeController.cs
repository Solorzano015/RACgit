using UnityEngine;

public class GolpeController : MonoBehaviour
{
    private Animator animator;

    [Header("Parámetros del Animator")]
    [SerializeField] private string golpeIzquierdoParam = "GolpeIzquierdo";
    [SerializeField] private string golpeDerechoParam = "GolpeDerecho";

    void Start()
    {
        animator = GetComponent<Animator>();
        if (animator == null)
        {
            Debug.LogError("No se encontró un componente Animator en el objeto.");
        }
    }

    void Update()
    {
        DetectarGolpe();
    }

    private void DetectarGolpe()
    {
        // Verificar si se presiona el clic izquierdo
        if (Input.GetMouseButtonDown(0))
        {
            ActivarGolpe(golpeIzquierdoParam, golpeDerechoParam);
        }
        // Verificar si se presiona el clic derecho
        else if (Input.GetMouseButtonDown(1))
        {
            ActivarGolpe(golpeDerechoParam, golpeIzquierdoParam);
        }
    }

    private void ActivarGolpe(string golpeActivado, string golpeDesactivado)
    {
        // Desactivar el golpe contrario y activar el seleccionado
        animator.SetBool(golpeDesactivado, false);
        animator.SetBool(golpeActivado, true);
    }
}
