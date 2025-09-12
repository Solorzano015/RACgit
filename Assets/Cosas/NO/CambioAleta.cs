using UnityEngine;
using System.Collections;

public class CambioAleta : MonoBehaviour
{
    public Camera targetCamera; // Cámara a activar
    public Camera mainCamera;  // Cámara principal
    public float moveDuration = 1f;  // Duración del movimiento de la cámara

    private Camera movingCamera;
    private bool isMoving = false;

    private void Start()
    {
        // Crear una cámara temporal para el movimiento
        movingCamera = new GameObject("MovingCamera").AddComponent<Camera>();
        movingCamera.enabled = false;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player") && !isMoving)
        {
            StartCoroutine(MoveCamera(mainCamera, targetCamera));
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player") && !isMoving)
        {
            StartCoroutine(MoveCamera(targetCamera, mainCamera));
        }
    }

    private IEnumerator MoveCamera(Camera startCamera, Camera endCamera)
    {
        isMoving = true;
        movingCamera.transform.position = startCamera.transform.position;
        movingCamera.transform.rotation = startCamera.transform.rotation;
        movingCamera.gameObject.SetActive(true);
        movingCamera.enabled = true;  // Asegurarse de que la cámara de movimiento esté activa y renderizando
        startCamera.gameObject.SetActive(false);

        Vector3 startPosition = movingCamera.transform.position;
        Quaternion startRotation = movingCamera.transform.rotation;
        Vector3 endPosition = endCamera.transform.position;
        Quaternion endRotation = endCamera.transform.rotation;

        float elapsedTime = 0f;

        while (elapsedTime < moveDuration)
        {
            elapsedTime += Time.deltaTime;
            float t = Mathf.Clamp01(elapsedTime / moveDuration);
            movingCamera.transform.position = Vector3.Lerp(startPosition, endPosition, t);
            movingCamera.transform.rotation = Quaternion.Lerp(startRotation, endRotation, t);
            yield return null;
        }

        // Desactivar todas las cámaras y activar la nueva
        SwitchCamera(endCamera);
        movingCamera.enabled = false;  // Desactivar la cámara de movimiento para evitar errores de renderizado
        movingCamera.gameObject.SetActive(false);
        isMoving = false;
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
            newCamera.enabled = true;  // Asegurarse de que la cámara objetivo esté activa y renderizando
        }
    }
}
