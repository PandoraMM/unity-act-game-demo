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
    None,      //无阶段
    PreAttack, //前摇阶段
    Attacking, //攻击阶段
    PostAttack,//后摇阶段
}
