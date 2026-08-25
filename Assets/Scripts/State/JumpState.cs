using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class JumpState : FSMState
{

    private JumpPhase currentJumpPhase; //当前跳跃阶段
    private float jumpEnterTime; //进入跳跃状态的时间
    private const float minAirTime = 0.05f; //最小空中时间，防止连续跳跃状态切换
    private const float jumpStartPresentationDuration = 0.1f; //跳跃预备阶段的持续时间，确保动画播放完整
    public JumpState(Player player, FSMStateMachine stateMachine) : base(player, stateMachine){}



    public override void OnEnter()
    {
        base.OnEnter();

        jumpEnterTime = Time.time;
        currentJumpPhase = JumpPhase.jumpStart;
        player.Jump();
        player.PlayAnimation(AnimClips.actionJumpStart, AnimClips.baseLayer);
              player.HandleInAirMove(player.inputDirection);
    }



    public override void OnUpdate()
    {
        base.OnUpdate();

        if(player.IsInHitStop())return; //如果在击中停顿中则不执行跳跃状态逻辑

        //1.已落地的表现中：等待Land动画结束。
        if(currentJumpPhase == JumpPhase.Landing)
        {
            if(player.IsAnimationComplete(AnimClips.actionLanding, AnimClips.baseLayer))
            {
                // 动画播放完成，可以切换状态
                if (player.inputDirection == 0)
                {
                    stateMachine.OnChangeState(player.idleState);
                }
                else
                {
                    stateMachine.OnChangeState(player.moveState);
                }
            }

            return; //如果当前阶段是落地阶段，则不执行后续逻辑，等待动画播放完成
        }

        //2.刚刚起跳时，为了避免被认为是落地，需要等待一段时间
        if (Time.time - jumpEnterTime < minAirTime) return;

        //3.第一次落地确认：进入Landing，而不是直接切换Idle/Move状态
        if (player.isOnGround)
        {
            currentJumpPhase = JumpPhase.Landing;
            player.PlayAnimation(AnimClips.actionLanding, AnimClips.baseLayer);

            return;
        }
    }



    public override void OnFixedUpdate()
    {
        base.OnFixedUpdate();

        if(player.IsInHitStop())return; //如果在击中停顿中则不执行攻击位移

        if(currentJumpPhase == JumpPhase.Landing)
        {
            player.PRB2D.gravityScale = player.defaultGravity; //落地阶段直接将速度归零，防止落地时的残余速度影响后续移动
            player.HandleMove(0); //落地阶段不允许移动，防止落地时的残余速度影响后续移动
            
            return;
        }

        float targetGravity = player.GetTargetGravity();
        player.ApplyGravity(targetGravity);
        //player.HandleInAirMove(player.inputDirection);

        UpdateAirPhase();
    }



    public override void OnExit()
    {
        base.OnExit();
        
        player.PRB2D.gravityScale = player.defaultGravity;
    }



    /// <summary>
    /// 更新跳跃阶段
    /// </summary>
    public void UpdateAirPhase()
    {
        //对于跳跃预备阶段的判断，如果当前阶段就是跳跃预备 并且 在规定的预设时间内 直接返回，不切换阶段，这样就能保证跳跃预备阶段的动画能够完整播放
        if(currentJumpPhase == JumpPhase.jumpStart && Time.time - jumpEnterTime < jumpStartPresentationDuration) return;

        JumpPhase nextPhase = player.Rising() ? JumpPhase.jumpRising : 
                              player.Apex() ? JumpPhase.jumpApex : 
                              JumpPhase.jumpFalling;
        
        if(currentJumpPhase == nextPhase) return; //如果当前阶段和下一阶段相同，则不切换阶段，让动画继续播放，防止出现刷新问题导致动画卡住
        currentJumpPhase = nextPhase; //更新当前阶段，这样才能保证下一次进入这个方法时，能够正确判断当前阶段和下一阶段是否相同
        player.PlayAnimation(GetAnimationOf(currentJumpPhase));
    }



    /// <summary>
    /// 根据跳跃阶段获取对应的动画片段
    /// </summary>
    /// <param name="phase"></param>
    /// <returns></returns>
    public int GetAnimationOf(JumpPhase phase)
    {
        switch (phase)
        {
            case JumpPhase.jumpStart:
                return AnimClips.actionJumpStart;
            case JumpPhase.jumpRising:
                return AnimClips.actionJumpRising;
            case JumpPhase.jumpApex:
                return AnimClips.actionJumpApex;
            case JumpPhase.jumpFalling:
                return AnimClips.actionJumpFalling;
            default:
                return AnimClips.actionJumpRising;
        }
    } 


}
