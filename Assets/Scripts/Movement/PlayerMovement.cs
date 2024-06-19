using Unity.VisualScripting;
using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField]
    private float walkSpeed;
    [SerializeField]
    private float sprintSpeed;
    private float moveSpeed;
    [SerializeField]
    private float wallrunSpeed;
    public bool enableMovement = true;


    [SerializeField]
    private float groundDrag;
    [Header("Jumping")]
    [SerializeField]
    private float jumpForce;
    [SerializeField]
    private float jumpCooldown;
    [SerializeField]
    private float airMultiplier;
    [SerializeField]
    bool isJumpReady;

    [Header("Crouching")]
    [SerializeField]
    private float crouchSpeed;
    [SerializeField]
    private float crouchYScale;
    private float startYScale;

    [Header("Keybinds")]
    [SerializeField]
    private KeyCode jumpKey = KeyCode.Space;
    private KeyCode sprintKey = KeyCode.LeftShift;
    private KeyCode crouchKey = KeyCode.LeftControl;

    [Header("Ground Check")]
    [SerializeField]
    private float playerHeight;
    [SerializeField]
    private LayerMask whatIsGround;
    [SerializeField]
    bool isGrounded;

    [Header("Slope Handling")]
    [SerializeField]
    private float maxSlopeAngle;
    private RaycastHit slopeHit;
    private bool exitingSlope;


    [Header("Misc")]
    [SerializeField]
    private Transform orientation;
    private float horizontalInput;
    private float verticalInput;
    private Vector3 moveDirection;

    [Header("Camera")]
    [SerializeField]
    private Camera playerCamera;  // Reference to the player's camera
    [SerializeField]
    private PlayerLook cam;
    private float normalFov = 80.0f;  // Normal FOV
    private float sprintFov = 90.0f;  // Increased FOV when sprinting

    Rigidbody rb;

    [SerializeField]
    private MovementState state;
    public enum MovementState
    {
        walking,
        sprinting,
        crouching,
        wallrunning,
        air
    }

    public bool wallrunning;

    [Header("Sound Effects")]
    [SerializeField] private AudioClip[] SFX_footsteps;
    [SerializeField] private AudioClip SFX_jump;

    private float timer = 0.0f;

    private void StateHandler()
    {
        if (wallrunning)
        {
            state = MovementState.wallrunning;
            moveSpeed = wallrunSpeed;
        }
        if(Input.GetKey(crouchKey))
        {
            state = MovementState.crouching;
            moveSpeed = crouchSpeed;
        }
        if(isGrounded && Input.GetKey(sprintKey))
        {
            state = MovementState.sprinting;
            moveSpeed = sprintSpeed;
            cam.DoFov(sprintFov);

        }
        else if(isGrounded)
        {
            state = MovementState.walking;
            moveSpeed = walkSpeed;
            cam.DoFov(normalFov);
        }
        else
        {
            state = MovementState.air;
            
        }
    }

    private void Start()
    {
        rb = GetComponent<Rigidbody>();
        rb.freezeRotation = true;

        isJumpReady = true;

        startYScale = transform.localScale.y;
    }
    private void Update()
    {
        if (!enableMovement) return;

        isGrounded = Physics.Raycast(transform.position, Vector3.down, playerHeight * 0.5f + 0.2f, whatIsGround);
        MyInput();
        SpeedControl();
        StateHandler();

        if (isGrounded)
        {
            rb.drag = groundDrag;
        }
        else
        {
            rb.drag = 0.0f;
        }
    }

    private void FixedUpdate()
    {
        if (!enableMovement) return;

        MovePlayer();
    }
    private void MyInput()
    {
        this.horizontalInput = Input.GetAxisRaw("Horizontal");
        this.verticalInput = Input.GetAxisRaw("Vertical");
        Debug.Log(this.horizontalInput);
        if(Input.GetKey(jumpKey) && isJumpReady && isGrounded)
        {
            isJumpReady = false;
            Jump();
            Invoke(nameof(ResetJump), jumpCooldown);
        }

        if(Input.GetKeyDown(crouchKey))
        {
            transform.localScale = new Vector3(transform.localScale.x, crouchYScale, transform.localScale.z);
            rb.AddForce(Vector3.down * 5.0f, ForceMode.Impulse);
        }

        if(Input.GetKeyUp(crouchKey))
        {
            transform.localScale = new Vector3(transform.localScale.x, startYScale, transform.localScale.z);
        }
    }

    private void MovePlayer()
    {
        this.moveDirection = orientation.forward * verticalInput + orientation.right * horizontalInput;

        if(OnSlope())
        {
            rb.AddForce(GetSlopeMoveDirection() * moveSpeed * 20.0f, ForceMode.Force);

            if (rb.velocity.y > 0)
            {
                rb.AddForce(Vector3.down * 80.0f, ForceMode.Force);
            }
        }

        if(isGrounded)
        {
            rb.AddForce(moveDirection.normalized * moveSpeed * 10.0f, ForceMode.Force);
            //PlaySFX_footsteps();

        }
        else if(!isGrounded)
        {
            rb.AddForce(moveDirection.normalized * moveSpeed * 10.0f * airMultiplier, ForceMode.Force);
           // PlaySFX_airGlide();
        }

        
        rb.useGravity = !OnSlope();

       
    }
    
    private void SpeedControl()
    {
        if(OnSlope() && !exitingSlope)
        {
            if(rb.velocity.magnitude > moveSpeed)
            {
                rb.velocity = rb.velocity.normalized * moveSpeed;
            }

        }
        else
        {
            Vector3 flatVel = new Vector3(rb.velocity.x, 0.0f, rb.velocity.z);

            if (flatVel.magnitude > moveSpeed)
            {
                Vector3 limitedVel = flatVel.normalized * moveSpeed;
                rb.velocity = new Vector3(limitedVel.x, rb.velocity.y, limitedVel.z);
            }
        }


        
    }

    private void Jump()
    {
        exitingSlope = true;

        rb.velocity = new Vector3(rb.velocity.x, 0.0f, rb.velocity.z);
        rb.AddForce(transform.up * jumpForce, ForceMode.Impulse);

        PlaySFX_jump();
    }

    private void ResetJump()
    {
        this.isJumpReady = true;

        exitingSlope = false;
    }

    private bool OnSlope()
    {
        if (Physics.Raycast(transform.position, Vector3.down, out slopeHit, playerHeight * 0.5f + 0.3f))
        {
            float angle = Vector3.Angle(Vector3.up, slopeHit.normal);
            return angle < maxSlopeAngle && angle != 0;
        }

        return false;
    }

    private Vector3 GetSlopeMoveDirection()
    {
        return Vector3.ProjectOnPlane(moveDirection, slopeHit.normal).normalized;
    }



    /*private void PlaySFX_footsteps()
    {
        float waitTime = 0.0f;

        if (state == MovementState.sprinting)
        {
            waitTime = 0.3f;
        }

        else
        {
            waitTime = 0.5f;
        }

            timer += Time.deltaTime;

        if(this.horizontalInput != 0 || this.verticalInput!= 0)
        {
            if (timer > waitTime)
            {
                //SoundFXManager.Instance.PlayRandomSFXClip(SFX_footsteps, transform, 1f);
                timer = 0;
            }
        }
    }*/

    private void PlaySFX_jump()
    {
        //SoundFXManager.Instance.PlaySFXClip(SFX_jump, transform, 1f);
    }
}
