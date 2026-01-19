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
    public float comboWindowStartTime; //连击输入窗口开始时间
    public float comboWindowEndTime;   //连击输入窗口结束时间
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
            comboWindowStartTime = 0.65f, 
            comboWindowEndTime = 0.98f,
            isCanBeInterrupted = true 
            },
        new ComboStep //第二段攻击数据
        {
            animShortHashName = AnimClips.actionAttack2, 
            animLayer = AnimClips.baseLayer,
            comboWindowStartTime = 0f, 
            comboWindowEndTime = 0f, 
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

        if(OnIsInAttackActionWindow() && player.OnIsAttackRequest())//检测到处于连击输入窗口内且有攻击输入请求  
        {
            player.OnAttackInputConsume();//消费掉攻击输入
            if(player.comboIndex >= comboSteps.Length){ OnRestComboIndex(); }//索引边界判断
            player.comboIndex++;
            OnPlayComboAction();
        }

        if (player.OnIsPendingJumpInput())//攻击状态被跳跃意图打断
        {
            if(OnIsCanBeInterruptedBy(FSMStateIntention.Jump))
            {
                OnRestComboIndex();
                player.OnJumpInputConsume(); //消费掉跳跃输入
                stateMachine.OnChangeState(player.jumpState);
            }       
        }

        if (OnIsComboActionFinished()) 
        {
            OnRestComboIndex();
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
    }



    /// <summary>
    /// 播放连击动作
    /// </summary>
    public void OnPlayComboAction()
    {
        var attackStep = comboSteps[OnGetCurrentComboIndex()];
        player.OnPlayAnimation(attackStep.animShortHashName, attackStep.animLayer);
    }



    /// <summary>
    /// 判断：当前连击动作是否播放完毕
    /// </summary>
    /// <returns></returns>
    public bool OnIsComboActionFinished()
    {
        var attackStep = comboSteps[OnGetCurrentComboIndex()];
        return player.IsCurrentActionFinished(attackStep.animShortHashName, attackStep.animLayer);
    }



    /// <summary>
    /// 判断：是否处于连击输入窗口内  
    /// </summary>
    /// <returns></returns>
    public bool OnIsInAttackActionWindow()
    {
        var attackStep = comboSteps[OnGetCurrentComboIndex()];
        return player.OnIsInActionWindow(attackStep.animShortHashName, attackStep.animLayer, attackStep.comboWindowStartTime, attackStep.comboWindowEndTime);
    }



    /// <summary>
    /// 判断：当前攻击动作是否可以被打断        
    /// </summary>
    /// <returns></returns>
    public bool OnIsCanBeInterruptedBy(FSMStateIntention intentionType)
    {
        var attackStep = comboSteps[OnGetCurrentComboIndex()];
        if(player.comboIndex == 0 || player.comboIndex >= comboSteps.Length) return false;
        if(attackStep.isCanBeInterrupted == false)return false;
        return intentionType == FSMStateIntention.Jump;
    }   



    /// <summary>
    /// 获取当前连击索引(消除魔法数字)    
    /// </summary>
    /// <returns></returns>
    public int OnGetCurrentComboIndex() => player.comboIndex-1;



    /// <summary>
    /// 重置连击索引
    /// </summary>
    public void OnRestComboIndex() => player.comboIndex = 0;    
    



}
