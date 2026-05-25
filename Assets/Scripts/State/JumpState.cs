using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class JumpState : FSMState
{

    private float jumpEnterTime; //��¼������Ծ״̬��ʱ��
    private const float minAirTime = 0.05f; //��С����ʱ�䣬��ֹ�����ⶶ������״̬Ƶ���л�



    public JumpState(Player player, FSMStateMachine stateMachine) : base(player, stateMachine){}



    public override void OnEnter()
    {
        base.OnEnter();
        jumpEnterTime = Time.time;

        player.Jump();

    }



    public override void OnUpdate()
    {
        base.OnUpdate();
        if(player.IsInHitStop())return; //如果在击中停顿中则不执行跳跃状态逻辑

        // ���ٵ�һС��ʱ�䣬��������ж��������ȴ��������£�
        if (Time.time - jumpEnterTime < minAirTime) return;

        if (player.OnIsCanFlip()) { player.OnFlip(); } //����ת��

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
        base.OnFixedUpdate();
        if(player.IsInHitStop())return; //如果在击中停顿中则不执行攻击位移

        float targetGravity = player.GetTargetGravity();
        player.ApplyGravity(targetGravity);
        player.HandleInAirMove(player.inputDirection);
        player.OnHandleVeriableJump(jumpEnterTime);


    }



    public override void OnExit()
    {
        base.OnExit();
        
        player.PRB2D.gravityScale = player.defaultGravity;
    }



}
