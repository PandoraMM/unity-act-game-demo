using UnityEngine;



/// <summary>
/// 玩家行为意图的枚举
/// </summary>
public enum StateIntention
{
    None,
    Idle,
    Move,
    Jump,
    Attack,
}  



/// <summary>
/// 攻击阶段的枚举
/// </summary>
public enum AttackStage
{
    /// <summary>
    /// 无阶段：当玩家不处于攻击状态时，攻击阶段为None
    /// </summary>
    None,
    
    /// <summary>
    /// 前摇阶段：玩家进入攻击状态后，首先进入前摇阶段。在这个阶段，玩家可能会有一个短暂的准备动作或者动画，这个阶段的持续时间通常较短。
    /// </summary>
    PreAttack,

    /// <summary>
    /// 攻击阶段：前摇结束后，玩家进入攻击阶段。在这个阶段，玩家执行实际的攻击动作，并且可以对敌人造成伤害。这个阶段的持续时间取决于攻击动作的长度。
    /// </summary>
    Attacking,

    /// <summary>
    /// 后摇阶段：攻击阶段结束后，玩家进入后摇阶段。在这个阶段，玩家可能会有一个短暂的恢复动作或者动画，且目前主流的动作游戏都是在后摇阶段比较靠前的位置允许玩家进行连击切换、动作打断等
    /// </summary>
    PostAttack,
}
