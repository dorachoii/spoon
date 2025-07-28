using UnityEngine;
using UnityEngine.Tilemaps;
using System.Collections;
using System.Collections.Generic;

public class BombItem : ItemBase
{
    private int explosionRadius = 30;  // 터지는 반경 (타일 단위)

    protected override void ApplyEffect(GameObject player)
    {
        Explode();

        // 필요하면 플레이어 쪽에 폭탄 먹었을 때 추가 효과도 여기서 호출 가능
        Debug.Log("[BombItem] Bomb exploded!");
    }

    void Explode()
    {
        Vector3Int centerCell = tilemap.WorldToCell(transform.position);

        // 지울 타일을 원형 범위로 계산
        List<Vector3Int> tilesToClear = new List<Vector3Int>();

        for (int x = -explosionRadius; x <= explosionRadius; x++)
        {
            for (int y = -explosionRadius; y <= explosionRadius; y++)
            {
                Vector3Int checkPos = new Vector3Int(centerCell.x + x, centerCell.y + y, centerCell.z);
                float dist = Mathf.Sqrt(x * x + y * y);
                if (dist <= explosionRadius && tilemap.HasTile(checkPos))
                {
                    tilesToClear.Add(checkPos);
                }
            }
        }

        // 한꺼번에 삭제
        Vector3Int[] positions = tilesToClear.ToArray();
        TileBase[] emptyTiles = new TileBase[positions.Length];

        // 모두 null로 설정해서 제거
        tilemap.SetTiles(positions, emptyTiles);
    }
}
