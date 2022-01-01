using System;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    CharacterController characterController;
    private Camera cam;
    [NonSerialized]
    public bool freezeMovement = false;
    [NonSerialized]
    public bool freezeGravity = false;

    [Header("Run Time Modifiers")]
    public int moveMode;

    [Header("Movement")]
    public float baseSpeed = 3;
    public float cappedMovementSpeedChangeRate = 2.5f;
    [Space]
    public bool doSprint;
    [Space]
    public float sprintSpeedMultiplier = 2;
    public float sprintDuration = 10;
    public float sprintRechargeSpeed = 8;
    public float sprintRechargeWait = 10;
    public bool onlyRechargeWaitAtZeroStamina;

    private float horizontal;
    private float vertical;

    private float sprinting = 1;
    private float timeSinceSprint;
    [System.NonSerialized]
    public float availableSprint;

    private float movementCappedMaxSpeed;
    private float targetMovementCappedMaxSpeed;
    [Space]
    public float diagonalMultiplier = 0.75f;
    public float horizontalMultiplier = 0.75f;
    public float backwardsMultiplier = 0.75f;

    [Header("Acceleration")]
    public float accelerationTime = 1.5f;
    [Range(0, 1)]
    public float startingAcceleration = 0.4f;
    
    private float currentAcceleration;

    [Header("Jump")]
    public bool doJump;
    [Space]
    public float jumpStrength;

    public float groundDetectionRange = 0.9f;

    [Header("Gravity")]

    public float gravityScale = 1f;
    const float gravity = -9.81f;

    private float velocity = 0;
    private bool grounded;

    [Header("Crouch")]
    public bool doCrouch;
    [Space]
    public bool toggleCrouch;
    public bool SprintWhileCrouching;
    [Space]
    [Range(0, 1)]
    public float crouchHeightMultiplier = 0.625f;
    [Range(0, 1)]
    public float crouchSpeedMultiplier = 0.625f;
    private float initalYScale;
    private float crouching = 1;

    /* [Header("Head Bob")]
    [SerializeField] private bool useHeadBob;

    [SerializeField] private float walkBobSpeed = 14f;
    [SerializeField] private float walkBobAmount = 0.5f;
    [Space]
    [SerializeField] private float sprintBobSpeed = 18f;
    [SerializeField] private float sprintBobAmount = 1f;
    [Space]
    [SerializeField] private float crouchBobSpeed = 8f;
    [SerializeField] private float crouchBobAmount = 0.25f;

    [System.NonSerialized] public float defaultYposition;
    private float timer;*/


    private void Start()
    {
        characterController = GetComponent<CharacterController>();
        cam = Camera.main;
        movementCappedMaxSpeed = baseSpeed;
        availableSprint = sprintDuration;
        initalYScale = transform.localScale.y;
    }

    void Update()
    {
        horizontal = Input.GetAxis("Horizontal") * movementCappedMaxSpeed;
        vertical = Input.GetAxis("Vertical") * movementCappedMaxSpeed;

        if (sprinting == sprintSpeedMultiplier && (horizontal != 0 || vertical != 0))
        {
            moveMode = 2;
        }
        else if (horizontal != 0 || vertical != 0)
        {
            moveMode = 1;
        }
        else
        {
            moveMode = 0;
        }

        if (!freezeMovement)
        {
            Acceleration();
            Movement();
        }
        if (!freezeGravity)
        {
            Gravity();
        }
    }

    private void Gravity()
    {
        if (Physics.Raycast(transform.position, Vector3.down, groundDetectionRange))
        {
            grounded = true;
            velocity = 0;
        }
        else
        {
            grounded = false;
            velocity += gravity * gravityScale * Time.deltaTime;
        }
        if (Input.GetKeyDown(KeyCode.Space) && doJump && grounded)
        {
            velocity = jumpStrength;
        }
        characterController.Move(new Vector3(0, velocity, 0) * Time.deltaTime);
    }


    private void Movement()
    {
        if (toggleCrouch && doCrouch)
        {
            if (Input.GetKeyDown(KeyCode.LeftControl))
            {
                if (crouching == 1)
                {
                    crouching = crouchSpeedMultiplier;
                    transform.localScale = new Vector3(transform.localScale.x, initalYScale * crouchHeightMultiplier, transform.localScale.z);
                    //remove for other projects
                    transform.GetChild(0).transform.GetChild(0).localScale = new Vector3(0.8f , 1, 0.8f * (2 - crouchHeightMultiplier));

                }
                else
                {
                    crouching = 1;
                    transform.localScale = new Vector3(transform.localScale.x, initalYScale, transform.localScale.z);
                    transform.GetChild(0).transform.GetChild(0).localScale = new Vector3(0.8f, 1, 0.8f);
                }
            }
        }
        else if (doCrouch)
        {
            if (Input.GetKey(KeyCode.LeftControl))
            {
                crouching = crouchSpeedMultiplier;
                transform.localScale = new Vector3(transform.localScale.x, initalYScale * crouchHeightMultiplier, transform.localScale.z);
                transform.GetChild(0).transform.GetChild(0).localScale = new Vector3(0.8f, 1, 0.8f * (2 - crouchHeightMultiplier));
            }
            else
            {
                crouching = 1;
                transform.localScale = new Vector3(transform.localScale.x, initalYScale, transform.localScale.z);
                transform.GetChild(0).transform.GetChild(0).localScale = new Vector3(0.8f, 1, 0.8f);
            }
        }
        
        if (doSprint)
        {
            if (Input.GetKey(KeyCode.LeftShift) && availableSprint > 0 && crouching != 1 && !SprintWhileCrouching)
            {
                SprintRegen();
            }
            else if (Input.GetKey(KeyCode.LeftShift) && availableSprint > 0)
            {
                sprinting = sprintSpeedMultiplier;
                availableSprint -= Time.deltaTime;
                timeSinceSprint = 0;
            }
            else
            {
                SprintRegen();
            }
        }
        

        //ajust max speed
        if (targetMovementCappedMaxSpeed > movementCappedMaxSpeed)
        {
            movementCappedMaxSpeed += Time.deltaTime * cappedMovementSpeedChangeRate;
            if (movementCappedMaxSpeed > targetMovementCappedMaxSpeed)
            {
                movementCappedMaxSpeed = targetMovementCappedMaxSpeed;
            }
        }
        else if (targetMovementCappedMaxSpeed < movementCappedMaxSpeed)
        {
            movementCappedMaxSpeed -= Time.deltaTime * cappedMovementSpeedChangeRate;
            if (movementCappedMaxSpeed < targetMovementCappedMaxSpeed)
            {
                movementCappedMaxSpeed = targetMovementCappedMaxSpeed;
            }
        }

        

        //target max speed
        if (vertical != 0 && horizontal != 0)
        {
            if (vertical < 0)
            {
                targetMovementCappedMaxSpeed = baseSpeed * diagonalMultiplier * backwardsMultiplier;
            }
            else
            {
                targetMovementCappedMaxSpeed = baseSpeed * diagonalMultiplier;
            }
        }
        else if (vertical == 0 && horizontal != 0)
        {
            targetMovementCappedMaxSpeed = baseSpeed * horizontalMultiplier;
        }
        else if (horizontal == 0 && vertical < 0)
        {
            targetMovementCappedMaxSpeed = baseSpeed * backwardsMultiplier;
        }
        else
        {
            targetMovementCappedMaxSpeed = baseSpeed;
        }

        characterController.Move((transform.right * horizontal + transform.forward * vertical) * (currentAcceleration / accelerationTime) * sprinting * crouching * Time.deltaTime);
    }

    private void Acceleration()
    {
        if (horizontal == 0 && vertical == 0)
        {
            currentAcceleration = startingAcceleration * accelerationTime;
        }
        else if (currentAcceleration < accelerationTime)
        {
            currentAcceleration += Time.deltaTime;
        }
        else
        {
            currentAcceleration = accelerationTime;
        }
    }

    private void SprintRegen()
    {
        sprinting = 1;
        timeSinceSprint += Time.deltaTime;
        if (timeSinceSprint > sprintRechargeWait || (onlyRechargeWaitAtZeroStamina == true && availableSprint != 0))
        {
            if (availableSprint < sprintDuration)
            {
                availableSprint += sprintDuration / sprintRechargeSpeed * Time.deltaTime;
            }
        }
        if (availableSprint < 0)
        {
            availableSprint = 0;
        }
        else if (availableSprint > sprintDuration)
        {
            availableSprint = sprintDuration;
        }
    }

}


