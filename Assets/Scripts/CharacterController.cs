using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.Tracing;
using System.Runtime.CompilerServices;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

public class CharacterController : MonoBehaviour
{
    //NonPhysical한 움직임을 위해 Rigidbody Component의 Gravity는 꺼두자!

    [Header("Move Component")]
    public float moveSpeed = 5;
    [Space]

    [Header("Jump Component")]
    public float jumpingGravity = 1;
    public float fallingGravity = 3;
    public float jumpPower = 6;
    float jumpVelocity;
    float gravity = -9.81f;
    [Space]

    [Header("Grounded Check Component")]
    public float groundedDistance = 0.55f;
    public float groundedWidth = 0.45f;
    bool firstPoint;
    bool secondPoint;
    bool isGrounded;
    [Space]

    [Header("Wall Interact Component")]
    public float stickedSlipSpeed = -0.5f;
    public float stickedJumpingGravity = 0.5f;
    public float stickedFallingGravity = 0.5f;
    public float stickedJumpTime = 0.6f;
    public float stickedJumpPower = 7;
    public float stickedMoveDistance = 2;
    float stickedJumpVelocity;
    float stickedMoveVelocity;
    bool isSticked;
    bool isStickedJump;

    [Header("Sticked Check Component")]
    public float stickedDistance = 0.55f;
    [Space]

    [Header("Dash Component")]
    public float dashCoolTime;                  //대쉬 쿨타임
    public float dashSpeed = 5.0f;              //대쉬 스피드
    public float dashTime = 0.5f;               //대쉬 시간
    public float dashInputTime = 0.3f;          //더블입력 받는 시간
    public bool canDashWhileJumping = true;     //점프 중 대쉬를 허용할 것 인가
    enum Dir { NONE, RIGHT, LEFT };
    Dir keyDowned;
    bool isDash;
    bool canDash;
    bool rightDash;
    bool leftDash;
    Coroutine DashCheck;
    [Space]

    Rigidbody2D rigid;
    SpriteRenderer spriteRenderer;
    Vector2 move;
    Vector2 forward;

    void Start()
    {
        isDash = false; canDash = true;  rightDash = false; leftDash = false;
        rigid = GetComponent<Rigidbody2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    void Update()
    {
        Move();
        StickedAction();
        Jump();
        Dash();
    }

    void FixedUpdate()
    {
        GroundedCheck();
        StickedCheck();
    }

    void Move()
    {
        move = new Vector2(Input.GetAxisRaw("Horizontal"), rigid.velocity.y);
        if (!isStickedJump)
        {
            forward = new Vector2(Input.GetAxisRaw("Horizontal"), 0);
            rigid.velocity = move * moveSpeed;

            if (Input.GetAxisRaw("Horizontal") > 0) spriteRenderer.flipX = false;
            else if (Input.GetAxisRaw("Horizontal") < 0) spriteRenderer.flipX = true;
        }
    }

    void Jump()
    {
        if (!isSticked)
        {
            if (jumpVelocity >= 0) jumpVelocity += gravity * jumpingGravity * Time.deltaTime; //상승
            else if (jumpVelocity < 0) jumpVelocity += gravity * fallingGravity * Time.deltaTime; //하강

            if (isGrounded && Input.GetKeyDown(KeyCode.Space)) //점프
            {
                jumpVelocity = jumpPower;
            }

            if (isGrounded && jumpVelocity < 0) jumpVelocity = 0; //땅에 닿아있고, 게속 추락하는 상태라면 정지상태로 한다.
            transform.Translate(new Vector2(0, jumpVelocity * Time.deltaTime)); //점프의 작용
        }
    }

    void StickedAction()
    {
        if (isSticked) jumpVelocity = 0;                //벽에 붙은 상태라면 점프 상태를 취소한다.

        if (isSticked && !isGrounded)                   //벽에 닿고 땅에 땋지 않은 상태라면 미끄러진다.
            transform.Translate(new Vector2(0, stickedSlipSpeed * Time.deltaTime));

        if (isSticked && !isGrounded)
        {
            if (!isStickedJump && Input.GetKeyDown(KeyCode.Space)) //점프
            {
                isStickedJump = true;
                stickedJumpVelocity = stickedJumpPower;
                Invoke("StickedJumpTime", stickedJumpTime);
            }
        }

        if (isStickedJump)
        {
            if (stickedJumpVelocity >= 0) stickedJumpVelocity += gravity * stickedJumpingGravity * Time.deltaTime; //상승
            else if (stickedJumpVelocity < 0) stickedJumpVelocity += gravity * stickedFallingGravity * Time.deltaTime; //하강

            if (stickedJumpVelocity >= 0) stickedMoveVelocity = stickedJumpVelocity;
            else if (stickedJumpVelocity < 0) stickedMoveVelocity = -1 * stickedJumpVelocity;

            transform.Translate(new Vector2(-1 * stickedMoveDistance * forward.x * stickedJumpVelocity * Time.deltaTime, stickedJumpVelocity * Time.deltaTime));
        }

        if (isStickedJump && isGrounded) stickedJumpVelocity = 0;
    }

    void Dash()
    {
        if (rightDash || leftDash)
        {
            //대쉬 구현
            if (canDash)
            {
                isDash = true;
                StartCoroutine(Dashing());

                canDash = false;
                StartCoroutine(DashCoolDown());
            }
        }

        if (!isDash && canDash)
        {            
            if (keyDowned == Dir.RIGHT)     //오른쪽 대쉬
            {
                if (Input.GetKeyDown(KeyCode.D) || Input.GetKeyDown(KeyCode.RightArrow))
                {
                    rightDash = true;
                    Debug.Log("오른쪽 대쉬!");
                }
                else if (Input.GetKeyDown(KeyCode.A) || Input.GetKeyDown(KeyCode.LeftArrow))
                {
                    keyDowned = Dir.LEFT;
                    Debug.Log("대쉬 취소!");
                }
                if (DashCheck != null) StopCoroutine(DashCheck);
            }
            else if (keyDowned == Dir.LEFT) //왼쪽 대쉬
            {
                if (Input.GetKeyDown(KeyCode.A) || Input.GetKeyDown(KeyCode.LeftArrow))
                {
                    leftDash = true;
                    Debug.Log("왼쪽 대쉬!");
                }
                else if (Input.GetKeyDown(KeyCode.D) || Input.GetKeyDown(KeyCode.RightArrow))
                {
                    keyDowned = Dir.RIGHT;
                    Debug.Log("대쉬 취소!");
                }
                if (DashCheck != null) StopCoroutine(DashCheck);
            }

            //키 입력
            if (Input.GetKeyDown(KeyCode.D) || Input.GetKeyDown(KeyCode.RightArrow))
            {
                keyDowned = Dir.RIGHT;
                Debug.Log("오른쪽 다운!");
                //if (DashCheck != null) StopCoroutine(DashCheck);
                DashCheck = StartCoroutine(KeyDowned());
            }
            else if (Input.GetKeyDown(KeyCode.A) || Input.GetKeyDown(KeyCode.LeftArrow))
            {
                keyDowned = Dir.LEFT;
                Debug.Log("왼쪽 다운!");
                //if(DashCheck != null) StopCoroutine(DashCheck);
                DashCheck = StartCoroutine(KeyDowned());
            }
        }


        IEnumerator DashCoolDown()
        {
            Debug.Log("대쉬 쿨 도는 중...");
            yield return new WaitForSeconds(dashCoolTime);
            Debug.Log("대쉬 쿨 다 돔!");
            canDash = true;
        }
        IEnumerator KeyDowned()
        {
            Debug.Log("대쉬입력 시작!");
            yield return new WaitForSeconds(0.3f); //dashInputTime
            Debug.Log("대쉬입력시간 초과!");
            keyDowned = Dir.NONE;
        }
        IEnumerator Dashing()
        {
            Debug.Log("대쉬!");
            yield return new WaitForSeconds(dashTime);
            isDash = false; rightDash = false; leftDash = false;
        }
    }

    void GroundedCheck()
    {
        Debug.DrawRay(new Vector2(transform.position.x + groundedWidth, transform.position.y), Vector2.down * groundedDistance, Color.blue);
        if (Physics2D.Raycast(new Vector2(transform.position.x + groundedWidth, transform.position.y), Vector2.down, groundedDistance, LayerMask.GetMask("Floor"))) firstPoint = true;
        else firstPoint = false;
        Debug.DrawRay(new Vector2(transform.position.x - groundedWidth, transform.position.y), Vector2.down * groundedDistance, Color.blue);
        if (Physics2D.Raycast(new Vector2(transform.position.x - groundedWidth, transform.position.y), Vector2.down, groundedDistance, LayerMask.GetMask("Floor"))) secondPoint = true;
        else secondPoint = false;

        if (!firstPoint && !secondPoint) isGrounded = false;
        else isGrounded = true;
    }

    void StickedCheck()
    {
        Debug.DrawRay(transform.position, forward * stickedDistance, Color.green);      //벽에 붙어 있는 것을 체크한다.
        if (Physics2D.Raycast(transform.position, forward, stickedDistance, LayerMask.GetMask("Wall"))) isSticked = true;
        else isSticked = false;

        if (isStickedJump && isGrounded)                       //벽점프중 땅에 충돌한다면
        {
            isStickedJump = false;
            CancelInvoke();
        }

        if (isStickedJump)
        {
            Debug.DrawRay(transform.position, new Vector2(-1 * forward.x, forward.y) * stickedDistance, Color.green);    //벽점프 중 반대 벽에 충돌하는 것을 체크한다.
            if (Physics2D.Raycast(transform.position, new Vector2(-1 * forward.x, forward.y), stickedDistance, LayerMask.GetMask("Wall")))
            {
                CancelInvoke();
                isStickedJump = false;
            }
        }
    }

    void StickedJumpTime()
    {
        isStickedJump = false;
    }
}