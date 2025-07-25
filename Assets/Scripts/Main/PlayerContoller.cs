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

    public int brushRadius = 10;
    private float digCooldown = 0.4f;
    private float lastDigTime = -999f;


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
            TryDig();
        }
        else
        {
            ChangeState(PlayerState.Idle);
        }
    }

    private HashSet<Vector3Int> removedTiles = new HashSet<Vector3Int>();

    private void Dig()
    {
        if (currentState != PlayerState.Dig) return;

        Vector2 playerPos = transform.position + Vector3.down * 0.5f;
        Vector3Int centerCell = targetTilemaps[0].WorldToCell(playerPos);

        List<Vector3Int> toRemove = new List<Vector3Int>();

        for (int y = -brushRadius; y <= brushRadius; y++)
        {
            for (int x = -brushRadius; x <= brushRadius; x++)
            {
                if (x * x + y * y > brushRadius * brushRadius) continue;

                Vector3Int cellPos = centerCell + new Vector3Int(x, y, 0);

                if (removedTiles.Contains(cellPos)) continue;

                if (targetTilemaps[0].HasTile(cellPos))
                {
                    toRemove.Add(cellPos);
                    removedTiles.Add(cellPos);
                }
            }
        }

        if (toRemove.Count > 0)
        {
            TileBase[] tiles = new TileBase[toRemove.Count];
            targetTilemaps[0].SetTiles(toRemove.ToArray(), tiles);
        }
    }

    private void TryDig()
    {
        if (Time.time - lastDigTime < digCooldown) return;
        Dig();
        lastDigTime = Time.time;
    }


    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Vector2 playerPos = transform.position + Vector3.down * 0.5f;
        float worldRadius = brushRadius * (targetTilemaps != null && targetTilemaps.Length > 0 ? targetTilemaps[0].cellSize.x : 1f);

        Gizmos.DrawWireSphere(playerPos, worldRadius);
    }
}
