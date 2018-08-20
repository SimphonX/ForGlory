using Assets.Scripts.Player.Player;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GetRotation : StateMachineBehaviour {
    private Vector3 lastPosition;
     // OnStateEnter is called when a transition starts and the state machine starts to evaluate this state
    override public void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex) {
        lastPosition = animator.gameObject.transform.position;
    }

	// OnStateUpdate is called on each Update frame between OnStateEnter and OnStateExit callbacks
	override public void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex) {
        var dir = SetRotation(animator.gameObject) + 1;

        animator.SetInteger("rotation", dir);

        /* if (Input.GetMouseButton(0))
             attack();*/
        var sk = Movement(animator.gameObject);

        animator.SetInteger("direction", sk);
        if (animator.gameObject.transform.parent.parent.GetChild(5).gameObject.activeSelf)
        {
            animator.SetBool("move", false);
            animator.SetBool("idle", false);
            animator.SetBool("attack", true);
        }
        else if (sk == 0)
        {
            animator.SetBool("move", false);
            animator.SetBool("idle", true);
            animator.SetBool("attack", false);
        }
        else
        {
            animator.SetBool("move", true);
            animator.SetBool("idle", false);
            animator.SetBool("attack", false);
        }
            
    }

    private int SetRotation(GameObject obj)
    {
        var rotation = obj.transform.parent.parent.GetChild(6).rotation.eulerAngles.y;
        
        if (rotation == 0)
            return 7;
        return (int) rotation / 45;
    }

    private int Movement(GameObject obj)
    {
        var direction = obj.GetComponentInParent<PlayerController>().GetDirection();
        Vector3 forward = obj.transform.parent.parent.forward;
        float rotation = Mathf.Round(Vector3.SignedAngle(direction, forward, Vector3.up));
        if (direction.x == 0 && direction.z == 0)
            return 0;
        if (23 >= Mathf.Abs(rotation))
            return 3;
        if (23 < Mathf.Abs(rotation) && 68 >= Mathf.Abs(rotation))
        {
            if (rotation > 0)
                return 2;
            else
                return 4;
        }
        if (68 < Mathf.Abs(rotation) && 113 >= Mathf.Abs(rotation))
        {
            if (rotation > 0)
                return 1;
            else
                return 5;
        }
        if (113 < Mathf.Abs(rotation) && 153 >= Mathf.Abs(rotation))
        {
            if (rotation > 0)
                return 8;
            else
                return 6;
        }
        if (153 < Mathf.Abs(rotation))
            return 7;
        return 0;
    }

    // OnStateExit is called when a transition ends and the state machine finishes evaluating this state
    //override public void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex) {
    //
    //}

    // OnStateMove is called right after Animator.OnAnimatorMove(). Code that processes and affects root motion should be implemented here
    //override public void OnStateMove(Animator animator, AnimatorStateInfo stateInfo, int layerIndex) {
    //
    //}

    // OnStateIK is called right after Animator.OnAnimatorIK(). Code that sets up animation IK (inverse kinematics) should be implemented here.
    //override public void OnStateIK(Animator animator, AnimatorStateInfo stateInfo, int layerIndex) {
    //
    //}
}
