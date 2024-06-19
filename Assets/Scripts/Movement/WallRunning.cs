using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WallRunning : MonoBehaviour
{
    [Header("Wallrunning")]
    [SerializeField]
    private LayerMask whatIsWall;
    [SerializeField]
    private LayerMask whatIsGround;
    [SerializeField]
    private float wallRunForce;
    [SerializeField]
    private float wallJumpSideForce;
    [SerializeField]
    private float wallJumpForce;

    [SerializeField]
    private float wallClimbSpeed;
    [SerializeField]
    private float maxWallRunTime;
    private float wallRunTimer;

    [Header("Input")]
    private float horizontalInput;
    private float verticalInput;
    public KeyCode jumpKey = KeyCode.Space;

    private bool upwardsRunning;
    private bool downwardsRunning;

    [Header("Detection")]
    [SerializeField]
    private float wallCheckDistance;
    [SerializeField]
    private float minJumpHeight;
    private RaycastHit leftWallHit;
    private RaycastHit rightWallHit;
    private bool wallLeft;
    private bool wallRight;

    [Header("Exiting")]
    private bool exitingWall;
    [SerializeField]
    private float exitWallTime;
    [SerializeField]
    private float exitWallTimer;

    [Header("References")]
    [SerializeField]
    private Transform orientation;
    [SerializeField]
    private PlayerLook cam;
    [SerializeField] 
    private Camera playerCamera;
    private PlayerMovement pm;
    private Rigidbody rb;

    [Header("Sound Effects")]
    [SerializeField] private AudioClip SFX_jump;

    private void Start()
    {
        rb = GetComponent<Rigidbody>();
        pm = GetComponent<PlayerMovement>();
    }

    private void Update()
    {
        CheckForWall();
        StateMachine();
    }

    private void FixedUpdate()
    {
        if(pm.wallrunning)
        {
            WallRunningMovement();
        }
    }

    private void CheckForWall()
    {
        wallRight = Physics.Raycast(transform.position, orientation.right, out rightWallHit, wallCheckDistance, whatIsWall);
        wallLeft = Physics.Raycast(transform.position, -orientation.right, out leftWallHit, wallCheckDistance, whatIsWall);
    }

    private bool AboveGround()
    {
        return !Physics.Raycast(transform.position, Vector3.down, minJumpHeight, whatIsGround);
    }

    private void StateMachine()
    {
        // Determine if looking up or down based on the camera pitch
        float pitch = playerCamera.transform.localEulerAngles.x;
        if (pitch > 180) pitch -= 360; // Normalize pitch to the range -180 to 180

        upwardsRunning = (pitch < -10);   // Adjust threshold as necessary
        downwardsRunning = (pitch > 10);  // Adjust threshold as necessary

        if ((wallLeft || wallRight) && AboveGround() && !exitingWall)
        {
            if (!pm.wallrunning)
            {
                StartWallRun();
            }

            if(Input.GetKeyDown(jumpKey))
            {
                WallJump();
            }
        }

        else if(exitingWall)
        {
            if(pm.wallrunning)
            {
                StopWallRun();
            }
            if(exitWallTimer > 0)
            {
                exitWallTimer -= Time.deltaTime;
            }
            if(exitWallTimer <= 0)
            {
                exitingWall = false;
            }
        }
        else
        {
            if (pm.wallrunning)
            {
                StopWallRun();
            }
        }
    }

    private void StartWallRun()
    {
        pm.wallrunning = true;

        cam.DoFov(90.0f);
        if(wallLeft)
        {
            cam.DoTilt(-5.0f);
        }
        if (wallRight)
        {
            cam.DoTilt(5.0f);
        }
    }

    private void WallRunningMovement()
    {
        rb.useGravity = false;
        rb.velocity = new Vector3(rb.velocity.x, 0.0f, rb.velocity.z);

        Vector3 wallNormal = wallRight ? rightWallHit.normal : leftWallHit.normal;

        Vector3 wallForward = Vector3.Cross(wallNormal, transform.up);

        if((orientation.forward - wallForward).magnitude > (orientation.forward - -wallForward).magnitude) 
        {
            wallForward = -wallForward;
        }

        rb.AddForce(wallForward * wallRunForce, ForceMode.Force);

        if(upwardsRunning)
        {
            rb.velocity = new Vector3(rb.velocity.x, wallClimbSpeed, rb.velocity.z);
        }
        if (downwardsRunning)
        {
            rb.velocity = new Vector3(rb.velocity.x, -wallClimbSpeed, rb.velocity.z);
        }
        if (!(wallLeft && horizontalInput > 0) && !(wallRight && horizontalInput < 0))
        {
            rb.AddForce(-wallNormal * 100, ForceMode.Force);
        }
        

    }

    private void StopWallRun()
    {
        pm.wallrunning = false;

        cam.DoFov(80.0f);
        cam.DoTilt(0.0f);
    }

    private void WallJump()
    {
        exitingWall = true;
        exitWallTimer = exitWallTime;

        Vector3 wallNormal = wallRight ? rightWallHit.normal : leftWallHit.normal;

        Vector3 forceToApply = transform.up * wallJumpForce + wallNormal * wallJumpSideForce;

        rb.velocity = new Vector3(rb.velocity.x, 0.0f, rb.velocity.z);
        rb.AddForce(forceToApply, ForceMode.Impulse);

        //PlaySFX_jump();
    }

    /*private void PlaySFX_jump()
    {
        SoundFXManager.Instance.PlaySFXClip(SFX_jump, transform, 1f);
    }*/
}
