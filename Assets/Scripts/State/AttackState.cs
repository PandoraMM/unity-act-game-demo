using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AttackState : FSMState
{

    public AttackState(Player player , FSMStateMachine stateMachine) : base(player , stateMachine)
    {

    }


    public override void OnEnter()
    {
        Debug.Log("??????" + Time.frameCount);

        base.OnEnter();

        player.OnPlayeAnimation(AnimClips.actionAttack, AnimClips.baseLayer);

    }


    public override void OnUpdate()
    {
        base.OnUpdate();

        if (player.IsCurrentActionFinished(AnimClips.actionAttack, AnimClips.baseLayer)) 
        {

            if (player.OnIsCanFlip())
            {
                player.OnFlip();
            }

            if (player.inputDirection == 0)
            {
                stateMachine.OnChangeState(player.idleState);
            }
            else 
            {
                stateMachine.OnChangeState(player.moveState);
            }
        }

    }



    public override void OnFixedUpdate()
    {
        base.OnFixedUpdate();
    }



    public override void OnExit()
    {
        base.OnExit();
    }

}
