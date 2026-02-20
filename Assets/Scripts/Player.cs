using System.Collections;
using System.Collections.Generic;
using UnityEngine;



[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(Animator))]
public class Player : MonoBehaviour
{

#region 状态

    public FSMStateMachine stateMachine;
    public GroundState groundState;
    public IdleState   idleState;
    public MoveState   moveState;
    public JumpState   jumpState;
    public AttackState attackState;

#endregion

#region 组件

    public Rigidbody2D PRB2D { get; set; }
    public Animator PAnimator { get; set; }

#endregion

#region 角色属性（共有变量）
    public float moveMaxSpeed = 2.0f; //移动的最大速率
    public float moveAcceleration = 10.0f; //移动的加速度（注意，这里是标量）
    public int   inputDirection = 0; // 输入：水平轴的数值
    public bool  isOnGround = true; //判断是否能进行跳
    public float jumpSpeed = 4; //起跳的速度
    public float JumpBufferDuration = 0.5f; //跳跃缓冲倒计时
    public float coyoteTimeDuration = 0.5f; //土狼时间
    public float miniJumpTime = 0.5f; //最小跳跃持续时间
    public bool isJumpInputHold = false; //是否持续按住了跳跃按键
    public float defaultGravity = 1; //角色默认的重力
    public float risingGravity = 1f; //角色上升的重力
    public float apexGravity = 0.2f; //角色达到最高点的重力
    public float fallingGravity = 4f; //角色下降的重力
    public float gravityChangeSpeed = 2f; //重力变化量
    public float attackBufferDuration = 0.8f; //攻击缓冲时间
    public int comboIndex = 0; //连击索引 0表示没有连击 1表示一段攻击 2表示二段攻击
    public GameObject groundCheckObject; //用于做地面检测的物体
    public LayerMask groundLayer;
    public float groundCheckRadius; //检测球的半径范围
#endregion

#region 其他字段（私有变量）

    private bool isPendingAttackInput; //挂起攻击输入的判定（可理解为记账）
    private bool isPendingJumpInput; //挂起跳跃输入的判定
    private int currentDirection = 1; //当前角色的面朝向
    private float lastGroundTime = -999f; //记录上一次落地时间
    private float lastJumpInputTime = -999f; //记录上一次按下跳跃的时间
    private float lastAttackInputTime = -999f; //记录上一次按下攻击的时间

#endregion



    private void Awake()
    {
        PRB2D     = GetComponent<Rigidbody2D>();
        PAnimator = GetComponent<Animator>();

        stateMachine = new FSMStateMachine();

        idleState    = new IdleState  (this, stateMachine);
        moveState    = new MoveState  (this, stateMachine);
        jumpState    = new JumpState  (this, stateMachine);
        attackState  = new AttackState(this, stateMachine);
    }



    void Start() 
    { 
        stateMachine.Initalize(idleState); 
    }



    void Update()
    {
        stateMachine.CurrentState.OnUpdate();

        inputDirection = (int)Input.GetAxisRaw("Horizontal");

        if (Input.GetKeyDown(KeyCode.J))        //跳跃按下
        {
            isJumpInputHold = true;
            lastJumpInputTime = Time.time;
            isPendingJumpInput = true;         //对跳跃输入进行挂起
        }

        if (Input.GetKey(KeyCode.J))           //跳跃持续按下
        {
            isJumpInputHold = true;

        } 
        else if (Input.GetKeyUp(KeyCode.J))    //跳跃松开
        {
            isJumpInputHold = false; 
        }

        if (Input.GetKeyDown(KeyCode.I))       //攻击
        {
            lastAttackInputTime = Time.time;
            isPendingAttackInput = true;       //对攻击输入进行“记账”也就是挂起
        }

    }



    private void FixedUpdate()
    {
        isOnGround = Physics2D.OverlapCircle(groundCheckObject.transform.position, groundCheckRadius, groundLayer); //判断物体是不是在地面上

        if (isOnGround) { lastGroundTime = Time.time; } //记录上一次落地的时间，用于做土狼时间的时间判断依据

        stateMachine.CurrentState.OnFixedUpdate();
    }



    /// <summary>   
    /// 判断：角色是否能翻转
    /// </summary>   
    /// <returns></returns> 
    public bool OnIsCanFlip() => inputDirection != 0 && inputDirection != currentDirection;



    /// <summary>
    /// 翻转角色朝向
    /// </summary>
    public void OnFlip()         
    {
        currentDirection *= -1;
        transform.Rotate(new Vector3(0, 180, 0));
    }



    /// <summary>
    /// 控制移动
    /// </summary>
    /// <param name="inputX"></param>
    public void HandleMove(float inputX)
    {
        float targetSpeed = inputX * moveMaxSpeed;
        float currentSpeed = Mathf.MoveTowards(PRB2D.linearVelocity.x, targetSpeed, moveAcceleration * Time.fixedDeltaTime);
        PRB2D.linearVelocity = new Vector2(currentSpeed, PRB2D.linearVelocity.y);
    }



    /// <summary>
    /// 控制空中移动
    /// </summary>
    /// <param name="inputX"></param>
    public void HandleInAirMove(float inputX)
    {
        HandleMove(inputX);//偷懒的做法，因为我暂时决定玩家可以自由的控制角色在空中时的水平方向的速度，所以直接取水平移动的方法~~哈哈哈~~
    }



    /// <summary>
    /// 正在上升
    /// </summary>
    /// <returns></returns>
    public bool Rising() => PRB2D.linearVelocity.y > 0.1f ;
    
    

    /// <summary>
    /// 在最高点
    /// </summary>
    /// <returns></returns>
    public bool Apex() => Mathf.Abs(PRB2D.linearVelocity.y) < 0.1f ;
    
    

    /// <summary>
    /// 正在下降
    /// </summary>
    /// <returns></returns>
    public bool Falling() => PRB2D.linearVelocity.y < 0f ;


    
    /// <summary>
    /// 获取目标重力
    /// </summary>
    /// <returns></returns>
    public float GetTargetGravity() 
    {
        if (Rising())       { return risingGravity;  }
        else if (Apex())    { return apexGravity;    }
        else if (Falling()) { return fallingGravity; }

        return defaultGravity;
    }



    /// <summary>
    /// 应用重力
    /// </summary>
    /// <param name="targetGravity"></param>
    public void ApplyGravity(float targetGravity) 
    {
        PRB2D.gravityScale = Mathf.MoveTowards(PRB2D.gravityScale, targetGravity, gravityChangeSpeed * Time.fixedDeltaTime);
    }



    /// <summary>
    /// 根据跳跃输入的时间判断角色是否可以跳跃（也就是跳跃缓冲）
    /// </summary>
    /// <returns></returns>
    public bool IsCanJump() =>Time.time - lastJumpInputTime <= JumpBufferDuration;



    /// <summary>
    /// 根据角色离开地面的时间判断角色是否在土狼时间内
    /// </summary>
    /// <returns></returns>
    public bool IsCoyoteTime() =>Time.time - lastGroundTime <= coyoteTimeDuration;



    /// <summary>
    /// 角色跳跃
    /// </summary>
    public void Jump()
    {
        PRB2D.linearVelocity = new Vector2(PRB2D.linearVelocity.x, jumpSpeed);
    }



    /// <summary>
    /// 控制可变化的跳跃
    /// </summary>
    /// <param name="jumpEnterTime">进入跳跃状态的时间</param>
    public void OnHandleVeriableJump(float jumpEnterTime) 
    {
        if (Time.time - jumpEnterTime > miniJumpTime)
        {
            if (Rising() && !isJumpInputHold)
            {
                float targetVelocityV = Mathf.MoveTowards(PRB2D.linearVelocity.y , 0 , 80 *Time.fixedDeltaTime); //先写一个魔法数字，让这里的操作手感稍微好一点

                PRB2D.linearVelocity = new Vector2(PRB2D.linearVelocity.x, targetVelocityV);
            }
        }
    }



    /// <summary>
    /// 跳跃挂起讯号
    /// </summary>
    /// <returns></returns>
    public bool OnIsPendingJumpInput() => isPendingJumpInput;       



    /// <summary>
    /// 消费跳跃挂起
    /// </summary>
    public void OnJumpInputConsume() 
    {
        isPendingJumpInput = false; //消费掉跳跃输入的挂起行为
    }



    /// <summary>
    /// 攻击请求
    /// </summary>
    /// <returns></returns>
    public bool OnIsAttackRequest() 
    {
        if (!isPendingAttackInput) //如果没有被记账 返回False
        { 
            return false; 
        }

        if (Time.time - lastAttackInputTime > attackBufferDuration) //如果上一次按下攻击按键的时间超过预设的攻击缓冲时间，则超时，返回Fasle
        {
            isPendingAttackInput = false; 
            return false ; 
        }

        return true; //除去以上两种情况之外，就返回True
    }



    /// <summary>
    /// 攻击消费，证明已经将攻击讯号使用了的方法
    /// </summary>
    public void OnAttackInputConsume() => isPendingAttackInput = false;



    /// <summary>
    /// 播放动画
    /// </summary>
    /// <param name="animName"></param>
    /// <param name="animLayer"></param>
    public void PlayAnimation(int animName, int animLayer = 0) 
    {
        PAnimator.Play(animName , animLayer);
        PAnimator.speed = 1;
    }



    /// <summary>
    /// 判断：获取当前动画的归一化时间，既可以判断动画名字是否匹配，也可以获取当前动画的归一化时间
    /// </summary>
    /// <param name="animName">动画的名字</param>
    /// <param name="normalizedTime">out参数，返回当前动画的归一化时间</param>
    /// <param name="animLayer">动画层级，给一个缺省值，表示基础层级，如果没有特殊层级变化，可以不写</param>
    /// <returns></returns>
    public bool TryGetNormalizedTimeOfAnimation(int animName, out float normalizedTime , int animLayer = 0) 
    {
        AnimatorStateInfo info = PAnimator.GetCurrentAnimatorStateInfo(animLayer);

        if (info.shortNameHash == animName) 
        {
            normalizedTime = info.normalizedTime;
            return true;
        }

        normalizedTime = 0;
        return false;
    }



    /// <summary>
    /// 检查当前动画是否播放完成    
    /// </summary>
    /// <param name="animName"></param>
    /// <param name="animLayer"></param>
    /// <returns></returns>
    public bool IsAnimationComplete(int animName, int animLayer = 0) 
    {
        if (PAnimator.IsInTransition(animLayer)) return false;
        return TryGetNormalizedTimeOfAnimation(animName, out var t , animLayer) && t >= 0.98f;    
    }









}