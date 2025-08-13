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



    private Color femaleColor = Color.yellow;
    private Color defaultColor = Color.white;

    void Start()
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.OnGameCleared += OnGameCleared;
            GameManager.Instance.OnIntroCompleted += OnIntroCompleted;
        }

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
        CheckAndStartDialogue();
    }

    private void CheckAndStartDialogue()
    {
        if (GameManager.Instance != null && GameManager.Instance.IsGameCleared())
        {
            SetDialogue(dialogues_outro);
            canvas_dialogue.SetActive(true);
            ShowDialogue(currentIndex);
            return;
        }

        if (GameManager.Instance != null && !GameManager.Instance.HasPlayedIntro())
        {
            SetDialogue(dialogues_intro);
            canvas_dialogue.SetActive(true);
            ShowDialogue(currentIndex);
            return;
        }

        canvas_dialogue.SetActive(false);
    }

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
        if (dia == null) return;
        dialogues = dia;
    }

    void ShowDialogue(int index)
    {
        if (dialogues == null || index < 0 || index >= dialogues.Length) return;

        dialogueText.text = dialogues[index].dialogue;

        speakerImage.sprite = dialogues[index].speakerImage;

        dialogueText.color = dialogues[index].isBoy ? defaultColor : femaleColor;
    }

    void NextDialogue()
    {
        if (dialogues == null || dialogues.Length == 0) return;
        
        currentIndex++;
        if (currentIndex >= dialogues.Length)
        {
            currentIndex = 0;
            canvas_dialogue.SetActive(false);

            if (dialogues == dialogues_intro)
            {
                if (GameManager.Instance != null)
                {
                    GameManager.Instance.SetIntroCompleted();
                }
            }

            return;
        }

        ShowDialogue(currentIndex);
    }

    private void OnIntroCompleted()
    {
    }
    
    private void OnDestroy()
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.OnGameCleared -= OnGameCleared;
            GameManager.Instance.OnIntroCompleted -= OnIntroCompleted;
        }
    }
}
