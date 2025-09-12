using UnityEngine;

public class walkanim : StateMachineBehaviour
{
    public GameObject Player;
    public GameObject Boss;
    public float distanciaPlayer;
    public Vector3 posicionInPlayer;
    /* public float walkSpeed=5; */
    public Vector3 posicionRangeB;
    public float patrolWait = 4;
    /* public float distAtt = 4;
    public float distRoll = 15;
    public float distWalk = 6; */



    override public void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        if (bossBrain.instance.follow == true)
        {
            Player = GameObject.FindGameObjectWithTag("Player");

        }
        else
        { 
            Player = bossBrain.instance.puntosRef[bossBrain.instance.puntoRef].gameObject;
        }
        Boss = bossBrain.instance.gameObject;
    }


    override public void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        if (bossBrain.instance.follow == true)
        {

            distanciaPlayer = Vector3.Distance(Boss.transform.position, Player.transform.position);
            posicionInPlayer = Player.transform.position;



            Boss.transform.position = Vector3.MoveTowards(Boss.transform.position, posicionInPlayer, bossBrain.instance.walkSpeed * Time.deltaTime);
            Boss.transform.rotation = Quaternion.LookRotation(Vector3.up, Boss.transform.position - posicionInPlayer);



            if (distanciaPlayer < bossBrain.instance.distAtt)
            {
                animator.SetTrigger("IsAttacking");
                animator.SetBool("IsWalking", false);

            }

            if (distanciaPlayer <= bossBrain.instance.distRoll && distanciaPlayer >= bossBrain.instance.distWalk)
            {
                animator.SetBool("IsWalking", false);

                animator.SetBool("IsRollingAttack", true);
            }

        }
        else
        { 

            distanciaPlayer = Vector3.Distance(Boss.transform.position, Player.transform.position);
            posicionInPlayer = Player.transform.position;



            Boss.transform.position = Vector3.MoveTowards(Boss.transform.position, posicionInPlayer, bossBrain.instance.walkSpeed * Time.deltaTime);
            Boss.transform.rotation = Quaternion.LookRotation(Vector3.up, Boss.transform.position - posicionInPlayer);



            if (distanciaPlayer <= 2)
            {
                bossBrain.instance.puntoRef += 1;
                if (bossBrain.instance.puntoRef >= bossBrain.instance.puntosRef.Length)
                {
                    bossBrain.instance.puntoRef = 0;
                }

                animator.SetBool("IsWalking", false);

                Player = bossBrain.instance.puntosRef[bossBrain.instance.puntoRef].gameObject;

            }

            /* if (distanciaPlayer <= bossBrain.instance.distRoll && distanciaPlayer >= bossBrain.instance.distWalk)
            {
                animator.SetBool("IsWalking", false);

                animator.SetBool("IsRollingAttack", true);
            } */
        }
    }

    // OnStateExit is called when a transition ends and the state machine finishes evaluating this state
    //override public void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    //{
    //    
    //}

    // OnStateMove is called right after Animator.OnAnimatorMove()
    //override public void OnStateMove(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    //{
    //    // Implement code that processes and affects root motion
    //}

    // OnStateIK is called right after Animator.OnAnimatorIK()
    //override public void OnStateIK(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    //{
    //    // Implement code that sets up animation IK (inverse kinematics)
    //}
}
