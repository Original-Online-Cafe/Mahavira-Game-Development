using Unity.VisualScripting;
using UnityEngine;

public class player_script : MonoBehaviour
{
    //Movement
    [SerializeField] private float speed = 5.0f;
    [SerializeField] private FloatingJoystick joystick;

    //Jump
    [SerializeField] private float jumpPower = 6.0f;
    [SerializeField] private int maxJumps = 2;

    //GroundCheck
    [SerializeField] private Transform groundCheck;
    [SerializeField] private float groundCheckRadius = 0.2f;
    [SerializeField] private LayerMask groundLayer;

    //WallBounce
    [SerializeField] private LayerMask wallLayer;
    [SerializeField] private float wallBounceHorizontalPower = 8.0f;
    [SerializeField] private float wallBounceVerticalPower = 5.0f;
    [SerializeField] private float wallBounceControlDelay = 0.15f;

    private Rigidbody2D rb;
    private Animator animator;

    private float horizontalInput;
    private float wallBounceTimer;

    private int jumpsRemaining;

    private bool isGrounded;
    private bool wasGrounded;

    private void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();

        jumpsRemaining = maxJumps;
    }

    private void Update()
    {
        ReadMovementInput();
        CheckGround();
        HandleKeyboardJump();
        UpdateDirection();
        UpdateAnimation();

        if (wallBounceTimer > 0.0f)
        {
            wallBounceTimer -= Time.deltaTime;
        }
    }

    private void FixedUpdate()
    {
        MovePlayer();
    }

    private void ReadMovementInput ()
    {
        float joyStickInput = 0.0f;

        if (joystick != null)
        {
            joyStickInput = joystick.Horizontal;
        }

        float keyboardInput = Input.GetAxisRaw("Horizontal");

        if (Mathf.Abs (keyboardInput) > 0.01f)
        {
            horizontalInput = keyboardInput;
        }
        else
        {
            horizontalInput = joyStickInput;
        }
    }

    private void MovePlayer()
    {
        if (wallBounceTimer > 0.0f)
        {
            return;
        }

        rb.linearVelocity = new Vector2(horizontalInput * speed, rb.linearVelocity.y);
    }

    private void CheckGround()
    {
        if (groundCheck == null)
        {
            isGrounded = false;
            return;
        }

        isGrounded = Physics2D.OverlapCircle(groundCheck.position, groundCheckRadius, groundLayer);

        if (isGrounded && !wasGrounded)
        {
            jumpsRemaining = maxJumps;
        }

        wasGrounded = isGrounded;
    }

    private void HandleKeyboardJump()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            Jump();
        }
    }

    public void Jump()
    {
        if (jumpsRemaining <= 0)
        {
            return;
        }

        rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpPower);
    }

    private void OnCollisionEnter2D (Collision2D collision)
    {
        if (!IsInLayerMask(collision.gameObject.layer, wallLayer))
        {
            return;
        }

        foreach (ContactPoint2D contact in collision.contacts)
        {
            if (Mathf.Abs (contact.normal.x) > 0.5f)
            {
                BounceFromWall(contact.normal.x);
                break;
            }
        }
    }

    private void BounceFromWall(float wallNormalX)
    {
        float bounceDirection = Mathf.Sign(wallNormalX);

        rb.linearVelocity = new Vector2(bounceDirection * wallBounceHorizontalPower, wallBounceVerticalPower);

        wallBounceTimer = wallBounceControlDelay;
    }

    private bool IsInLayerMask(int objectLayer, LayerMask layerMask)
    {
        return (layerMask.value & (1 << objectLayer)) != 0;
    }

    private void UpdateDirection()
    {
        if (horizontalInput > 0.05f)
        {
            transform.localScale = new Vector3(1.0f, 1.0f, 1.0f);
        }
        else if (horizontalInput < -0.05f)
        {
            transform.localScale = new Vector3(-1.0f, 1.0f, 1.0f);
        }
    }

    private void UpdateAnimation()
    {
        if (animator == null)
        {
            return;
        }

        animator.SetFloat("Speed", Mathf.Abs(horizontalInput));
    }

    private void OnDrawGizmmosSelected()
    {
        if (groundCheck == null)
        {
            return;
        }

        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(groundCheck.position, groundCheckRadius);
    }
}
