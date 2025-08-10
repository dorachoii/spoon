using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class GameUIManager : MonoBehaviour
{
    [Header("Game UI - GameOver")]
    [SerializeField] private GameObject gameover_restartButton;
    [SerializeField] private GameObject gameover_resumeButton;
    [SerializeField] private IrisEffector irisEffector;

    [Header("Game UI - Pause")]
    [SerializeField] private GameObject pauseButton;
    [SerializeField] private GameObject pause_resumeButton;
    [SerializeField] private GameObject pause_newGameButton;
    [SerializeField] private GameObject pause_backToTitleButton;

    [Header("Layer Change UI")]
    [SerializeField] private TextMeshProUGUI titleText;
    [SerializeField] private CanvasGroup canvasGroup;
    [SerializeField] private float showDuration = 2f;
    [SerializeField] private float fadeTime = 0.2f;

    [Header("Boss Death UI")]
    [SerializeField] private GameObject[] fireworksEffects;

    private Coroutine centerTextCoroutine;


    private Button gameover_restartButtonComp;
    private Button gameover_resumeButtonComp;
    private Button pause_resumeButtonComp;
    private Button pause_newGameButtonComp;
    private Button pause_backToTitleButtonComp;

    private void Awake()
    {
        gameover_restartButton.SetActive(false);
        gameover_resumeButton.SetActive(false);
        irisEffector = GetComponent<IrisEffector>();

        pause_resumeButton.SetActive(false);
        pause_newGameButton.SetActive(false);
        pause_backToTitleButton.SetActive(false);

        if (gameover_restartButton != null)
            gameover_restartButtonComp = gameover_restartButton.GetComponent<Button>();
        if (gameover_resumeButton != null)
            gameover_resumeButtonComp = gameover_resumeButton.GetComponent<Button>();
        
        if (pause_resumeButton != null)
            pause_resumeButtonComp = pause_resumeButton.GetComponent<Button>();
        if (pause_newGameButton != null)
            pause_newGameButtonComp = pause_newGameButton.GetComponent<Button>();
        if (pause_backToTitleButton != null)
            pause_backToTitleButtonComp = pause_backToTitleButton.GetComponent<Button>();
    }

    void Start()
    {
        PlayerStat.Instance.OnDied += HandlePlayerDied;
        LayerManager.Instance.OnLayerChangedForPlayer += HandleLayerChanged;
        
        // 정적 보스 죽음 이벤트 구독
        BossHP.OnAnyBossDeath += HandleBossDeath;

        // Button Event 연결 (接続)
        if (gameover_restartButtonComp != null)
        {
            gameover_restartButtonComp.onClick.RemoveAllListeners();
            gameover_restartButtonComp.onClick.AddListener(() =>
            {
                // new game
                GameManager.Instance.StartNewGame(); 
            });
        }

        if (gameover_resumeButtonComp != null)
        {
            gameover_resumeButtonComp.onClick.RemoveAllListeners();
            gameover_resumeButtonComp.onClick.AddListener(() =>
            {
                // continue
                GameManager.Instance.StartFromSavedGame();
            });
        }

        if (pause_resumeButtonComp != null)
        {
            pause_resumeButtonComp.onClick.RemoveAllListeners();
            pause_resumeButtonComp.onClick.AddListener(() =>
            {
                // resume
                TogglePauseUI();
            });
        }

        if (pause_newGameButtonComp != null)
        {
            pause_newGameButtonComp.onClick.RemoveAllListeners();
            pause_newGameButtonComp.onClick.AddListener(() =>
            {
                // new game
                GameManager.Instance.StartNewGame();
            });
        }

        if (pause_backToTitleButtonComp != null)
        {
            pause_backToTitleButtonComp.onClick.RemoveAllListeners();
            pause_backToTitleButtonComp.onClick.AddListener(() =>
            {
                // back to title
                GameManager.Instance.BackToTitle();
            });
        }

        if (pauseButton != null)
        {
            Button pauseButtonComp = pauseButton.GetComponent<Button>();
            if (pauseButtonComp != null)
            {
                pauseButtonComp.onClick.RemoveAllListeners();
                pauseButtonComp.onClick.AddListener(() =>
                {
                    // pause UI toggle
                    TogglePauseUI();
                });
            }
        }
    }

    void OnDestroy()
    {
        PlayerStat.Instance.OnDied -= HandlePlayerDied;
        LayerManager.Instance.OnLayerChangedForPlayer -= HandleLayerChanged;
        
        // 정적 보스 죽음 이벤트 구독 해제
        BossHP.OnAnyBossDeath -= HandleBossDeath;
    }

    private void HandlePlayerDied()
    {
        // iris in
        if (irisEffector != null) irisEffector.IrisIn();

        // button 갱신 (更新)
        bool hasSavedData = PersistenceManager.Instance != null && PersistenceManager.Instance.HasSavedData();
        if (gameover_resumeButton != null) gameover_resumeButton.SetActive(hasSavedData);

        if (gameover_restartButton != null) gameover_restartButton.SetActive(true);
    }

    private void TogglePauseUI()
    {
        bool isPauseUIActive = pause_resumeButton.activeSelf;
        
        pause_resumeButton.SetActive(!isPauseUIActive);
        pause_newGameButton.SetActive(!isPauseUIActive);
        pause_backToTitleButton.SetActive(!isPauseUIActive);
        
        Time.timeScale = isPauseUIActive ? 1f : 0f;
    }

    private void HandleLayerChanged(int layerIndex)
    {
        ShowLayerText();
    }

    // 보스 죽음 처리
    public void HandleBossDeath()
    {
        ShowBossDeathUI();
        SpawnFireworksEffect();
    }

    // 보스 죽음 UI 표시 (centerText 사용)
    private void ShowBossDeathUI()
    {
        ShowBossClearedText();
    }

    // 폭죽 효과 생성
    private void SpawnFireworksEffect()
    {
        // 모든 폭죽 효과 켜기
        for (int i = 0; i < fireworksEffects.Length; i++)
        {
            if (fireworksEffects[i] != null)
            {
                fireworksEffects[i].SetActive(true);
            }
        }
        
        // 3초 후 자동으로 끄기
        StartCoroutine(HideFireworksAfterDelay());
    }

    private IEnumerator HideFireworksAfterDelay()
    {
        yield return new WaitForSeconds(3f);
        
        // 모든 폭죽 효과 끄기
        for (int i = 0; i < fireworksEffects.Length; i++)
        {
            if (fireworksEffects[i] != null)
            {
                fireworksEffects[i].SetActive(false);
            }
        }
    }

    // 보스 클리어 텍스트 표시
    private void ShowBossClearedText()
    {
        string title = "BOSS CLEARED!";
        titleText.text = title;
        titleText.color = new Color(1f, 0.8f, 0f); // 골드 색상
        
        if (centerTextCoroutine != null) StopCoroutine(centerTextCoroutine);
        centerTextCoroutine = StartCoroutine(IShowBossClearedText());
    }

    private IEnumerator IShowBossClearedText()
    {
        float t = 0f;
        while (t < fadeTime)
        {
            t += Time.deltaTime;
            canvasGroup.alpha = Mathf.Lerp(0f, 1f, t / fadeTime);
            yield return null;
        }
        canvasGroup.alpha = 1f;

        yield return new WaitForSeconds(3f); // 3초 동안 표시

        t = 0f;
        while (t < fadeTime)
        {
            t += Time.deltaTime;
            canvasGroup.alpha = Mathf.Lerp(1f, 0f, t / fadeTime);
            yield return null;
        }
        canvasGroup.alpha = 0f;
    }

    private void ShowLayerText(Color textColor = default)
    {
        string title = LayerManager.Instance.GetCurrentLayerName();
        titleText.text = title;
        
        // 현재 일반 층이면 검정, 보스 층이면 보라 이렇게 표시
        if (textColor == default)
        {
            if (true)
            {
                textColor = new Color(0.5f, 0f, 1f); // 보라색
            }
            else
            {
                textColor = Color.black; // 검정색
            }
        }
        titleText.color = textColor;
        
        if (centerTextCoroutine != null) StopCoroutine(centerTextCoroutine);
        centerTextCoroutine = StartCoroutine(IShowLayerText());
    }

    private IEnumerator IShowLayerText()
    {
        float t = 0f;
        while (t < fadeTime)
        {
            t += Time.deltaTime;
            canvasGroup.alpha = Mathf.Lerp(0f, 1f, t / fadeTime);
            yield return null;
        }
        canvasGroup.alpha = 1f;

        yield return new WaitForSeconds(showDuration - fadeTime * 2f);

        t = 0f;
        while (t < fadeTime)
        {
            t += Time.deltaTime;
            canvasGroup.alpha = Mathf.Lerp(1f, 0f, t / fadeTime);
            yield return null;
        }
        canvasGroup.alpha = 0f;
    }
}
