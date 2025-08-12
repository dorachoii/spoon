using UnityEngine;
using UnityEngine.SceneManagement;
public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }
    
    // 게임 상태 관리
    private bool isGameCleared = false;
    private bool hasPlayedIntro = false;
    
    // 게임 클리어 이벤트
    public System.Action OnGameCleared;
    public System.Action OnIntroCompleted;


    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;

        DontDestroyOnLoad(gameObject);
        
        // 앱 시작 시 인트로 재생 상태 로드
        LoadIntroState();
    }


    public void StartNewGame()
    {
        PersistenceManager.Instance?.ClearSave();
        // 게임 클리어 상태만 리셋 (인트로 재생 상태는 유지)
        ResetGameCleared();
        
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
    
    // 게임 클리어 처리
    public void SetGameCleared()
    {
        if (!isGameCleared)
        {
            isGameCleared = true;
            Debug.Log("[GameManager] 게임 클리어!");
            OnGameCleared?.Invoke();
        }
    }
    
    // 게임 클리어 상태 확인
    public bool IsGameCleared()
    {
        return isGameCleared;
    }
    
    // 게임 클리어 상태 리셋 (새 게임 시작 시)
    public void ResetGameCleared()
    {
        isGameCleared = false;
    }
    
    // Intro 완료 처리
    public void SetIntroCompleted()
    {
        if (!hasPlayedIntro)
        {
            hasPlayedIntro = true;
            SaveIntroState(); // 상태 변경 시 즉시 저장
            Debug.Log("[GameManager] Intro 완료!");
            OnIntroCompleted?.Invoke();
        }
    }
    
    // Intro 재생 상태 확인
    public bool HasPlayedIntro()
    {
        return hasPlayedIntro;
    }
    
    // 게임 상태 리셋 (새 게임 시작 시)
    public void ResetGameState()
    {
        isGameCleared = false;
        hasPlayedIntro = false;
    }
    
    // 인트로 재생 상태 저장
    private void SaveIntroState()
    {
        PlayerPrefs.SetInt("HasPlayedIntro", hasPlayedIntro ? 1 : 0);
        PlayerPrefs.Save();
        Debug.Log($"[GameManager] 인트로 재생 상태 저장: {hasPlayedIntro}");
    }
    
    // 인트로 재생 상태 로드
    private void LoadIntroState()
    {
        hasPlayedIntro = PlayerPrefs.GetInt("HasPlayedIntro", 0) == 1;
        Debug.Log($"[GameManager] 인트로 재생 상태 로드: {hasPlayedIntro}");
    }
    
    // 인트로 재생 상태 리셋 (앱 재시작 시 사용)
    public void ResetIntroState()
    {
        hasPlayedIntro = false;
        PlayerPrefs.DeleteKey("HasPlayedIntro");
        Debug.Log("[GameManager] 인트로 재생 상태 리셋");
    }
}
