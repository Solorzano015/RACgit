using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class UIManager : MonoBehaviour
{
    [Header("Referencias UI")]
    public GameObject mainMenu;    // Menú principal
    public GameObject pauseMenu;   // Menú de pausa
    public GameObject deathScreen; // Pantalla de muerte

    [Header("Referencias de escena")]
    public GameObject cameraObject; // Cámara con el script "Camera Collision Follow"
    public GameObject playerObject; // Referencia al personaje

    [Header("Tiempos")]
    public float deathScreenDelay = 3.5f; // Espera antes de mostrar la pantalla de muerte
    public float delay = 3f;              // Tiempo antes de cambiar de escena

    private bool isPaused = false;
    private bool isDead = false;

    private MonoBehaviour cameraFollowScript;
    private CharacterDamage characterDamage;

    // Variable temporal usada solo en WebGL para invocar el cambio de escena
#if UNITY_WEBGL
    private string sceneToLoad;
#endif

    void Start()
    {
        ShowMainMenu(true);
        ShowPauseMenu(false);
        ShowDeathScreen(false);

        if (cameraObject != null)
            cameraFollowScript = cameraObject.GetComponent("CameraCollisionFollow") as MonoBehaviour;

        if (playerObject != null)
            characterDamage = playerObject.GetComponent<CharacterDamage>();
    }

    void Update()
    {
        bool anyOtherMenuActive = (mainMenu != null && mainMenu.activeSelf)
                                  || (deathScreen != null && deathScreen.activeSelf);

        if (!isDead && !anyOtherMenuActive && Keyboard.current.escapeKey.wasPressedThisFrame)
            TogglePause();
        else if (isDead && Keyboard.current.escapeKey.wasPressedThisFrame)
            ReturnToMainMenu();

        if (!isDead && !anyOtherMenuActive && Gamepad.current != null && Gamepad.current.startButton.wasPressedThisFrame)
            TogglePause();
        else if (isDead && Gamepad.current != null && Gamepad.current.startButton.wasPressedThisFrame)
            ReturnToMainMenu();

        CheckDeathState();
    }

    // --- Cambiar de escena de forma segura en WebGL ---
    public void LoadSceneWithDelay(string sceneName)
    {
#if UNITY_WEBGL
        sceneToLoad = sceneName;
        Invoke(nameof(LoadSceneInvoked), delay);
#else
        StartCoroutine(LoadSceneAfterDelay(sceneName));
#endif
    }

#if UNITY_WEBGL
    private void LoadSceneInvoked()
    {
        SceneManager.LoadScene(sceneToLoad);
    }
#else
    private IEnumerator LoadSceneAfterDelay(string sceneName)
    {
        yield return new WaitForSeconds(delay);
        SceneManager.LoadScene(sceneName);
    }
#endif

    // --- Funciones de botones ---
    public void StartGame()
    {
        LoadSceneWithDelay("esc2");
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

    public void ReturnToMainMenu()
    {
        Time.timeScale = 1f;
        LoadSceneWithDelay("MM");
    }

    public void RestartGame()
    {
        int nivel = SceneManager.GetActiveScene().buildIndex;
        Time.timeScale = 1f;
        SceneManager.LoadScene(nivel);
    }

    // --- Control de pausa ---
    public void TogglePause()
    {
        if (isDead) return;
        if ((mainMenu != null && mainMenu.activeSelf) || (deathScreen != null && deathScreen.activeSelf))
            return;

        isPaused = !isPaused;
        ShowPauseMenu(isPaused);
        Time.timeScale = isPaused ? 0f : 1f;

        if (cameraFollowScript != null)
            cameraFollowScript.enabled = !isPaused;

        Cursor.visible = isPaused;
        Cursor.lockState = isPaused ? CursorLockMode.None : CursorLockMode.Locked;
    }

    public void ResumeGame()
    {
        isPaused = false;
        ShowPauseMenu(false);
        Time.timeScale = 1f;

        if (cameraFollowScript != null)
            cameraFollowScript.enabled = true;

        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;
    }

    // --- Control de pantallas ---
    private void ShowMainMenu(bool show)
    {
        if (mainMenu != null)
            mainMenu.SetActive(show);
    }

    private void ShowPauseMenu(bool show)
    {
        if (pauseMenu != null)
            pauseMenu.SetActive(show);
    }

    private void ShowDeathScreen(bool show)
    {
        if (deathScreen != null)
            deathScreen.SetActive(show);
    }

    // --- Control de muerte y reaparición ---
    private void CheckDeathState()
    {
        if (isDead || characterDamage == null) return;

        if (characterDamage.DEAD)
        {
            isDead = true;
            Invoke(nameof(ActivateDeathScreen), deathScreenDelay);
        }
    }

    private void ActivateDeathScreen()
    {
        if (cameraFollowScript != null)
            cameraFollowScript.enabled = false;

        ShowDeathScreen(true);
        ShowPauseMenu(false);
        Time.timeScale = 0f;

        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;
    }

    public void RevivePlayer()
    {
        Debug.Log("Botón de revivir presionado");

        if (playerObject != null)
        {
            var cd = playerObject.GetComponent<CharacterDamage>();
            if (cd != null)
            {
                Debug.Log("Solicitud de reaparición enviada a CharacterDamage");
                cd.respawnRequested = true;
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
            cameraFollowScript.enabled = true;
    }
}
