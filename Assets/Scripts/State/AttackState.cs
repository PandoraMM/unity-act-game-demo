using System.Collections;
using System.Collections.Generic;
using Mono.Cecil.Cil;
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
    public bool isCanBeInterrupted;    //该段攻击是否可以被打断
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
        player.OnPlayAnimation(AnimClips.actionAttack1, AnimClips.baseLayer);

    }



    public override void OnUpdate()
    {
        base.OnUpdate();

        if(IsInComboWindow() && player.OnIsAttackRequest())//检测到处于连击输入窗口内且有攻击输入请求  
        {
            player.OnAttackInputConsume();//消费掉攻击输入
            if(player.comboIndex >= comboSteps.Length){ RestComboIndex(); }//索引边界判断
            player.comboIndex++;
            PlayComboAction();
        }

        if (player.OnIsPendingJumpInput())//攻击状态被跳跃意图打断
        {
            if(IsCanBeInterruptedBy(StateIntention.Jump, AttackStage.PostAttack) ) //只有在后摇阶段才允许被跳跃意图打断
            {
                RestComboIndex();
                player.OnJumpInputConsume(); //消费掉跳跃输入
                stateMachine.OnChangeState(player.jumpState);
            }       
        }

        if (IsComboActionFinished()) //当前连击动作播放完毕
        {
            RestComboIndex();
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

        RestComboIndex();
    }



    /// <summary>
    /// 播放连击动作
    /// </summary>
    public void PlayComboAction()
    {
        var attackStep = comboSteps[GetCurrentComboIndex()];
        player.OnPlayAnimation(attackStep.animShortHashName, attackStep.animLayer);
    }



    /// <summary>
    /// 判断：当前连击动作是否播放完毕
    /// </summary>
    /// <returns></returns>
    public bool IsComboActionFinished()
    {
        if(player.comboIndex == 0 || player.comboIndex >= comboSteps.Length) return false;  //索引边界判断，如果越界返回false
        var attackStep = comboSteps[GetCurrentComboIndex()];
        return player.IsCurrentActionFinished(attackStep.animShortHashName, attackStep.animLayer);
    }


    
    /// <summary>
    /// 更新攻击阶段：这个方法很重要，用来检测当前处于攻击的哪个阶段
    /// </summary>
    public AttackStage GetAttackStage()
    {
        if(player.comboIndex == 0 || player.comboIndex >= comboSteps.Length) {return AttackStage.None;}
        var attackStep = comboSteps[GetCurrentComboIndex()]; 
        if(!player.TryGetActionNormalizedTime(attackStep.animShortHashName, out var t, attackStep.animLayer)){return AttackStage.None;}//获取当前动画归一化时间失败，返回None

        if     (t < attackStep.preAttackEnd)   {return AttackStage.PreAttack; } //前摇阶段
        else if(t < attackStep.attackingEnd)   {return AttackStage.Attacking; } //攻击阶段
        else if(t <= attackStep.postAttackEnd) {return AttackStage.PostAttack;} //后摇阶段 

        return AttackStage.None; //都不是则返回None 
    }



    /// <summary>
    /// 是否在攻击窗口内
    /// </summary>
    /// <param name="actionName">攻击动画的名字</param>
    /// <param name="targetActionLayer">攻击动画所在动画层级</param>
    /// <param name="tempStage">攻击阶段枚举</param>
    /// <param name="endTime">窗口结束时间</param>
    /// <returns></returns>
    public bool IsInAttackWindow(int actionName , int targetActionLayer , AttackStage tempStage , float endTime)
    {
        return player.TryGetActionNormalizedTime(actionName, out var t , targetActionLayer) && t <= endTime && tempStage == GetAttackStage();
    }



    /// <summary>
    /// 判断：是否处于连击输入窗口内  
    /// </summary>
    /// <returns></returns>
    public bool IsInComboWindow()
    {
        var attackStep = comboSteps[GetCurrentComboIndex()];
        return IsInAttackWindow(attackStep.animShortHashName, attackStep.animLayer, AttackStage.PostAttack , attackStep.comboWindowEnd);
    }



    /// <summary>
    /// 攻击动作可被打断的枚举        
    /// </summary>
    /// <returns></returns>
    public StateIntention AttackInterrupted(StateIntention intentionType)
    {

        if(player.comboIndex == 0 || player.comboIndex >= comboSteps.Length) return StateIntention.None;  //索引边界判断，如果越界返回false    
        var attackStep = comboSteps[GetCurrentComboIndex()];
        if(attackStep.isCanBeInterrupted == false) return StateIntention.None;                            //数据判断，如果该段攻击不允许被打断则返回false
        return intentionType;                                                                             //目前只允许跳跃意图打断攻击
    }   



    /// <summary>
    /// 判断：当前攻击动作是否可以被打断        
    /// </summary>
    /// <returns></returns>
    public bool IsCanBeInterruptedBy(StateIntention intentionType , AttackStage requiredStage)
    {
        return GetAttackStage() == requiredStage && AttackInterrupted(intentionType) == intentionType;
    }



    /// <summary>
    /// 获取当前连击索引(消除魔法数字)    
    /// </summary>
    /// <returns></returns>
    public int GetCurrentComboIndex() => player.comboIndex-1;



    /// <summary>
    /// 重置连击索引
    /// </summary>
    public void RestComboIndex() => player.comboIndex = 0;    
    



}
