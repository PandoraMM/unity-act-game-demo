using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MoveState : GroundState
{
    public MoveState(Player player , FSMStateMachine stateMachine) : base(player , stateMachine) 
    {

    }


    public override void OnEnter()
    {
        base.OnEnter();
        Debug.Log("进入移动状态");
    }


    public override void OnExit()
    {
        base.OnExit();
        Debug.Log("退出移动状态");
    }


    public override void OnUpdate()
    {

        base.OnUpdate();

        if (player.OnIsCanFlip())
        {
            player.OnFlip();
        }

        if (player.inputDirection == 0) 
        {
            stateMachine.OnChangeState(player.idleState);
        }
    }


    public override void OnFixedUpdate()
    {
        base.OnFixedUpdate();

        player.HandleMove(player.inputDirection);
    }

}
