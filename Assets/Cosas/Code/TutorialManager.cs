using UnityEngine;
using System.Collections.Generic;
using System.Collections; // Necesario para usar Coroutines

public class TutorialManager : MonoBehaviour
{
    public List<GameObject> tutorialScreens;
    private int currentTutorialIndex = 0; // Para llevar la cuenta de la pantalla actual

    // Variables para la transición 2 a 3 (Mantener Shift)
    private float shiftHoldStartTime = 0f;
    private const float requiredShiftHoldTime = 0.2f; // Tiempo que debe mantenerse Shift

    void Start()
    {
        // Asegúrate de que la lista no esté vacía
        if (tutorialScreens != null && tutorialScreens.Count > 0)
        {
            // Desactiva todas las pantallas inicialmente
            foreach (GameObject screen in tutorialScreens)
            {
                screen.SetActive(false);
            }

            // Activa solo la primera pantalla
            tutorialScreens[currentTutorialIndex].SetActive(true);
        }
        else
        {
            Debug.LogWarning("La lista de pantallas de tutorial está vacía o nula. ¡Asegúrate de asignar tus canvases en el Inspector!");
        }
    }

    void Update()
    {
        // Lógica de transición basada en la pantalla actual
        switch (currentTutorialIndex)
        {
            case 0: // Transición de la pantalla 1 a la 2
                CheckTransition1to2();
                break;
            case 1: // Transición de la pantalla 2 a la 3
                CheckTransition2to3();
                break;
            case 2: // Transición de la pantalla 3 a la 4
                CheckTransition3to4();
                break;
            case 3: // Última pantalla (la 4), se cerrará después de 5 segundos
                // No necesitamos una condición de entrada, el StartCoroutine ya lo maneja
                break;
        }
    }

    // Método para avanzar a la siguiente pantalla
    void AdvanceTutorial()
    {
        // Desactiva la pantalla actual
        tutorialScreens[currentTutorialIndex].SetActive(false);

        // Incrementa el índice para ir a la siguiente pantalla
        currentTutorialIndex++;

        // Verifica si todavía hay pantallas en la lista
        if (currentTutorialIndex < tutorialScreens.Count)
        {
            // Activa la siguiente pantalla
            tutorialScreens[currentTutorialIndex].SetActive(true);

            // Si es la última pantalla, inicia el temporizador de cierre
            if (currentTutorialIndex == tutorialScreens.Count - 1)
            {
                StartCoroutine(CloseLastTutorialScreenAfterDelay(10f));
            }
        }
        else
        {
            Debug.Log("Tutorial completado.");
            // Aquí puedes añadir más lógica, como desactivar el propio TutorialManager
            // o cargar una nueva escena.
            gameObject.SetActive(false); // Desactiva este GameObject para que el script deje de ejecutarse
        }
    }

    // --- Métodos de verificación de transición ---

    void CheckTransition1to2()
    {
        // Al menos una tecla de movimiento (WASD o flechas)
        bool movementInput = Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.A) ||
                             Input.GetKey(KeyCode.S) || Input.GetKey(KeyCode.D) ||
                             Input.GetKey(KeyCode.UpArrow) || Input.GetKey(KeyCode.LeftArrow) ||
                             Input.GetKey(KeyCode.DownArrow) || Input.GetKey(KeyCode.RightArrow);

        // Y la barra espaciadora
        bool spaceInput = Input.GetKey(KeyCode.Space);

        if (movementInput || spaceInput)
        {
            AdvanceTutorial();
        }
    }

    void CheckTransition2to3()
    {
        // Si se mantiene la tecla Shift
        if (Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift))
        {
            // Si es la primera vez que se presiona Shift o se soltó y volvió a presionar
            if (shiftHoldStartTime == 0f)
            {
                shiftHoldStartTime = Time.time; // Registra el tiempo de inicio
            }

            // Verifica si ha pasado el tiempo requerido
            if (Time.time - shiftHoldStartTime >= requiredShiftHoldTime)
            {
                AdvanceTutorial();
                shiftHoldStartTime = 0f; // Reinicia para la siguiente vez
            }
        }
        else
        {
            // Si Shift no está presionado, reinicia el temporizador
            shiftHoldStartTime = 0f;
        }
    }

    void CheckTransition3to4()
    {
        // Si Shift está oprimido Y se da clic (izquierdo o derecho)
        bool shiftPressed = Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift);
        bool mouseClicked = Input.GetMouseButtonDown(0) || Input.GetMouseButtonDown(1); // 0 es clic izquierdo, 1 es clic derecho

        if (shiftPressed && mouseClicked)
        {
            AdvanceTutorial();
        }
    }

    // Coroutine para cerrar la última pantalla después de un retraso
    IEnumerator CloseLastTutorialScreenAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay); // Espera la cantidad de segundos especificada
        AdvanceTutorial(); // Llama a AdvanceTutorial para "cerrar" la última pantalla
    }
}