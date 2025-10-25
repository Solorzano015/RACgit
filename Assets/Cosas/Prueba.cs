using UnityEngine;
using UnityEngine.InputSystem;

public class Prueba : MonoBehaviour
{
    public Vector2 controlAxis;
    public bool botonA;


    public void OnMove(InputValue value) 
    {

        controlAxis = value.Get<Vector2>();
    

    }

    public void OnButtonRegular()
    {

     

    }


    public void Update()
    {
        if (Gamepad.current.buttonSouth.isPressed)
        {

            Debug.Log("botón oprimido");
            botonA = true;

        }

        if (Gamepad.current.buttonSouth.wasReleasedThisFrame)
        {

            Debug.Log("botón suelto");
            botonA = false;

        }
    }

}
