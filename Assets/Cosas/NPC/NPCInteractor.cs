using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class NPCInteractor : MonoBehaviour
{
    [Header("Configuración")]
    public float interactionRadius = 3f;
    public KeyCode interactionKey = KeyCode.E;
    public GameObject dialogCanvas;
    public TMP_Text dialogText;
    public Button nextButton;
    public List<string> initialDialogPages;
    [TextArea(3, 10)] public string postDialogText = "Gracias por tu ayuda.";
    [Header("Objetos a Desactivar en Interacción")]
    public List<GameObject> objectsToDisable;
    [Header("Efecto de Escritura")]
    public float defaultTypeSpeed = 0.05f;
    [Header("Efecto de Temblor")]
    public float defaultShakeMagnitude = 0.5f;
    public float defaultShakeSpeed = 0.1f;
    [Header("Materiales de Interacción")]
    public GameObject rendererObject; // Nuevo campo para seleccionar el objeto con el Renderer
    public Material defaultMaterial;
    public Material highlightMaterial;

    private NPCHeadAnimatorController headAnimator;
    private Transform playerTransform;
    private bool canInteract = false;
    private int currentPageIndex = 0;
    private bool initialDialogShown = false;
    private SphereCollider interactionCollider;
    private List<bool> originalObjectStates;
    private bool isTyping = false;
    private bool isShaking = false;
    private Renderer npcRenderer;
    private Material originalNPCMaterial;


    void Start()
    {
        if (dialogCanvas != null)
        {
            dialogCanvas.SetActive(false);
        }
        else
        {
            Debug.LogError("El Canvas de diálogo no está asignado en el Inspector del NPC.");
            enabled = false;
            return;
        }

        headAnimator = GetComponent<NPCHeadAnimatorController>();
        if (headAnimator == null)
        {
            Debug.LogWarning("No se encontró el script NPCHeadAnimatorController en este NPC.");
        }

        interactionCollider = gameObject.AddComponent<SphereCollider>();
        interactionCollider.radius = interactionRadius;
        interactionCollider.isTrigger = true;

        if (dialogText == null)
        {
            Debug.LogError("El componente TMP_Text para el diálogo no está asignado en el Inspector del NPC.");
            enabled = false;
            return;
        }

        if (nextButton == null)
        {
            Debug.LogError("El botón siguiente no está asignado en el Inspector del NPC.");
            enabled = false;
            return;
        }

        nextButton.onClick.AddListener(ShowNextDialogPage);

        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            playerTransform = player.transform;
        }
        else
        {
            Debug.LogError("No se encontró ningún GameObject con el tag 'Player'. Asegúrate de que tu jugador tenga este tag.");
            enabled = false;
        }

        originalObjectStates = new List<bool>();
        foreach (GameObject obj in objectsToDisable)
        {
            if (obj != null)
            {
                originalObjectStates.Add(obj.activeSelf);
            }
        }

        // Buscar el componente Renderer en el objeto seleccionado
        if (rendererObject != null)
        {
            npcRenderer = rendererObject.GetComponent<Renderer>();
            if (npcRenderer == null)
            {
                Debug.LogWarning("No se encontró ningún componente Renderer en el objeto seleccionado para los materiales de interacción.");
            }
            else
            {
                originalNPCMaterial = npcRenderer.material; // Guardar el material original
                if (defaultMaterial == null)
                {
                    defaultMaterial = originalNPCMaterial; // Si no se asigna uno, usar el original como default
                }
                if (highlightMaterial == null)
                {
                    Debug.LogWarning("No se ha asignado un material de 'Highlight' en el Inspector del NPC.");
                }
                // Establecer el material por defecto al inicio
                npcRenderer.material = defaultMaterial;
            }
        }
        else
        {
            Debug.LogWarning("No se ha seleccionado ningún objeto con el Renderer para los materiales de interacción en el Inspector del NPC.");
        }
    }

    void Update()
    {
        if (canInteract && Input.GetKeyDown(interactionKey))
        {
            Interact();
        }
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            canInteract = true;
            if (npcRenderer != null && highlightMaterial != null)
            {
                npcRenderer.material = highlightMaterial; // Cambiar al material de highlight al entrar
            }
        }
    }

    void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            canInteract = false;
            if (dialogCanvas != null && dialogCanvas.activeSelf)
            {
                dialogCanvas.SetActive(false);
                ReactivateDisabledObjects();
                currentPageIndex = 0;
            }
            else if (dialogCanvas != null && !dialogCanvas.activeSelf)
            {
                ReactivateDisabledObjects();
            }

            // Volver al material por defecto al salir
            if (npcRenderer != null)
            {
                npcRenderer.material = defaultMaterial;
            }
        }
    }

    void Interact()
    {
        // Activar el Animator
        if (headAnimator != null && !headAnimator.enabled)
        {
            headAnimator.enabled = true;
        }

        // Desactivar los objetos especificados
        foreach (GameObject obj in objectsToDisable)
        {
            if (obj != null)
            {
                obj.SetActive(false);
            }
        }

        // Activar el Canvas de diálogo
        if (dialogCanvas != null)
        {
            dialogCanvas.SetActive(true);
            currentPageIndex = 0;

            // Mostrar el diálogo inicial o posterior
            if (!initialDialogShown)
            {
                ShowCurrentDialogPage(initialDialogPages);
            }
            else
            {
                dialogText.text = ""; // Limpiar el texto anterior
                StopAllCoroutines(); // Detener cualquier corrutina activa
                StartCoroutine(TypeTextEffect(postDialogText)); // Procesar el postDialogText con el efecto de escritura (y shake si está en el texto)
                if (nextButton != null)
                {
                    nextButton.gameObject.SetActive(false); // El botón se desactiva ya que solo hay una página en el post diálogo
                }
            }
        }
    }

    void ShowNextDialogPage()
    {
        if (!initialDialogShown)
        {
            StopAllCoroutines();
            currentPageIndex++;
            ShowCurrentDialogPage(initialDialogPages);
        }
        else
        {
            dialogCanvas.SetActive(false);
            ReactivateDisabledObjects();
        }
    }

    void ShowCurrentDialogPage(List<string> dialogList)
    {
        if (dialogText != null)
        {
            if (currentPageIndex < dialogList.Count)
            {
                dialogText.text = "";
                StopAllCoroutines();
                StartCoroutine(TypeTextEffect(dialogList[currentPageIndex]));
                if (nextButton != null)
                {
                    nextButton.gameObject.SetActive(true);
                }
            }
            else
            {
                dialogCanvas.SetActive(false);
                initialDialogShown = true;
                currentPageIndex = 0;
                ReactivateDisabledObjects();
            }
        }
    }

    IEnumerator TypeTextEffect(string fullText)
    {
        isTyping = true;
        string processedText = "";
        for (int i = 0; i < fullText.Length; i++)
        {
            if (fullText[i] == '[')
            {
                if (fullText.Substring(i).StartsWith("[shake]"))
                {
                    i += "[shake]".Length - 1;
                    string shakeText = "";
                    int shakeStartIndex = processedText.Length;
                    while (i + 1 < fullText.Length && !fullText.Substring(i + 1).StartsWith("[/shake]"))
                    {
                        shakeText += fullText[i + 1];
                        processedText += fullText[i + 1];
                        i++;
                    }
                    StartCoroutine(ShakeTextEffect(shakeText, shakeStartIndex));
                    // El texto tembloroso se añade al processedText, que se mostrará con el efecto de escritura
                    if (fullText.Substring(i + 1).StartsWith("[/shake]"))
                    {
                        i += "[/shake]".Length;
                    }
                }
                else
                {
                    processedText += fullText[i];
                }
            }
            else
            {
                processedText += fullText[i];
            }
            dialogText.text = processedText;
            yield return new WaitForSeconds(defaultTypeSpeed);
        }
        isTyping = false;
    }

    IEnumerator ShakeTextEffect(string textToShake, int startIndex)
    {
        isShaking = true;
        TMP_TextInfo textInfo = dialogText.textInfo;
        float startTime = Time.time;

        while (isShaking)
        {
            if (Time.time - startTime > 0.5f) // Limitar la duración del temblor para pruebas
            {
                isShaking = false;
            }
            dialogText.ForceMeshUpdate();
            for (int i = 0; i < textToShake.Length; i++)
            {
                int charIndex = startIndex + i;
                if (charIndex < textInfo.characterCount)
                {
                    TMP_CharacterInfo charInfo = textInfo.characterInfo[charIndex];
                    int meshIndex = charInfo.materialReferenceIndex;
                    int vertexIndex = charInfo.vertexIndex;

                    Vector3[] vertices = textInfo.meshInfo[meshIndex].vertices;
                    Vector3 offset = Random.insideUnitSphere * defaultShakeMagnitude;

                    vertices[vertexIndex + 0] += offset;
                    vertices[vertexIndex + 1] += offset;
                    vertices[vertexIndex + 2] += offset;
                    vertices[vertexIndex + 3] += offset;

                    textInfo.meshInfo[meshIndex].vertices = vertices;
                }
            }
            dialogText.UpdateVertexData(TMP_VertexDataUpdateFlags.Vertices);
            yield return new WaitForSeconds(defaultShakeSpeed);
        }
        dialogText.ForceMeshUpdate();
        dialogText.UpdateVertexData(TMP_VertexDataUpdateFlags.Vertices);
    }

    void ReactivateDisabledObjects()
    {
        for (int i = 0; i < objectsToDisable.Count; i++)
        {
            if (objectsToDisable[i] != null && i < originalObjectStates.Count)
            {
                objectsToDisable[i].SetActive(originalObjectStates[i]);
            }
        }
    }

    public float GetInteractionRadius()
    {
        return interactionRadius;
    }

    public void SetInteractionRadius(float newRadius)
    {
        interactionRadius = newRadius;
        if (interactionCollider != null)
        {
            interactionCollider.radius = newRadius;
        }
    }
}