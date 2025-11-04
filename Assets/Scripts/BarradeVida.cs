using Unity.Mathematics;
using UnityEngine;
using UnityEngine.UI;

public class BarradeVida : MonoBehaviour
{
    public Slider barraVida;
    public Image relleno;
    public Color[] colores; // Solo deben ser 4 colores (vida 3, 2, 1, 0)
    [Range(0, 3)]
    public float vida;
    public float velocidadRelleno;
    public Image cara;
    public Sprite[] caras; // Solo deben ser 3 sprites (vida alta, media, baja)

    public GameObject koObj;
    public GameObject deathCanvas;

    public static BarradeVida instance;

    private void Awake()
    {
        instance = this;
    }

    void Update()
    {
        barraVida.value = Mathf.MoveTowards(barraVida.value, vida, velocidadRelleno);
        ActualizarVida();
    }

    public void ActualizarVida()
    {
        switch (Mathf.RoundToInt(barraVida.value))
        {
            case 3:
                relleno.color = colores[0];
                cara.sprite = caras[0];
                koObj.SetActive(false);
                break;

            case 2:
                relleno.color = colores[1];
                cara.sprite = caras[1];
                koObj.SetActive(false);
                break;

            case 1:
                relleno.color = colores[2];
                cara.sprite = caras[2];
                koObj.SetActive(false);
                break;

            case 0:
                relleno.color = colores[3];
                cara.sprite = caras[2];
                koObj.SetActive(true);
                Invoke("Muerte", 3);
                break;
        }
    }

    public void Muerte()
    {
        deathCanvas.SetActive(true);
        Time.timeScale = 0.2f;
    }
}





































//using Unity.Mathematics;
//using UnityEngine;
//using UnityEngine.UI;
//public class BarradeVida : MonoBehaviour
//{
//    public Slider barraVida;
//    public Image relleno;
//    public Color[] colores;
//    [Range(0, 13)]
//    public float vida;
//    public float velocidadRelleno;
//    public Image cara;
//    public Sprite[] caras;

//    public GameObject koObj;

//    public static BarradeVida instance;

//    public GameObject deathCanvas;



//    // Start is called once before the first execution of Update after the MonoBehaviour is created
//    void Start()
//    {

//    }

//    // Update is called once per frame
//    void Update()
//    {
//        barraVida.value = Mathf.MoveTowards(barraVida.value, vida, velocidadRelleno);
//        ActualizarVida();

//    }

//    private void Awake()
//    {
//        instance = this;
//    }

//    public void ActualizarVida()
//    {

//        Debug.Log("barra 0");
//        switch (Mathf.RoundToInt(barraVida.value))
//        {
//            case 13:
//                relleno.color = colores[0];
//                cara.sprite = caras[0];
//                koObj.SetActive(false);

//                break;
//            case 12:
//                relleno.color = colores[1];
//                cara.sprite = caras[0];
//                koObj.SetActive(false);


//                break;
//            case 11:
//                relleno.color = colores[2];
//                cara.sprite = caras[0];
//                koObj.SetActive(false);


//                break;
//            case 10:
//                relleno.color = colores[3];
//                cara.sprite = caras[0];
//                koObj.SetActive(false);


//                break;
//            case 9:
//                relleno.color = colores[4];
//                cara.sprite = caras[0];
//                koObj.SetActive(false);


//                break;
//            case 8:
//                relleno.color = colores[5];
//                cara.sprite = caras[1];
//                koObj.SetActive(false);


//                break;
//            case 7:
//                relleno.color = colores[6];
//                cara.sprite = caras[1];
//                koObj.SetActive(false);


//                break;
//            case 6:
//                relleno.color = colores[7];
//                cara.sprite = caras[1];
//                koObj.SetActive(false);


//                break;
//            case 5:
//                relleno.color = colores[8];
//                cara.sprite = caras[1];
//                koObj.SetActive(false);


//                break;
//            case 4:
//                relleno.color = colores[9];
//                cara.sprite = caras[1];
//                koObj.SetActive(false);

//                break;
//            case 3:
//                relleno.color = colores[10];
//                cara.sprite = caras[2];
//                koObj.SetActive(false);

//                break;
//            case 2:
//                relleno.color = colores[11];
//                cara.sprite = caras[2];
//                koObj.SetActive(false);


//                break;
//            case 1:
//                relleno.color = colores[12];
//                cara.sprite = caras[2];
//                koObj.SetActive(false);


//                break;
//            case 0:
//                relleno.color = colores[13];
//                cara.sprite = caras[2];

//                koObj.SetActive(true);

//                Invoke("Muerte", 3);


//                break;
//        }

//    }


//    public void Muerte()
//    {

//        deathCanvas.SetActive(true);
//        Time.timeScale = 0.2f;


//    }

//}