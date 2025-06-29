using UnityEngine;
using System.Collections;
using Unity.VisualScripting;

public class MovementPlayerImproved : MonoBehaviour
{
    public PlayerDataWithDash Data;

    #region COMPONENTS
    public Rigidbody2D RB { get; private set; }
    #endregion

    #region STATE PARAMETERS
    //Variables control the various actions the player can perform at any time.
    //These are fields which can are public allowing for other sctipts to read them
    //but can only be privately written to.
    public bool IsFacingRight { get; private set; }
    public bool IsJumping { get; private set; }
    public bool IsDashing { get; private set; }
    public bool IsSliding { get; private set; }

    //Timers (also all fields, could be private and a method returning a bool could be used)
    public float LastOnGroundTime { get; private set; }
    public float LastOnWallTime { get; private set; }
    public float LastOnWallRightTime { get; private set; }
    public float LastOnWallLeftTime { get; private set; }

    //Jump
    private bool _isJumpCut;
    private bool _isJumpFalling;

    //Dash
    private int _dashesLeft;
    private bool _dashRefilling;
    private Vector2 _lastDashDir;
    private bool _isDashAttacking;

    //Animator
    public Animator animator { get; private set; }

    #endregion

    #region INPUT PARAMETERS
    private Vector2 _moveInput;

    public float LastPressedJumpTime { get; private set; }
    public float LastPressedDashTime { get; private set; }
    #endregion

    #region CHECK PARAMETERS
    //Set all of these up in the inspector
    [Header("Checks")]
    [SerializeField] private Transform _groundCheckPoint;
    //Size of groundCheck depends on the size of your character generally you want them slightly small than width (for ground) and height (for the wall check)
    [SerializeField] private Vector2 _groundCheckSize = new Vector2(0.49f, 0.03f);
    [Space(5)]
    [SerializeField] private Transform _frontWallCheckPoint;
    [SerializeField] private Transform _backWallCheckPoint;
    [SerializeField] private Vector2 _wallCheckSize = new Vector2(0.5f, 1f);
    #endregion

    #region LAYERS & TAGS
    [Header("Layers & Tags")]
    [SerializeField] private LayerMask _groundLayer;
    #endregion

    //Ascensores
    private PlatformMoving currentPlatform;
    private bool playerOnMovingPlatform = false;
    private Vector2 smoothDampVelocityRef;
    public bool isActivated = true;

    public bool isDead = false;

    private void Awake()
    {
        RB = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
    }

    private void Start()
    {
        SetGravityScale(Data.gravityScale);
        IsFacingRight = true;
    }

    private void Update()
    {
        if (isDead) return;
        if (isActivated == false) return;
        #region TIMERS
        LastOnGroundTime -= Time.deltaTime;
        LastOnWallTime -= Time.deltaTime;
        LastOnWallRightTime -= Time.deltaTime;
        LastOnWallLeftTime -= Time.deltaTime;

        LastPressedJumpTime -= Time.deltaTime;
        LastPressedDashTime -= Time.deltaTime;
        #endregion

        #region INPUT HANDLER
        _moveInput.x = Input.GetAxisRaw("Horizontal");
        _moveInput.y = Input.GetAxisRaw("Vertical");

        if (_moveInput.x != 0)
            CheckDirectionToFace(_moveInput.x > 0);

        if (Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.C) || Input.GetKeyDown(KeyCode.J))
        {
            OnJumpInput();
        }

        if (Input.GetKeyUp(KeyCode.Space) || Input.GetKeyUp(KeyCode.C) || Input.GetKeyUp(KeyCode.J))
        {
            OnJumpUpInput();
        }

        if (Input.GetKeyDown(KeyCode.X) || Input.GetKeyDown(KeyCode.LeftShift) || Input.GetKeyDown(KeyCode.K))
        {
            OnDashInput();
        }
        #endregion

        #region COLLISION CHECKS
        if (!IsDashing && !IsJumping)
        {
            //Ground Check
            if (Physics2D.OverlapBox(_groundCheckPoint.position, _groundCheckSize, 0, _groundLayer) && !IsJumping)
            {
                LastOnGroundTime = Data.coyoteTime;
            }

            //Right Wall Check
            if (((Physics2D.OverlapBox(_frontWallCheckPoint.position, _wallCheckSize, 0, _groundLayer) && IsFacingRight)
                    || (Physics2D.OverlapBox(_backWallCheckPoint.position, _wallCheckSize, 0, _groundLayer) && !IsFacingRight)))
                LastOnWallRightTime = Data.coyoteTime;

            //Right Wall Check
            if (((Physics2D.OverlapBox(_frontWallCheckPoint.position, _wallCheckSize, 0, _groundLayer) && !IsFacingRight)
                || (Physics2D.OverlapBox(_backWallCheckPoint.position, _wallCheckSize, 0, _groundLayer) && IsFacingRight)))
                LastOnWallLeftTime = Data.coyoteTime;


            LastOnWallTime = Mathf.Max(LastOnWallLeftTime, LastOnWallRightTime);
        }
        #endregion

        #region JUMP CHECKS
        if (IsJumping && RB.linearVelocity.y < 0)
        {
            IsJumping = false;
        }

        if (LastOnGroundTime > 0 && !IsJumping)
        {
            _isJumpCut = false;

            if (!IsJumping)
                _isJumpFalling = false;
        }

        if (!IsDashing)
        {
            //Jump
            if (CanJump() && LastPressedJumpTime > 0)
            {
                IsJumping = true;
                _isJumpCut = false;
                _isJumpFalling = false;
                Jump();
            }
        }
        #endregion

        #region DASH CHECKS
        if (CanDash() && LastPressedDashTime > 0)
        {

            Sleep(Data.dashSleepTime);


            if (_moveInput != Vector2.zero)
                _lastDashDir = _moveInput;
            else
                _lastDashDir = IsFacingRight ? Vector2.right : Vector2.left;



            IsDashing = true;
            IsJumping = false;
            _isJumpCut = false;

            StartCoroutine(nameof(StartDash), _lastDashDir);
        }
        #endregion

        #region SLIDE CHECKS
        if (CanSlide() && ((LastOnWallLeftTime > 0 && _moveInput.x < 0) || (LastOnWallRightTime > 0 && _moveInput.x > 0)))
            IsSliding = true;
        else
            IsSliding = false;
        #endregion

        #region GRAVITY
        if (!_isDashAttacking)
        {
            if (IsSliding)
            {
                SetGravityScale(0);
            }
            else if (RB.linearVelocity.y < 0 && _moveInput.y < 0)
            {
                SetGravityScale(Data.gravityScale * Data.fastFallGravityMult);
                RB.linearVelocity = new Vector2(RB.linearVelocity.x, Mathf.Max(RB.linearVelocity.y, -Data.maxFastFallSpeed));
            }
            else if (_isJumpCut)
            {
                SetGravityScale(Data.gravityScale * Data.jumpCutGravityMult);
                RB.linearVelocity = new Vector2(RB.linearVelocity.x, Mathf.Max(RB.linearVelocity.y, -Data.maxFallSpeed));
            }
            else if ((IsJumping || _isJumpFalling) && Mathf.Abs(RB.linearVelocity.y) < Data.jumpHangTimeThreshold)
            {
                SetGravityScale(Data.gravityScale * Data.jumpHangGravityMult);
            }
            else if (RB.linearVelocity.y < 0)
            {
                SetGravityScale(Data.gravityScale * Data.fallGravityMult);
                RB.linearVelocity = new Vector2(RB.linearVelocity.x, Mathf.Max(RB.linearVelocity.y, -Data.maxFallSpeed));
            }
            else
            {
                SetGravityScale(Data.gravityScale);
            }
        }
        else
        {
            SetGravityScale(0);
        }
        #endregion

        #region Animations
        UpdateAnimations();
        #endregion
    }

    private void FixedUpdate()
    {
        if (isDead)
        {
            RB.linearVelocity = Vector2.zero;
        }

        //Handle Run
        if (!IsDashing)
        {
            Run(1);
        }
        else if (_isDashAttacking)
        {
            Run(Data.dashEndRunLerp);
        }

        //Handle Slide
        if (IsSliding)
            Slide();
    }

    #region INPUT CALLBACKS
    public void OnJumpInput()
    {
        LastPressedJumpTime = Data.jumpInputBufferTime;
    }

    public void OnJumpUpInput()
    {
        if (CanJumpCut())
            _isJumpCut = true;
    }

    public void OnDashInput()
    {
        LastPressedDashTime = Data.dashInputBufferTime;
    }
    #endregion

    #region GENERAL METHODS
    public void SetGravityScale(float scale)
    {
        RB.gravityScale = scale;
    }

    private void Sleep(float duration)
    {
        StartCoroutine(nameof(PerformSleep), duration);
    }

    private IEnumerator PerformSleep(float duration)
    {
        Time.timeScale = 0;
        yield return new WaitForSecondsRealtime(duration);
        Time.timeScale = 1;
    }
    #endregion

    //MOVEMENT METHODS
    #region RUN METHODS
    private void Run(float lerpAmount)
    {
        if (MovingPlatformAndMoving() == false)
        {
            float targetSpeed = _moveInput.x * Data.runMaxSpeed;
            targetSpeed = Mathf.Lerp(RB.linearVelocity.x, targetSpeed, lerpAmount);

            #region Calculate AccelRate
            float accelRate;


            if (LastOnGroundTime > 0)
                accelRate = (Mathf.Abs(targetSpeed) > 0.01f) ? Data.runAccelAmount : Data.runDeccelAmount;
            else
                accelRate = (Mathf.Abs(targetSpeed) > 0.01f) ? Data.runAccelAmount * Data.accelInAir : Data.runDeccelAmount * Data.deccelInAir;
            #endregion

            #region Add Bonus Jump Apex Acceleration
            if ((IsJumping || _isJumpFalling) && Mathf.Abs(RB.linearVelocity.y) < Data.jumpHangTimeThreshold)
            {
                accelRate *= Data.jumpHangAccelerationMult;
                targetSpeed *= Data.jumpHangMaxSpeedMult;
            }
            #endregion

            #region Conserve Momentum
            if (Data.doConserveMomentum && Mathf.Abs(RB.linearVelocity.x) > Mathf.Abs(targetSpeed) && Mathf.Sign(RB.linearVelocity.x) == Mathf.Sign(targetSpeed) && Mathf.Abs(targetSpeed) > 0.01f && LastOnGroundTime < 0)
            {

                accelRate = 0;
            }
            #endregion
            float speedDif = targetSpeed - RB.linearVelocity.x;


            float movement = speedDif * accelRate;


            RB.AddForce(movement * Vector2.right, ForceMode2D.Force);
        }


    }

    private void Turn()
    {
        Vector3 scale = transform.localScale;
        scale.x *= -1;
        transform.localScale = scale;

        IsFacingRight = !IsFacingRight;
    }
    #endregion

    #region JUMP METHODS
    private void Jump()
    {
        LastPressedJumpTime = 0;
        LastOnGroundTime = 0;

        #region Perform Jump
        float force = Data.jumpForce;
        if (RB.linearVelocity.y < 0)
            force -= RB.linearVelocity.y;

        RB.AddForce(Vector2.up * force, ForceMode2D.Impulse);
        #endregion
    }
    #endregion

    #region DASH METHODS
    //Dash Coroutine
    private IEnumerator StartDash(Vector2 dir)
    {

        LastOnGroundTime = 0;
        LastPressedDashTime = 0;

        float startTime = Time.time;

        _dashesLeft--;
        _isDashAttacking = true;

        SetGravityScale(0);

        while (Time.time - startTime <= Data.dashAttackTime)
        {
            RB.linearVelocity = dir.normalized * Data.dashSpeed;
            yield return null;
        }

        startTime = Time.time;

        _isDashAttacking = false;

        SetGravityScale(Data.gravityScale);
        RB.linearVelocity = Data.dashEndSpeed * dir.normalized;

        while (Time.time - startTime <= Data.dashEndTime)
        {
            yield return null;
        }

        //Dash over
        IsDashing = false;
    }

    private IEnumerator RefillDash(int amount)
    {
        _dashRefilling = true;
        yield return new WaitForSeconds(Data.dashRefillTime);
        _dashRefilling = false;
        _dashesLeft = Mathf.Min(Data.dashAmount, _dashesLeft + 1);
    }
    #endregion

    #region OTHER MOVEMENT METHODS
    private void Slide()
    {
        float speedDif = Data.slideSpeed - RB.linearVelocity.y;
        float movement = speedDif * Data.slideAccel;
        movement = Mathf.Clamp(movement, -Mathf.Abs(speedDif) * (1 / Time.fixedDeltaTime), Mathf.Abs(speedDif) * (1 / Time.fixedDeltaTime));

        RB.AddForce(movement * Vector2.up);
    }
    #endregion

    #region  ASCENSORES

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Moving Platform"))
        {
            PlatformMoving platform = collision.gameObject.GetComponent<PlatformMoving>();
            {
                currentPlatform = platform;
                SetPlayerOnMovingPlatform(true);
                SetCurrentMovingPlatform(platform);
            }
            if (platform != null)
            {
                if (currentPlatform != platform)
                {
                    currentPlatform = platform;
                    SetCurrentMovingPlatform(platform);
                }
            }
        }
    }
    private void OnCollisionExit2D(Collision2D collision)
    {
        if (currentPlatform != null && collision.gameObject.CompareTag("Moving Platform"))
        {
            SetPlayerOnMovingPlatform(false);
            SetCurrentMovingPlatform(null);

        }
    }
    public void SetPlayerOnMovingPlatform(bool isOnPlatform)
    {
        playerOnMovingPlatform = isOnPlatform;
    }
    public void SetCurrentMovingPlatform(PlatformMoving platform)
    {
        currentPlatform = platform;
    }
    private bool MovingPlatformAndMoving()
    {
        return playerOnMovingPlatform && currentPlatform != null && currentPlatform.CurrentVelocity.magnitude > 0.01f;
    }
    #endregion

    #region CHECK METHODS
    public void CheckDirectionToFace(bool isMovingRight)
    {
        if (isMovingRight != IsFacingRight)
            Turn();
    }

    private bool CanJump()
    {

        return LastOnGroundTime > 0 && !IsJumping && !MovingPlatformAndMoving();
    }

    private bool CanJumpCut()
    {
        return IsJumping && RB.linearVelocity.y > 0 && !MovingPlatformAndMoving();
    }

    private bool CanDash()
    {
        if (!IsDashing && _dashesLeft < Data.dashAmount && LastOnGroundTime > 0 && !_dashRefilling && !MovingPlatformAndMoving())
        {
            StartCoroutine(nameof(RefillDash), 1);
        }

        return _dashesLeft > 0;
    }

    public bool CanSlide()
    {
        if (LastOnWallTime > 0 && !IsJumping && !IsDashing && LastOnGroundTime <= 0 && !MovingPlatformAndMoving())
            return true;
        else
            return false;
    }
    #endregion


    #region EDITOR METHODS
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawWireCube(_groundCheckPoint.position, _groundCheckSize);
        Gizmos.color = Color.blue;
        Gizmos.DrawWireCube(_frontWallCheckPoint.position, _wallCheckSize);
        Gizmos.DrawWireCube(_backWallCheckPoint.position, _wallCheckSize);
    }
    #endregion

    #region Animations

    private void UpdateAnimations()
    {
        bool isGrounded = LastOnGroundTime > 0;

        animator.SetBool("isGrounded", isGrounded);

        animator.SetFloat("Run", Mathf.Abs(RB.linearVelocityX));

        animator.SetFloat("yVelocity", RB.linearVelocityY);

        animator.SetBool("isJumping", !isGrounded && RB.linearVelocityY > 0.1 && IsJumping);
        animator.SetBool("isFalling", !isGrounded && RB.linearVelocityY < -0.1f && !IsJumping);
    }
    #endregion

}
