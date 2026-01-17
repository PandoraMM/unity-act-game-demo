using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class IdleState : GroundState
{

    public IdleState(Player player, FSMStateMachine stateMachine) : base(player , stateMachine)
    {

    }

    public override void OnEnter()
    {
        base.OnEnter();
        Debug.Log("??????");

        player.OnPlayeAnimation(AnimClips.actionIdle, AnimClips.baseLayer);
    }

    public override void OnExit()
    {
        base.OnExit();
    }

    public override void OnUpdate()
    {


        base.OnUpdate();

        if (player.inputDirection != 0)
        {
            stateMachine.OnChangeState(player.moveState);

        }
    }

    public override void OnFixedUpdate()
    {
        base.OnFixedUpdate();
        player.OnHandleMove(0);
    }



}
