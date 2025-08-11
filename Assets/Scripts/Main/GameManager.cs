using UnityEngine;
using UnityEngine.SceneManagement;
using System;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }
    // 플레이어 생성 완료 이벤트
    public static event Action OnPlayerReady;
    
    // 플레이어 준비 이벤트를 발생시키는 public 메서드
    public static void TriggerPlayerReady()
    {
        Debug.Log("[GameManager] TriggerPlayerReady");
        OnPlayerReady?.Invoke();
    }

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


    public void StartNewGame()
    {
        PersistenceManager.Instance?.ClearSave();
        // 먼저 씬을 로드하고, 씬 로드 완료 후 데이터를 로드
        SceneManager.LoadScene(SceneNames.GAME_SCENE_NAME);
        Time.timeScale = 1;
        AudioManager.Instance.ChangeBGM(BGMType.Game);
        
        // 씬 로드 완료 후 데이터 로드
        SceneManager.sceneLoaded += OnNewGameSceneLoaded;
    }
    
    private void OnNewGameSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (scene.name == SceneNames.GAME_SCENE_NAME)
        {
            SceneManager.sceneLoaded -= OnNewGameSceneLoaded;
            Debug.Log("1:[GameManager] New game scene loaded, starting data loading");
            PersistenceManager.Instance?.LoadGame();
        }
    }
        
    public void StartFromSavedGame()
    {
        // 먼저 씬을 로드하고, 씬 로드 완료 후 데이터를 로드
        SceneManager.LoadScene(SceneNames.GAME_SCENE_NAME);
        Time.timeScale = 1;
        AudioManager.Instance.ChangeBGM(BGMType.Game);
        
        // 씬 로드 완료 후 데이터 로드
        SceneManager.sceneLoaded += OnSavedGameSceneLoaded;
    }
    
    private void OnSavedGameSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (scene.name == SceneNames.GAME_SCENE_NAME)
        {
            SceneManager.sceneLoaded -= OnSavedGameSceneLoaded;
            Debug.Log("1:[GameManager] Saved game scene loaded, starting data loading");
            PersistenceManager.Instance?.LoadGame();
        }
    }
    
    public void BackToTitle()
    {
        SceneManager.LoadScene(SceneNames.INTRO_SCENE_NAME);
        AudioManager.Instance.ChangeBGM(BGMType.Intro);
    }
}
