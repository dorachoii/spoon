using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class PlayerTileContactor : MonoBehaviour
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
            Debug.LogWarning("[PlayerTileContactor] shardPrefab is not assigned.");
    }

    private void OnCollisionEnter2D(Collision2D collision) => HandleTileContacts(collision);

    private void HandleTileContacts(Collision2D collision)
    {
        if (groundTilemap == null) return;

        HashSet<Vector3Int> toCrumble = new();
        int touchedTileCount = 0;

        foreach (var contact in collision.contacts)
        {
            Vector3 worldPoint = contact.point;
            Vector3Int touchedCell = groundTilemap.WorldToCell(worldPoint);
            TileBase touchedTile = groundTilemap.GetTile(touchedCell);
            if (touchedTile == null) continue;

            touchedTileCount++;
            Vector3Int bottomCell = GetLowestTileInColumn(touchedCell.x);
            if (bottomCell == Vector3Int.zero) continue;

            if (recentlyCrumbled.Contains(bottomCell))
            {
                if (verbose)
                    Debug.Log($"[Skip Cooldown] Bottom cell {bottomCell} already crumbling.");
                continue;
            }

            toCrumble.Add(bottomCell);
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
            StartCoroutine(CrumbleWithCooldown(cell));
            totalShardsSpawned += 1; // 한 타일당 한 개
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

    private IEnumerator CrumbleWithCooldown(Vector3Int cell)
    {
        recentlyCrumbled.Add(cell);
        CrumbleTileAt(cell);
        yield return new WaitForSeconds(cellCooldown);
        recentlyCrumbled.Remove(cell);
    }

    private void CrumbleTileAt(Vector3Int cell)
    {
        TileBase tile = groundTilemap.GetTile(cell);
        if (tile == null)
        {
            if (verbose)
                Debug.LogWarning($"[CrumbleTileAt] No tile at {cell} to crumble.");
            return;
        }

        // 타일 색 가져오기
  
        // 제거
        groundTilemap.SetTile(cell, null);

        if (verbose)
            Debug.Log($"[CrumbleTileAt] Crumbling tile at {cell}, spawning 1 shard.");

        // 한 타일당 하나만 생성
        Vector3 origin = groundTilemap.GetCellCenterWorld(cell);
        GameObject shard = Instantiate(shardPrefab, origin + new Vector3(Random.Range(-shardSpread, shardSpread), 0f, 0f), Quaternion.identity);

        var rb = shard.GetComponent<Rigidbody2D>();
        if (rb != null)
        {
            Vector2 impulse = new Vector2(Random.Range(-0.3f, 0.3f), -1f).normalized * Random.Range(shardMinImpulse, shardMaxImpulse);
            rb.AddForce(impulse, ForceMode2D.Impulse);
        }
    }
}
