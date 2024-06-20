using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    #region Public
    #endregion

    #region Private
    private Rigidbody2D rb;
    BoxCollider2D localBox2D;
    private SpriteRenderer spriteCharacter;
    private Animator anim;
    [SerializeField] BoxCollider2D standingCollider;

    private float extraHeightBoxCast = 0.03f;
    [SerializeField] private LayerMask groundLayer;
    private bool isGrounded = true;
    private bool wasGrounded = true;

    [SerializeField] private float moveSpeed;
    private float walkingSpeedModifier = 0.5f;
    private float horizontal;
    public float Horizontal { get => horizontal; }
    static private bool isFacingRight = true;
    private bool isWalking = false;

    [SerializeField] private float jumpForce;
    [SerializeField] private float holdJumpForce;
    private bool isJumping = false;
    private bool fell = false; // This variable makes character run the Landing animation only if it fell
    public bool Fell
    {
        get => fell; set => fell = value;
    }
    private bool isUp = false;
    [SerializeField] private float maxUpTime;
    private float currentUpTime;
    private bool ableToMultipleJump = true;
    private bool isMultipleJumping = false;
    private int additionalNumOfJumps = 1; // Dont count first jump
    private int avaiableJumps;
    private float multipleJumpForce = 5.6f;
    private float coyoteJumpDelayTime = 0.2f;
    private bool ableToCoyoteJump = false;
    private bool isCoyoteJumping = false;
    private bool ableToWallJump = true;
    private bool isWallJumping = false;
    private float limitTimeUpWallJump = 0.1f;
    [SerializeField] private GameObject jumpEffectPrefab;
    [SerializeField] private GameObject doubleJumpEffectPrefab;

    [SerializeField] private Transform headCheck;
    private bool isCrouching;
    private float crouchingSpeedModifier = 0.25f;

    private bool isCastingSpell = false;
    private float castSpellDelayTime = 1f;
    private float currentSpellDelayTime = 0;

    [SerializeField] private Transform wallCheck;
    [SerializeField] private LayerMask wallLayer;
    private bool isWallSliding;
    private bool wasWallSliding;
    private float wallSlideSpeed = 0.5f;

    private bool ableToDash = true; // after claim dash ability, this turns to true
    private bool canDash = true; // works as dash cooldown
    private bool isDashing = false;
    private float dashSpeed = 15f;
    private float dashTime = 0.175f;
    private float dashDelayTime = 1.5f;
    [SerializeField] private TrailRenderer dashTrail;
    #endregion

    void Start()
    {
        localBox2D = GetComponent<BoxCollider2D>();
        rb = GetComponent<Rigidbody2D>();
        spriteCharacter = GetComponentInChildren<SpriteRenderer>();
        anim = GetComponentInChildren<Animator>();
        avaiableJumps = additionalNumOfJumps;
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
        if (isDashing) return;
        ResetAllTriggers();
        if (rb.bodyType == RigidbodyType2D.Dynamic)
        {
            Crouch();
            Move();
            Jump();
            WallSlide();
        }
        if (isCastingSpell) CastSpell();
    }

    void InputManager()
    {
        if (isDashing) return; // Char cant do anything while dashing

        #region Move
        horizontal = Input.GetAxisRaw("Horizontal");
        if (Input.GetKeyDown(KeyCode.LeftShift)) isWalking = true;
        else if (Input.GetKeyUp(KeyCode.LeftShift)) isWalking = false;
        #endregion

        #region Jump
        if (!isJumping && Input.GetButtonDown("Jump") && isGrounded)
        // Must have isGrounded here because while char is fall from ground without jump, the condition of
        // !isJumping && Input.GetButtonDown("Jump") will be true. That means we now cant perform multiple jumps
        // without the initial jump
        {
            currentUpTime = Time.time;
            isJumping = true;
        }
        else if (ableToCoyoteJump && Input.GetButtonDown("Jump") && rb.velocity.y <= 0)
        // check coyote trc vi multiple jump la jump phu. neu multiplejump dc check trc thi coyote jump se khong bao h dc goi.
        {
            currentUpTime = Time.time;
            isCoyoteJumping = true;
        }
        else if (ableToWallJump && isWallSliding && Input.GetButtonDown("Jump"))
        {
            currentUpTime = Time.time - limitTimeUpWallJump;
            isWallJumping = true;
        }
        else if (ableToMultipleJump && Input.GetButtonDown("Jump") && avaiableJumps > 0)
        // Else if because we dont want the system check 2 types of jump simultanously
        {
            avaiableJumps--;
            isMultipleJumping = true;
        }
        else if (avaiableJumps == additionalNumOfJumps && Input.GetButton("Jump") && Time.time - currentUpTime <= maxUpTime)
        {
            isUp = true;
        }
        else if (avaiableJumps == additionalNumOfJumps && Time.time - currentUpTime > maxUpTime || Input.GetButtonUp("Jump"))
        {
            isUp = false;
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

        #region Cast spell
        if (Input.GetKeyDown(KeyCode.P) && isGrounded)
        {
            if (Time.time - currentSpellDelayTime >= castSpellDelayTime)
            {
                currentSpellDelayTime = Time.time;
                isCastingSpell = true;
                anim.SetTrigger("CastingSpell");
            }
        }
        #endregion

        #region Dash
        if (ableToDash && Input.GetKeyDown(KeyCode.Q) && canDash)
        {
            StartCoroutine(Dash());
        }
        #endregion
    }

    void ResetAllTriggers()
    {
        anim.ResetTrigger("TakingOff");
        anim.ResetTrigger("Landing");
        anim.ResetTrigger("Falling");
        anim.ResetTrigger("StartCrouching");
        anim.ResetTrigger("OnAir");
        anim.ResetTrigger("CastingSpell");
    }

    void Move()
    {
        float xSpeed = horizontal * moveSpeed * 100 * Time.fixedDeltaTime;
        if (!isCrouching && isWalking) xSpeed *= walkingSpeedModifier;
        else if (isCrouching) xSpeed *= crouchingSpeedModifier;

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
        if ((isJumping && !fell) || isCoyoteJumping || isWallJumping)
        // Must have the !fell here becuase after fell, char will be grounded, now the program will run this line first then realize that 
        // char is grounded and the var isJumping still true (it hasnt run the line 147 yet) so Taking off again.
        {
            if (isCoyoteJumping)
            {
                isCoyoteJumping = false;
            }

            if (isWallJumping)
            {
                transform.position += new Vector3(0.1f * -PlayerController.IsFacingRight(), 0);
                isWallJumping = false;
                // WallSlide() will reset multiplejump times everytime that char starts wallslide
            }

            isJumping = false;
            fell = false;
            GameObject tmpPrefab = Instantiate(jumpEffectPrefab, transform.position - new Vector3(0, 0.3f), Quaternion.identity);
            Destroy(tmpPrefab, 1);
            rb.velocity = Vector2.up * jumpForce;
            anim.SetTrigger("TakingOff");
        }
        else if (isUp)
        {
            rb.velocity = new Vector2(rb.velocity.x, rb.velocity.y + holdJumpForce);
        }
        else if (isMultipleJumping)
        {
            isMultipleJumping = false; // Disable immediately unless add velocity forever
            fell = false;
            GameObject tmpPrefab = Instantiate(doubleJumpEffectPrefab, transform.position - new Vector3(0, 0.3f), Quaternion.identity);
            Destroy(tmpPrefab, 0.4f);
            rb.velocity = Vector2.up * multipleJumpForce;
            anim.SetTrigger("TakingOff");
            //rb.AddForce(new Vector2(0, jumpForce * jumpForceModifier), ForceMode2D.Force);
        }

        if (Mathf.Round(rb.velocity.y) == 2) anim.SetTrigger("OnAir");
        else if ((isGrounded && !wasGrounded) || Mathf.Round(rb.velocity.y) == -2)
        // isGrounded && !wasGrounded means char fell then reached to the ground. Look at logic of wasGrounded and isGrounded at IsGroundedCheck()
        {
            anim.SetTrigger("Falling");
            fell = true;
        }
        if (fell && isGrounded) // Activate Landing only if character fell then now grounded
        {
            anim.SetTrigger("Landing");
            fell = false;
            avaiableJumps = additionalNumOfJumps;
        }

        if (fell) rb.velocity = new Vector2(rb.velocity.x, Mathf.Clamp(rb.velocity.y, -10f, -1)); //lock ySpeed
    }

    IEnumerator CoyoteJumpEnable()
    {
        ableToCoyoteJump = true;
        yield return new WaitForSeconds(coyoteJumpDelayTime);
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
        if (anim.GetCurrentAnimatorClipInfo(0)[0].clip.name == "Spell") isCastingSpell = true;
        else isCastingSpell = false;

        if (isCastingSpell) rb.bodyType = RigidbodyType2D.Static;
        else rb.bodyType = RigidbodyType2D.Dynamic;
    }

    void WallSlide()
    {
        wasWallSliding = isWallSliding;
        if (!isGrounded && rb.velocity.y < 0)
            isWallSliding = Physics2D.OverlapCircle(wallCheck.position, 0.05f, wallLayer);
        else isWallSliding = false;

        if (isWallSliding)
        {
            rb.velocity = new Vector2(rb.velocity.x, -wallSlideSpeed);
        }

        if (!isGrounded && isWallSliding && !wasWallSliding)
        {
            avaiableJumps = additionalNumOfJumps;
            anim.SetTrigger("WallSliding");
        }
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(wallCheck.position, 0.05f);
    }

    IEnumerator Dash()
    {
        canDash = false;
        isDashing = true;
        float originalGravity = rb.gravityScale;
        rb.gravityScale = 0f;
        rb.velocity = new Vector2(dashSpeed * IsFacingRight(), 0f);
        dashTrail.emitting = true;
        yield return new WaitForSeconds(dashTime);
        dashTrail.emitting = false;
        rb.gravityScale = originalGravity;
        isDashing = false;
        yield return new WaitForSeconds(dashDelayTime);
        canDash = true;
    }

}