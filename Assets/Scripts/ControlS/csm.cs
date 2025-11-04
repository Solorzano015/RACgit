using Cinemachine;
using UnityEngine;
using UnityEngine.InputSystem;

public class CustomCinemachineInput : MonoBehaviour, AxisState.IInputAxisProvider
{
    public InputActionReference lookAction;
    public float mouseSensitivity = 0.5f; // <= Ajusta aquí
    public float gamepadSensitivity = 1f;

    public int NumAxes => 2;

    public float GetAxisValue(int axis)
    {
        if (lookAction == null) return 0f;

        Vector2 input = lookAction.action.ReadValue<Vector2>();

        // Detecta si viene del mouse o del mando
        var device = lookAction.action.activeControl?.device;

        if (device is Mouse)
            input *= mouseSensitivity;
        else
            input *= gamepadSensitivity;

        return axis == 0 ? input.x : input.y;
    }
}
