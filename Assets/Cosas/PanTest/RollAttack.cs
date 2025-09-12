using JetBrains.Annotations;
using Unity.Mathematics;
using UnityEngine;

public class RollAttack : StateMachineBehaviour
{

    public GameObject Player;
    public GameObject Boss;
    /* public float rollSpeed; */
    public float distanciaPlayer;
    public Vector3 posicionInPlayer;
    public float limiteDetenerse;
    public int nRebotes;
    public int nRebotesTotal;
    public int pared;
    public LayerMask limite;
    public float distRay;


    // OnStateEnter is called when a transition starts and the state machine starts to evaluate this state
    override public void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {

        Player = GameObject.FindGameObjectWithTag("Player");
        posicionInPlayer = Player.transform.position;
        Boss = bossBrain.instance.gameObject;
        nRebotes = nRebotesTotal;

        Boss.transform.rotation = quaternion.LookRotation(Vector3.up, Boss.transform.position - posicionInPlayer);


        Ray ray = new Ray(Boss.transform.position, -Boss.transform.up);
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit, distRay, limite))
        {
            posicionInPlayer=hit.point;


            Debug.Log("Impacto en: " + hit.point);
        }



        

        //Boss.transform.rotation = Quaternion.LookRotation(Vector3.up, Boss.transform.position - posicionInPlayer);

    }

    // OnStateUpdate is called on each Update frame between OnStateEnter and OnStateExit callbacks
    override public void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {

        if (bossBrain.instance.follow == true)
        {
            Boss.transform.position = Vector3.MoveTowards(Boss.transform.position, posicionInPlayer, bossBrain.instance.rollSpeed * Time.deltaTime);

            distanciaPlayer = Vector3.Distance(Boss.transform.position, posicionInPlayer);

            if (distanciaPlayer <= limiteDetenerse)
            {

                if (nRebotes <= 0)
                {

                    animator.SetBool("IsRollingAttack", false);

                }
                else
                {

                    Boss.transform.rotation = quaternion.LookRotation(Vector3.up, Boss.transform.position - Player.transform.position);


                    Ray ray = new Ray(Boss.transform.position, -Boss.transform.up);
                    RaycastHit hit;

                    if (Physics.Raycast(ray, out hit, distRay, limite))
                    {
                        posicionInPlayer = hit.point;


                        Debug.Log("Impacto en: " + hit.point);
                    }
                    nRebotes -= 1;
                }

            }

        }
        else
        { 
            animator.SetBool("IsRollingAttack", false);

        }
    }

    // OnStateExit is called when a transition ends and the state machine finishes evaluating this state
    override public void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        
    }

}
