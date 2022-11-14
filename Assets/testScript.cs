using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class testScript : StateMachineBehaviour
{
    //OnStateEnter is called when a transition starts and the state machine starts to evaluate this state
    override public void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        base.OnStateEnter(animator, stateInfo, layerIndex);
        if (stateInfo.IsName("Shield"))
		{
            float x = animator.GetFloat("remainingShieldTime");
            //get the shield animation
            //if from added shield... add X
            //else set it's remaining time to X
            //Debug.Log("s");
		}

        if (stateInfo.IsName("Attack"))
		{
            //Debug.Log("a");
        }
    }

    // OnStateUpdate is called on each Update frame between OnStateEnter and OnStateExit callbacks
    //override public void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    //{
    //    base.OnStateEnter(animator, stateInfo, layerIndex);
    //    if (stateInfo.IsName("Attack"))
    //    {
    //        //Debug.Log("test");
    //    }
    //}

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
