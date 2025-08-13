using UnityEngine;
using UnityEngine.SceneManagement;
public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }
    
    private bool isGameCleared = false;
    private bool hasPlayedIntro = false;
    
    public System.Action OnGameCleared;
    public System.Action OnIntroCompleted;
    public System.Action OnGameResumed;


    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;

        DontDestroyOnLoad(gameObject);
        
        LoadIntroState();
    }


    public void StartNewGame()
    {
        PersistenceManager.Instance?.ClearSave();
        ResetGameCleared();
        
        SceneManager.LoadScene(SceneNames.GAME_SCENE_NAME);
        Time.timeScale = 1;
        AudioManager.Instance.ChangeBGM(BGMType.Game);
        
        SceneManager.sceneLoaded += OnNewGameSceneLoaded;
    }
    
    private void OnNewGameSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (scene.name == SceneNames.GAME_SCENE_NAME)
        {
            SceneManager.sceneLoaded -= OnNewGameSceneLoaded;
            PersistenceManager.Instance?.LoadGame();
        }
    }
        
    public void StartFromSavedGame()
    {
        SceneManager.LoadScene(SceneNames.GAME_SCENE_NAME);
        Time.timeScale = 1;
        AudioManager.Instance.ChangeBGM(BGMType.Game);
        
        SceneManager.sceneLoaded += OnSavedGameSceneLoaded;
    }
    
    private void OnSavedGameSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (scene.name == SceneNames.GAME_SCENE_NAME)
        {
            SceneManager.sceneLoaded -= OnSavedGameSceneLoaded;
    
            PersistenceManager.Instance?.LoadGame();
            
            // 게임이 resume되었음을 알림
            Debug.Log("GameManager: OnGameResumed 이벤트 호출");
            OnGameResumed?.Invoke();
        }
    }
    
    public void BackToTitle()
    {
        SceneManager.LoadScene(SceneNames.INTRO_SCENE_NAME);
        AudioManager.Instance.ChangeBGM(BGMType.Intro);
    }
    
    public void SetGameCleared()
    {
        if (!isGameCleared)
        {
            isGameCleared = true;
    
            OnGameCleared?.Invoke();
        }
    }
    
    public bool IsGameCleared()
    {
        return isGameCleared;
    }
    
    public void ResetGameCleared()
    {
        isGameCleared = false;
    }
    
    public void SetIntroCompleted()
    {
        if (!hasPlayedIntro)
        {
            hasPlayedIntro = true;
            SaveIntroState();
    
            OnIntroCompleted?.Invoke();
        }
    }
    
    public bool HasPlayedIntro()
    {
        return hasPlayedIntro;
    }
    
    public void ResetGameState()
    {
        isGameCleared = false;
        hasPlayedIntro = false;
    }
    
    private void SaveIntroState()
    {
        PlayerPrefs.SetInt("HasPlayedIntro", hasPlayedIntro ? 1 : 0);
        PlayerPrefs.Save();

    }
    
    private void LoadIntroState()
    {
        hasPlayedIntro = PlayerPrefs.GetInt("HasPlayedIntro", 0) == 1;

    }
    
    public void ResetIntroState()
    {
        hasPlayedIntro = false;
        PlayerPrefs.DeleteKey("HasPlayedIntro");

    }
}
