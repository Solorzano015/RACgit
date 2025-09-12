using UnityEngine;
using System.Collections; // Necesario para Coroutines (IEnumerator y WaitForSeconds)
using System.Collections.Generic; // Necesario para List

public class AdvancedCollisionDetector : MonoBehaviour
{
    [Header("Configuración de Colisión")]
    [Tooltip("Selecciona el Layer de los GameObjects con los que quieres detectar la colisión.")]
    public LayerMask targetCollisionLayer; // El Layer de los GameObjects con los que colisionar

    [Tooltip("Arrastra aquí el Collider específico (por ejemplo, el Sphere Collider) que detectará la colisión.")]
    public Collider collisionDetectorCollider; // El Collider específico que usará este script para la detección

    [Header("Configuración de Animator")]
    [Tooltip("El nombre del parámetro booleano en el Animator que se activará.")]
    public string animatorParameterName = "HasCollided"; // Nombre del parámetro booleano en el Animator

    [Header("Acciones Post-Colisión")]
    [Tooltip("Tiempo en segundos que esperará antes de activar el Canvas y pausar el juego.")]
    public float delayBeforeActions = 2.0f; // Tiempo de espera por defecto: 2 segundos

    [Tooltip("Arrastra aquí el objeto Canvas que se activará después de la espera.")]
    public GameObject canvasToActivate; // El Canvas que se activará

    private Animator objectAnimator; // Referencia al componente Animator
    private Rigidbody objectRigidbody; // Referencia al componente Rigidbody
    private bool collisionDetected = false; // Para asegurar que la acción solo se ejecute una vez

    void Start()
    {
        // Obtener el componente Animator
        objectAnimator = GetComponent<Animator>();
        if (objectAnimator == null)
        {
            Debug.LogError("Error: No se encontró un componente Animator en este GameObject. " +
                           "Asegúrate de que el GameObject '" + gameObject.name + "' tenga un Animator adjunto.", this);
        }

        // Obtener el componente Rigidbody
        objectRigidbody = GetComponent<Rigidbody>();
        if (objectRigidbody == null)
        {
            Debug.LogWarning("Advertencia: No se encontró un componente Rigidbody en este GameObject. La física no será modificada.", this);
        }

        // Verificar si el collider detector ha sido asignado
        if (collisionDetectorCollider == null)
        {
            Debug.LogError("Error: El campo 'Collision Detector Collider' no ha sido asignado en el Inspector para el GameObject '" + gameObject.name + "'. " +
                           "Por favor, arrastra el Collider específico (ej. Sphere Collider) que deseas usar.", this);
        }
        else
        {
            // Asegurarse de que el collider asignado pertenece a este mismo GameObject
            if (collisionDetectorCollider.gameObject != this.gameObject)
            {
                Debug.LogError("Error: El 'Collision Detector Collider' asignado no pertenece a este GameObject. " +
                               "Asegúrate de arrastrar un collider de este mismo GameObject.", this);
                collisionDetectorCollider = null; // Anular la asignación si es incorrecta
            }
        }

        // Verificar si el Canvas a activar ha sido asignado
        if (canvasToActivate == null)
        {
            Debug.LogError("Error: El campo 'Canvas To Activate' no ha sido asignado en el Inspector para el GameObject '" + gameObject.name + "'. " +
                           "Por favor, arrastra el GameObject Canvas que deseas activar.", this);
        }
        else
        {
            // Asegurarse de que el Canvas está inicialmente inactivo
            canvasToActivate.SetActive(false);
        }
    }

    // Método llamado cuando un collider (marcado como Is Trigger) entra en contacto con otro collider
    void OnTriggerEnter(Collider other)
    {
        // Solo reaccionar si el collider que detecta es el específico que nos interesa
        if (collisionDetectorCollider != null && other == collisionDetectorCollider)
        {
            // Verificar si el otro objeto está en el layer de interés
            if (((1 << other.gameObject.layer) & targetCollisionLayer) != 0)
            {
                DetectAndReact(other.gameObject);
            }
        }
    }

    // Método llamado cuando este collider (NO es un Trigger) entra en contacto con otro collider
    void OnCollisionEnter(Collision collision)
    {
        // Verificar si la colisión proviene del collider específico de este GameObject
        if (collisionDetectorCollider != null && collision.contacts.Length > 0 && collision.contacts[0].thisCollider == collisionDetectorCollider)
        {
            // Verificar si el otro objeto está en el layer de interés
            if (((1 << collision.gameObject.layer) & targetCollisionLayer) != 0)
            {
                DetectAndReact(collision.gameObject);
            }
        }
    }


    private void DetectAndReact(GameObject otherGameObject)
    {
        // Verificar si ya se detectó una colisión para evitar ejecuciones múltiples
        if (collisionDetected)
        {
            return;
        }

        // 1. Activar el parámetro booleano en el Animator
        if (objectAnimator != null)
        {
            if (HasParameter(objectAnimator, animatorParameterName, AnimatorControllerParameterType.Bool))
            {
                objectAnimator.SetBool(animatorParameterName, true);
                Debug.Log($"Parámetro Animator '{animatorParameterName}' activado a true.");
            }
            else
            {
                Debug.LogWarning($"Advertencia: El parámetro '{animatorParameterName}' no se encontró en el Animator de '{gameObject.name}'. " +
                                 "Asegúrate de que el nombre del parámetro sea correcto y exista en el Animator Controller.");
            }
        }

        // 2. Modificar el Rigidbody
        if (objectRigidbody != null)
        {
            objectRigidbody.useGravity = false; // Desactivar la gravedad

            // Congelar posición en todos los ejes usando RigidbodyConstraints
            objectRigidbody.constraints = RigidbodyConstraints.FreezePositionX |
                                          RigidbodyConstraints.FreezePositionY |
                                          RigidbodyConstraints.FreezePositionZ;
            Debug.Log("Rigidbody: Gravedad desactivada y posición congelada.");
        }

        // 3. Desactivar todos los componentes excepto el Animator y el propio script
        DeactivateAllComponentsExceptAnimator();

        // Marcar que la colisión ha sido detectada para evitar futuras ejecuciones
        collisionDetected = true;

        // 4. Iniciar la coroutine para las acciones post-colisión
        StartCoroutine(PostCollisionActions(delayBeforeActions));
    }

    private IEnumerator PostCollisionActions(float delay)
    {
        Debug.Log($"Esperando {delay} segundos antes de las acciones post-colisión...");
        yield return new WaitForSeconds(delay); // Espera el tiempo definido

        Debug.Log("Tiempo de espera terminado. Realizando acciones post-colisión.");

        // Activar el Canvas
        if (canvasToActivate != null)
        {
            canvasToActivate.SetActive(true);
            Debug.Log($"Canvas '{canvasToActivate.name}' activado.");
        }
        else
        {
            Debug.LogWarning("Advertencia: No hay un Canvas asignado para activar.");
        }

        // Pausar el juego
        Time.timeScale = 0f;
        Debug.Log("Juego pausado (Time.timeScale = 0).");
    }


    private void DeactivateAllComponentsExceptAnimator()
    {
        Component[] allComponents = GetComponents<Component>();

        foreach (Component comp in allComponents)
        {
            // Excluir el Transform, el Animator, el propio script y el Rigidbody
            // (ahora modificado en lugar de desactivado)
            if (comp != transform && comp != objectAnimator && comp != this && comp != objectRigidbody)
            {
                // Solo intentar desactivar si el componente es un MonoBehaviour, Collider o Renderer
                if (comp is MonoBehaviour monoBehaviour)
                {
                    monoBehaviour.enabled = false;
                    Debug.Log($"Componente desactivado: {comp.GetType().Name}");
                }
                else if (comp is Collider collider)
                {
                    collider.enabled = false;
                    Debug.Log($"Collider desactivado: {comp.GetType().Name}");
                }
                else if (comp is Renderer renderer)
                {
                    renderer.enabled = false;
                    Debug.Log($"Renderer desactivado: {comp.GetType().Name}");
                }
            }
        }
    }

    // Función auxiliar para verificar si un parámetro existe en el Animator
    private bool HasParameter(Animator animator, string paramName, AnimatorControllerParameterType paramType)
    {
        foreach (AnimatorControllerParameter param in animator.parameters)
        {
            if (param.name == paramName && param.type == paramType)
            {
                return true;
            }
        }
        return false;
    }
}