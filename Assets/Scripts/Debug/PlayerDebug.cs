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

    private void OnDrawGizmos()
    {
        OnGroundCheckDebug();
        OnAttackHitDebug();

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

    private void OnAttackHitDebug()
    {
        if (player == null) return;

        if (player.debugHitTimer > 0)
        {
            Gizmos.color = attackHitColor;
            Gizmos.DrawWireSphere(player.debugHitCenter, player.debugHitRadius);
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

}
