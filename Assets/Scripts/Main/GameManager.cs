using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }
    bool isGameReady = false;

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
        PersistenceManager.OnDataLoaded += OnDataLoadedComplete;
    }

    private void OnDestroy()
    {
        PersistenceManager.OnDataLoaded -= OnDataLoadedComplete;
    }

    private void OnDataLoadedComplete()
    {
        isGameReady = true;
    }

    public void StartNewGame()
    {
        
        PersistenceManager.Instance?.ClearSave();
        SceneManager.LoadScene(SceneNames.GAME_SCENE_NAME);
        Time.timeScale = 1;
        AudioManager.Instance.ChangeBGM(BGMType.Game);
    }

    public void StartFromSavedGame()
    {
        // 먼저 씬을 로드하고, 씬 로드 완료 후 데이터를 로드
        SceneManager.LoadScene(SceneNames.GAME_SCENE_NAME);
        Time.timeScale = 1;
        AudioManager.Instance.ChangeBGM(BGMType.Game);
        
        // 씬 로드 완료 후 데이터 로드 (SceneManager.sceneLoaded 이벤트 사용)
        SceneManager.sceneLoaded += OnGameSceneLoaded;
    }
    
    private void OnGameSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (scene.name == SceneNames.GAME_SCENE_NAME)
        {
            SceneManager.sceneLoaded -= OnGameSceneLoaded; // 구독 해제
            
            // 게임 씬이 로드된 후 데이터 로드
            PersistenceManager.Instance?.LoadGame();
        }
    }
    



    public void BackToTitle()
    {
        SceneManager.LoadScene(SceneNames.INTRO_SCENE_NAME);
        AudioManager.Instance.ChangeBGM(BGMType.Intro);
    }
}
