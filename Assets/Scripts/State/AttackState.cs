using System.Collections;
using System.Collections.Generic;
using UnityEngine;



/// <summary>
/// 连击步骤结构体
/// </summary>
public struct ComboStep
{
    public int   nextStepIndex;        //下一段攻击的索引，可以用来直接跳转到下一段攻击，适用于一些特殊的连击设计，比如第三段攻击可以直接跳转到第五段攻击等，这样就不需要按照顺序一段一段来，可以增加连击的多样性和灵活性
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
        new(){
            nextStepIndex = 1, 
            animShortHashName = AnimClips.actionAttack1, 
            animLayer = AnimClips.baseLayer,
            comboWindowEnd = 0.9f,
            preAttackEnd   = 0.167f,
            attackingEnd   = 0.667f,
            postAttackEnd  = 0.98f, 
            isCanBeInterrupted = true 
        },
        new(){
            nextStepIndex = -1,   
            animShortHashName = AnimClips.actionAttack2, 
            animLayer = AnimClips.baseLayer,
            comboWindowEnd = 0f, 
            preAttackEnd   = 0.4f,
            attackingEnd   = 0.9f,
            postAttackEnd  = 0.98f, 
            isCanBeInterrupted = false 
        },
    };  

    private AttackStage currentAttackStage; //当前攻击阶段




    public AttackState(Player player , FSMStateMachine stateMachine) : base(player , stateMachine){}



    public override void OnEnter()
    {
        base.OnEnter();

        player.currentStepIndex = 0; //进入攻击状态时，连击索引设为0，表示第一段攻击       
        PlayComboAnimation();

    }



    public override void OnUpdate()
    {
        base.OnUpdate();

        currentAttackStage = GetCurrentAttackStage(); //获取当前攻击阶段然后进行缓存，后续的逻辑都使用缓存的值，避免多次调用函数带来性能损耗和获取带来的动画同步不确定性

        if(CanInputNextCombo() && player.OnIsAttackRequest())//检测到处于连击输入窗口内且有攻击输入请求  
        {
            player.OnAttackInputConsume();//消费掉攻击输入
            var step = comboSteps[player.currentStepIndex];
            if(step.nextStepIndex != -1 )
            {
                player.currentStepIndex =step.nextStepIndex;
                PlayComboAnimation();
            }
            
        }

        if (player.OnIsPendingJumpInput())//攻击状态被跳跃意图打断
        {
            if(CanBeInterruptedNow(StateIntention.Jump, AttackStage.PostAttack) ) //只有在后摇阶段才允许被跳跃意图打断
            {
                ResetComboIndex();
                player.OnJumpInputConsume(); //消费掉跳跃输入
                stateMachine.OnChangeState(player.jumpState);
            }
            else
            {
                player.OnJumpInputConsume(); //消费掉跳跃输入
            }       
        }

        if (CheckCurrentAttackAnimationOver()) //动作播放完毕
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
    /// 获取攻击阶段枚举
    /// </summary>
    public AttackStage GetCurrentAttackStage()
    {
        if(player.currentStepIndex >= comboSteps.Length) {return AttackStage.None;}
        var attackStep = comboSteps[player.currentStepIndex]; 
        if(!player.TryGetNormalizedTimeOfAnimation(attackStep.animShortHashName, out var t, attackStep.animLayer)){return AttackStage.None;}//获取当前动画归一化时间失败，返回None

        if     (t <  attackStep.preAttackEnd ) {return AttackStage.PreAttack; } //前摇阶段
        else if(t <  attackStep.attackingEnd ) {return AttackStage.Attacking; } //攻击阶段
        else if(t <= attackStep.postAttackEnd) {return AttackStage.PostAttack;} //后摇阶段 

        return AttackStage.None; //都不是则返回None 
    }



    /// <summary>
    /// 播放连击动作
    /// </summary>
    public void PlayComboAnimation()
    {
        var attackStep = comboSteps[player.currentStepIndex];
        player.PlayAnimation(attackStep.animShortHashName, attackStep.animLayer);
    }



    /// <summary>
    /// 判断：当前段攻击动画是否播放完毕
    /// </summary>
    /// <returns></returns>
    public bool CheckCurrentAttackAnimationOver()
    {
        if(player.currentStepIndex >= comboSteps.Length) return false;  //索引边界判断，如果越界返回false
        var attackStep = comboSteps[player.currentStepIndex];
        return player.IsAnimationComplete(attackStep.animShortHashName, attackStep.animLayer);
    }


    
 
    /// <summary>
    /// 通过时间检测：是否处于攻击阶段的某个时间窗口内
    /// </summary>
    /// <param name="animName">攻击动画的名字</param>
    /// <param name="animLayer">攻击动画所在动画层级</param>
    /// <param name="tempStage">攻击阶段枚举</param>
    /// <param name="endTime">窗口结束时间</param>
    /// <returns></returns>
    public bool CheckAttackStageWindowByTime(int animName , int animLayer , AttackStage tempStage , float endTime)
    {
        return player.TryGetNormalizedTimeOfAnimation(animName, out var t , animLayer) && tempStage == currentAttackStage && t <= endTime;
    }



    /// <summary>
    /// 判断：是否可以进行下一段攻击输入
    /// </summary>
    /// <returns></returns>
    public bool CanInputNextCombo()
    {
        var attackStep = comboSteps[player.currentStepIndex];
        return CheckAttackStageWindowByTime(attackStep.animShortHashName, attackStep.animLayer, AttackStage.PostAttack , attackStep.comboWindowEnd);
    }



    /// <summary>
    /// 攻击动作可被打断的枚举        
    /// </summary>
    /// <param name="intentionType">打断意图的类型</param>
    /// <returns></returns>
    public StateIntention GetAttackInterrupted(StateIntention intentionType)
    {

        if(player.currentStepIndex >= comboSteps.Length) return StateIntention.None;  //索引边界判断，如果越界返回false    
        var attackStep = comboSteps[player.currentStepIndex];
        if(attackStep.isCanBeInterrupted == false) return StateIntention.None;  //数据判断，如果该段攻击不允许被打断则返回false
        return intentionType;                                                   //都通过则返回传入的意图类型
    }   



    /// <summary>
    /// 判断：当前攻击动作是否可以被打断        
    /// </summary>
    /// <param name="intentionType">打断意图的类型</param>
    /// <param name="requiredStage">允许被打断的攻击阶段</param>
    /// <returns></returns>
    public bool CanBeInterruptedNow(StateIntention intentionType , AttackStage requiredStage)
    {
        return GetAttackInterrupted(intentionType) == intentionType && currentAttackStage == requiredStage ;
    }



    /// <summary>
    /// 重置连击索引
    /// </summary>
    public void ResetComboIndex() => player.currentStepIndex = 0;    
    



}
