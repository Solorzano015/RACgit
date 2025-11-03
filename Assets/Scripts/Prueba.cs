using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;

public class Prueba : MonoBehaviour
{
    public Vector2 controlMove;
    public Vector2 controlCam;
    public bool botonA;
    public bool botonX;
    public bool LTrigger;
    public bool RTrigger;
    public bool botonStart;
    public bool LBumper;
    public bool RBumper;



    public void OnMove(InputValue value) 
    {

        controlMove = value.Get<Vector2>();

    }

    public void OnLook(InputValue value)
    {

        controlCam = value.Get<Vector2>();

    }

    public void OnButtonRegular()
    {

    }


    public void Update()
    {
        if (Gamepad.current.buttonSouth.isPressed)
        {
            botonA = true;
        }
        if (Gamepad.current.buttonSouth.wasReleasedThisFrame)
        {
            botonA = false;
        }

        if (Gamepad.current.buttonWest.isPressed)
        {
            botonX = true;
        }
        if (Gamepad.current.buttonWest.wasReleasedThisFrame)
        {
            botonX = false;
        }

        if (Gamepad.current.leftTrigger.isPressed)
        {
            LTrigger = true;
        }
        if (Gamepad.current.leftTrigger.wasReleasedThisFrame)
        {
            LTrigger = false;
        }

        if (Gamepad.current.rightTrigger.isPressed)
        {
            RTrigger = true;
        }
        if (Gamepad.current.rightTrigger.wasReleasedThisFrame)
        {
            RTrigger = false;
        }

        if (Gamepad.current.startButton.isPressed)
        {
            botonStart = true;
        }
        if (Gamepad.current.startButton.wasReleasedThisFrame)
        {
            botonStart = false;
        }


        if (Gamepad.current.leftShoulder.isPressed)
        {
            LBumper = true;
        }
        if (Gamepad.current.leftShoulder.wasReleasedThisFrame)
        {
            LBumper = false;
        }

        if (Gamepad.current.rightShoulder.isPressed)
        {
            RBumper = true;
        }
        if (Gamepad.current.rightShoulder.wasReleasedThisFrame)
        {
            RBumper = false;
        }
    }

}
