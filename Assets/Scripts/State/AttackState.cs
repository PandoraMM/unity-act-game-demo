using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;



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
    public float attackMoveStartTime;  //攻击位移开始时间，攻击动作中从这个时间点开始会有攻击位移的效果
    public float attackMoveEndTime;    //攻击位移结束时间，攻击动作中从这个时间点开始攻击位移的效果结束，恢复到正常移动的状态
    public float attackMoveSpeed;      //攻击位移的速度，在攻击位移的时间窗口内，角色会以这个速度进行移动，通常是一个较高的速度，用来表现攻击动作中的冲刺或者推进效果，增加攻击的打击感和流畅度
    public float attackBackForce;      //攻击的击退力道，敌人被攻击时会根据这个力道进行击退，通常是一个较大的数值，用来表现攻击的强烈程度和对敌人的影响力
    public float hitStartTime;         //攻击判定开始时间，攻击动作中从这个时间点开始会有攻击判定的效果，通常是一个较早的时间点，用来表现攻击动作中的挥出武器或者身体的一瞬间，这个时间点之后敌人如果进入攻击判定范围就会被判定为命中
    public float hitEndTime;           //攻击判定结束时间，攻击动作中从这个时间点开始攻击判定的效果结束，通常是一个较晚的时间点，用来表现攻击动作中的收回武器或者身体的一瞬间，这个时间点之后敌人如果进入攻击判定范围就不会被判定为命中
    public Vector2 hitOffset;          //攻击判定的中心，调整攻击判定的偏移位置
    public float hitRadius;            //攻击判定的半径，整攻击判定的半径范围
    public float hitStopDuration;      //主角击中停顿的持续时间，在这个时间内可以实现击中停顿效果，增加打击感等
    public float enemyHitStopDuration; //敌人被击停顿的持续时间，在这个时间内敌人会进入击中停顿状态，增加打击感等

    
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
            preAttackEnd   = 0.29f,
            attackingEnd   = 0.4f,
            postAttackEnd  = 0.98f, 
            isCanBeInterrupted = true,
            attackMoveStartTime = 0.29f,
            attackMoveEndTime = 0.4f,
            attackMoveSpeed = 3f,
            attackBackForce = 5f,
            hitStartTime = 0.3f,
            hitEndTime = 0.4f,
            hitOffset = new Vector2(1.2f, 1f),
            hitRadius = 0.7f,
            hitStopDuration = 0.05f,
            enemyHitStopDuration = 0.05f

        },
        new(){
            nextStepIndex = -1,   
            animShortHashName = AnimClips.actionAttack2, 
            animLayer = AnimClips.baseLayer,
            comboWindowEnd = 0f, 
            preAttackEnd   = 0.4f,
            attackingEnd   = 0.9f,
            postAttackEnd  = 0.98f, 
            isCanBeInterrupted = false,
            attackMoveStartTime = 0.05f,
            attackMoveEndTime = 0.25f,
            attackMoveSpeed = 5f,
            attackBackForce = 18f,
            hitStartTime = 0.2f,
            hitEndTime = 0.32f,
            hitOffset = new Vector2(1.5f, 1.2f),
            hitRadius = 0.85f,
            hitStopDuration = 0.1f,
            enemyHitStopDuration = 0.05f
        },
    };  

    private AttackStage currentAttackStage; //当前攻击阶段
    private bool hasTriggeredHitStop = false;//是否已经触发过击中停顿了，避免在攻击的有效时间内多次触发击中停顿等问题
    private HashSet<Enemy> hitEnemies = new HashSet<Enemy>(); //已经被当前攻击打过的敌人集合，用于记录已经被打过的敌人，避免在攻击的有效时间内多次触发对同一个敌人造成重复伤害等问题



    public AttackState(Player player , FSMStateMachine stateMachine) : base(player , stateMachine){}



    public override void OnEnter()
    {
        base.OnEnter();

        player.currentStepIndex = 0; //进入攻击状态时，连击索引设为0，表示第一段攻击       
        PlayComboAnimation();
        hitEnemies.Clear(); //清空已击中的敌人集合
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

            stateMachine.OnChangeState(player.idleState);
        }

        UpdateAttackDebugPreview();
    }



    public override void OnFixedUpdate()
    {
        base.OnFixedUpdate();

        TryDoAttackMove(); //尝试执行攻击位移
        TryDoHit(); //尝试执行攻击判定  

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
    


    /// <summary>
    /// 尝试执行攻击判定    
    /// </summary>
    public void TryDoHit()
    {

        if(player.currentStepIndex >= comboSteps.Length) return;  //索引边界判断，如果越界返回false
        var attackStep = comboSteps[player.currentStepIndex];
        if(player.TryGetNormalizedTimeOfAnimation(attackStep.animShortHashName, out var t , attackStep.animLayer))
        {
            if(t >= attackStep.hitStartTime && t <= attackStep.hitEndTime) //如果本帧还没有执行过攻击判定，并且当前时间在攻击判定的时间窗口内
            {
                //执行攻击判定，传入偏移和半径参数，并且把已经被打过的敌人集合传进去，供函数内部进行判断和记录
                bool didHit = player.DoAttackHitContinuous(attackStep.hitOffset, attackStep.hitRadius , attackStep.attackBackForce, attackStep.enemyHitStopDuration, hitEnemies);   
                
                if (didHit && !hasTriggeredHitStop)
                {
                    hasTriggeredHitStop = true;
                    player.DoHitStop(attackStep.hitStopDuration); //执行击中停顿，传入持续时间参数
                }
            }
        }

        //hasTriggeredHitStop = false; //如果不在攻击判定时间窗口内，则重置击中停顿触发标志，准备下一次攻击判定的触发
        hitEnemies.Clear(); //如果不在攻击判定时间窗口内，则清空已击中的敌人集合，准备下一次攻击判定的记录
    }



    /// <summary>
    /// 尝试执行攻击位移
    /// </summary>
    public void TryDoAttackMove()
    {
        if(player.currentStepIndex >= comboSteps.Length) return;  //索引边界判断，如果越界返回false
        var step = comboSteps[player.currentStepIndex];
        if(player.TryGetNormalizedTimeOfAnimation(step.animShortHashName, out var t, step.animLayer))
        {
            if(t >= step.attackMoveStartTime && t <= step.attackMoveEndTime)
            {
                player.HandleAttackMove(step.attackMoveSpeed);
            }
            else
            {
                player.HandleAttackMove(0); //攻击时的移动
            }
        }
    }



    /// <summary>
    /// 用于Debug，进行实时预览调试的方法
    /// </summary>
    void UpdateAttackDebugPreview()
    {
        if(player.currentStepIndex >= comboSteps.Length)
        {
            player.debugShowPreview = false;
            return;
        }

        var step = comboSteps[player.currentStepIndex];

        Vector2 center = new Vector2(
            player.transform.position.x + (step.hitOffset.x * player.CurrentDirection),
            player.transform.position.y + step.hitOffset.y
        );

        player.debugPreviewCenter = center;
        player.debugPreviewRadius = step.hitRadius;
        player.debugShowPreview = true;
    }




}
