using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GestureEvents : StateMachineBehaviour
{
    protected static Wand wand;

    public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        if (wand == null) wand = animator.GetComponent<Wand>();
        wand.BeginGesture();
        
        Debug.LogFormat("BEGIN animation spell => {0}", stateInfo.fullPathHash);
    }

    public override void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        wand.EndGesture();
        Debug.LogFormat("END animation spell => {0}", stateInfo.fullPathHash);
    }
}
