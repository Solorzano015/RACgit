using UnityEngine;

public class MusicManager : MonoBehaviour
{
    [Header("Configuración de Música")]
    [Tooltip("Arrastra aquí el GameObject con la música de ambiente.")]
    public GameObject ambientMusicGameObject;

    [Tooltip("Arrastra aquí el GameObject con la música de área.")]
    public GameObject areaMusicGameObject;

    [Header("Configuración de Detección")]
    [Tooltip("Arrastra aquí el GameObject que, al activarse, cambia a música de ambiente.")]
    public GameObject specificDetectionGameObject;

    private void Start()
    {
        // Asegúrate de que solo la música de ambiente esté activa al inicio
        if (ambientMusicGameObject != null)
        {
            ambientMusicGameObject.SetActive(true);
        }
        if (areaMusicGameObject != null)
        {
            areaMusicGameObject.SetActive(false);
        }
    }

    private void Update()
    {
        // Detecta si el GameObject específico se ha activado
        if (specificDetectionGameObject != null && specificDetectionGameObject.activeSelf)
        {
            PlayAmbientMusic();
        }
    }

    // Se llama cuando otro collider entra en este trigger
    private void OnTriggerEnter(Collider other)
    {
        // Asegúrate de que el collider que entra es el jugador (puedes ajustar el Tag si es diferente)
        if (other.CompareTag("Player")) // Asegúrate de que tu jugador tenga el Tag "Player"
        {
            PlayAreaMusic();
        }
    }

    // Se llama cuando otro collider sale de este trigger
    private void OnTriggerExit(Collider other)
    {
        // Asegúrate de que el collider que sale es el jugador
        if (other.CompareTag("Player")) // Asegúrate de que tu jugador tenga el Tag "Player"
        {
            PlayAmbientMusic();
        }
    }

    /// <summary>
    /// Activa la música de ambiente y desactiva la de área.
    /// </summary>
    private void PlayAmbientMusic()
    {
        if (ambientMusicGameObject != null && !ambientMusicGameObject.activeSelf)
        {
            ambientMusicGameObject.SetActive(true);
        }
        if (areaMusicGameObject != null && areaMusicGameObject.activeSelf)
        {
            areaMusicGameObject.SetActive(false);
        }
    }

    /// <summary>
    /// Activa la música de área y desactiva la de ambiente.
    /// </summary>
    private void PlayAreaMusic()
    {
        if (areaMusicGameObject != null && !areaMusicGameObject.activeSelf)
        {
            areaMusicGameObject.SetActive(true);
        }
        if (ambientMusicGameObject != null && ambientMusicGameObject.activeSelf)
        {
            ambientMusicGameObject.SetActive(false);
        }
    }
}