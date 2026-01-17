using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FSMState
{
    public Player player;
    public FSMStateMachine stateMachine;

    public FSMState(Player player , FSMStateMachine stateMachine ) 
    {
        this.player = player;
        this.stateMachine = stateMachine;
    }

    /// <summary>
    /// 进入状态
    /// </summary>
    public virtual void OnEnter() {}

    /// <summary>
    /// 状态更新
    /// </summary>
    public virtual void OnUpdate() {}

    /// <summary>
    /// 状态物理更新
    /// </summary>
    public virtual void OnFixedUpdate() {}

    /// <summary>
    /// 状态退出
    /// </summary>
    public virtual void OnExit() {}


}
