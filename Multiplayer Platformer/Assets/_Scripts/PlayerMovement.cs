using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;
using Photon.Pun;

public class PlayerMovement : MonoBehaviourPun
{
    #region Variables

    [HideInInspector]
    public bool canMove = true;

    [Header("Character Stats")]
    public float moveSpeed = 18f;
    public float jumpHeight = 2f;
    public bool dash;
    public bool airJump;

    [Header("Movement")]
    public float groundAcceleration = .35f;
    public float airAcceleration = .1f;
    public float currentSpeed;

    [Header("Gravity")]
    public float gravityScale = 4f;
    public float fallGravityScale = 12f;
    float gravityConstant = -9.81f;
    float gravity;

    [Header("Jump Checks")]
    public float hangTime = .1f;
    public float jumpBuffer = .1f;
    float hangTimeCounter;
    float jumpBufferCounter;
    bool isGrounded;
    bool isJumping;
    bool canLand;

    [Header("Ability")]
    public int abilityCount = 3;
    public int maxAbilityCount = 5;

    [Header("Dash")]
    public float dashSpeedMultiplier = 3f;
    public float dashDuration = 1f;
    int dashRepeat = 2;
    bool dashing;

    [Header("Air Jump")]
    public float airJumpHeight = 3f;
    public float airJumpSpeedMultiplier = 1.5f;
    int airJumpRepeat = 2;
    bool airJumping;

    [Header("Boosting")]
    public float boostSpeedMultiplier = 1.5f;
    public float boostDuration = .3f;
    bool boosting;
    Vector3 boostDir;

    [Header("Respawning")]
    public float respawnTime = 1f;
    Vector3 respawnPos;

    [Header("Other")]
    public float groundDistance = .5f;
    public float turnTime = 0.1f;
    float turnSmoothVelocity;
    bool canFinishLap = true;
    Vector3 direction;
    Vector3 velocity;
    Vector3 moveDir;
    bool autoRun;

    [Header("Components")]
    public CharacterController controller;
    public PlayerManager playerManager;
    public GameObject graphics;
    public Transform camDirection;
    public GameObject followCam;
    public Camera mainCam;
    public Camera rearviewCam;
    public Transform groundCheck;
    public LayerMask playerMask;
    public LayerMask groundMask;
    public LayerMask respawnMask;
    public LayerMask finishMask;
    public LayerMask boostMask;
    public LayerMask movingMask;
    public Animator animator;
    public ParticleSystem dust;
    public TrailRenderer dashTrail;
    public ParticleSystem airJumpParticles;

    #endregion

    private void Start()
    {
        canMove = false;

        if (!photonView.IsMine && PhotonNetwork.IsConnected)
        {
            followCam.SetActive(false);
            return;
        }

        gravity = gravityConstant * gravityScale;
        autoRun = PlayerPrefs.GetInt("autoRun") == 1 ? true : false;
    }

    private void Update()
    {
        if (!photonView.IsMine && PhotonNetwork.IsConnected)
        {
            return;
        }

        RearviewCamera();

        if (!canMove)
        {
            return;
        }

        Dash();
        AirJump();
        Boost();

        Move();
        CheckGrounded();
        CheckJumpAllowed();
        Jump();
        ApplyGravity();
        CheckMovingPlatform();

        CheckRespawn();
        CheckLapComplete();
    }

    void RearviewCamera()
    {
        if(Input.GetMouseButtonDown(1))
        {
            mainCam.enabled = false;
            rearviewCam.enabled = true;
        }      
        
        if(Input.GetMouseButtonUp(1))
        {
            rearviewCam.enabled = false;
            mainCam.enabled = true;
        }
    }

    void Move()
    {

        float horizontal = Input.GetAxisRaw("Horizontal");
        float vertical = Input.GetAxisRaw("Vertical");

        if (autoRun && vertical != -1f)
        {
            vertical = 1f;
        }

        direction = new Vector3(horizontal, 0f, vertical).normalized;

        if (dashing || direction.magnitude != 0)
        {
            float targetAngle = Mathf.Atan2(direction.x, direction.z) * Mathf.Rad2Deg + camDirection.eulerAngles.y;
            float angle = Mathf.SmoothDampAngle(transform.eulerAngles.y, targetAngle, ref turnSmoothVelocity, turnTime);            

            if (!dashing)
            {
                transform.rotation = Quaternion.Euler(0f, angle, 0f);
                moveDir = Quaternion.Euler(0f, targetAngle, 0f) * Vector3.forward;             
            }

            if (!boosting && !dashing && !airJumping)
            {
                currentSpeed = Mathf.Lerp(currentSpeed, moveSpeed, isGrounded ? groundAcceleration : airAcceleration);
            }

            if (!boosting)
            {
                controller.Move(moveDir.normalized * currentSpeed * Time.deltaTime);
            }

            animator.SetBool("running", true);
        }
        else
        {
            if (!boosting && !airJumping && !boosting)
            {
                currentSpeed = Mathf.Lerp(currentSpeed, 0f, isGrounded ? groundAcceleration : airAcceleration);
                controller.Move(moveDir.normalized * currentSpeed * Time.deltaTime);
            }

            animator.SetBool("running", false);
        }
    }

    void CheckGrounded()
    {
        isGrounded = Physics.CheckSphere(groundCheck.position, groundDistance, groundMask + boostMask + finishMask + movingMask);

        if (isGrounded && velocity.y < 0f)
        {
            velocity.y = -5f;
        }
        
        if(!isGrounded)
        {
            canLand = true;
        }

        if(canLand && isGrounded)
        {
            dust.Play();
            canLand = false;
        }

        animator.SetBool("jumping", !isGrounded);
    }

    void CheckJumpAllowed()
    {
        if (isGrounded)
        {
            hangTimeCounter = hangTime;
        }
        else
        {
            hangTimeCounter -= Time.deltaTime;
        }

        if (Input.GetButtonDown("Jump"))
        {
            jumpBufferCounter = jumpBuffer;
        }
        else
        {
            jumpBufferCounter -= Time.deltaTime;
        }
    }

    void Jump()
    {
        if (!isJumping && jumpBufferCounter > 0f && hangTimeCounter > 0f)
        {
            velocity.y = Mathf.Sqrt(jumpHeight * -2f * gravity);
            jumpBufferCounter = 0f;
            StartCoroutine(JumpCooldown());
        }

        if (velocity.y < 0)
        {
            gravity = gravityConstant * fallGravityScale;
        }
        else
        {
            gravity = gravityConstant * gravityScale;
        }

        if (Input.GetButtonUp("Jump") && velocity.y > 0f && !airJumping)
        {
            velocity.y *= .6f;
            hangTimeCounter = 0f;
        }
    }

    IEnumerator JumpCooldown()
    {
        isJumping = true;
        yield return new WaitForSeconds(.4f);
        isJumping = false;
    }

    void ApplyGravity()
    {
        if(dashing)
        {
            return;
        }

        velocity.y += gravity * Time.deltaTime;
        controller.Move(velocity * Time.deltaTime);
    }

    void Dash()
    {
        if(!dash)
        {
            return;
        }

        if (!dashing && dashRepeat > 0 && abilityCount > 0 && Input.GetKeyDown(KeyCode.E))
        {
            StartCoroutine(StartDash());
        }
    }

    IEnumerator StartDash()
    {
        dashing = true;
        abilityCount--;
        dashRepeat--;

        moveDir = transform.forward;
        currentSpeed = moveSpeed * dashSpeedMultiplier;
        animator.speed = dashSpeedMultiplier;
        dashTrail.emitting = true;
        yield return new WaitForSeconds(dashDuration);

        velocity.y = 0f;
        animator.speed = 1f;
        dashTrail.emitting = false;

        dashing = false;

        yield return new WaitUntil(() => isGrounded);
        dashRepeat = 2;
    }

    void AirJump()
    {
        if(!airJump)
        {
            return;
        }

        if (!airJumping && airJumpRepeat > 0 && abilityCount > 0 && Input.GetKeyDown(KeyCode.E))
        {
            airJumping = true;
            abilityCount--;
            airJumpRepeat--;

            velocity.y = Mathf.Sqrt(airJumpHeight * -2f * gravityConstant * fallGravityScale);
            
            airJumpParticles.Play();
        }

        if(airJumping && velocity.y < 0f)
        {
            if(direction.magnitude == 0)
            {
                currentSpeed = 0f;
            }

            airJumping = false;
            StartCoroutine(ResetAirJump());
        }

        if(airJumping)
        {
            if (direction.magnitude == 0)
            {
                currentSpeed = Mathf.Lerp(currentSpeed, 0f, isGrounded ? groundAcceleration : airAcceleration);
                controller.Move(moveDir.normalized * currentSpeed * Time.deltaTime);
            }
            else
            {
                currentSpeed = moveSpeed * airJumpSpeedMultiplier;
            }
        }
    }

    IEnumerator ResetAirJump()
    {
        yield return new WaitUntil(() => isGrounded);
        airJumpRepeat = 2;
    }

    void Boost()
    {
        if(dashing || airJumping)
        {
            if (boosting)
            {
                animator.speed = 1f;
                boosting = false;
            }

            return;
        }

        if (Physics.CheckSphere(groundCheck.position, groundDistance, boostMask))
        {
            boosting = true;
            boostDir = Physics.OverlapSphere(groundCheck.position, groundDistance, boostMask)[0].transform.right * -1;
            animator.speed = boostSpeedMultiplier;
            StartCoroutine(EndBoost());
        }

        if(boosting)
        {
            currentSpeed = moveSpeed * boostSpeedMultiplier;
            controller.Move(boostDir * currentSpeed * Time.deltaTime);
        }
    }

    IEnumerator EndBoost()
    {
        yield return new WaitUntil(() => !Physics.CheckSphere(groundCheck.position, groundDistance, boostMask));
        yield return new WaitForSeconds(boostDuration);

        animator.speed = 1f;
        boosting = false;
    }

    void CheckMovingPlatform()
    {
        if(Physics.CheckSphere(groundCheck.position, groundDistance, movingMask))
        {
            gameObject.transform.SetParent(Physics.OverlapSphere(groundCheck.position, groundDistance, movingMask)[0].transform, true);
        }
        else
        {
            gameObject.transform.parent = null;
        }
    }

    void CheckRespawn()
    { 
        if (Physics.CheckSphere(groundCheck.position, groundDistance, groundMask))
        {
            GameObject platform = Physics.OverlapSphere(groundCheck.position, groundDistance, groundMask)[0].gameObject;
            
            if(platform.transform.childCount > 0)
            {
                respawnPos = platform.transform.GetChild(0).transform.position;
            }
        }

        if (Physics.CheckSphere(groundCheck.position, groundDistance, respawnMask))
        {
            transform.position = respawnPos;
            velocity.y = 0f;
            currentSpeed = 0f;
            StartCoroutine(Respawn());
        }
    }

    IEnumerator Respawn()
    {
        canMove = false;

        AddAbility();

        for (int i = 0; i < 6; i++)
        {
            graphics.SetActive(!graphics.activeSelf);
            yield return new WaitForSeconds(respawnTime / 6f);
        }

        canMove = true;
    }

    void CheckLapComplete()
    {
        if (canFinishLap && Physics.CheckSphere(groundCheck.position, groundDistance, finishMask))
        {
            canFinishLap = false;
            playerManager.finishedLap = true;

            StartCoroutine(ResetLapCheck());
        }
    }

    IEnumerator ResetLapCheck()
    {
        yield return new WaitForSeconds(2f);
        canFinishLap = true;
    }

    public void AddAbility()
    {
        if (abilityCount < maxAbilityCount)
        {
            abilityCount++;
        }
    }
}
