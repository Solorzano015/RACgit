using UnityEngine;



public class IdleAnim : StateMachineBehaviour
{

    public GameObject Player;
    public GameObject Boss;
    public float distanciaPlayer;
    /* public float distRoll = 15;
    public float distWalk = 6; */
    public float think;
    


    // OnStateEnter is called when a transition starts and the state machine starts to evaluate this state
    override public void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        Player = GameObject.FindGameObjectWithTag("Player");
        Boss = bossBrain.instance.gameObject;
        think = Random.Range(0.5f, 2);
    }



    // OnStateUpdate is called on each Update frame between OnStateEnter and OnStateExit callbacks
    override public void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        think -= Time.deltaTime;

        if (think > 0)
        {
            return;
        }


        if (bossBrain.instance.follow == true)
        {
            Debug.DrawRay(Boss.transform.position, Boss.transform.forward * 10, Color.green);

            //Boss.transform.rotation = Quaternion.LookRotation(Player.transform.position - Boss.transform.position);



            distanciaPlayer = Vector3.Distance(Boss.transform.position, Player.transform.position);

            if (distanciaPlayer <= bossBrain.instance.distRoll && distanciaPlayer >= bossBrain.instance.distWalk)
            {
                animator.SetBool("IsRollingAttack", true);
            }
            else
            {
                animator.SetBool("IsWalking", true);
            }
        }
        else
        {
            animator.SetBool("IsRollingAttack", false);
            animator.SetBool("IsWalking", true);


        }

    }



    // OnStateExit is called when a transition ends and the state machine finishes evaluating this state
    override public void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {

    }

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
