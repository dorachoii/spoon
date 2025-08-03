using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class BreakableTilemap : MonoBehaviour
{
    [SerializeField] private Tilemap groundTilemap;

    [Header("Shard prefab version")]
    [SerializeField] private GameObject shardPrefab;
    [SerializeField] private float shardSpread = 0.2f;
    [SerializeField] private float shardMinImpulse = 0.5f;
    [SerializeField] private float shardMaxImpulse = 1.5f;

    [Header("Cooldown")]
    [SerializeField] private float cellCooldown = 0.3f;
    private HashSet<Vector3Int> recentlyCrumbled = new();

    [Header("Debug")]
    [SerializeField] private bool verbose = false;

    private void Start()
    {
        if (groundTilemap == null)
        {
            GameObject tm = GameObject.FindGameObjectWithTag("Tilemap");
            if (tm != null) groundTilemap = tm.GetComponent<Tilemap>();
        }

        if (shardPrefab == null)
            Debug.LogWarning("[BreakableTilemap] shardPrefab is not assigned.");
    }

    private void OnCollisionEnter2D(Collision2D collision) => HandleTileContacts(collision);

    private void HandleTileContacts(Collision2D collision)
    {
        if (groundTilemap == null) return;

        HashSet<Vector3Int> toCrumble = new();
        Dictionary<Vector3Int, bool> crumbleDirections = new(); // true = 위로 부서짐, false = 아래로 부서짐
        int touchedTileCount = 0;

        foreach (var contact in collision.contacts)
        {
            Vector3 worldPoint = contact.point;
            Vector3Int touchedCell = groundTilemap.WorldToCell(worldPoint);
            TileBase touchedTile = groundTilemap.GetTile(touchedCell);
            if (touchedTile == null) continue;

            touchedTileCount++;

            Vector3Int targetCell;
            bool crumbleUpwards;

            // contact.normal.y가 양수면 충돌체가 아래에서 위로 부딪힌 것 (부서질 타일은 위쪽)
            // 음수면 위에서 아래로 부딪힌 것 (부서질 타일은 아래쪽)
            if (contact.normal.y > 0.1f)
            {
                targetCell = GetHighestTileInColumn(touchedCell.x);
                crumbleUpwards = true;
            }
            else
            {
                targetCell = GetLowestTileInColumn(touchedCell.x);
                crumbleUpwards = false;
            }

            if (targetCell == Vector3Int.zero) continue;
            if (recentlyCrumbled.Contains(targetCell)) continue;

            toCrumble.Add(targetCell);
            crumbleDirections[targetCell] = crumbleUpwards;
        }

        if (toCrumble.Count == 0)
        {
            if (verbose)
                Debug.Log($"[Contact] touched {touchedTileCount} tile(s), but none eligible to crumble.");
            return;
        }

        int totalShardsSpawned = 0;
        foreach (var cell in toCrumble)
        {
            bool upwards = crumbleDirections[cell];
            StartCoroutine(CrumbleWithCooldown(cell, upwards));
            totalShardsSpawned++;
        }

        Debug.Log($"[Crumble Summary] touchedTiles={touchedTileCount}, uniqueColumnsToCrumble={toCrumble.Count}, shardsSpawned={totalShardsSpawned}");
    }

    private Vector3Int GetLowestTileInColumn(int columnX)
    {
        BoundsInt bounds = groundTilemap.cellBounds;
        for (int y = bounds.yMin; y <= bounds.yMax; y++)
        {
            Vector3Int cell = new Vector3Int(columnX, y, 0);
            if (groundTilemap.HasTile(cell))
                return cell;
        }
        return Vector3Int.zero;
    }

    private Vector3Int GetHighestTileInColumn(int columnX)
    {
        BoundsInt bounds = groundTilemap.cellBounds;
        for (int y = bounds.yMax; y >= bounds.yMin; y--)
        {
            Vector3Int cell = new Vector3Int(columnX, y, 0);
            if (groundTilemap.HasTile(cell))
                return cell;
        }
        return Vector3Int.zero;
    }

    private IEnumerator CrumbleWithCooldown(Vector3Int cell, bool upwards)
    {
        recentlyCrumbled.Add(cell);
        CrumbleTileAt(cell, upwards);
        yield return new WaitForSeconds(cellCooldown);
        recentlyCrumbled.Remove(cell);
    }

    private void CrumbleTileAt(Vector3Int cell, bool upwards)
    {
        TileBase tile = groundTilemap.GetTile(cell);
        if (tile == null)
        {
            if (verbose)
                Debug.LogWarning($"[CrumbleTileAt] No tile at {cell} to crumble.");
            return;
        }

        // 타일 제거
        groundTilemap.SetTile(cell, null);

        if (verbose)
            Debug.Log($"[CrumbleTileAt] Crumbling tile at {cell}, spawning 1 shard.");

        Vector3 origin = groundTilemap.GetCellCenterWorld(cell);
        GameObject shard = Instantiate(shardPrefab, origin + new Vector3(Random.Range(-shardSpread, shardSpread), 0f, 0f), Quaternion.identity);

        Rigidbody2D rb = shard.GetComponent<Rigidbody2D>();
if (rb != null)
{
    if (upwards)
    {
        rb.gravityScale = -Mathf.Abs(rb.gravityScale);  // 중력 위쪽 방향 (음수)
    }
    else
    {
        rb.gravityScale = Mathf.Abs(rb.gravityScale);   // 중력 아래쪽 방향 (양수)
    }

    // 부스러기 초기 속도는 조금 줘서 튀어나오게 하려면 아래처럼 AddForce도 살짝 줄 수 있습니다.
    Vector2 initialImpulse = new Vector2(Random.Range(-0.3f, 0.3f), upwards ? 1f : -1f).normalized * Random.Range(shardMinImpulse, shardMaxImpulse);
    rb.AddForce(initialImpulse, ForceMode2D.Impulse);
}

    }
}
