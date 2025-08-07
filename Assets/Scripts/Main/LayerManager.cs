using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class LayerManager : MonoBehaviour
{
    public static LayerManager Instance { get; private set; }
    private Camera mainCam;

    [Header("Layer")]
    [SerializeField] private float layerHeight = 50f;
    private int lastLayer = -1;
    
    public int CurrentLayer { get; private set; } = 0;
    public float CurrentLayerHardness { get; private set; } = 1f;
    public int CurrentBossLayer { get; private set; } = -1;
    private int lastBossLayer = -1;

    public Action<int> OnLayerChanged;
   

    [Header("Tilemap")]
    private Tilemap tilemap;
    private int maxTilesPerFrame = 40;  // 한 프레임에 처리할 최대 타일 수 (1Frameに処理する最大タイル数)


    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        mainCam = Camera.main;
    }

    void Start()
    {
        tilemap = TileGenerator.Instance.tilemap;
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
            // 보스 레이어 변경 시 UI 업데이트
            if (CurrentBossLayer >= 0)
            {
                GameUIManager gameUI = FindObjectOfType<GameUIManager>();
                if (gameUI != null)
                {
                    gameUI.ShowBossLayerText(CurrentBossLayer);
                }
            }
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
        return CurrentLayerHardness = 40f + layer * Mathf.Sqrt(layer) * 20f;
    }



    private float GetCameraBottomY()
    {
        Vector3 bottomCenterWorldPos = mainCam.ViewportToWorldPoint(new Vector3(0.5f, 0f, mainCam.nearClipPlane));
        return bottomCenterWorldPos.y;
    }

}
