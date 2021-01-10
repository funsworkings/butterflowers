using System.Collections;
using System.Collections.Generic;
using butterflowersOS.Objects.Entities.Interactables;
using UnityEngine;
using UnityEngine.Animations;

public class BeaconAppearBehaviour : ComponentStateMachineBehaviour<Beacon>
{
    public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        base.OnStateEnter(animator, stateInfo, layerIndex);
        //component.AppearPS.Play();
    }

    public override void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        base.OnStateExit(animator, stateInfo, layerIndex);
        //component.AppearPS.Stop();
    }
}
