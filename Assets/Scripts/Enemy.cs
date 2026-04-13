using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Enemy : MonoBehaviour
{

    public Animator EAnimator { get; set; }
    public Rigidbody2D Rigidbody { get; set; }
    public float force = 5.0f; //受击时的推力
    public Vector2 direction = Vector2.right; //受击时的推力方向



    private void Awake()
    {
        EAnimator = GetComponent<Animator>();
        Rigidbody = GetComponent<Rigidbody2D>();
    }



    public void OnHurt()
    {
        Rigidbody.linearVelocity = direction * force;
        PlayAnimation(AnimClips.actionHurt);

        Invoke(nameof(StopKnockback), 0.1f); // 关键
    }

    void StopKnockback()
    {
        Rigidbody.linearVelocity = Vector2.zero;
    } 



    public void PlayAnimation(int animName, int animLayer = 0) 
    {
        EAnimator.Play(animName , animLayer);
        EAnimator.speed = 1;
    }



}
