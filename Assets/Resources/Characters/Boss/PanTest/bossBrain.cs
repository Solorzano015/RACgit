using UnityEngine;
using System.Collections;
using System.Collections.Generic;


public class bossBrain : MonoBehaviour
{

    public Transform Player;
    public float runSp;
    public float walkSp;
    public bool follow;
    public Vector3 posicionObjetivo;
    public float distAtt = 4;
    public float distRoll = 15;
    public float distWalk = 6;
    public float distanciaPlayer;
    public float rollSpeed;
    public float walkSpeed = 5;
    public Animator animator;
    public GameObject impactFX;
    public GameObject impactFXPared;
    public int bossVidas;
    public int bossMaxVidas = 3;
    public bool canGetHit;
    public GameObject camaraEntrada;
    public Renderer renderLife;
    public Color [] colorCresta;
    public Material mat;
    public float intensidad;
    public float intensidad1;
    public float intensidad0;

    public List<Material> materials;


    public Transform[] puntosRef;
    public int puntoRef;

    public static bossBrain instance;


    public void Awake()
    {
        instance = this;
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        bossVidas = bossMaxVidas;
        canGetHit = true;
    }

    private void OnEnable()
    {
        Invoke("IniciarBatalla", 3);
    }

    // Update is called once per frame
    void Update()
    {

        Debug.DrawRay(transform.position, -transform.up * 100, Color.yellow);
        
        if (bossVidas==3)
        {
            mat.SetColor("_EmissionColor", colorCresta[0] * intensidad);
        }

        if (bossVidas == 2)
        {
            mat.SetColor("_EmissionColor", colorCresta[1] * intensidad0);
        }

        if (bossVidas == 1)
        {
            mat.SetColor("_EmissionColor", colorCresta[2] * intensidad1);
        }

        if (bossVidas == 0)
        {
            mat.SetColor("_EmissionColor", colorCresta[3] * 0);
        }

    }

    public void IniciarBatalla()
    {
        camaraEntrada.SetActive(false);
        animator.SetTrigger("Entrada");
    }
    public void OnTriggerEnter(Collider other)
    {
        if (bossVidas <= 0 || canGetHit==false)
        {

            return;

        }


        if (other.gameObject.tag == "Punch")
        {
            animator.SetTrigger("GetHit");
            canGetHit = false;
            bossVidas -= 1;

            if (bossVidas <= 0)
            {

                animator.SetTrigger("Dead");
                

            }

            Vector3 puntoDeContacto = other.ClosestPoint(transform.position);
            GameObject g = Instantiate(impactFX, puntoDeContacto, Quaternion.identity);
            StartCoroutine(CambiarColor());
            Destroy(g, 1f);
        }

        if (other.gameObject.tag == "Pared")
        {
            Vector3 puntoDeContacto = other.ClosestPoint(transform.position);
            GameObject g = Instantiate(impactFXPared, puntoDeContacto, Quaternion.identity);
            //StartCoroutine(CambiarColor());
            Destroy(g, 1f);
        }
    }

    //public void OnCollisionEnter(Collision other)
    //{
    //    if (other.gameObject.tag == "Pared")
    //    {
    //        ContactPoint contacto = other.GetContact(0);
    //        Vector3 puntoDeContacto = contacto.point;
    //        GameObject g = Instantiate(impactFX, puntoDeContacto, Quaternion.identity);
    //        StartCoroutine(CambiarColor());
    //        //Destroy(g, 1f);
    //    }
    //}

    IEnumerator CambiarColor()
    {
        foreach (var item in materials)
        {
            item.color = Color.red;
        }
        yield return new WaitForSeconds(0.5f);
        foreach (var item in materials)
        {
            item.color = Color.white;

        }

        canGetHit = true;
    }
}
