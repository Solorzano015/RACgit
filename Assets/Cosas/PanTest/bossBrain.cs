using UnityEngine;

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
    

}
