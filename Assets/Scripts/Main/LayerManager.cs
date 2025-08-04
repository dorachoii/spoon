using System;
using System.Collections;
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
        TryRebindReferences();
        UpdateLayer(); // 초기 레이어 계산
    }

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
        TryRebindReferences();
        UpdateLayer();
    }

    private void Update()
    {
        UpdateLayer();
    }

    private void TryRebindReferences()
    {
        if (tilemap == null)
            tilemap = FindObjectOfType<Tilemap>();
        if (mainCam == null)
            mainCam = Camera.main;
    }

    private void UpdateLayer()
    {
        int newLayer = CalculateCurrentLayer();

        if (newLayer != lastLayer)
        {
            CurrentLayer = newLayer;
            lastLayer = newLayer;
            OnLayerChanged?.Invoke(CurrentLayer);
            ShowLayerText(CurrentLayer);
        }
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

    public float GetLevelStartY(int layer)
    {
        return -layer * layerHeight;
    }

    public float GetLevelEndY(int layer)
    {
        return -(layer + 1) * layerHeight;
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
}
