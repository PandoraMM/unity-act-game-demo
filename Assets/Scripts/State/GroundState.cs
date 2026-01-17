using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GroundState : FSMState
{

    public GroundState(Player player , FSMStateMachine stateMachine) : base(player , stateMachine) { }

    public override void OnEnter()
    {
        Debug.Log("µØÃæ×´Ì¬");
    }

    public override void OnExit()
    {

    }

    public override void OnUpdate()
    {
        base.OnUpdate();

        if (player.OnIsCanJump() && player.OnIsCoyoteTime())
        {

            stateMachine.OnChangeState(player.jumpState);
        }

        if (player.OnIsAttackRequest()) 
        {
            player.OnAttackInputConsume();
            stateMachine.OnChangeState(player.attackState);
        }
    }

    public override void OnFixedUpdate()
    {
        base.OnFixedUpdate();
    }
}
