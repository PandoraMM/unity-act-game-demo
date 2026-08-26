using System.Collections;
using System.Collections.Generic;
using UnityEngine;



public static class AnimClips
{
    public static readonly int baseLayer = 0; //动画层级-基础层级
    public static readonly int actionIdle = Animator.StringToHash("idle");     //动画片段-待机
    public static readonly int actionAttack1 = Animator.StringToHash("attack1"); //动画片段-攻击一段
    public static readonly int actionAttack2 = Animator.StringToHash("attack2"); //动画片段-攻击二段
    public static readonly int actionHurt = Animator.StringToHash("hit"); //动画片段-受伤
    public static readonly int actionJumpStart = Animator.StringToHash("jumpStart"); //动画片段-原地跳跃预备
    public static readonly int actionJumpRising = Animator.StringToHash("jumpRising"); //动画片段-原地跳跃上升
    public static readonly int actionJumpApex = Animator.StringToHash("jumpApex"); //动画片段-原地跳跃最高点
    public static readonly int actionJumpFalling = Animator.StringToHash("jumpFalling"); //动画片段-原地跳跃下降
    public static readonly int actionFrontFlipStart = Animator.StringToHash("frontFlipStart");//动画片段-前空翻预备
    public static readonly int actionFrontFlipRising = Animator.StringToHash("frontFlipRising");//动画片段-前空翻上升
    public static readonly int actionFrontFlipApex = Animator.StringToHash("frontFlipApex");//动画片段-前空翻最高点
    public static readonly int actionFrontFlipFalling = Animator.StringToHash("frontFlipFalling");//动画片段-前空翻下降
    public static readonly int actionLanding = Animator.StringToHash("landing"); //动画片段-通用着陆

}