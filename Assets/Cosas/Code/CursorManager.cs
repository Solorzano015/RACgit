using UnityEngine;
using UnityEngine.EventSystems; // Necesario para detectar elementos UI

public class CursorManager : MonoBehaviour
{
    // Puedes arrastrar tus paneles UI aquí en el Inspector de Unity
    // para que el script pueda verificar su estado.
    public GameObject[] UIPanels; 

    void Start()
    {
        // Asegúrate de que el cursor esté configurado correctamente al inicio del juego.
        SetCursorState(false); 
    }

    void Update()
    {
        // Verifica si el puntero del ratón está sobre un elemento de la UI
        // O si alguno de los paneles UI que le asignamos está activo.
        bool isUIVisible = EventSystem.current.IsPointerOverGameObject();

        // Si tienes paneles UI específicos que quieres que activen el cursor,
        // puedes añadir una comprobación para ellos.
        foreach (GameObject panel in UIPanels)
        {
            if (panel != null && panel.activeInHierarchy)
            {
                isUIVisible = true;
                break; 
            }
        }

        // Si la UI está visible, haz que el cursor también lo esté y libéralo.
        // De lo contrario, ocúltalo y bloquéalo.
        SetCursorState(isUIVisible);
    }

    void SetCursorState(bool visible)
    {
        Cursor.visible = visible; // true para visible, false para oculto

        if (visible)
        {
            Cursor.lockState = CursorLockMode.None; // El cursor puede moverse libremente
        }
        else
        {
            Cursor.lockState = CursorLockMode.Locked; // El cursor está bloqueado en el centro de la pantalla
        }
    }
}