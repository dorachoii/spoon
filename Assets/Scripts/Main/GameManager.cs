using System.Collections;
using System.Collections.Generic;
using System.IO;
using Unity.Collections;
using UnityEngine;
using UnityEngine.Tilemaps;

[System.Serializable]
public class SaveData
{
    public Vector3 playerPosition;
    public List<TileData> tilemapData;
}

[System.Serializable]
public class TileData
{
    public int x, y;
    public string tileName;
}

public class GameManager : MonoBehaviour
{
    public Tilemap tilemap;
    void Update()
    {
        if(Input.GetKeyDown(KeyCode.Space))
        {
            LoadGame();
        }
    }
    public void SaveGame()
    {
        SaveData saveData = new SaveData();
        saveData.playerPosition = GameObject.FindGameObjectWithTag("Player").transform.position;

        BoundsInt bounds = tilemap.cellBounds;

        List<TileData> tileList = new List<TileData>();

        for (int y = bounds.yMin; y < bounds.yMax; y++)
        {
            for (int x = bounds.xMin; x < bounds.xMax; x++)
            {
                Vector3Int position = new Vector3Int(x, y, 0);
                TileBase tile = tilemap.GetTile(position);
                if (tile != null)
                {
                    TileData tileData = new TileData
                    {
                        x = x,
                        y = y,
                        tileName = tile.name
                    };
                    tileList.Add(tileData);
                }
            }
        }

        saveData.tilemapData = tileList;

        string json = JsonUtility.ToJson(saveData, true);
        File.WriteAllText(Path.Combine(Application.persistentDataPath, "savefile.json"), json);
    }

  public void LoadGame()
{
    string path = Path.Combine(Application.persistentDataPath, "savefile.json");
    if (File.Exists(path))
    {
        string json = File.ReadAllText(path);
        SaveData saveData = JsonUtility.FromJson<SaveData>(json);

        if (saveData.tilemapData == null)
        {
            Debug.LogError("[SaveLoad] saveData.tilemapData가 null입니다!");
            return;
        }

        Debug.Log("[SaveLoad] 타일맵 데이터 개수: " + saveData.tilemapData.Count);

        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            player.transform.position = saveData.playerPosition;
            Debug.Log("[SaveLoad] Player 위치 복원 완료");
        }
        else
        {
            Debug.LogError("[SaveLoad] Player 오브젝트를 찾을 수 없습니다.");
        }

        if (tilemap == null)
        {
            Debug.LogError("[SaveLoad] Tilemap이 null입니다!");
            return;
        }
        Debug.Log("[SaveLoad] Tilemap 준비 완료");

        tilemap.ClearAllTiles();
        Debug.Log("[SaveLoad] 타일맵 클리어 완료");

        foreach (TileData tileData in saveData.tilemapData)
        {
            if (tileData == null)
            {
                Debug.LogWarning("[SaveLoad] tileData가 null입니다! 무시합니다.");
                continue;
            }

            Vector3Int position = new Vector3Int(tileData.x, tileData.y, 0);
            Debug.Log($"[SaveLoad] 타일 로드 시도: {tileData.tileName} 위치: {position}");

            Tile tile = Resources.Load<Tile>("Tilemap/" + tileData.tileName);
            if (tile != null)
            {
                tilemap.SetTile(position, tile);
                Debug.Log("[SaveLoad] 타일 세팅 성공: " + tileData.tileName);
            }
            else
            {
                Debug.LogWarning("[SaveLoad] 타일 로드 실패: " + tileData.tileName);
            }
        }
    }
    else
    {
        Debug.LogError("[SaveLoad] Save file not found!");
    }
}


}
