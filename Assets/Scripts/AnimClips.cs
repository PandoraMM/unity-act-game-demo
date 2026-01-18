using System.Collections;
using System.Collections.Generic;
using UnityEngine;



public static class AnimClips
{
    public static readonly int baseLayer = 0; //动画层级-基础层级
    public static readonly int actionIdle = Animator.StringToHash("idle");     //动画片段-待机
    public static readonly int actionAttack1 = Animator.StringToHash("attack1"); //动画片段-攻击一段
    public static readonly int actionAttack2 = Animator.StringToHash("attack2"); //动画片段-攻击二段

}