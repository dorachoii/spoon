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
    public Tilemap tilemap;

    [SerializeField]
    private float speed;
    private float jumpForce;

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
                rb.velocity = new Vector2(rb.velocity.x, 0);
                rb.AddForce(Vector2.up * jumpForce, ForceMode2D.Impulse);
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

        tilePositions = new Vector3Int[LayerManager.Instance.GetMaxTile()];
        nullTiles = new TileBase[LayerManager.Instance.GetMaxTile()];

        for (int i = 0; i < nullTiles.Length; i++)
        {
            nullTiles[i] = null;
        }
        jumpForce = PlayerStat.Instance.JumpForce;
    }

    public void FixedUpdate()
{
    Vector2 inputDirection = new Vector2(floatingJoystick.Horizontal, floatingJoystick.Vertical);

    if (inputDirection.magnitude < 0.1f)
    {
        ChangeState(PlayerState.Idle);
        return;
    }

    float angle = Vector2.Angle(Vector2.up, inputDirection); 

    bool isLeft = inputDirection.x < 0;

    float signedAngle = isLeft ? 360f - angle : angle;

    if (signedAngle >= 90f && signedAngle <= 270f)
    {
        // 아래 방향 → Dig
        ChangeState(PlayerState.Dig);
        rb.AddForce(inputDirection.normalized * speed * Time.fixedDeltaTime, ForceMode2D.Force);
        StartDig();
    }
    else if ((signedAngle >= 60f && signedAngle <= 120f))
    {
        // 좌우 부채꼴 → Idle
        ChangeState(PlayerState.Idle);
    }
    else
    {
        // 나머지 위쪽 → Jump
        ChangeState(PlayerState.Jump);
    }
}


    private HashSet<Vector3Int> removedTiles = new HashSet<Vector3Int>();
    private List<Vector3Int> positionsToDig = new List<Vector3Int>();


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
        Vector3Int centerCell = tilemap.WorldToCell(playerPos);

        for (int y = brushRadius; y > -brushRadius; y--)
        {
            for (int x = -brushRadius; x <= brushRadius; x++)
            {
                if (x * x + y * y > brushRadius * brushRadius) continue;

                Vector3Int cellPos = centerCell + new Vector3Int(x, y, 0);

                if (!tilemap.cellBounds.Contains(cellPos)) continue;
                if (removedTiles.Contains(cellPos)) continue;
                if (!tilemap.HasTile(cellPos)) continue;

                positionsToDig.Add(cellPos);
            }
        }

        int total = positionsToDig.Count;
        int current = 0;

        if (positionsToDig.Count > 0)
        {
            float hardness = LayerManager.Instance.GetCurrentHardness(); // 또는 GetCurrentHardness()
            float digPower = PlayerStat.Instance.DigPower;

            if (digPower < hardness)
            {
                ChangeState(PlayerState.Jump);
                isDigging = false;
                yield break;
            }
        }

        while (current < total)
        {
            int count = Mathf.Min(LayerManager.Instance.GetMaxTile(), total - current);

            for (int i = 0; i < count; i++)
            {
                tilePositions[i] = positionsToDig[current + i];
            }

            tilemap.SetTiles(tilePositions, nullTiles);

            for (int i = 0; i < count; i++)
            {
                removedTiles.Add(tilePositions[i]);
            }

            current += count;

            yield return new WaitForSeconds(PlayerStat.Instance.GetDigDelay());
        }
        isDigging = false;
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Vector2 playerPos = transform.position + Vector3.down * 0.5f;
        float worldRadius = brushRadius * (tilemap != null ? tilemap.cellSize.x : 1f);

        Gizmos.DrawWireSphere(playerPos, worldRadius);
    }

    public IEnumerable<Vector3Int> GetRemovedTiles() => removedTiles;
    public void LoadRemovedTiles(IEnumerable<Vector3IntSerializable> savedPositions)
    {
        removedTiles.Clear();
        foreach (var pos in savedPositions)
        {
            removedTiles.Add(pos.ToVector3Int());
        }
    }

}
