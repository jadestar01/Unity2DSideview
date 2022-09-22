using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharacterController : MonoBehaviour
{
    //NonPhysical�� �������� ���� Rigidbody Component�� Gravity�� ������!

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
            if (jumpVelocity >= 0) jumpVelocity += gravity * jumpingGravity * Time.deltaTime; //���
            else if (jumpVelocity < 0) jumpVelocity += gravity * fallingGravity * Time.deltaTime; //�ϰ�

            if (isGrounded && Input.GetKeyDown(KeyCode.Space)) //����
            {
                jumpVelocity = jumpPower;
            }

            if (isGrounded && jumpVelocity < 0) jumpVelocity = 0; //���� ����ְ�, �Լ� �߶��ϴ� ���¶�� �������·� �Ѵ�.
            transform.Translate(new Vector2(0, jumpVelocity * Time.deltaTime)); //������ �ۿ�
        }
    }

    void StickedAction()
    {
        if (isSticked) jumpVelocity = 0;                //���� ���� ���¶�� ���� ���¸� ����Ѵ�.

        if (isSticked && !isGrounded)                   //���� ��� ���� ���� ���� ���¶�� �̲�������.
            transform.Translate(new Vector2(0, stickedSlipSpeed * Time.deltaTime));

        if (isSticked && !isGrounded)
        {
            if (!isStickedJump && Input.GetKeyDown(KeyCode.Space)) //����
            {
                isStickedJump = true;
                stickedJumpVelocity = stickedJumpPower;
                Invoke("StickedJumpTime", stickedJumpTime);
            }
        }

        if (isStickedJump)
        {
            if (stickedJumpVelocity >= 0) stickedJumpVelocity += gravity * stickedJumpingGravity * Time.deltaTime; //���
            else if (stickedJumpVelocity < 0) stickedJumpVelocity += gravity * stickedFallingGravity * Time.deltaTime; //�ϰ�

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
        Debug.DrawRay(transform.position, forward * stickedDistance, Color.green);      //���� �پ� �ִ� ���� üũ�Ѵ�.
        if (Physics2D.Raycast(transform.position, forward, stickedDistance, LayerMask.GetMask("Wall"))) isSticked = true;
        else isSticked = false;

        if (isStickedJump)
        {
            Debug.DrawRay(transform.position, backward * stickedDistance, Color.green);    //������ �� �ݴ� ���� �浹�ϴ� ���� üũ�Ѵ�.
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