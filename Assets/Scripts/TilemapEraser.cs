using UnityEngine;
using UnityEngine.Tilemaps;

public class TilemapEraser : MonoBehaviour
{
    public Tilemap tilemap;
    public int brushRadius = 2; // Inspector에서 반경 조절

    void Update()
    {
        if (Input.GetMouseButton(0))
        {
            Vector3 mouseWorldPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            Vector3Int centerCell = tilemap.WorldToCell(mouseWorldPos);

            for (int y = -brushRadius; y <= brushRadius; y++)
            {
                for (int x = -brushRadius; x <= brushRadius; x++)
                {
                    // 원형 브러시: 반지름 내만 지움
                    if (x * x + y * y > brushRadius * brushRadius) continue;

                    Vector3Int cellPos = centerCell + new Vector3Int(x, y, 0);
                    if (tilemap.HasTile(cellPos))
                    {
                        tilemap.SetTile(cellPos, null);
                    }
                }
            }
        }
    }
}