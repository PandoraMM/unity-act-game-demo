using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FSMStateMachine
{
    public FSMState CurrentState { get; set; } //访问修饰符限定，防止其他对象对其进行修改


    /// <summary>
    /// 初始化
    /// </summary>
    /// <param name="startState"></param>
    public void Initalize(FSMState startState) 
    {
        CurrentState = startState;
        CurrentState.OnEnter();
    }

    public void OnChangeState(FSMState newState) 
    {
        if (CurrentState == newState)
        {
            // 已经在这个状态了，不需要切换
            return;
        }

        CurrentState.OnExit();
        CurrentState = newState;
        CurrentState.OnEnter();

    }

}
