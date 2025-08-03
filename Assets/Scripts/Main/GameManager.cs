using System;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [Header("UI Buttons (death UI)")]
    public GameObject restartButton;
    public GameObject resumeButton;

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

        if (restartButton != null)
            restartButton.SetActive(true);
        UpdateResumeButtonState();
    }

    private void Start()
    {
        if (AutoLoadOnStart)
        {
            AutoLoadOnStart = false;
            if (PersistenceManager.Instance != null && PersistenceManager.Instance.HasSaveData())
            {
                PersistenceManager.Instance.LoadGame();
            }
            else
            {
                Debug.LogWarning("[GameManager] AutoLoad requested but no save exists.");
            }
        }

        PersistenceManager.Instance?.GetType();
    }

    private void UpdateResumeButtonState()
    {
        if (resumeButton != null)
        {
            bool hasSave = PersistenceManager.Instance != null && PersistenceManager.Instance.HasSaveData();
            resumeButton.SetActive(hasSave);
        }
    }

    // --- 게임 내 이벤트 트리거용 ---
    public void OnPlayerDeath()
    {
        if (isGamePaused) return;
        isGamePaused = true;

        if (restartButton != null)
            restartButton.SetActive(true);
        if (resumeButton != null)
            resumeButton.SetActive(PersistenceManager.Instance != null && PersistenceManager.Instance.HasSaveData());

        Debug.Log("[GameManager] Player died, showing restart/resume UI.");
    }

    // UI에 연결
    public void OnRestartButton()
    {
        if (restartButton != null)
            restartButton.SetActive(false);
        if (resumeButton != null)
            resumeButton.SetActive(false);

        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        Debug.Log("[GameManager] Restart clicked.");
    }

    public void OnResumeButton()
    {
        if (restartButton != null)
            restartButton.SetActive(false);
        if (resumeButton != null)
            resumeButton.SetActive(false);

        if (PersistenceManager.Instance == null || !PersistenceManager.Instance.HasSaveData())
        {
            Debug.LogWarning("[GameManager] No save to resume from. Restarting.");
            OnRestartButton();
            return;
        }

        PersistenceManager.Instance.LoadGame();
    }

    // Intro 씬 API는 별도 IntroUI/Launcher에 두는 게 더 깔끔하다.
    // 필요하면 여기서도 아래처럼 단순 위임만 둔다.
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
