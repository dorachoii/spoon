using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public enum LayerState
{
    Normal,
    Transition,
    Boss,
}

[System.Serializable]
public class LayerData
{
    public int layerIndex;
    public int tileIndex;
    public LayerState layerState;
    public float layerHeight;
    public int bossIndex; // 보스 종류 구분 (-1이면 일반 층, 전환 층)
    public string layerName; // 레이어 이름

    public LayerData(int layerIndex, int tileIndex, LayerState layerState, float layerHeight, int bossIndex = -1)
    {
        this.layerIndex = layerIndex;
        this.tileIndex = tileIndex;
        this.layerState = layerState;
        this.layerHeight = layerHeight;
        this.bossIndex = bossIndex;
        layerName = GetLayerName(layerIndex);
    }

    // 레이어 인덱스에 따른 이름 반환
    private string GetLayerName(int layerIndex)
    {
        return layerIndex switch
        {
            0 => "Mine Zone",
            1 => "Crypt Zone 1",
            2 => "Boss Chamber I",
            3 => "",
            4 => "Crypt Zone 2",
            5 => "Lava Zone",
            6 => "Ultimate Zone",
            7 => "Boss Chamber II",
            8 => "",
            _ => $"Layer{layerIndex}"
        };
    }
}

public class LayerManager : MonoBehaviour
{
    public static LayerManager Instance { get; private set; }

    [Header("Camera")]
    private Camera mainCam;
    private float mainCamStartY = 0f;


    // layer
    public int CurrentTileLayer { get; private set; } = -1;
    public int CurrentPlayerLayer { get; private set; } = -1; // 플레이어 위치 기준 레이어
    public float CurrentLayerHardness { get; private set; } = 1f;
    public LayerState CurrentLayerState { get; private set; } = LayerState.Normal;
    public LayerState CurrentPlayerLayerState { get; private set; } = LayerState.Normal; // 플레이어 위치 기준 레이어 상태




    // 층 데이터 관리
    private List<LayerData> layerDataList = new List<LayerData>();
    private float currentLayerEndY = 0f; // 현재 층의 끝 높이 (캐시)


    // 이벤트
    public event Action<int> OnLayerChangedForTilemapGeneration;
    public event Action<int> OnLayerChangedForPlayer;
    public event Action<int> OnTransitionLayerEntered; // 전환 층 진입
    public event Action<int> OnBossLayerEntered; // 보스 층 진입
   



    [Header("Tilemap")]
    private Tilemap tilemap;
    private int maxTilesPerFrame = 40;  // 한 프레임에 처리할 최대 타일 수 (1Frameに処理する最大タイル数)

    #region Initialize
    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        mainCam = Camera.main;
        InitializeLayerData();
    }

    void Start()
    {
        tilemap = TileGenerator.Instance.tilemap;
        BossHP.OnAnyBossDeath += HandleBossDeath;
    }


    private void InitializeLayerData()
    {
        mainCamStartY = mainCam.ViewportToWorldPoint(new Vector3(0.5f, 0f, mainCam.nearClipPlane)).y;
        layerDataList.Clear();

        LayerData layer1 = new LayerData(0, 0, LayerState.Normal, 20f, -1);
        LayerData layer2 = new LayerData(1, 1, LayerState.Normal, 20f, -1);

        LayerData boss1_transition = new LayerData(2, 0, LayerState.Transition, 12f, -1);
        LayerData boss1 = new LayerData(3, 1, LayerState.Boss, 80f, 0);

        LayerData layer3 = new LayerData(4, 2, LayerState.Normal, 20f, -1);
        LayerData layer4 = new LayerData(5, 3, LayerState.Normal, 20f, -1);
        LayerData layer5 = new LayerData(6, 4, LayerState.Normal, 20f, -1);

        LayerData boss2_transition = new LayerData(7, 0, LayerState.Transition, 12f, -1);
        LayerData boss2 = new LayerData(8, 2, LayerState.Boss, 80f, 1);


        layerDataList.Add(layer1);
        layerDataList.Add(layer2);
        layerDataList.Add(boss1_transition);
        layerDataList.Add(boss1);
        layerDataList.Add(layer3);
        layerDataList.Add(layer4);
        layerDataList.Add(layer5);
        layerDataList.Add(boss2_transition);
        layerDataList.Add(boss2);

        // 초기 층의 끝 높이 계산
        UpdateCurrentLayerEndHeight();
    }

    void OnDestroy()
    {
        BossHP.OnAnyBossDeath -= HandleBossDeath;
    }
    #endregion

    private void Update()
    {
        UpdateTilemapLayer();
        UpdatePlayerLayer();
    }

    #region Tilemap Layer
    private void UpdateTilemapLayer()
    {
        Vector3 bottomCenterWorldPos = mainCam.ViewportToWorldPoint(new Vector3(0.5f, 0f, mainCam.nearClipPlane));
        float currentViewportY = bottomCenterWorldPos.y;

        UpdateCurrentLayerEndHeight();

        if (currentViewportY <= mainCamStartY - currentLayerEndY)
        {
            // 다음 층으로 이동
            if (CurrentTileLayer < layerDataList.Count - 1)
            {
                CurrentTileLayer++;
                LayerData newLayerData = layerDataList[CurrentTileLayer];
                CurrentLayerState = newLayerData.layerState;

                UpdateCurrentLayerEndHeight();

                OnLayerChangedForTilemapGeneration?.Invoke(CurrentTileLayer);

                if (CurrentLayerState == LayerState.Transition)
                {
                    OnTransitionLayerEntered?.Invoke(newLayerData.bossIndex);
                }

                if (CurrentLayerState == LayerState.Boss)
                {
                    OnBossLayerEntered?.Invoke(newLayerData.bossIndex);
                }
            }
        }
    }
    #endregion
    
    #region Player Layer
    private void UpdatePlayerLayer()
    {
        Vector3 playerPos = PlayerStat.Instance.transform.position;
        float playerY = playerPos.y;

        int newPlayerLayer = CalculatePlayerLayer(playerY);

        if (newPlayerLayer != CurrentPlayerLayer)
        {
            CurrentPlayerLayer = newPlayerLayer;

            if (CurrentPlayerLayer >= 0 && CurrentPlayerLayer < layerDataList.Count)
            {
                LayerData playerLayerData = layerDataList[CurrentPlayerLayer];
                CurrentPlayerLayerState = playerLayerData.layerState;

                OnLayerChangedForPlayer?.Invoke(CurrentPlayerLayer);
            }
        }
    }
    private int CalculatePlayerLayer(float playerY)
    {
        float accumulatedHeight = 0f;

        for (int i = 0; i < layerDataList.Count; i++)
        {
            accumulatedHeight += layerDataList[i].layerHeight;
            if (playerY >= mainCamStartY - accumulatedHeight)
            {
                return i;
            }
        }

        return layerDataList.Count - 1;
    }
    #endregion


    private void UpdateCurrentLayerEndHeight()
    {
        currentLayerEndY = 0f;
        for (int i = 0; i <= CurrentTileLayer; i++)
        {
            currentLayerEndY += layerDataList[i].layerHeight;
        }
    }


    public void HandleBossDeath()
    {
        Vector3 bottomCenterWorldPos = mainCam.ViewportToWorldPoint(new Vector3(0.5f, 0f, mainCam.nearClipPlane));
        float currentViewportY = bottomCenterWorldPos.y;

        // 보스 층이 시작된 시점의 높이 계산 (이전 층들의 총 높이)
        float bossLayerStartY = mainCamStartY;
        if (CurrentTileLayer > 0)
        {
            float previousLayersHeight = 0f;
            for (int i = 0; i < CurrentTileLayer; i++)
            {
                previousLayersHeight += layerDataList[i].layerHeight;
            }
            bossLayerStartY = mainCamStartY - previousLayersHeight;
        }

        // 보스 층의 실제 높이 = 보스 층 시작 시점부터 현재까지의 높이
        float currentLayerHeight = bossLayerStartY - currentViewportY;
        if (CurrentTileLayer >= 0 && CurrentTileLayer < layerDataList.Count)
        {
            layerDataList[CurrentTileLayer].layerHeight = currentLayerHeight;
        }

        // 현재 층의 끝 높이 재계산
        UpdateCurrentLayerEndHeight();
        
    }

    #region Getter
    public float GetCurrentHardness()
    {
        int layer = Mathf.Max(0, CurrentPlayerLayer);
        return CurrentLayerHardness = 40f + layer * Mathf.Sqrt(layer) * 20f;
    }


    public string GetCurrentLayerName()
    {
        if (CurrentPlayerLayer >= 0 && CurrentPlayerLayer < layerDataList.Count)
        {
            return layerDataList[CurrentPlayerLayer].layerName;
        }
        return "Unknown Layer";
    }


    public int GetCurrentLayerTileIndex()
    {
        Debug.Log($"지금 layer는 {CurrentTileLayer}, 지금 tile index는 {layerDataList[CurrentTileLayer].tileIndex}");
        if (CurrentTileLayer >= 0 && CurrentTileLayer < layerDataList.Count)
        {
            return layerDataList[CurrentTileLayer].tileIndex;
        }
        return 0; // 기본값
    }


    public float GetTilemapTotalHeight()
    {
        float totalHeight = 0;
        foreach (var layerData in layerDataList)
        {
            totalHeight += layerData.layerHeight;
        }
        return totalHeight;
    }

    public int GetMaxTile()
    {
        return maxTilesPerFrame;
    }
    #endregion
}
