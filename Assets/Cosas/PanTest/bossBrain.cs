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
    public float walkSpeed=5;
    public Animator animator;
    public GameObject impactFX;

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

    }

    // Update is called once per frame
    void Update()
    {

        Debug.DrawRay(transform.position, -transform.up * 100, Color.yellow);
    }
    
    public void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.tag == "Punch")
        {
            animator.SetTrigger("GetHit");
            Vector3 puntoDeContacto = other.ClosestPoint(transform.position);
            GameObject g = Instantiate(impactFX, puntoDeContacto, Quaternion.identity);
            StartCoroutine(CambiarColor());
            Destroy(g, 1f);
        } 
    }

    public void OnCollisionEnter(Collision other)
    {
        if (other.gameObject.tag == "Pared")
        {
            ContactPoint contacto = other.GetContact(0);
            Vector3 puntoDeContacto = contacto.point;
            GameObject g = Instantiate(impactFX, puntoDeContacto, Quaternion.identity);
            StartCoroutine(CambiarColor());
            //Destroy(g, 1f);
        }
    }

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
    }
}
