using System;
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
    public List<Vector3IntSerializable> removedTilePositions = new List<Vector3IntSerializable>();
}

[System.Serializable]
public class TileData
{
    public int x, y;
    public string tileName;
}

[System.Serializable]
public struct Vector3IntSerializable
{
    public int x, y, z;
    public Vector3IntSerializable(Vector3Int v)
    {
        x = v.x;
        y = v.y;
        z = v.z;
    }

    public Vector3Int ToVector3Int() => new Vector3Int(x, y, z);
}

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }
    public Tilemap tilemap;
    public PlayerContoller playerController;
    public TileMaker tileMaker;
    public static event Action OnGameLoaded;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            StartCoroutine(LoadGame());
        }
    }
    public void SaveGame()
    {
        SaveData saveData = new SaveData();
        saveData.playerPosition = GameObject.FindGameObjectWithTag("Player").transform.position + Vector3.up;

        saveData.tilemapData = tileMaker.GetTileDataList();
        saveData.removedTilePositions = new List<Vector3IntSerializable>();
        foreach (var pos in playerController.GetRemovedTiles())
        {
            saveData.removedTilePositions.Add(new Vector3IntSerializable(pos));
        }

        string json = JsonUtility.ToJson(saveData, true);
        File.WriteAllText(Path.Combine(Application.persistentDataPath, "savefile.json"), json);
    }

    public IEnumerator LoadGame()
{
    string path = Path.Combine(Application.persistentDataPath, "savefile.json");
    if (!File.Exists(path))
    {
        Debug.LogError("[SaveLoad] Save file not found!");
        yield break;
    }

    string json = File.ReadAllText(path);
    SaveData saveData = JsonUtility.FromJson<SaveData>(json);

    if (saveData.tilemapData == null) yield break;

    GameObject player = GameObject.FindGameObjectWithTag("Player");
    if (player != null)
    {
        player.transform.position = saveData.playerPosition;

        Vector3 camPos = Camera.main.transform.position;
        camPos.y = saveData.playerPosition.y; // 카메라 z 고정

        Camera.main.transform.position = camPos;
    }

    // 한 프레임 기다려서 LateUpdate가 실행된 뒤에 타일맵 로드 실행
    yield return new WaitForEndOfFrame();

    tileMaker.LoadTilemapData(saveData.tilemapData);
    playerController.LoadRemovedTiles(saveData.removedTilePositions);

    OnGameLoaded?.Invoke();
}



}
