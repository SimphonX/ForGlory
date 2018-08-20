using Assets.Scripts.Units;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SoldierState : StateMachineBehaviour {

    override public void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        var dir = Movement(animator.gameObject);
        if (animator.transform.parent.GetComponent<Soldier>().idle)
            dir = Rotation(animator.transform.parent.parent.GetChild(0).gameObject)+1;
        if(dir != -1)
        {
            animator.SetInteger("rotation", dir);
            animator.SetInteger("direction", dir);
        }
        
        animator.SetBool("move", animator.transform.parent.GetComponent<Soldier>().move);
        animator.SetBool("idle", animator.transform.parent.GetComponent<Soldier>().idle);
        animator.SetBool("attack", animator.transform.parent.GetComponent<Soldier>().attack);
    }

    private int Rotation(GameObject obj)
    {
        var rotation = obj.transform.rotation.eulerAngles.y;

        if (rotation == 0)
            return 7;
        return (int)rotation / 45;
    }

    private int Movement(GameObject obj)
    {
        var direction = obj.GetComponentInParent<Soldier>().GetDirection();
        
        Vector3 forward = obj.transform.parent.forward;
        float rotation = Mathf.Round(Vector3.SignedAngle(direction, forward, Vector3.up));
        if (direction.x == 0 && direction.z == 0)
            return -1;
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
        return -1;
    }
}
