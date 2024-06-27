using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    #region Public
    #endregion

    #region Private
    private Rigidbody2D rb;
    private SpriteRenderer spriteCharacter;
    private Animator anim;
    private BoxCollider2D bodyCollider;
    private float normalGravityScale = 2;
    private float superGravityScale = 1000;
    private float playerGravityScale;

    [SerializeField] private LayerMask groundLayer;
    private bool isGrounded = true;
    private bool wasGrounded = true;

    [SerializeField] private float moveSpeed;
    private float walkingSpeedModifier = 0.5f;
    private float horizontal;
    static private bool isFacingRight = true;
    private bool isWalking = false;

    [SerializeField] private float jumpForce;
    [SerializeField] private float holdJumpForce;
    private bool isJumping = false;
    private bool fell = false; // This variable makes character run the Landing animation only if it fell
    public bool Fell { get => fell; set => fell = value; }
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
    private bool nowDash = false; // Indicator for start coroutine
    private bool isDashing = false;
    private float dashSpeed = 15f;
    private float dashTime = 0.175f;
    private float dashDelayTime = 1.5f;
    [SerializeField] private TrailRenderer dashTrail;

    private GameObject currentOWP;
    private bool nowFallThroughOWP = false;
    private float timeForFallThroughOWP = 1f;

    private float vertical;
    [SerializeField] private LayerMask ladderLayer;
    [SerializeField] private float climbingSpeed;
    private bool isClimbing;
    private bool insideLadder;

    bool onMovingPlatform;
    public bool OnMovingPlatform { get => onMovingPlatform; set => onMovingPlatform = value; }
    public float speedUpWhileOnMovingPlatform;
    private Rigidbody2D mpRb;
    #endregion

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        bodyCollider = GetComponent<BoxCollider2D>();  
        spriteCharacter = GetComponentInChildren<SpriteRenderer>();
        anim = GetComponentInChildren<Animator>();
        avaiableJumps = additionalNumOfJumps;
        playerGravityScale = 2;
    }

    void Update()
    {
        InputManager();
        SetGravityOnMovingPlatform();
        //Debug.Log(anim.GetCurrentAnimatorClipInfo(0)[0].clip.name);
        //print(rb.velocity);
    }

    void InputManager()
    {
        if (isDashing) return; // Char cant do anything while dashing

        #region OneWayPlatform
        if (isCrouching && Input.GetButtonDown("Jump"))
        {
            if (currentOWP != null)
            {
                if(onMovingPlatform) onMovingPlatform = false;
                nowFallThroughOWP = true;
            }
        }
        #endregion

        #region Move
        horizontal = Input.GetAxisRaw("Horizontal");
        if (Input.GetKeyDown(KeyCode.LeftShift)) isWalking = true;
        else if (Input.GetKeyUp(KeyCode.LeftShift)) isWalking = false;
        #endregion

        #region Jump
        float localYVelocity = default;
        if (mpRb != null) localYVelocity = 0;
        else localYVelocity = Mathf.Round(rb.velocity.y);
        if (!isJumping && Input.GetButtonDown("Jump") && isGrounded && localYVelocity <= 0) // There are some cases where char is inside ground (OWP) but velocity > 0 
        // Must have isGrounded here because while char is fall from ground without jump, the condition of
        // !isJumping && Input.GetButtonDown("Jump") will be true. That means we now cant perform multiple jumps
        // without the initial jump
        {
            OnMovingPlatform = false;
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

        #region CastSpell
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
            nowDash = true;
        }
        #endregion

        #region LadderClimb
        if (insideLadder)
        {
            if (Input.GetKeyDown(KeyCode.C))
            {
                isClimbing = true;
                anim.SetBool("ClimbingLadder", true);
                anim.SetTrigger("EnterClimbingLadder");
            }
        }
        else
        {
            isClimbing = false;
            anim.SetBool("ClimbingLadder", false);
        }

        vertical = Input.GetAxisRaw("Vertical");
        #endregion
    }

    private void FixedUpdate()
    {
        IsGroundedCheck();
        InsideLadderCheck();
        if (wasGrounded && !isGrounded) StartCoroutine(CoyoteJumpEnable());
        CharacterMotionManager();
    }

    void CharacterMotionManager()
    {
        ResetAllTriggers();
        if (isDashing) return;
        if (rb.bodyType == RigidbodyType2D.Dynamic)
        {
            Crouch();
            Move();
            Jump();
            WallSlide();
            Dash();
            FallThroughOWP();
            ClimbLadder();
        }
        if (isCastingSpell) CastSpell();
    }

    void ResetAllTriggers()
    {
        anim.ResetTrigger("TakingOff");
        anim.ResetTrigger("Landing");
        anim.ResetTrigger("Falling");
        anim.ResetTrigger("StartCrouching");
        anim.ResetTrigger("OnAir");
        anim.ResetTrigger("CastingSpell");
        anim.ResetTrigger("EnterClimbingLadder");
    }

    void FallThroughOWP()
    {
        if (nowFallThroughOWP) StartCoroutine(FallThroughOWPProcedure());
    }

    void Dash()
    {
        if (nowDash) StartCoroutine(DashProcedure());
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
        isGrounded = Physics2D.BoxCast(new Vector2(bodyCollider.bounds.center.x, bodyCollider.bounds.center.y - bodyCollider.bounds.extents.y), new Vector2(bodyCollider.bounds.size.x, 0.05f), 0f, Vector2.down, 0, groundLayer);
        //isGrounded = Physics2D.OverlapCircle(groundCheck.position, 0.05f, groundLayer);
        Color rayColor;
        if (isGrounded) rayColor = Color.red;
        else rayColor = Color.green;

        Debug.DrawRay(bodyCollider.bounds.center + new Vector3(bodyCollider.bounds.extents.x, 0), Vector2.down * (bodyCollider.bounds.extents.y + 0.03f), rayColor);
        Debug.DrawRay(bodyCollider.bounds.center - new Vector3(bodyCollider.bounds.extents.x, 0), Vector2.down * (bodyCollider.bounds.extents.y + 0.03f), rayColor);
        Debug.DrawRay(bodyCollider.bounds.center - new Vector3(bodyCollider.bounds.extents.x, bodyCollider.bounds.extents.y + 0.03f), Vector2.right * (bodyCollider.bounds.extents.x * 2f), rayColor);
    }

    void Jump()
    {
        if (isCrouching || isClimbing) 
        {
            isMultipleJumping = false;
            isJumping = false; // add this line because while crouching, if press space, the isJumping will turn to true and after standing, char will jump
            return;
        } // Lock jumping while crouching or climbing

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
            rb.velocity = new Vector2(rb.velocity.x, jumpForce);
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
            if (!isCrouching)
            {
                bodyCollider.size = new Vector2(bodyCollider.size.x, 0.8070784f);
                bodyCollider.offset = new Vector2(bodyCollider.offset.x, 0.04622972f);
            }
            else
            {
                bodyCollider.size = new Vector2(bodyCollider.size.x, 0.6040835f);
                bodyCollider.offset = new Vector2(bodyCollider.offset.x, -0.05526769f);
            }
            anim.SetBool("Crouching", isCrouching);
        }
        else isCrouching = false;
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

    //private void OnDrawGizmos()
    //{
    //    Gizmos.color = Color.red;
    //    Gizmos.DrawWireSphere(wallCheck.position, 0.05f);
    //}

    IEnumerator DashProcedure()
    {
        nowDash = false;
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

    private IEnumerator FallThroughOWPProcedure()
    {
        nowFallThroughOWP = false;
        BoxCollider2D platformCollider = currentOWP.GetComponent<BoxCollider2D>();

        Physics2D.IgnoreCollision(bodyCollider, platformCollider);
        anim.SetTrigger("Falling");
        yield return new WaitForSeconds(timeForFallThroughOWP);
        Physics2D.IgnoreCollision(bodyCollider, platformCollider, false);
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.layer == 9) // 9 means OneWayPlatform
        {
            currentOWP = collision.gameObject;
        }
    }

    private void OnCollisionExit2D(Collision2D collision)
    {
        if (collision.gameObject.layer == 9)
        {
            currentOWP = null;
        }
    }

    void InsideLadderCheck()
    {
        insideLadder = Physics2D.BoxCast(new Vector2(bodyCollider.bounds.center.x, bodyCollider.bounds.center.y - bodyCollider.bounds.extents.y + 0.025f), new Vector2(bodyCollider.bounds.size.x - 0.1f, 0.049f), 0f, Vector2.down, 0, ladderLayer);
    }

    void ClimbLadder()
    {
        if (isClimbing)
        {
            rb.velocity = new Vector2(rb.velocity.x, climbingSpeed * vertical);
            anim.SetFloat("ySpeed", Mathf.Round(rb.velocity.y));
            //print(anim.GetFloat("ySpeed"));
            rb.gravityScale = 0;
        }
        else rb.gravityScale = playerGravityScale;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.CompareTag("MovingPlatform"))
        {
            mpRb = collision.GetComponent<Rigidbody2D>();
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.gameObject.CompareTag("MovingPlatform"))
        {
            mpRb = null;
        }
    }

    void SetGravityOnMovingPlatform()
    {
        if (OnMovingPlatform && isGrounded) playerGravityScale = superGravityScale;
        else playerGravityScale = normalGravityScale;
    }
}