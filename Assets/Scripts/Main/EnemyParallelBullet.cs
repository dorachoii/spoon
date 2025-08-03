using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class EnemyParallelBullet : ItemBase
{
    [SerializeField] private float speed = 5f;
    [SerializeField] private float lifeTime = 4f;
     [SerializeField] private LayerMask playerLayer;
    [SerializeField] private int damage = 5;

    private Vector2 direction = Vector2.up; // 항상 위 방향
    private Tilemap targetTilemap;

    void Start()
    {
        Destroy(gameObject, lifeTime);
        targetTilemap = TileMaker.Instance.tilemap;
    }

    protected override void Update()
    {
        base.Update();

        // 이동
        Vector3 delta = (Vector3)(direction.normalized * speed * Time.deltaTime);
        transform.Translate(delta);

        // 지나간 위치의 타일 삭제
        if (targetTilemap != null)
        {
            ClearTilesUnderBullet();
        }
    }

    protected override void ApplyEffect(GameObject player)
    {
        PlayerStat.Instance.DamageHP(damage);
    }
[SerializeField] private int halfWidth = 2;   // 좌우 폭 (수평)
[SerializeField] private int halfHeight = 2;  // 앞뒤 깊이 (이동 방향 기준, up 방향으로도 확장)

private void ClearTilesUnderBullet()
{
    if (targetTilemap == null) return;

    Vector3 worldPos = transform.position;
    Vector3Int centerCell = targetTilemap.WorldToCell(worldPos);

    Vector2 mainDir = direction.normalized; // up 방향
    Vector2 perp = new Vector2(-mainDir.y, mainDir.x).normalized; // 좌우

 
    Vector3Int perpInt = new Vector3Int(Mathf.RoundToInt(perp.x), Mathf.RoundToInt(perp.y), 0);
    Vector3Int mainInt = new Vector3Int(Mathf.RoundToInt(mainDir.x), Mathf.RoundToInt(mainDir.y), 0);

    for (int w = -halfWidth; w <= halfWidth; w++)
    {
        for (int h = -halfHeight; h <= halfHeight; h++)
        {
            Vector3Int offset = perpInt * w + mainInt * h;
            Vector3Int cell = centerCell + offset;
            if (targetTilemap.GetTile(cell) != null)
            {
                targetTilemap.SetTile(cell, null);
            }
        }
    }
}

}
