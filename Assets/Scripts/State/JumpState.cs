using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class JumpState : FSMState
{

    private float jumpEnterTime; //记录进入跳跃状态的时间
    private const float minAirTime = 0.05f; //最小空中时间，防止地面检测抖动导致状态频繁切换



    public JumpState(Player player, FSMStateMachine stateMachine) : base(player, stateMachine){}



    public override void OnEnter()
    {
        jumpEnterTime = Time.time;

        player.Jump();

    }



    public override void OnUpdate()
    {
        // 至少等一小段时间，避免地面判定抖动（等待物理更新）
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
        float targetGravity = player.GetTargetGravity();
        player.ApplyGravity(targetGravity);
        player.HandleInAirMove(player.inputDirection);
        player.OnHandleVeriableJump(jumpEnterTime);


    }



    public override void OnExit()
    {
        player.PRB2D.gravityScale = player.defaultGravity;
    }



}
