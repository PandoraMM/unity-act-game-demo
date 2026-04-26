using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(Animator))]
public class Enemy : MonoBehaviour
{
    public Animator EAnimator { get; set; }
    public Rigidbody2D Rigidbody { get; set; }
    private Vector2 direction = Vector2.right; // 受击时的推力方向
    private float enemyHitStopTimer = 0f; // 击中停顿的计时器
    private bool isEnemyHitStop = false; // 是否处于击中停顿状态
    public Vector2 pendingHitVelocity; // 存储即将受到的攻击的速度


    private void Awake()
    {
        EAnimator = GetComponent<Animator>();
        Rigidbody = GetComponent<Rigidbody2D>();
    }



    private void Start()
    {
        PlayAnimation(AnimClips.actionIdle, 0, 0f);
    }



    private void Update()
    {
        if (isEnemyHitStop) // 如果处于击中停顿状态
        {
            enemyHitStopTimer -= Time.deltaTime; // 减少击中停顿的计时器
            Rigidbody.linearVelocity = Vector2.zero;// 将敌人的速度设置为0，确保敌人停在原地不动

            if (enemyHitStopTimer <= 0)// 如果击中停顿的时间结束了
            {
                isEnemyHitStop = false;
                EAnimator.speed = 1;

                Rigidbody.linearVelocity = pendingHitVelocity; // 在击中停顿结束后应用存储的攻击速度，实现击退效果
                Invoke(nameof(StopKnockback), 0.1f); // 在击中停顿结束后0.1秒调用StopKnockback方法，确保敌人不会被击退过远
            }
        }

        if (!isEnemyHitStop)// 如果不处于击中停顿状态，检查当前动画状态是否是受伤动画，并且动画是否播放完了，如果是的话就切换回待机动画
        {
            var state = EAnimator.GetCurrentAnimatorStateInfo(0);

            if (state.shortNameHash == AnimClips.actionHurt && state.normalizedTime >= 1f)
            {
                PlayAnimation(AnimClips.actionIdle, 0, 0f);
            }
        }
    }



    /// <summary>
    /// 处理敌人受伤的方法，接收攻击来源的坐标、攻击的推力和敌人击中停顿的持续时间作为参数，以便计算击退方向和实现击中停顿效果
    /// </summary>
    /// <param name="hitDirection"></param>
    /// <param name="attackBackForce"></param>
    /// <param name="enemyHitStopDuration"></param>
    public void OnHurt(Vector2 hitDirection, float attackBackForce, float enemyHitStopDuration)
    {
        Debug.Log("我TM被干了！！！");

        //1.先击退：计算击退方向，敌人会朝着远离攻击来源的方向被击退，所以用敌人当前坐标减去攻击来源坐标得到一个向量，然后归一化这个向量得到方向
        direction = ((Vector2)transform.position - hitDirection).normalized;

        //2.再击中停顿：调用EnemyHitStop方法，传入敌人击中停顿的持续时间参数，让敌人进入击中停顿状态，增加打击感等
        EnemyHitStop(enemyHitStopDuration);

        //3.存储攻击的速度：将击退方向乘以攻击的推力得到一个速度向量，存储在pendingHitVelocity变量中，在击中停顿结束后再应用这个速度，实现击退效果  
        pendingHitVelocity = direction * attackBackForce;

        //4.播放受伤动画：调用PlayAnimation方法，传入受伤动画片段的哈希值、动画层级和过渡持续时间参数，播放受伤动画，并将动画速度设置为0，确保动画停在第一帧，增加击中停顿的效果
        PlayAnimation(AnimClips.actionHurt, 0, 0f);
    }



    /// <summary>
    /// 实现敌人击中停顿的方法，接收持续时间作为参数，在这个时间内敌人会进入击中停顿状态，动画速度为0，增加打击感等
    /// </summary>
    /// <param name="duration"></param>
    public void EnemyHitStop(float duration)
    {
        isEnemyHitStop = true;
        enemyHitStopTimer = duration;
        EAnimator.speed = 0;
    }



    /// <summary>
    /// 停止击退的方法，在击中停顿结束后调用，将敌人的速度设置为0，停止击退效果
    /// </summary>
    void StopKnockback()
    {
        Rigidbody.linearVelocity = Vector2.zero;
    }



    /// <summary>
    /// 播放动画的方法，接收动画片段的哈希值、动画层级和过渡持续时间作为参数，调用Animator的Play方法来播放指定的动画片段，并将动画速度设置为1，确保动画正常播放
    /// </summary>
    /// <param name="animName"></param>
    /// <param name="animLayer"></param>
    /// <param name="transitionDuration"></param>
    public void PlayAnimation(int animName, int animLayer = 0, float transitionDuration = 0f)
    {
        EAnimator.Play(animName, animLayer, transitionDuration);
        EAnimator.speed = 1;
    }



}