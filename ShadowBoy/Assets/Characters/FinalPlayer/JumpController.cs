using UnityEngine;

public class JumpController : MonoBehaviour
{
    Rigidbody2D rb;

    [Header("Jump System")]
    [SerializeField] float jumpTime;
    [SerializeField] int jumpPower;
    [SerializeField] float fallMultiplier;
    [SerializeField] float jumpMultiplier;
    private Animator animator;
    

    public Transform groundCheck;
    public LayerMask groundLayer;
    Vector2 vecGravity;

    public bool isJumping;
    float jumpCounter; 

    void Start()
    {
        vecGravity = new Vector2(0, -Physics2D.gravity.y);
        rb = GetComponent<Rigidbody2D>();
    }

   
    void Update()
    {
       if (Input.GetButtonDown("Jump") && IsGrounded())
        {
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpPower);
            isJumping = true;
            jumpCounter = 0;
            animator.SetBool("Jump", true);
            
        }


       if (rb.linearVelocityY > 0 && isJumping)
        {
            jumpCounter += Time.deltaTime;

            if(jumpCounter > jumpTime)
            {
                isJumping = false;
            }

            float t = jumpCounter /jumpTime;
            float currentJumpM = jumpMultiplier;

            if(t > 0)
            {
                currentJumpM = jumpMultiplier * (1-t);
            }

            rb.linearVelocity += vecGravity * currentJumpM * Time.deltaTime;
        }

       if (Input.GetButtonUp("Jump"))
        {
            isJumping = false;
            jumpCounter = 0;

            if (rb.linearVelocityY > 0)
            {
                rb.linearVelocity = new Vector2(rb.linearVelocityX, rb.linearVelocityY * 0.6f);
            }
        }
       if (rb.linearVelocityY < 0)
        {
            rb.linearVelocity -= vecGravity * fallMultiplier *Time.deltaTime;
        }

       
    }

    public void SetAnimator(Animator anim)
    {
        animator = anim;
    }

    public bool IsGrounded()
    {
        return Physics2D.OverlapCapsule(groundCheck.position, new Vector2(0.5f, 0.12f), CapsuleDirection2D.Horizontal, 0, groundLayer);

    }
}
