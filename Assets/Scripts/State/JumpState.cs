using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class JumpState : FSMState
{

    private float jumpEnterTime;
    private const float minAirTime = 0.05f;

    public JumpState(Player player, FSMStateMachine stateMachine) : base(player, stateMachine) 
    {

    }

    public override void OnEnter()
    {
        jumpEnterTime = Time.time;

        player.OnJump();

    }

    public override void OnExit()
    {
        player.PRB2D.gravityScale = player.defaultGravity;
    }

    public override void OnUpdate()
    {
        // 至少等一小段时间，避免地面判定抖动
        if (Time.time - jumpEnterTime < minAirTime) return;

        if (player.OnIsCanFlip()) { player.OnFlip(); } //空中转身


        if (player.isOnGround)
        {
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
        player.OnHandleInAirMove(player.inputDirection);

        player.OnHandleVeriableJump(jumpEnterTime);


        float targetGravity = player.OnGetTargetGravity();
        player.OnApplyGravity(targetGravity);

    }



}
