using UnityEngine;
using UnityEngine.UI;

public class GameUIManager : MonoBehaviour
{
    [Header("Game UI")]
    [SerializeField] private GameObject restartButton;
    [SerializeField] private GameObject gameResumeButton;
    [SerializeField] private IrisEffector irisEffector;

    private Button restartButtonComp;
    private Button resumeButtonComp;

    private void Awake()
    {
        restartButton.SetActive(false);
        gameResumeButton.SetActive(false);
        irisEffector = GetComponent<IrisEffector>();
        if (restartButton != null)
            restartButtonComp = restartButton.GetComponent<Button>();
        if (gameResumeButton != null)
            resumeButtonComp = gameResumeButton.GetComponent<Button>();
    }

    void Start()
    {
        PlayerStat.Instance.OnDied += HandlePlayerDied;

        // Button Event 연결 (接続)
        if (restartButtonComp != null)
        {
            restartButtonComp.onClick.RemoveAllListeners();
            restartButtonComp.onClick.AddListener(() =>
            {
                // new game
                GameManager.Instance.StartNewGame(SceneNames.GAME_SCENE_NAME); 
            });
        }

        if (resumeButtonComp != null)
        {
            resumeButtonComp.onClick.RemoveAllListeners();
            resumeButtonComp.onClick.AddListener(() =>
            {
                // continue
                GameManager.Instance.ResumeFromIntro(SceneNames.GAME_SCENE_NAME);
            });
        }
    }

    void OnDestroy()
    {
        PlayerStat.Instance.OnDied -= HandlePlayerDied;
    }

    private void HandlePlayerDied()
    {
        // iris in
        if (irisEffector != null) irisEffector.IrisIn();

        // button 갱신 (更新)
        bool hasSavedData = PersistenceManager.Instance != null && PersistenceManager.Instance.HasSavedData();
        if (gameResumeButton != null) gameResumeButton.SetActive(hasSavedData);

        if (restartButton != null) restartButton.SetActive(true);
    }


}
