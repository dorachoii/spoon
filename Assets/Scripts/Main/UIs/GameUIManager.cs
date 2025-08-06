using UnityEngine;
using UnityEngine.UI;

public class GameUIManager : MonoBehaviour
{
    public static GameUIManager Instance { get; private set; }


    [Header("Game UI (Death UI)")]
    [SerializeField] private GameObject restartButton;
    [SerializeField] private GameObject gameResumeButton;

    [SerializeField] private IrisEffectController irisEffectController;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;

        restartButton.SetActive(false);
        gameResumeButton.SetActive(false);
    }

    private void OnEnable()
    {
        GameManager.OnGameLoaded += HandleGameLoaded;

        if (PlayerStat.Instance != null)
            PlayerStat.Instance.OnDied += HandlePlayerDied;

    }

    private void OnDisable()
    {
        GameManager.OnGameLoaded -= HandleGameLoaded;

        if (PlayerStat.Instance != null)
            PlayerStat.Instance.OnDied -= HandlePlayerDied;
    }

    private void HandleGameLoaded()
    {
        UpdateGameResumeButton();
    }

    private void HandlePlayerDied()
    {
        if (irisEffectController != null)
        {
            irisEffectController.IrisIn();
        }

        if (restartButton != null) restartButton.SetActive(true);
        if (gameResumeButton != null) gameResumeButton.SetActive(PersistenceManager.Instance != null && PersistenceManager.Instance.HasSaveData());
    }


    public void UpdateGameResumeButton()
    {
        if (gameResumeButton != null)
        {
            bool hasSave = PersistenceManager.Instance != null && PersistenceManager.Instance.HasSaveData();
            gameResumeButton.SetActive(hasSave);
        }
    }

    // UI 버튼에 연결 (GameObject SetActive는 UIManager에서 담당)
    public void OnStartButtonClicked()
    {
        GameManager.Instance.StartNewGame("GameScene"); // 예시 씬명
    }

    public void OnIntroResumeButtonClicked()
    {
        GameManager.Instance.ResumeFromIntro("GameScene");
    }

    public void OnRestartButtonClicked()
    {
        GameManager.Instance.RestartGame();
    }

    public void OnGameResumeButtonClicked()
    {
        GameManager.Instance.ResumeGame();
    }
}
