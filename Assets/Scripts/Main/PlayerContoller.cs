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

        tilePositions = new Vector3Int[maxTilesPerFrame];
        nullTiles = new TileBase[maxTilesPerFrame];

        for(int i = 0; i < nullTiles.Length; i++)
        {
            nullTiles[i] = null;
        }
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
            StartDig();
        }
        else
        {
            ChangeState(PlayerState.Idle);
        }
    }

    private HashSet<Vector3Int> removedTiles = new HashSet<Vector3Int>();
    private List<Vector3Int> positionsToDig = new List<Vector3Int>();

    public int maxTilesPerFrame = 40;
    private Vector3Int[] tilePositions;
    private TileBase[] nullTiles;



    private bool isDigging = false;
    public void StartDig()
    {
        if (currentState != PlayerState.Dig || isDigging) return;

        StopAllCoroutines();
        StartCoroutine(DigCoroutine());
    }

    private IEnumerator DigCoroutine()
    {
        isDigging = true;
        positionsToDig.Clear();

        Vector2 playerPos = transform.position + Vector3.down * 0.5f;
        Vector3Int centerCell = targetTilemaps[0].WorldToCell(playerPos);

        for (int y = brushRadius; y > -brushRadius; y--)
        {
            for (int x = -brushRadius; x <= brushRadius; x++)
            {
                if (x * x + y * y > brushRadius * brushRadius) continue;

                Vector3Int cellPos = centerCell + new Vector3Int(x, y, 0);

                if (!targetTilemaps[0].cellBounds.Contains(cellPos)) continue;
                if (removedTiles.Contains(cellPos)) continue;
                if (!targetTilemaps[0].HasTile(cellPos)) continue;

                positionsToDig.Add(cellPos);
            }
        }

        int total = positionsToDig.Count;
        int current = 0;

        while (current < total)
        {
            int count = Mathf.Min(maxTilesPerFrame, total - current);

            for (int i = 0; i < count; i++)
            {
                tilePositions[i] = positionsToDig[current + i];
            }

            targetTilemaps[0].SetTiles(tilePositions, nullTiles);

            for (int i = 0; i < count; i++)
            {
                removedTiles.Add(tilePositions[i]);
            }

            current += count;
            yield return null;
        }
        isDigging = false;
    }


    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Vector2 playerPos = transform.position + Vector3.down * 0.5f;
        float worldRadius = brushRadius * (targetTilemaps != null && targetTilemaps.Length > 0 ? targetTilemaps[0].cellSize.x : 1f);

        Gizmos.DrawWireSphere(playerPos, worldRadius);
    }
}
