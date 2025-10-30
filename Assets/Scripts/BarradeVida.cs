using Unity.Mathematics;
using UnityEngine;
using UnityEngine.UI;
public class BarradeVida : MonoBehaviour
{
    public Slider barraVida;
    public Image relleno;
    public Color[] colores;
    public float vida;
    public float velocidadRelleno;


    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        barraVida.value = Mathf.MoveTowards(barraVida.value,vida,velocidadRelleno);
        ActualizarVida();
    }

    public void ActualizarVida()
    { 
    
      
        switch (barraVida.value)
        {
            case 13:
                relleno.color = colores[0];
                break;
            case 12:
                relleno.color = colores[1];
                break;
            case 11:
                relleno.color = colores[2];
                break;
            case 10:
                relleno.color = colores[3];
                break;
            case 9:
                relleno.color = colores[4];
                break;
            case 8:
                relleno.color = colores[5];
                break;
            case 7:
                relleno.color = colores[6];
                break;
            case 6:
                relleno.color = colores[7];
                break;
            case 5:
                relleno.color = colores[8];
                break;
            case 4:
                relleno.color = colores[9];
                break;
            case 3:
                relleno.color = colores[10];
                break;
            case 2:
                relleno.color = colores[11];
                break;
            case 1:
                relleno.color = colores[12];
                break;
            case 0:
                relleno.color = colores[13];
                break;
        }

    }




}
