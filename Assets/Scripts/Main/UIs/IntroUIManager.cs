using UnityEngine;
using UnityEngine.UI;

public static class SceneNames
{
    public const string INTRO_SCENE_NAME = "IntroScene";
    public const string GAME_SCENE_NAME = "GameScene";
}

public class IntroUIManager : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] private GameObject startBtn;
    [SerializeField] private GameObject continueBtn;

    private Button startButtonComp;
    private Button continueButtonComp;

    private void Awake()
    {
        if (startBtn != null)
            startButtonComp = startBtn.GetComponent<Button>();
        if (continueBtn != null)
            continueButtonComp = continueBtn.GetComponent<Button>();
    }

    void Start()
    {
        // saved data exists: Resume Button 활성화 (有効化)
        bool hasSavedData = PersistenceManager.Instance != null && PersistenceManager.Instance.HasSavedData();
        continueBtn.SetActive(hasSavedData);

        // Button Event 연결 (接続)
        if (startButtonComp != null)
        {
            startButtonComp.onClick.RemoveAllListeners();
            startButtonComp.onClick.AddListener(() =>
            {
                // new game
                GameManager.Instance.StartNewGame(SceneNames.GAME_SCENE_NAME); 
            });
        }

        if (continueButtonComp != null)
        {
            continueButtonComp.onClick.RemoveAllListeners();
            continueButtonComp.onClick.AddListener(() =>
            {
                // continue
                GameManager.Instance.ResumeFromIntro(SceneNames.GAME_SCENE_NAME);
            });
        }
    }
}
