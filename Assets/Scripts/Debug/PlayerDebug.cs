using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;

public class PlayerDebug : MonoBehaviour
{
    public Player player;

    public Color groundCheckNormalColor;
    public Color groundCheckHitColor;
    public Color attackHitColor = Color.blue;

    //public Vector2 debugHitCenter;
    //public float debgugHitRadius;

    private void OnDrawGizmos()
    {
        OnGroundCheckDebug();
        DrawAttackHitBoxRuntime();

    }

    private void OnGroundCheckDebug()
    {
        if (player != null) 
        {
            if (player.groundCheckObject != null)
            {
                if (player.isOnGround)
                {
                    Gizmos.color = groundCheckHitColor;
                    Gizmos.DrawWireSphere(player.groundCheckObject.transform.position, player.groundCheckRadius);
                }
                else
                {
                    Gizmos.color = groundCheckNormalColor;
                    Gizmos.DrawWireSphere(player.groundCheckObject.transform.position, player.groundCheckRadius);
                }
            }
        }


    }



    private void OnGUI()
    {
        if (player == null) return;


        GUI.skin.label.fontSize = 80;
        GUI.Label(new Rect(10, 10, 1920, 600), $"PlayerState: {player.stateMachine.CurrentState.GetType().Name + " " +Time.frameCount}");

        GUI.skin.label.fontSize = 30;
        GUI.Label(new Rect(10, 110, 1920, 600), $"AttackStage: {player.attackState.GetCurrentAttackStage()}");

        GUI.skin.label.fontSize = 30;
        GUI.Label(new Rect(10, 150, 1920, 600), $"Step Index: {player.currentStepIndex}");

        if (player.attackState != null)
        {
            var step = player.attackState.comboSteps[player.currentStepIndex];

            if (player.TryGetNormalizedTimeOfAnimation(step.animShortHashName, out var t))
            {
                GUI.skin.label.fontSize = 30;
                GUI.Label(new Rect(10, 200, 1920, 600), $"Anim Time: {t:F2}");

                GUI.skin.label.fontSize = 30;
                GUI.Label(new Rect(10, 250, 1920, 600), $"Hit Stop: {step.hitStopDuration}s ");
            }

        }

    }



    private void DrawAttackHitBoxRuntime()
    {
        if (player == null) return;
        if (player.attackState == null) return;
        var attackState = player.attackState;
        
        if (player.currentStepIndex >= attackState.comboSteps.Length) return;
        var step = attackState.comboSteps[player.currentStepIndex];

        // 拿动画时间
        if (!player.TryGetNormalizedTimeOfAnimation(step.animShortHashName, out var t)) return;

        // 👉 判断是否在攻击判定窗口
        if (t < step.hitStartTime || t > step.hitEndTime) return;

        // 👉 计算攻击中心
        Vector2 center = new Vector2(player.transform.position.x + (step.hitOffset.x * player.CurrentDirection),player.transform.position.y + step.hitOffset.y);
        Gizmos.color = attackHitColor;
        Gizmos.DrawWireSphere(center, step.hitRadius);
    }

}
