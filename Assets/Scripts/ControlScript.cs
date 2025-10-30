using UnityEngine;
using UnityEngine.InputSystem;

public class ControlScript : MonoBehaviour
{
    public Vector2 controlAxis;


    public void OnMove(InputValue value)
    {

        controlAxis = value.Get<Vector2>();

    }

}
