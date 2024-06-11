using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    #region Public
    public LayerMask groundLayer;
    #endregion

    #region Private
    private Rigidbody2D rb;
    private SpriteRenderer spriteCharacter;
    private Animator anim;
    [SerializeField] CapsuleCollider2D standingCollider;

    [SerializeField] private Transform groundCheck;
    [SerializeField] private Transform headCheck;
    private float physics2DCheckRadius = 0.05f;
    private bool isGrounded = true;

    [SerializeField] private float moveSpeed;
    private float runningSpeedModifier = 2f;
    private float horizontal;
    private bool isFacingRight = true;
    private bool isRunning = false;

    [SerializeField] private float jumpForce;
    private bool isJumping = false;
    private bool fell = false; // This variable makes character run the Landing animation only if it fell
    private bool ableToMultipleJump = true;
    private bool isMultipleJumping = false;
    private int initialNumOfJumps = 1; // Dont count first jump
    private int avaiableJumps;
    private float jumpForceModifier = 64;

    private bool isCrouching;
    private float crouchingSpeedModifier = 0.5f;
    #endregion

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        spriteCharacter = GetComponentInChildren<SpriteRenderer>();
        anim = GetComponentInChildren<Animator>();
        avaiableJumps = initialNumOfJumps;
    }

    void Update()
    {
        InputManager();
    }

    private void FixedUpdate()
    {
        IsGroundedCheck();
        CharacterMotionManager();
    }

    void CharacterMotionManager() 
    {
        ResetAllTriggers();
        Crouch();
        Move();
        Jump();
        if (!isJumping) Fall(); // Activate FallOnly if character hasnt jumped yet
    }

    void InputManager()
    {
        #region Move
        horizontal = Input.GetAxisRaw("Horizontal");
        if (Input.GetKeyDown(KeyCode.LeftShift)) isRunning = true;
        else if (Input.GetKeyUp(KeyCode.LeftShift)) isRunning = false;
        #endregion

        #region Jump
        if (!isJumping && Input.GetButtonDown("Jump") && isGrounded)
        // Must have isGrounded here because while char is fall from ground without jump, the condition of
        // !isJumping && Input.GetButtonDown("Jump") will be true. That means we now cant perform multiple jumps
        // without the initial jump
        {
            isJumping = true;
        }
        else if (ableToMultipleJump && Input.GetButtonDown("Jump") && avaiableJumps > 0)
        {
            avaiableJumps--;
            isMultipleJumping = true;
        }
        #endregion

        #region Crouch
        if (Input.GetButtonDown("Crouch") && isGrounded)
        {
            isCrouching = true;
            anim.SetTrigger("StartCrouching");
        }
        else if (!Input.GetButton("Crouch")) isCrouching = false;
        #endregion
    }

    void ResetAllTriggers()
    {
        anim.ResetTrigger("TakingOff");
        anim.ResetTrigger("Landing");
        anim.ResetTrigger("SkipOnAir");
        anim.ResetTrigger("OnlyFall");
        anim.ResetTrigger("StartCrouching");
        anim.ResetTrigger("OnAir");
    }

    void Move()
    {
        float xSpeed = horizontal * moveSpeed * 100 * Time.fixedDeltaTime;
        if (!isCrouching && isRunning) xSpeed *= runningSpeedModifier;
        if (isCrouching) xSpeed *= crouchingSpeedModifier;
        
        rb.velocity = new Vector2(xSpeed, rb.velocity.y);

        if ((horizontal > 0 && !isFacingRight) || (horizontal < 0 && isFacingRight))
        {
            Flip();
        }

        anim.SetFloat("xSpeed", Mathf.Abs(Mathf.Round(rb.velocity.x)));
    }

    void Flip()
    {
        isFacingRight = !isFacingRight;
        // -> Flip the sprite itself, init value of isFacingRight can be either true or false. But for better practice, it should be true.
        //spriteCharacter.flipX = !isFacingRight;

        // -> Flip the Player object, init value of isFacingRight has to be true
        Vector3 localScale = transform.localScale;
        localScale.x *= -1f;
        transform.localScale = localScale;
    }

    void IsGroundedCheck()
    {
        isGrounded = Physics2D.OverlapCircle(groundCheck.position, physics2DCheckRadius, groundLayer);
    }

    void Jump()
    {
        if (isCrouching) isJumping = false; // Lock jumping while crouching
        if (isGrounded && isJumping && !fell)
        // Must have the !fell here becuase after fell, char will be grounded, now the program will run this line first then realize that 
        // char is grounded and the var isJumping still true (it hasnt run the line 147 yet) so Taking off again.
        {
            rb.AddForce(new Vector2(0, jumpForce), ForceMode2D.Force);
            anim.SetTrigger("TakingOff");
        }
        else if (isMultipleJumping)
        {
            isMultipleJumping = false;
            isJumping = true;
            rb.velocity = Vector2.up * jumpForce / jumpForceModifier;
            anim.SetTrigger("TakingOff");
            //rb.AddForce(new Vector2(0, jumpForce * jumpForceModifier), ForceMode2D.Force);
        }

        if (isJumping)
        {
            // Need SkipOnAir because if while jumping, there is ground overhead, char will falling down immediately
            // so, char cant reach to the velocity y of 1 so OnAir anim cant be trigger => anim Jump forever
            if (Mathf.Round(rb.velocity.y) == 1) anim.SetTrigger("OnAir");
            else if (rb.velocity.y < -0.01) 
            // Dont round up hear becuase the fall must be sensetive.
            // The moment velocity < -0.01 is when fell = true
            // Reason of -0.01 is that the buildin 2d sometimes because of floating point error, y velocity will < 0 without falling
            {
                anim.SetTrigger("SkipOnAir");
                fell = true;
            }
            else if (fell && isGrounded) // Activate Landing only if character fell then now grounded
            {
                anim.SetTrigger("Landing");
                isJumping = false;
                fell = false;
                avaiableJumps = initialNumOfJumps;
            }
        }
    }

    void Fall()
    {
        if (rb.velocity.y < -0.01)
        {
            anim.SetTrigger("OnlyFall");
            fell = true;
        }
        else if (fell && isGrounded)
        {
            anim.SetTrigger("Landing");
            isJumping = false;
            fell = false;
        }
    }

    void Crouch()
    {
        if (isGrounded)
        {
            if (!isCrouching && Physics2D.OverlapCircle(headCheck.position, physics2DCheckRadius, groundLayer)) isCrouching = true;
            standingCollider.enabled = !isCrouching;
            anim.SetBool("Crouching", isCrouching);
        }
    }
}