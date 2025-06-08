using System.Collections;
using System.Runtime.CompilerServices;
using UnityEngine;
using UnityEngine.Rendering;

public class MovementPlayer : MonoBehaviour
{
    public Animator animator; 
    private Rigidbody2D rb2D;
    [Header("Movimiento")]

    private float horizontalMovement = 0f;
    [SerializeField] private float velocityMovement;
    [SerializeField] private float smoothMovement;
    //private Vector2 velocity = Vector2.zero;
    private bool lookingRight = true;

    // Ascensores
    private PlatformMoving currentPlatform;
    private bool playerOnMovingPlatform = false;
    private Vector2 smoothDampVelocityRef;


    //Dash
    private bool canDash = true;
    private bool isDashing;
    private float dashingPower = 12f;
    private float dashingTime = 0.1f;
    private float dashingCooldown = 1f;


    private bool isWallSliding;
    private float wallSpeedSliding = 2f;

    private bool isWallJumping;
    private float wallJumpingDirection;
    private float wallJumpingTime = 0.2f;
    private float wallJumpingCounter;
    private float wallJumpingDuration = 0.4f;
    private Vector2 wallJumpingPower = new Vector2(8f, 16f);

   
    
    [SerializeField] private TrailRenderer tr;
    [SerializeField] private Transform wallCheck;
    [SerializeField] private LayerMask wallLayer;





    [Header("Jump System")]
    [SerializeField] float jumpTime;
    [SerializeField] int jumpPower;
    [SerializeField] float fallMultiplier;
    [SerializeField] float jumpMultiplier;
   
    
    public Transform groundCheck;
    public LayerMask groundLayer;
    Vector2 vecGravity;

    public bool isJumping;
    float jumpCounter;


    private void Start()
    {
        rb2D = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
        vecGravity = new Vector2(0, -Physics2D.gravity.y);
        smoothDampVelocityRef = Vector2.zero;
    }

    private void Update()
    {
        if (Mathf.Abs(rb2D.linearVelocity.x) > 0.1f)
        {
            animator.SetFloat("Run", 1);
        }
        else if (rb2D.linearVelocity.x < 0.1f)
        {
            animator.SetFloat("Run", 0);
        }

        WallSlide();
        WallJump();


        Jump();


        if (isDashing)
        {
            return;
        }
      
        horizontalMovement = Input.GetAxisRaw("Horizontal") * velocityMovement;

        if (!isWallJumping && !isDashing)
        {
            horizontalMovement = Input.GetAxisRaw("Horizontal") * velocityMovement;
        }

        if (Input.GetKeyDown(KeyCode.LeftShift) && canDash)
        {
            StartCoroutine(Dash());
        }
         
        

        //Attack

        if (Input.GetMouseButtonDown(0) && IsGrounded() == true)
        {
            animator.SetTrigger("Attack");
        }
        
    }

    private void FixedUpdate()
    {
        IsGrounded();
        if (isDashing)
        {
            return; 
        }
        //Mover
        Move(horizontalMovement);
    }
    public void Jump()
    {

        if (Input.GetButtonDown("Jump") && IsGrounded() && !playerOnMovingPlatform)
        {
            rb2D.linearVelocity = new Vector2(rb2D.linearVelocity.x, jumpPower);
            animator.SetBool("Jump", true);
            isJumping = true;
            jumpCounter = 0;

        }

        if (rb2D.linearVelocityY > 0 && isJumping)
        {
            jumpCounter += Time.deltaTime;

            if (jumpCounter > jumpTime)
            {
                isJumping = false;
            }

            float t = jumpCounter / jumpTime;
            float currentJumpM = jumpMultiplier;

            if (t > 0)
            {
                currentJumpM = jumpMultiplier * (1 - t);
            }

            if (!isWallSliding && !isDashing)
            {
                rb2D.linearVelocity += vecGravity * currentJumpM * Time.deltaTime;
            }

        }

        if (Input.GetButtonUp("Jump"))
        {
            isJumping = false;
            jumpCounter = 0;

            if (rb2D.linearVelocityY > 0 && !isWallSliding && !isDashing)
            {
                rb2D.linearVelocity = new Vector2(rb2D.linearVelocityX, rb2D.linearVelocityY * 0.6f);
            }
        }
        if (rb2D.linearVelocityY < 0 && !isWallSliding && !isDashing)
        {
            rb2D.linearVelocity -= vecGravity * fallMultiplier * Time.deltaTime;
        }

        if (!isDashing && !isWallSliding && !isWallJumping)
        {
            animator.SetBool("Jump", !IsGrounded());
            animator.SetFloat("yVelocity", rb2D.linearVelocity.y);
        }
        else if (isWallSliding) 
        {
            animator.SetFloat("yVelocity", 0);
        }

    }
    private void Move(float move)
    {
        Vector2 playerInputVelocity = new Vector2 (move, rb2D.linearVelocityY);
        Vector2 targetVelocity = playerInputVelocity;
        
        //Ascensores
        rb2D.linearVelocity = Vector2.SmoothDamp(rb2D.linearVelocity, targetVelocity, ref smoothDampVelocityRef, smoothMovement);
        
        
        if(move > 0 && !lookingRight)
        {
            
            Rotate();
        }
        else if (move < 0 && lookingRight)
        {
            
            Rotate();
        }
        
    }

    private void Rotate()
    {
        lookingRight = !lookingRight;
        Vector3 scale = transform.localScale;
        scale.x *= -1;
        transform.localScale = scale;
    }

    private IEnumerator Dash()
    {
        canDash = false;
        isDashing = true;
        float originalGravity = rb2D.gravityScale;
        rb2D.gravityScale = 0f;
        rb2D.linearVelocity = new Vector2(transform.localScale.x * dashingPower, 0f);
        tr.emitting = true;
        yield return new WaitForSeconds(dashingTime);
        tr.emitting = false;
        rb2D.gravityScale = originalGravity;
        isDashing = false;
        yield return new WaitForSeconds(dashingCooldown);
        canDash = true;
    }

    private bool IsWalled()
    {
        return Physics2D.OverlapCircle(wallCheck.position, 0.2f, wallLayer);
    }

    private void WallSlide()
    {
        if (IsWalled() && !IsGrounded() && horizontalMovement != 0f)
        {
            isWallSliding = true;
            rb2D.linearVelocity = new Vector2(rb2D.linearVelocityX, Mathf.Clamp(rb2D.linearVelocityY, -wallSpeedSliding, float.MaxValue));
        }
        else
        {
            isWallSliding = false;
        }
    }

    private void WallJump()
    {
        if (isWallSliding)
        {
            isWallSliding = false;
            wallJumpingDirection = -transform.localScale.x;
            wallJumpingCounter = wallJumpingTime;
            CancelInvoke(nameof(StopWallJumping));
        }
        else
        {
            wallJumpingCounter -= Time.deltaTime;
        }

        if (Input.GetButtonDown("Jump") && wallJumpingCounter > 0f)
        {
            isWallSliding = true;
            rb2D.linearVelocity = new Vector2(wallJumpingDirection * wallJumpingPower.x, wallJumpingPower.y);
            wallJumpingCounter = 0f;

            if (transform.localScale.x != wallJumpingDirection)
            {
                lookingRight = !lookingRight;
                Vector3 localScale = transform.localScale;
                localScale.x *= -1f;
                transform.localScale = localScale;
            }
            Invoke(nameof(StopWallJumping), wallJumpingDuration);
        }
    }
    private void StopWallJumping()
    {
        isWallSliding = false;
    }


    
    public bool IsGrounded()
    {
        
        return Physics2D.OverlapCapsule(groundCheck.position, new Vector2(0.5f, 0.12f), CapsuleDirection2D.Horizontal, 0, groundLayer);

    }

    // Ascensores

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
            if (platform != null) { 
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
        if (currentPlatform!= null && collision.gameObject.CompareTag("Moving Platform"))
        {
          SetPlayerOnMovingPlatform(false);
          SetCurrentMovingPlatform(null);
            
        }
    }
    public void SetPlayerOnMovingPlatform (bool isOnPlatform)
    {
        playerOnMovingPlatform = isOnPlatform;
    }
    public void SetCurrentMovingPlatform (PlatformMoving platform)
    {
        currentPlatform = platform;
    }
}
