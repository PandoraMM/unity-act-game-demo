using UnityEngine;
/// <summary>
/// 动作枚举
/// </summary>

/// <summary>
/// 玩家的行为意图的枚举
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
    None,      //无阶段
    PreAttack, //前摇阶段
    Attacking, //攻击阶段
    PostAttack,//后摇阶段
}
