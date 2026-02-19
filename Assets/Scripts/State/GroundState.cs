using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GroundState : FSMState
{



    public GroundState(Player player , FSMStateMachine stateMachine) : base(player , stateMachine) { }



    public override void OnEnter(){Debug.Log("进入地面状态 ");}



    public override void OnExit(){}



    public override void OnUpdate()
    {
        base.OnUpdate();

        //=======================跳跃状态切换=======================
        if (player.OnIsCanJump() && player.OnIsCoyoteTime())
        {
            player.OnJumpInputConsume(); //消费掉跳跃输入
            stateMachine.OnChangeState(player.jumpState);
        }

        //=======================攻击状态切换=======================      
        if (player.OnIsAttackRequest()) 
        {
            player.OnAttackInputConsume();//消费掉攻击输入  
            stateMachine.OnChangeState(player.attackState);
        }
    }



    public override void OnFixedUpdate(){base.OnFixedUpdate();}

    
}
