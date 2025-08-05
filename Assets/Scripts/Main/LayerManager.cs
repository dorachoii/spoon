using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Rendering.Universal.Internal;
using UnityEngine.SceneManagement;
using UnityEngine.Tilemaps;

public class LayerManager : MonoBehaviour
{
    public static LayerManager Instance { get; private set; }

    public int CurrentLayer { get; private set; } = 0;
    public float CurrentLayerHardness { get; private set; } = 1f;
    public float layerHeight = 50f;

    public int CurrentBossLayer { get; private set; } = -1;
    private int lastBossLayer = -1;

    private Camera mainCam;

    public Action<int> OnLayerChanged;
    private int lastLayer = -1;

    private int maxTilesPerFrame = 40;

    private Tilemap tilemap;

    [Header("Layer Display UI")]
    [SerializeField] private TextMeshProUGUI titleText;
    [SerializeField] private CanvasGroup canvasGroup;
    [SerializeField] private float showDuration = 2f;
    [SerializeField] private float fadeTime = 0.2f;

    private Coroutine displayRoutine;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }

    private void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void Start()
    {
        if (tilemap == null)
            tilemap = FindObjectOfType<Tilemap>();
        if (mainCam == null)
            mainCam = Camera.main;

        UpdateLayer();
    }

    // ** 요구사항
    // 기본 레이어들은 layerheight = 50f을 가짐
    // 근데 특수로 보스 레이어가 두번 등장함.
    // 1) 2와 3 사이
    // 2) 4의 끝

    // 이 보스 레이어들은 기존 layerHeight와 관계없이 생성되어야함.
    // 어떻게 구현하는 게 가장 좋을까?
    // 예를 들어 2가 끝났다! 트리거를 주고 보스 생성, 보스 생성 후 다시 기본 레이어 생성시기라는 플래그 주기?


    private string GetLayerName(int layerIndex)
    {
        return layerIndex switch
        {
            0 => "Mine Zone",
            1 => "Crypt Zone 1",
            2 => "Crypt Zone 2",
            3 => "Lava Zone",
            4 => "Ultimate Zone",
            _ => $"Layer{layerIndex}"
        };
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (tilemap == null)
            tilemap = FindObjectOfType<Tilemap>();
        if (mainCam == null)
            mainCam = Camera.main;
        UpdateLayer();
    }

    private void Update()
    {
        UpdateLayer();
    }

    private void UpdateLayer()
    {
        int newLayer = CalculateCurrentLayer();
        int newBossLayer = CalculateBossLayer();

        bool changed = false;

        if (newLayer != lastLayer)
        {
            CurrentLayer = newLayer;
            lastLayer = newLayer;
            OnLayerChanged?.Invoke(CurrentLayer);
            changed = true;
        }

        if (newBossLayer != lastBossLayer)
        {
            CurrentBossLayer = newBossLayer;
            lastBossLayer = newBossLayer;

            // 여기서 보스 등장 관련 처리
            if (CurrentBossLayer >= 0)
            {
                Debug.Log($"보스 {CurrentBossLayer + 1} 등장!");
                // 예: BossManager.Instance.SpawnBoss(CurrentBossLayer);
            }

            changed = true;
        }

        if (changed)
        {
            ShowLayerText(); // 보스 또는 일반 레이어 이름
        }
    }
    private void ShowLayerText()
    {
        string title = CurrentBossLayer switch
        {
            0 => "Boss Chamber I",
            1 => "Boss Chamber II",
            _ => GetLayerName(CurrentLayer)
        };

        titleText.text = title;

        if (displayRoutine != null) StopCoroutine(displayRoutine);
        displayRoutine = StartCoroutine(PlayDisplayRoutine());
    }



    private int CalculateCurrentLayer()
    {
        if (tilemap == null)
        {
            tilemap = FindObjectOfType<Tilemap>();
            if (tilemap == null)
                return Mathf.Max(0, lastLayer);
        }

        if (mainCam == null)
        {
            mainCam = Camera.main;
            if (mainCam == null)
                return Mathf.Max(0, lastLayer);
        }

        Vector3 bottomCenterWorldPos = mainCam.ViewportToWorldPoint(new Vector3(0.5f, 0f, mainCam.nearClipPlane));
        Vector3Int bottomCenterCell = tilemap.WorldToCell(bottomCenterWorldPos);
        Vector3 cellWorldPos = tilemap.CellToWorld(bottomCenterCell);

        float originY = 0f;
        int layer = Mathf.FloorToInt((originY - cellWorldPos.y) / layerHeight);
        return Mathf.Max(0, layer);
    }

    private int CalculateBossLayer()
    {
        float playerY = GetCameraBottomY();

        // 2와 3 사이에 첫 보스 구간
        float boss1StartY = GetLevelEndY(2);
        float boss1EndY = GetLevelStartY(3);

        // 4 끝에 두 번째 보스 구간
        float boss2StartY = GetLevelEndY(4);
        float boss2EndY = boss2StartY - 50f; // 예시: 보스 높이 50f

        if (playerY <= boss1StartY && playerY >= boss1EndY)
            return 0; // 첫 번째 보스
        if (playerY <= boss2StartY && playerY >= boss2EndY)
            return 1; // 두 번째 보스

        return -1; // 보스 없음
    }

    public float levelHeight = 20f;
    public List<Transform> levelList;

    // 해당 레벨의 시작 Y 좌표
    public float GetLevelStartY(int levelIndex)
    {
        if (levelIndex < 0 || levelIndex >= levelList.Count) return 0f;
        return levelList[levelIndex].position.y;
    }

    // 해당 레벨의 끝 Y 좌표
    public float GetLevelEndY(int levelIndex)
    {
        return GetLevelStartY(levelIndex) + levelHeight;
    }



    public float GetTilemapTotalHeight()
    {
        return layerHeight * 5f + 10f;
    }

    public int GetMaxTile()
    {
        return maxTilesPerFrame;
    }

    public float GetCurrentHardness()
    {
        int layer = Mathf.Max(0, CurrentLayer);
        return CurrentLayerHardness = 40f + (layer * Mathf.Sqrt(layer)) * 20f;
    }

    private void ShowLayerText(int layer)
    {
        string title = GetLayerName(layer);
        titleText.text = title;
        if (displayRoutine != null) StopCoroutine(displayRoutine);
        displayRoutine = StartCoroutine(PlayDisplayRoutine());
    }

    private IEnumerator PlayDisplayRoutine()
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
            canvasGroup.alpha += Mathf.Lerp(1f, 0f, t / fadeTime);
            yield return null;
        }
        canvasGroup.alpha = 0f;
    }

    private float GetCameraBottomY()
    {
        Vector3 bottomCenterWorldPos = mainCam.ViewportToWorldPoint(new Vector3(0.5f, 0f, mainCam.nearClipPlane));
        return bottomCenterWorldPos.y;
    }

}
