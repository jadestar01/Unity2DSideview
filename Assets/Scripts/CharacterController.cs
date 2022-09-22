using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharacterController : MonoBehaviour
{
    //NonPhysical한 움직임을 위해 Rigidbody Component의 Gravity는 꺼두자!

    [Header("Move Component")]
    public float moveSpeed = 5;
    [Space]

    [Header("Jump Component")]
    public float jumpingGravity = 1;
    public float fallingGravity = 3;
    public float jumpPower = 5;
    float jumpVelocity;
    float gravity = -9.81f;
    public float jumpHeight = 5;
    [Space]

    [Header("Grounded Check Component")]
    public float groundedDistance = 0.55f;
    public float groundedWidth = 0.45f;
    bool isGrounded;
    [Space]

    [Header("Wall Interact Component")]
    public float stickedDistance = 0.55f;
    public float stickedSlipSpeed = -0.5f;
    public float stickedJumpingGravity = 0.5f;
    public float stickedFallingGravity = 0.5f;
    public float stickedJumpTime = 0.6f;
    public float stickedJumpPower = 7;
    public float stickedMoveDistance = 2;
    float stickedJumpVelocity;
    bool isSticked;
    bool isStickedJump;

    Rigidbody2D rigid;
    SpriteRenderer spriteRenderer;
    Vector2 move;
    Vector2 forward;
    Vector2 backward;

    void Start()
    {
        isGrounded = true;
        rigid = GetComponent<Rigidbody2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    void Update()
    {
        Move();
        GroundedCheck();
        StickedCheck();
        StickedAction();
        Jump();
    }

    void Move()
    {
        if (!isStickedJump)
        {
            move = new Vector2(Input.GetAxisRaw("Horizontal"), rigid.velocity.y);
            forward = new Vector2(Input.GetAxisRaw("Horizontal"), 0);
            backward = new Vector2(-1 * Input.GetAxisRaw("Horizontal"), 0);
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

            transform.Translate(new Vector2(-1 * stickedMoveDistance * forward.x * stickedJumpVelocity * Time.deltaTime, stickedJumpVelocity * Time.deltaTime));
        }

        if (isStickedJump && isGrounded) stickedJumpVelocity = 0;
    }

    void GroundedCheck()
    {
        Debug.DrawRay(new Vector2(transform.position.x + groundedWidth, transform.position.y), Vector2.down * groundedDistance, Color.blue);
        if (Physics2D.Raycast(new Vector2(transform.position.x + groundedWidth, transform.position.y), Vector2.down, groundedDistance, LayerMask.GetMask("Floor"))) isGrounded = true;
        else isGrounded = false;
        Debug.DrawRay(new Vector2(transform.position.x - groundedWidth, transform.position.y), Vector2.down * groundedDistance, Color.blue);
        if (Physics2D.Raycast(new Vector2(transform.position.x - groundedWidth, transform.position.y), Vector2.down, groundedDistance, LayerMask.GetMask("Floor"))) isGrounded = true;
        else isGrounded = false;
    }

    void StickedCheck()
    {
        Debug.DrawRay(transform.position, forward * stickedDistance, Color.green);      //벽에 붙어 있는 것을 체크한다.
        if (Physics2D.Raycast(transform.position, forward, stickedDistance, LayerMask.GetMask("Wall"))) isSticked = true;
        else isSticked = false;

        if (isStickedJump)
        {
            Debug.DrawRay(transform.position, backward * stickedDistance, Color.green);    //벽점프 중 반대 벽에 충돌하는 것을 체크한다.
            if (Physics2D.Raycast(transform.position, backward, stickedDistance, LayerMask.GetMask("Wall")))
            {
                CancelInvoke("StickedJumpTIme");
                isStickedJump = false;
            }
        }
    }

    void StickedJumpTime()
    {
        isStickedJump = false;
    }
}