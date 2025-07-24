using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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

    [SerializeField]
    private float speed;

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

        rb.gravityScale = 0; 
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
        }else
        {
            ChangeState(PlayerState.Idle);
        }
    }
}
