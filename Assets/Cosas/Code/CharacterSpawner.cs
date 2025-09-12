using UnityEngine;
using System.Collections;

public class CharacterSpawner : MonoBehaviour
{
    [Header("Prefab del personaje a instanciar")]
    public GameObject characterPrefab;

    [Header("Cantidad de personajes a generar si no hay hijos")]
    public int cantidad = 1;

    [Header("Offset de posición entre personajes")]
    public Vector3 offset = new Vector3(2f, 0f, 0f);

    [Header("Tiempo de espera antes de generar (en segundos)")]
    public float tiempoDeEspera = 2f;

    private bool esperando = false;

    void Update()
    {
        // Si no tiene hijos y no está ya esperando, comienza la generación
        if (transform.childCount == 0 && !esperando)
        {
            StartCoroutine(GenerarConRetraso());
        }
    }

    IEnumerator GenerarConRetraso()
    {
        esperando = true;
        yield return new WaitForSeconds(tiempoDeEspera);

        // Verificamos nuevamente después del retraso, por si acaso se generó algo mientras tanto
        if (transform.childCount == 0)
        {
            for (int i = 0; i < cantidad; i++)
            {
                Vector3 posicion = transform.position + offset * i;
                GameObject nuevoPersonaje = Instantiate(characterPrefab, posicion, Quaternion.identity, transform);
                nuevoPersonaje.name = characterPrefab.name + "_" + i;
            }
        }

        esperando = false;
    }
}
