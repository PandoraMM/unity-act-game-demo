using System.Collections;
using System.Collections.Generic;
using UnityEngine;



public static class AnimClips
{
    public static readonly int baseLayer = 0; //动画层级-基础层级
    public static readonly int actionIdle = Animator.StringToHash("idle");     //动画片段-待机
    public static readonly int actionAttack = Animator.StringToHash("attack"); //动画片段-攻击

}