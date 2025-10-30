using System.Collections;

using UnityEngine;
using UnityEngine.SceneManagement;

public class UIManager : MonoBehaviour
{
    public GameObject mainMenu;    // Menú principal
    public GameObject pauseMenu;   // Menú de pausa
    public GameObject deathScreen; // Pantalla de muerte
    public GameObject cameraObject; // Cámara con el script "Camera Collision Follow"
    public GameObject playerObject; // Referencia al personaje
    public float deathScreenDelay = 3.5f; // Tiempo de espera antes de mostrar la pantalla de muerte
    public float delay = 3f; // Cambia esto al número de segundos que necesites


    private bool isPaused = false;
    private bool isDead = false;
    private MonoBehaviour cameraFollowScript; // Referencia al script de la cámara
    private CharacterDamage characterDamage;    // Referencia al script del personaje

    void Start()
    {
        ShowMainMenu(true);
        ShowPauseMenu(false);
        ShowDeathScreen(false);

        if (cameraObject != null)
        {
            // Obtiene el script específico en la cámara (asegúrate de que el nombre del script es correcto)
            cameraFollowScript = cameraObject.GetComponent("CameraCollisionFollow") as MonoBehaviour;
        }

        if (playerObject != null)
        {
            characterDamage = playerObject.GetComponent<CharacterDamage>();
        }
    }

    void Update()
    {
        if (!isDead && Input.GetKeyDown(KeyCode.Escape))
        {
            TogglePause();
        }
        else if (isDead && Input.GetKeyDown(KeyCode.Escape))
        {
            ReturnToMainMenu();
        }

        CheckDeathState();
    }

    public void StartGame()
    {
        LoadSceneWithDelay("esc2"); // Cambia "esc2" por el nombre de tu escena real
    }

    private IEnumerator LoadSceneAfterDelay()
    {
        yield return new WaitForSeconds(delay);
        SceneManager.LoadScene("esc2"); // Cambia "esc2" por el nombre real de tu escena
    }

    public void LoadSampleScene()
    {
        SceneManager.LoadScene("SampleScene");
    }

    public void ExitGame()
    {
        Application.Quit();
        Debug.Log("Saliendo del juego...");
    }

    public void ShowMainMenu(bool show)
    {
        if (mainMenu != null)
        {
            mainMenu.SetActive(show);
        }
    }

    public void TogglePause()
    {
        if (isDead) return; // No permite pausar si el jugador está muerto

        isPaused = !isPaused;
        ShowPauseMenu(isPaused);
        Time.timeScale = isPaused ? 0f : 1f;

        if (cameraFollowScript != null)
        {
            cameraFollowScript.enabled = !isPaused;
        }

        Cursor.visible = isPaused;
        Cursor.lockState = isPaused ? CursorLockMode.None : CursorLockMode.Locked;
    }

    public void ResumeGame()
    {
        isPaused = false;
        ShowPauseMenu(false);
        Time.timeScale = 1f;

        if (cameraFollowScript != null)
        {
            cameraFollowScript.enabled = true;
        }

        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;
    }

    public void ReturnToMainMenu()
    {
        Time.timeScale = 1f;
        LoadSceneWithDelay("MM"); // Cambia "MM" por el nombre real de tu escena de menú principal
        
    }

    private void ShowPauseMenu(bool show)
    {
        if (pauseMenu != null)
        {
            pauseMenu.SetActive(show);
        }
    }

    private void ShowDeathScreen(bool show)
    {
        if (deathScreen != null)
        {
            deathScreen.SetActive(show);
        }
    }

    public void LoadSceneWithDelay(string sceneName)
    {
        StartCoroutine(LoadSceneAfterDelay(sceneName));

        
    }

    private IEnumerator LoadSceneAfterDelay(string sceneName)
    {
        yield return new WaitForSeconds(delay);
        SceneManager.LoadScene(sceneName);
    }

    private void CheckDeathState()
    {
        if (isDead || characterDamage == null) return;

        // Detecta si la variable DEAD está activa en el script CharacterDamage
        if (characterDamage.DEAD)
        {
            isDead = true;
            Invoke(nameof(ActivateDeathScreen), deathScreenDelay);
        }
    }

    private void ActivateDeathScreen()
    {
        if (cameraFollowScript != null)
        {
            cameraFollowScript.enabled = false; // Desactiva el seguimiento de cámara tras el retraso
        }
        ShowDeathScreen(true);
        ShowPauseMenu(false);
        Time.timeScale = 0f; // Detiene el juego

        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;
    }

    // Función para revivir al jugador
    public void RevivePlayer()
{
    Debug.Log("Botón de revivir presionado"); // Para depuración

    if (playerObject != null)
    {
        var cd = playerObject.GetComponent<CharacterDamage>();
        if (cd != null)
        {
            Debug.Log("Solicitud de reaparición enviada a CharacterDamage");
            cd.respawnRequested = true; // Solicita el respawn en el script CharacterDamage
        }
        else
        {
            Debug.LogError("CharacterDamage no encontrado en el playerObject");
        }
    }
    else
    {
        Debug.LogError("playerObject no está asignado en UIManager");
    }

    isDead = false;
    ShowDeathScreen(false);
    Time.timeScale = 1f;

    if (cameraFollowScript != null)
    {
        cameraFollowScript.enabled = true;
    }
}

}
