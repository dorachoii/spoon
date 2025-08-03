using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.Tilemaps;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

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

    [Header("UI Buttons")]
    public GameObject restartButton;
    public GameObject resumeButton;

    public static event Action OnGameLoaded;

    private bool isGamePaused = false;
    private SaveData loadedSaveData = null;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;

        // UI 버튼은 처음에 비활성화
        if (restartButton != null)
            restartButton.SetActive(false);
        if (resumeButton != null)
            resumeButton.SetActive(false);
    }

    private IEnumerator Start()
    {
        // 저장된 게임 불러오기 시도 (UI Resume 버튼 위해)
        string path = Path.Combine(Application.persistentDataPath, "savefile.json");
        if (File.Exists(path))
        {
            string json = File.ReadAllText(path);
            loadedSaveData = JsonUtility.FromJson<SaveData>(json);
        }
        yield return null;
    }

    private void Update()
    {
        // 테스트용 : 스페이스바 누르면 저장 불러오기 (필요 없으면 제거)
        if (Input.GetKeyDown(KeyCode.Space))
        {
            StartCoroutine(LoadGame());
        }
    }

    public void SaveGame()
    {
        SaveData saveData = new SaveData();
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player == null)
        {
            Debug.LogError("[SaveGame] Player object not found.");
            return;
        }

        saveData.playerPosition = player.transform.position + Vector3.up;
        saveData.tilemapData = tileMaker.GetTileDataList();

        saveData.removedTilePositions = new List<Vector3IntSerializable>();
        foreach (var pos in playerController.GetRemovedTiles())
        {
            saveData.removedTilePositions.Add(new Vector3IntSerializable(pos));
        }

        string json = JsonUtility.ToJson(saveData, true);
        File.WriteAllText(Path.Combine(Application.persistentDataPath, "savefile.json"), json);

        loadedSaveData = saveData;  // 저장할 때 로컬에도 저장
        Debug.Log("[SaveGame] Game saved.");
    }

    public IEnumerator LoadGame()
    {
        string path = Path.Combine(Application.persistentDataPath, "savefile.json");
        if (!File.Exists(path))
        {
            Debug.LogError("[LoadGame] Save file not found!");
            yield break;
        }

        string json = File.ReadAllText(path);
        SaveData saveData = JsonUtility.FromJson<SaveData>(json);

        if (saveData.tilemapData == null)
        {
            Debug.LogError("[LoadGame] Save data is invalid.");
            yield break;
        }

        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            player.transform.position = saveData.playerPosition;

            Vector3 camPos = Camera.main.transform.position;
            camPos.y = saveData.playerPosition.y;
            Camera.main.transform.position = camPos;
        }

        yield return new WaitForEndOfFrame();

        tileMaker.LoadTilemapData(saveData.tilemapData);
        playerController.LoadRemovedTiles(saveData.removedTilePositions);

        loadedSaveData = saveData;

        OnGameLoaded?.Invoke();

        Debug.Log("[LoadGame] Game loaded.");
    }

    // --- 죽음 발생 시 호출할 함수 ---
    public void OnPlayerDeath()
    {
        if (isGamePaused) return;

        isGamePaused = true;
        //Time.timeScale = 0f;  // 게임 일시정지

        if (restartButton != null)
            restartButton.SetActive(true);
        if (resumeButton != null)
            resumeButton.SetActive(true);

        Debug.Log("[GameManager] Player died, showing restart/resume UI.");
    }

    // UI 버튼에 연결할 함수
    public void OnRestartButton()
    {
        Time.timeScale = 1f;
        isGamePaused = false;

        if (restartButton != null)
            restartButton.SetActive(false);
        if (resumeButton != null)
            resumeButton.SetActive(false);

        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);

        Debug.Log("[GameManager] Restart button clicked - scene reloaded.");
    }

    public void OnResumeButton()
    {
        Time.timeScale = 1f;
        isGamePaused = false;

        if (restartButton != null)
            restartButton.SetActive(false);
        if (resumeButton != null)
            resumeButton.SetActive(false);

        if (loadedSaveData == null)
        {
            Debug.LogWarning("[GameManager] No save data to resume from. Restarting instead.");
            OnRestartButton();
            return;
        }

        StartCoroutine(ResumeGameRoutine(loadedSaveData));
    }

    private IEnumerator ResumeGameRoutine(SaveData saveData)
    {
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            player.transform.position = saveData.playerPosition;

            Vector3 camPos = Camera.main.transform.position;
            camPos.y = saveData.playerPosition.y;
            Camera.main.transform.position = camPos;
        }

        yield return new WaitForEndOfFrame();

        tileMaker.LoadTilemapData(saveData.tilemapData);
        playerController.LoadRemovedTiles(saveData.removedTilePositions);

        OnGameLoaded?.Invoke();

        Debug.Log("[GameManager] Game resumed from save data.");
    }
}
