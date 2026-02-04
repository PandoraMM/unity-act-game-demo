using System.Collections;
using System.Collections.Generic;
using UnityEngine;



/// <summary>
/// 连击步骤结构体
/// </summary>
public struct ComboStep
{
    public int   animShortHashName;    //攻击动画名字的哈希值
    public int   animLayer;            //攻击动画所在动画层级
    public float comboWindowEnd;       //连击输入窗口结束时间
    public float preAttackEnd;         //前摇结束时间
    public float attackingEnd;         //攻击中结束时间
    public float postAttackEnd;        //后摇结束时间
    public bool  isCanBeInterrupted;   //该段攻击是否可以被打断
}



public class AttackState : FSMState
{

    //连击步骤数据数组：以结构体作为数组类型，存储数据
    public ComboStep[] comboSteps = new ComboStep[]
    {
        new ComboStep //第一段攻击数据
        { 
            animShortHashName = AnimClips.actionAttack1, 
            animLayer = AnimClips.baseLayer,
            comboWindowEnd = 0.9f,
            preAttackEnd   = 0.167f,
            attackingEnd   = 0.667f,
            postAttackEnd  = 0.98f, 
            isCanBeInterrupted = true 
            },
        new ComboStep //第二段攻击数据
        {
            animShortHashName = AnimClips.actionAttack2, 
            animLayer = AnimClips.baseLayer,
            comboWindowEnd = 0f, 
            preAttackEnd   = 0.4f,
            attackingEnd   = 0.9f,
            postAttackEnd  = 0.98f, 
            isCanBeInterrupted = false 
            },
    };  



    public AttackState(Player player , FSMStateMachine stateMachine) : base(player , stateMachine){}



    public override void OnEnter()
    {
        Debug.Log("进入攻击状态" + Time.frameCount);

        base.OnEnter();

        player.comboIndex=1; //进入攻击状态时，连击索引设为1，表示第一段攻击       
        PlayComboAnimation();

    }



    public override void OnUpdate()
    {
        base.OnUpdate();

        if(CanInputNextCombo() && player.OnIsAttackRequest())//检测到处于连击输入窗口内且有攻击输入请求  
        {
            player.OnAttackInputConsume();//消费掉攻击输入
            if(player.comboIndex > comboSteps.Length){ ResetComboIndex(); }//索引边界判断
            player.comboIndex++;
            PlayComboAnimation();
            
        }

        if (player.OnIsPendingJumpInput())//攻击状态被跳跃意图打断
        {
            if(CanBeInterruptedNow(StateIntention.Jump, AttackStage.PostAttack) ) //只有在后摇阶段才允许被跳跃意图打断
            {
                ResetComboIndex();
                player.OnJumpInputConsume(); //消费掉跳跃输入
                stateMachine.OnChangeState(player.jumpState);
            }else
            {
                player.OnJumpInputConsume(); //消费掉跳跃输入
            }       
        }

        if (CheckCurrentAttackAnimationOver()) //当前连击动作播放完毕
        {
            ResetComboIndex();
            if (player.OnIsCanFlip()) { player.OnFlip(); }

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

        ResetComboIndex();
    }

//=================================================================================================================================================================================================

    /// <summary>
    /// 播放连击动作
    /// </summary>
    public void PlayComboAnimation()
    {
        var attackStep = comboSteps[GetCurrentComboIndex()];
        player.PlayAnimation(attackStep.animShortHashName, attackStep.animLayer);
    }



    /// <summary>
    /// 判断：当前段攻击动画是否播放完毕
    /// </summary>
    /// <returns></returns>
    public bool CheckCurrentAttackAnimationOver()
    {
        if(player.comboIndex == 0 || player.comboIndex > comboSteps.Length) return false;  //索引边界判断，如果越界返回false
        var attackStep = comboSteps[GetCurrentComboIndex()];
        return player.IsAnimationComplete(attackStep.animShortHashName, attackStep.animLayer);
    }


    
    /// <summary>
    /// 获取攻击阶段枚举
    /// </summary>
    public AttackStage GetCurrentAttackStage()
    {
        if(player.comboIndex == 0 || player.comboIndex > comboSteps.Length) {return AttackStage.None;}
        var attackStep = comboSteps[GetCurrentComboIndex()]; 
        if(!player.TryGetNormalizedTimeOfAnimation(attackStep.animShortHashName, out var t, attackStep.animLayer)){return AttackStage.None;}//获取当前动画归一化时间失败，返回None

        if     (t < attackStep.preAttackEnd)   {return AttackStage.PreAttack; } //前摇阶段
        else if(t < attackStep.attackingEnd)   {return AttackStage.Attacking; } //攻击阶段
        else if(t <= attackStep.postAttackEnd) {return AttackStage.PostAttack;} //后摇阶段 

        return AttackStage.None; //都不是则返回None 
    }



    /// <summary>
    /// 通过时间检测：是否处于某个攻击阶段的时间窗口内
    /// </summary>
    /// <param name="animName">攻击动画的名字</param>
    /// <param name="animLayer">攻击动画所在动画层级</param>
    /// <param name="tempStage">攻击阶段枚举</param>
    /// <param name="endTime">窗口结束时间</param>
    /// <returns></returns>
    public bool CheckAttackStageWindowByTime(int animName , int animLayer , AttackStage tempStage , float endTime)
    {
        return player.TryGetNormalizedTimeOfAnimation(animName, out var t , animLayer) && tempStage == GetCurrentAttackStage() && t <= endTime;
    }



    /// <summary>
    /// 判断：是否可以进行下一段攻击输入
    /// </summary>
    /// <returns></returns>
    public bool CanInputNextCombo()
    {
        var attackStep = comboSteps[GetCurrentComboIndex()];
        return CheckAttackStageWindowByTime(attackStep.animShortHashName, attackStep.animLayer, AttackStage.PostAttack , attackStep.comboWindowEnd);
    }



    /// <summary>
    /// 攻击动作可被打断的枚举        
    /// </summary>
    /// <returns></returns>
    public StateIntention AttackInterrupted(StateIntention intentionType)
    {

        if(player.comboIndex == 0 || player.comboIndex > comboSteps.Length) return StateIntention.None;  //索引边界判断，如果越界返回false    
        var attackStep = comboSteps[GetCurrentComboIndex()];
        if(attackStep.isCanBeInterrupted == false) return StateIntention.None;                            //数据判断，如果该段攻击不允许被打断则返回false
        return intentionType;                                                                             //都通过则返回传入的意图类型
    }   



    /// <summary>
    /// 判断：当前攻击动作是否可以被打断        
    /// </summary>
    /// <returns></returns>
    public bool CanBeInterruptedNow(StateIntention intentionType , AttackStage requiredStage)
    {
        return AttackInterrupted(intentionType) == intentionType && GetCurrentAttackStage() == requiredStage ;
    }



    /// <summary>
    /// 获取当前连击索引(消除魔法数字)    
    /// </summary>
    /// <returns></returns>
    public int GetCurrentComboIndex() => player.comboIndex-1;



    /// <summary>
    /// 重置连击索引
    /// </summary>
    public void ResetComboIndex() => player.comboIndex = 0;    
    



}
