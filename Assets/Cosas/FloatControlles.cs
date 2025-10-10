using UnityEngine;
using UnityEngine.InputSystem;

public class FloatController : MonoBehaviour
{
    public PlayerInputActions inputActions;
    public float inputValue; // Aquí verás el número en el Inspector

    private void Awake()
    {
        // Crear instancia del InputActions generado
        inputActions = new PlayerInputActions();
    }
    private void Update()
    {
        // Leer el valor del eje (float entre -1 y 1)
        inputValue = inputActions.Player.Move.ReadValue<float>();

        // Solo para prueba
        Debug.Log("Valor del eje: " + inputValue);
    }
}
