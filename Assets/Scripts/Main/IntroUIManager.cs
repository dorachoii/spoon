using UnityEngine;
using UnityEngine.UI;

public class IntroUIManager : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] private GameObject startBtn;
    [SerializeField] private GameObject resumeBtn;

    private Button startButtonComp;
    private Button resumeButtonComp;

    private void Awake()
    {
        if (startBtn != null)
            startButtonComp = startBtn.GetComponent<Button>();
        if (resumeBtn != null)
            resumeButtonComp = resumeBtn.GetComponent<Button>();
    }

    void Start()
    {
        // Resume 활성화 여부 (저장된 게 있으면)
        bool hasSave = PersistenceManager.Instance != null && PersistenceManager.Instance.HasSaveData();
        if (resumeBtn != null)
            resumeBtn.SetActive(hasSave);

        // 버튼 이벤트 연결
        if (startButtonComp != null)
        {
            startButtonComp.onClick.RemoveAllListeners();
            startButtonComp.onClick.AddListener(() =>
            {
                // 새 게임: 기존 저장 지우고 GameScene 로드
                GameManager.Instance.StartNewGame("GameScene"); // 씬 이름 맞추기
            });
        }

        if (resumeButtonComp != null)
        {
            resumeButtonComp.onClick.RemoveAllListeners();
            resumeButtonComp.onClick.AddListener(() =>
            {
                // 이어하기: 플래그 세우고 GameScene 로드
                GameManager.Instance.ResumeFromIntro("GameScene");
            });
        }
    }
}
