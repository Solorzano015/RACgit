using UnityEngine;

public class CameraTrigger : MonoBehaviour
{
    public Camera targetCamera; // Cámara a activar
    public Camera mainCamera;  // Cámara principal

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            SwitchCamera(targetCamera);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            SwitchCamera(mainCamera);
        }
    }

    private void SwitchCamera(Camera newCamera)
    {
        Camera[] cameras = Camera.allCameras;
        foreach (Camera cam in cameras)
        {
            cam.gameObject.SetActive(false);
        }

        if (newCamera != null)
        {
            newCamera.gameObject.SetActive(true);
        }
    }
}
