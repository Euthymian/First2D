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
    BoxCollider2D localBox2D;
    private SpriteRenderer spriteCharacter;
    private Animator anim;
    [SerializeField] BoxCollider2D standingCollider;

    [SerializeField] private Transform headCheck;
    private float extraHeightBoxCast = 0.03f;
    private bool isGrounded = true;
    private bool wasGrounded = true;

    [SerializeField] private float moveSpeed;
    private float runningSpeedModifier = 2f;
    private float horizontal;
    public float Horizontal { get => horizontal; }
    static private bool isFacingRight = true;
    private bool isRunning = false;

    [SerializeField] private float jumpForce;
    private bool isJumping = false;
    private bool fell = false; // This variable makes character run the Landing animation only if it fell

    private bool ableToMultipleJump = true;
    private bool isMultipleJumping = false;
    private int initialNumOfJumps = 1; // Dont count first jump
    private int avaiableJumps;
    private float jumpForceModifier = 64;
    private float delayTimeForCoyoteJump = 0.3f;
    private bool ableToCoyoteJump = false;
    private bool isCoyoteJumping = false;
    [SerializeField] private GameObject jumpEffectPrefab;
    [SerializeField] private GameObject doubleJumpEffectPrefab;

    private bool isCrouching;
    private float crouchingSpeedModifier = 0.5f;

    private bool isCastingSpell = false;
    #endregion

    void Start()
    {
        localBox2D = GetComponent<BoxCollider2D>();
        rb = GetComponent<Rigidbody2D>();
        spriteCharacter = GetComponentInChildren<SpriteRenderer>();
        anim = GetComponentInChildren<Animator>();
        avaiableJumps = initialNumOfJumps;
    }

    void Update()
    {
        InputManager();
        //Debug.Log(anim.GetCurrentAnimatorClipInfo(0)[0].clip.name);
    }

    private void FixedUpdate()
    {
        IsGroundedCheck();
        if (wasGrounded && !isGrounded) StartCoroutine(CoyoteJumpEnable());
        CharacterMotionManager();
    }

    void CharacterMotionManager()
    {
        ResetAllTriggers();
        Crouch();
        Move();
        Jump();
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
        else if (ableToCoyoteJump && Input.GetButtonDown("Jump") && rb.velocity.y <= 0)
        {
            isCoyoteJumping = true;
        }
        // check coyote trc vi multiple jump la jump phu. neu multiplejump dc check trc thi coyote jump se khong bao h dc goi.
        else if (ableToMultipleJump && Input.GetButtonDown("Jump") && avaiableJumps > 0)
        // Else if because we dont want the system check 2 types of jump simultanously
        {
            avaiableJumps--;
            isMultipleJumping = true;
        }
        #endregion

        #region Crouch
        if (Input.GetButtonDown("Crouch") && isGrounded && !isCrouching)
        {
            isCrouching = true;
            anim.SetTrigger("StartCrouching");
        }
        else if (!Input.GetButton("Crouch")) isCrouching = false;
        #endregion

        if (Input.GetKeyDown(KeyCode.P) && isGrounded)
        {
            anim.SetTrigger("CastingSpell");
        }
    }

    void ResetAllTriggers()
    {
        anim.ResetTrigger("TakingOff");
        anim.ResetTrigger("Landing");
        anim.ResetTrigger("Falling");
        anim.ResetTrigger("StartCrouching");
        anim.ResetTrigger("OnAir");
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
        wasGrounded = isGrounded;
        isGrounded = Physics2D.BoxCast(localBox2D.bounds.center, localBox2D.bounds.size, 0f, Vector2.down, extraHeightBoxCast, groundLayer);
        //isGrounded = Physics2D.OverlapCircle(groundCheck.position, 0.05f, groundLayer);
        Color rayColor;
        if (isGrounded) rayColor = Color.red;
        else rayColor = Color.green;

        Debug.DrawRay(localBox2D.bounds.center + new Vector3(localBox2D.bounds.extents.x, 0), Vector2.down * (localBox2D.bounds.extents.y + extraHeightBoxCast), rayColor);
        Debug.DrawRay(localBox2D.bounds.center - new Vector3(localBox2D.bounds.extents.x, 0), Vector2.down * (localBox2D.bounds.extents.y + extraHeightBoxCast), rayColor);
        Debug.DrawRay(localBox2D.bounds.center - new Vector3(localBox2D.bounds.extents.x, localBox2D.bounds.extents.y + extraHeightBoxCast), Vector2.right * (localBox2D.bounds.extents.x * 2f), rayColor);
    }

    void Jump()
    {
        if (isCrouching) isJumping = false; // Lock jumping while crouching
        if ((isGrounded && isJumping && !fell) || (isCoyoteJumping))
        // Must have the !fell here becuase after fell, char will be grounded, now the program will run this line first then realize that 
        // char is grounded and the var isJumping still true (it hasnt run the line 147 yet) so Taking off again.
        {
            if (isCoyoteJumping)
            {
                isCoyoteJumping = false;
            }

            isJumping = false;
            GameObject tmpPrefab = Instantiate(jumpEffectPrefab, transform.position - new Vector3(0, 0.3f), Quaternion.identity);
            Destroy(tmpPrefab, 1);
            rb.AddForce(new Vector2(0, jumpForce), ForceMode2D.Force);
            anim.SetTrigger("TakingOff");
        }
        else if (isMultipleJumping)
        {
            isMultipleJumping = false; // Disable immediately unless add velocity forever
            GameObject tmpPrefab = Instantiate(doubleJumpEffectPrefab, transform.position - new Vector3(0, 0.3f), Quaternion.identity);
            Destroy(tmpPrefab, 0.4f);
            rb.velocity = Vector2.up * jumpForce / jumpForceModifier;
            anim.SetTrigger("TakingOff");
            //rb.AddForce(new Vector2(0, jumpForce * jumpForceModifier), ForceMode2D.Force);
        }

        if (Mathf.Round(rb.velocity.y) == 3) anim.SetTrigger("OnAir");
        else if (Mathf.Round(rb.velocity.y) == -2 || (isGrounded && !wasGrounded))
        // isGrounded && !wasGrounded means char fell then reached to the ground. Look at logic of wasGrounded and isGrounded at IsGroundedCheck()
        {
            anim.SetTrigger("Falling");
            fell = true;
        }
        if (fell && isGrounded) // Activate Landing only if character fell then now grounded
        {
            anim.SetTrigger("Landing");
            fell = false;
            avaiableJumps = initialNumOfJumps;
        }
    }

    IEnumerator CoyoteJumpEnable()
    {
        ableToCoyoteJump = true;
        yield return new WaitForSeconds(delayTimeForCoyoteJump);
        ableToCoyoteJump = false;
    }

    void Crouch()
    {
        if (isGrounded)
        {
            if (!isCrouching && Physics2D.OverlapCircle(headCheck.position, 0.05f, groundLayer)) isCrouching = true;
            // Here should use circle collision check with small radius because we dont want thes edge of overhead ground can reach to boxcast
            standingCollider.enabled = !isCrouching;
            anim.SetBool("Crouching", isCrouching);
        }
    }

    static public int IsFacingRight()
    {
        if (isFacingRight) return 1;
        else return -1;
    }

    void CastSpell()
    {
        if(anim.GetCurrentAnimatorClipInfo(0)[0].clip.name == "Spell") isCastingSpell = true;
        else isCastingSpell = false;

        if (isCastingSpell)
        {
            rb.velocity = Vector2.zero;
        }
    }
}