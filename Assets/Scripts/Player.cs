using System.Collections;
using System.Collections.Generic;
using UnityEngine;



public class Player : MonoBehaviour
{
    public FSMStateMachine stateMachine;
    public GroundState groundState;
    public IdleState idleState;
    public MoveState moveState;
    public JumpState jumpState;
    public AttackState attackState;

    public Rigidbody2D RB2D { get; set; }
    public Animator animator { get; set; }

    public float moveMaxSpeed = 2.0f; //移动的最大速率
    public float moveAcceleration = 10.0f; //移动的加速度（注意，这里是标量）
    public  int  inputDirection = 0; // 输入：水平轴的数值

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

    private bool isPendingAttackInput; //挂起攻击输入的判定（可理解为记账）
    private int currentDirection = 1; //当前角色的面朝向
    private float lastGroundTime = -999f;  //记录上一次落地时间
    private float lastJumpInputTime = -999f;  //记录上一次按下跳跃的时间
    private float lastAttackInputTime = -999f;//记录上一次按下攻击的时间



    private void Awake()
    {
        RB2D = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();

        stateMachine = new FSMStateMachine();
        idleState = new IdleState(this, stateMachine);
        moveState = new MoveState(this, stateMachine);
        jumpState = new JumpState(this, stateMachine);
        attackState = new AttackState(this, stateMachine);
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

        if (isOnGround) { lastGroundTime = Time.time; }

        stateMachine.CurrentState.OnFixedUpdate();
    }



    /// <summary>   
    /// //是否能翻转
    /// <summary>   
    /// <returns></returns> 
    public bool OnIsCanFlip() => (inputDirection != 0 && inputDirection != currentDirection);



    public void OnFlip()         
    {
        currentDirection = currentDirection * -1;
        this.transform.Rotate(new Vector3(0, 180, 0));
    }



    public void OnJump()
    {
        RB2D.linearVelocity = new Vector2(RB2D.linearVelocity.x, jumpSpeed);
    }



    public bool OnIsCanJump() =>Time.time - lastJumpInputTime <= JumpBufferDuration;



    public bool OnIsCoyoteTime() =>Time.time - lastGroundTime <= coyoteTimeDuration;



    /// <summary>
    /// 攻击请求
    /// </summary>
    /// <returns></returns>
    public bool OnIsAttackRequest() 
    {
        if (!isPendingAttackInput) { return false; }//如果没有被记账 返回False

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
    public void OnAttackInputConsume() 
    {
        isPendingAttackInput = false; //消费被记账的挂起行为
    }



    /// <summary>
    /// 检查当前动画是否播放完成    
    /// </summary>
    /// <param name="actionName"></param>
    /// <param name="targetActionLayer"></param>
    /// <returns></returns>
    public bool IsCurrentActionFinished(int actionName, int targetActionLayer) 
    {
        if (animator.IsInTransition(targetActionLayer)) return false;

        AnimatorStateInfo info = animator.GetCurrentAnimatorStateInfo(targetActionLayer);

        return info.shortNameHash == actionName && info.normalizedTime >= 0.98f;
    }



    /// <summary>
    /// 播放动画
    /// </summary>
    /// <param name="actionName"></param>
    /// <param name="targetActionLayer"></param>
    public void OnPlayeAnimation(int actionName, int targetActionLayer) 
    {
        animator.Play(actionName , targetActionLayer);
        animator.speed = 1;
    }



    /// <summary>
    /// 是否在连击窗口内
    /// </summary>
    /// <param name="startTime">窗口开始时间</param>
    /// <param name="endTime">窗口结束时间</param>
    /// <param name="actionName">攻击动画的名字</param>
    /// <param name="targetActionLayer">攻击动画所在动画层级</param>
    /// <returns></returns>
    public bool OnIsComboWindow(int actionName , int targetActionLayer , float startTime , float endTime)
    {
        AnimatorStateInfo info = animator.GetCurrentAnimatorStateInfo(targetActionLayer);  
        return info.shortNameHash== actionName && info.normalizedTime >= startTime && info.normalizedTime <= endTime;
    }



    /// <summary>
    /// 正在上升
    /// </summary>
    /// <returns></returns>
    public bool OnIsRising() => RB2D.linearVelocity.y > 0.1f ;
    
    

    /// <summary>
    /// 在最高点
    /// </summary>
    /// <returns></returns>
    public bool OnIsApex() => Mathf.Abs(RB2D.linearVelocity.y) < 0.1f ;
    
    

    /// <summary>
    /// 正在下降
    /// </summary>
    /// <returns></returns>
    public bool OnIsFalling() => RB2D.linearVelocity.y < 0f ;


    
    /// <summary>
    /// 获取目标重力
    /// </summary>
    /// <returns></returns>
    public float OnGetTargetGravity() 
    {
        if (OnIsRising())       { return risingGravity;       }
        else if (OnIsApex())    { return apexGravity;         }
        else if (OnIsFalling()) { return fallingGravity;      }

        return defaultGravity;
    }



    /// <summary>
    /// 应用重力
    /// </summary>
    /// <param name="targetGravity"></param>
    public void OnApplyGravity(float targetGravity) 
    {
        RB2D.gravityScale = Mathf.MoveTowards(RB2D.gravityScale, targetGravity, gravityChangeSpeed * Time.fixedDeltaTime);
    }



    /// <summary>
    /// 控制移动
    /// </summary>
    /// <param name="inputX"></param>
    public void OnHandleMove(float inputX)
    {
        float targetSpeed = inputX * moveMaxSpeed;
        float currentSpeed = Mathf.MoveTowards(RB2D.linearVelocity.x, targetSpeed, moveAcceleration * Time.fixedDeltaTime);
        RB2D.linearVelocity = new Vector2(currentSpeed, RB2D.linearVelocity.y);
    }



    /// <summary>
    /// 控制空中移动
    /// </summary>
    /// <param name="inputX"></param>
    public void OnHandleInAirMove(float inputX)
    {
        OnHandleMove(inputX);//偷懒的做法，因为我暂时决定玩家可以自由的控制角色在空中时的水平方向的速度，所以直接取水平移动的方法~~哈哈哈~~
    }



    /// <summary>
    /// 控制可变化的跳跃
    /// </summary>
    /// <param name="jumpEnterTime"></param>
    public void OnHandleVeriableJump(float jumpEnterTime) 
    {
        if (Time.time - jumpEnterTime > miniJumpTime)
        {
            if (OnIsRising() && !isJumpInputHold)
            {
                float targetVelocityVertical = Mathf.MoveTowards(RB2D.linearVelocity.y , 0 , 80 *Time.fixedDeltaTime); //先写一个魔法数字，让这里的操作手感稍微好一点

                RB2D.linearVelocity = new Vector2(RB2D.linearVelocity.x, targetVelocityVertical);
            }
        }
    }




}