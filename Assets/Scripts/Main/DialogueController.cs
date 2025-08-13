using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class DialogueController : MonoBehaviour
{
    [System.Serializable]
    public struct MakeDialogue
    {
        public bool isBoy;
        public string dialogue;
        public Sprite speakerImage;

        public MakeDialogue(bool flag, string text, Sprite image = null)
        {
            isBoy = flag;
            dialogue = text;
            speakerImage = image;
        }
    }

    [Header("UI 참조")]
    [SerializeField] private GameObject canvas_dialogue;
    [SerializeField] private TextMeshProUGUI dialogueText;
    [SerializeField] private Image speakerImage;


    [SerializeField] private Sprite[] intro_img;
    [SerializeField] private Sprite[] outro_img;

    private MakeDialogue[] dialogues_intro;
    private MakeDialogue[] dialogues_outro;

    private MakeDialogue[] dialogues;

    private int currentIndex = 0;

    // 대화 상태는 GameManager에서 관리

    private Color femaleColor = Color.yellow;
    private Color defaultColor = Color.white;

    void Start()
    {
        // GameManager 이벤트 구독
        if (GameManager.Instance != null)
        {
            GameManager.Instance.OnGameCleared += OnGameCleared;
            GameManager.Instance.OnIntroCompleted += OnIntroCompleted;
        }

        // 여기서 대화 배열 초기화 (예: 아까 준 대화)
        dialogues_intro = new MakeDialogue[] {
            new MakeDialogue(true, "俺、マジで猫舌なんだニャン〜\nアイスクリーム最高だニャン〜！", intro_img[0]),
            new MakeDialogue(false, "(ニャン〜って…うるせぇな)\nほんとにおいしいの〜♡", intro_img[0]),
            new MakeDialogue(true, "…！", intro_img[1]),
            new MakeDialogue(false, "（マンホールにスポンと\n落ちちゃった彼女）", intro_img[2]),
            new MakeDialogue(true, "位置共有アプリ、\n確認しようニャン〜",intro_img[3]),
            new MakeDialogue(true, "え？\n地球の裏側に落ちたってニャン〜",intro_img[4]),
            new MakeDialogue(true, "武器はこのスプーンだけニャン〜\nこれで地面掘ってみるかニャン〜！", intro_img[5])
        };

        dialogues_outro = new MakeDialogue[] {
            new MakeDialogue(true, "やっと着いたニャン〜", outro_img[0]),
            new MakeDialogue(true, "あれ？", outro_img[1]),
            new MakeDialogue(true, "信号が消えた？なんだニャン〜それ！", outro_img[2]),
            new MakeDialogue(false, "肉まんモグモグ、湯気モクモク〜", outro_img[3]),
            new MakeDialogue(true, "うさぎと浮気ニャン〜？！", outro_img[4]),
            new MakeDialogue(true, "冷ませば肉まん食べられるけどニャン〜", outro_img[5]),
            new MakeDialogue(true, "またかニャン〜これ？", outro_img[6]),
            new MakeDialogue(true, "..", outro_img[7]),
            new MakeDialogue(true, "じゃあニャン〜", outro_img[8]),
            new MakeDialogue(false, "(ぽこんと)", outro_img[9]),
        };
            // 대화 초기화 및 상태 확인
        CheckAndStartDialogue();
    }

    // 대화 상태를 확인하고 적절한 대화 시작
    private void CheckAndStartDialogue()
    {
        // 게임이 클리어되었으면 outro 재생
        if (GameManager.Instance != null && GameManager.Instance.IsGameCleared())
        {
            SetDialogue(dialogues_outro);
            canvas_dialogue.SetActive(true);
            ShowDialogue(currentIndex);
            return;
        }

        // Intro가 아직 재생되지 않았으면 intro 재생
        if (GameManager.Instance != null && !GameManager.Instance.HasPlayedIntro())
        {
            SetDialogue(dialogues_intro);
            canvas_dialogue.SetActive(true);
            ShowDialogue(currentIndex);
            return;
        }

        // Intro가 이미 재생되었으면 캔버스를 켜지 않음
        canvas_dialogue.SetActive(false);
    }

    // GameManager의 게임 클리어 이벤트 핸들러
    private void OnGameCleared()
    {

        CheckAndStartDialogue();
    }

    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            NextDialogue();
        }
    }

    void SetDialogue(MakeDialogue[] dia)
    {

        dialogues = dia;
    }

    void ShowDialogue(int index)
    {
        if (index < 0 || index >= dialogues.Length) return;

        dialogueText.text = dialogues[index].dialogue;


        speakerImage.sprite = dialogues[index].speakerImage;

        dialogueText.color = dialogues[index].isBoy ? defaultColor : femaleColor;

    }

    void NextDialogue()
    {
        currentIndex++;
        if (currentIndex >= dialogues.Length)
        {
            Debug.Log("대화가 끝났습니다.");
            currentIndex = 0;
            canvas_dialogue.SetActive(false);

            // Intro 대화가 완료되면 GameManager에 알림
            if (dialogues == dialogues_intro)
            {
                if (GameManager.Instance != null)
                {
                    GameManager.Instance.SetIntroCompleted();
                }
                Debug.Log("[DialogueController] Intro 대화 완료 - GameManager에 알림");
            }

            return;
        }

        ShowDialogue(currentIndex);
    }

        // GameManager의 Intro 완료 이벤트 핸들러
    private void OnIntroCompleted()
    {
        Debug.Log("[DialogueController] Intro 완료 이벤트 수신");
    }
    
    private void OnDestroy()
    {
        // GameManager 이벤트 구독 해제
        if (GameManager.Instance != null)
        {
            GameManager.Instance.OnGameCleared -= OnGameCleared;
            GameManager.Instance.OnIntroCompleted -= OnIntroCompleted;
        }
    }
}
