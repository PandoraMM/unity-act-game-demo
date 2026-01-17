using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;

public class PlayerDebug : MonoBehaviour
{
    public Player player;

    public Color groundCheckNormalColor;
    public Color groundCheckHitColor;

    private void OnDrawGizmos()
    {
        OnGroundCheckDebug();

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

        GUI.Label(new Rect(10, 10, 1920, 600), $"State: {player.stateMachine.CurrentState.GetType().Name}");
        GUI.skin.label.fontSize = 100;

    }

}
