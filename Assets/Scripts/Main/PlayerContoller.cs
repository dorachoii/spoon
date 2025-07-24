using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public enum PlayerState
{
    Idle,
    Jump,
    Dig,
    Damaged
}

public class PlayerContoller : MonoBehaviour
{
    public FloatingJoystick floatingJoystick;
    private Rigidbody2D rb;
    public Tilemap[] targetTilemaps;

    [SerializeField]
    private float speed;
    private float jumpForce = 0.0005f;

    [SerializeField]
    private float verticalThreshold = 0.2f;

    private Animator animator;

    public PlayerState currentState { get; private set; }

    public void ChangeState(PlayerState newState)
    {
        if (currentState == newState) return;

        currentState = newState;

        switch (newState)
        {
            case PlayerState.Idle:
                animator.SetBool("IsDigging", false);
                break;
            case PlayerState.Jump:
                animator.SetTrigger("JumpTrigger");
                // rb.velocity = new Vector2(rb.velocity.x, 0); 
                // rb.AddForce(Vector2.up * jumpForce, ForceMode2D.Impulse);
                break;
            case PlayerState.Dig:
                animator.SetBool("IsDigging", true);
                break;
            case PlayerState.Damaged:
                // Handle Damaged state logic
                break;
        }
    }


    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
    }

    public void FixedUpdate()
    {
        Vector3 direction = Vector3.up * floatingJoystick.Vertical + Vector3.right * floatingJoystick.Horizontal;
        rb.AddForce(direction * speed * Time.fixedDeltaTime, ForceMode2D.Force);

        float verticalInput = floatingJoystick.Vertical;

        if (verticalInput > verticalThreshold)
        {
            Debug.Log("Moving Up");
            ChangeState(PlayerState.Jump);
        }
        else if (verticalInput < -verticalThreshold)
        {
            Debug.Log("Moving Down");
            ChangeState(PlayerState.Dig);
        }
        else
        {
            ChangeState(PlayerState.Idle);
        }
    }

    private void OnCollisionStay2D(Collision2D collision)
    {
        if (currentState != PlayerState.Dig) return;

        if (collision.collider.gameObject == targetTilemaps[0].gameObject)
        {
            foreach (ContactPoint2D contact in collision.contacts)
            {
                Vector3 hitPoint = contact.point;
                Vector3Int tilePos = targetTilemaps[0].WorldToCell(hitPoint);

                if(targetTilemaps[0].HasTile(tilePos))
                {
                    targetTilemaps[0].SetTile(tilePos, null);
                }
            }
        }
    }
}
