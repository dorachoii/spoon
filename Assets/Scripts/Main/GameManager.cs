using System;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    public static event Action OnGameLoaded;

    private bool isGamePaused = false;

    // Intro → GameScene 구분 플래그 (PersistenceManager가 AutoLoad를 관리해도 된다면 이거도 옮겨도 됨)
    public static bool AutoLoadOnStart = false;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;

        DontDestroyOnLoad(gameObject);
    }

    private void Start()
    {
        if (AutoLoadOnStart)
        {
            AutoLoadOnStart = false;
            if (PersistenceManager.Instance != null && PersistenceManager.Instance.HasSavedData())
            {
                PersistenceManager.Instance.LoadGame();
                OnGameLoaded?.Invoke();
            }
            else
            {
                Debug.LogWarning("[GameManager] AutoLoad requested but no save exists.");
            }
        }
    }

    // 게임 내 이벤트 트리거용
    public void OnPlayerDeath()
    {
        if (isGamePaused) return;
        isGamePaused = true;

        // UIManager가 이벤트 구독해서 UI 제어하도록
        Debug.Log("[GameManager] Player died, triggering death event.");
    }

    public void RestartGame()
    {
        isGamePaused = false;
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        Debug.Log("[GameManager] Restart clicked.");
    }

    public void ResumeGame()
    {
        if (PersistenceManager.Instance == null || !PersistenceManager.Instance.HasSavedData())
        {
            Debug.LogWarning("[GameManager] No save to resume from. Restarting.");
            RestartGame();
            return;
        }

        PersistenceManager.Instance.LoadGame();
        OnGameLoaded?.Invoke();
    }

    public void StartNewGame(string sceneName)
    {
        PersistenceManager.Instance?.DeleteSave();
        AutoLoadOnStart = false;
        SceneManager.LoadScene(sceneName);
    }

    public void ResumeFromIntro(string sceneName)
    {
        AutoLoadOnStart = true;
        SceneManager.LoadScene(sceneName);
    }
}
