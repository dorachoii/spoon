using System;
using System.Collections;
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
            1 => "Skull Zone 1",
            2 => "Boss Chamber I",
            3 => "",
            4 => "Skull Zone 2",
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
    private GameObject player;
    private float maincamStartY = -22f; // 플레이어 기본 위치(8)에서 tileOffset(30)만큼 아래


    // layer
    int currentPlayerLayer = -1;
    public int CurrentTilemapLayer { get; private set; } = -1;
    public int CurrentPlayerLayer { get; private set; } = -1; // 플레이어 위치 기준 레이어
    public float CurrentLayerHardness { get; private set; } = 1f;
    public LayerState CurrentLayerState { get; private set; } = LayerState.Normal;
    public LayerState CurrentPlayerLayerState { get; private set; } = LayerState.Normal; // 플레이어 위치 기준 레이어 상태
    
    // 플레이어 준비 상태
    private bool isPlayerFound = false;


    // 층 데이터 관리
    private List<LayerData> layerDataList = new List<LayerData>();
    private float currentLayerEndY = 0f; // 현재 층의 끝 높이 (캐시)
    


    // 이벤트
    public event Action<int> OnLayerChangedForTilemapGeneration;
    public event Action<int> OnLayerChangedForPlayer;
    public event Action<int> OnTransitionLayerEntered; // 전환 층 진입
    public event Action<int> OnBossLayerEntered; // 보스 층 진입
    public event Action OnLavaLayerEntered; // Lava Zone 진입
    public event Action OnLavaLayerExited;
    public event Action OnAllLayersCompleted; // 모든 층 완료
   




    
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

        
    }

    void OnEnable()
    {
        // PersistenceManager 이벤트 구독
        PersistenceManager.OnDataLoaded += OnDataLoaded;
    }

    void OnDisable()
    {
        // PersistenceManager 이벤트 구독 해제
        PersistenceManager.OnDataLoaded -= OnDataLoaded;
    }

    // OnDataLoaded 이벤트 핸들러
    private void OnDataLoaded()
    {

        if (PersistenceManager.Instance.HasSavedData())
        {
            // 저장된 데이터가 있으면 playerPosition과 cameraPosition을 기반으로 레이어 계산
            GameData data = PersistenceManager.Instance.CurrentGameData;
            CalculateLayerFromPlayerPosition(data.playerPosition, data.cameraPosition);
        }
        else
        {
            // 플레이어가 준비되면 레이어 계산을 다시 수행하도록 플래그 설정
            StartCoroutine(CalculateLayerWhenPlayerReady());
        }
    }

    // 플레이어가 준비되면 레이어를 계산하는 코루틴
    private IEnumerator CalculateLayerWhenPlayerReady()
    {
        // 플레이어가 생성될 때까지 대기
        while (player == null)
        {
            yield return null;
        }
        
        // 플레이어가 준비되면 현재 위치로 레이어 계산
        CalculateLayerFromPlayerPosition(player.transform.position, mainCam.transform.position);
    }

    void Start()
    {
        tilemap = TileGenerator.Instance.tilemap;
        
        // 플레이어를 찾을 때까지 코루틴으로 대기
        StartCoroutine(FindPlayerCoroutine());
    }


    private void InitializeLayerData()
    {
        layerDataList.Clear();

        // 기본 레이어 데이터
        LayerData layer1 = new LayerData(0, 0, LayerState.Normal, 20f, -1);
        LayerData layer2 = new LayerData(1, 1, LayerState.Normal, 20f, -1);
        LayerData boss1_transition = new LayerData(2, 0, LayerState.Transition, 12f, -1);
        LayerData boss1 = new LayerData(3, 1, LayerState.Boss, 20f, 0);
        LayerData layer3 = new LayerData(4, 2, LayerState.Normal, 20f, -1);
        LayerData layer4 = new LayerData(5, 3, LayerState.Normal, 20f, -1);
        LayerData layer5 = new LayerData(6, 4, LayerState.Normal, 20f, -1);
        LayerData boss2_transition = new LayerData(7, 0, LayerState.Transition, 12f, -1);
        LayerData boss2 = new LayerData(8, 2, LayerState.Boss, 20f, 1);

        layerDataList.Add(layer1);
        layerDataList.Add(layer2);
        layerDataList.Add(boss1_transition);
        layerDataList.Add(boss1);
        layerDataList.Add(layer3);
        layerDataList.Add(layer4);
        layerDataList.Add(layer5);
        layerDataList.Add(boss2_transition);
        layerDataList.Add(boss2);

  
    }

    private IEnumerator FindPlayerCoroutine()
    {
        // PlayerStat을 찾을 때까지 대기
        while (GameObject.FindGameObjectWithTag("Player") == null)
        {
            yield return null;
        }
        player = GameObject.FindGameObjectWithTag("Player");
        isPlayerFound = true;
        
        // 레이어 계산은 OnDataLoaded에서 처리되므로 여기서는 하지 않음
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


        if (currentViewportY <= maincamStartY - currentLayerEndY)
        {
            // 다음 층으로 이동
            if (CurrentTilemapLayer < layerDataList.Count - 1)
            {
                CurrentTilemapLayer = CalculateTilemapLayer(currentViewportY);
                LayerData newLayerData = layerDataList[CurrentTilemapLayer];
                CurrentLayerState = newLayerData.layerState;

                UpdateCurrentLayerEndHeight();

                OnLayerChangedForTilemapGeneration?.Invoke(CurrentTilemapLayer);

                if (CurrentLayerState == LayerState.Transition)
                {
                    OnTransitionLayerEntered?.Invoke(newLayerData.bossIndex);
                }

                if (CurrentLayerState == LayerState.Boss)
                {
                    OnBossLayerEntered?.Invoke(newLayerData.bossIndex);
                }
            }
            else
            {
                // 모든 층을 완료했을 때
                OnAllLayersCompleted?.Invoke();
            }
        }
    }
    #endregion
    
    #region Player Layer
    private void UpdatePlayerLayer()
    {
        // 플레이어가 준비되지 않았으면 처리하지 않음
        if (!isPlayerFound) return;
        
        float playerY = player.transform.position.y;
        int prevLayer = CurrentPlayerLayer;
        int newPlayerLayer = CalculatePlayerLayer(playerY);

        if (newPlayerLayer != CurrentPlayerLayer)
        {
            CurrentPlayerLayer = newPlayerLayer;

            if (CurrentPlayerLayer >= 0 && CurrentPlayerLayer < layerDataList.Count)
            {
                LayerData playerLayerData = layerDataList[CurrentPlayerLayer];
                CurrentPlayerLayerState = playerLayerData.layerState;

                // 더 깊은 층으로 내려갈 때만 이벤트 발생 (레이어 인덱스가 증가할 때)
                if (CurrentPlayerLayer > prevLayer)
                {
                    if(currentPlayerLayer == CurrentPlayerLayer) return;
                    currentPlayerLayer = CurrentPlayerLayer;
                    OnLayerChangedForPlayer?.Invoke(CurrentPlayerLayer);

                }

                // Lava Zone 진입 감지 (layerIndex 5가 Lava Zone)
                if (playerLayerData.layerIndex == 5)
                {
                    OnLavaLayerEntered?.Invoke();
                }
                else
                {
                    // 이전 레이어가 Lava Zone이었을 때만 Exit 이벤트 발생
                    if (prevLayer == 5 && playerLayerData.layerIndex != 5)
                    {
                        OnLavaLayerExited?.Invoke();
                    }
                }
            }
        }
    }
    #endregion

    #region  Calculate Layers
    private int CalculatePlayerLayer(float playerY)
    {
        float accumulatedHeight = 0f;

        for (int i = 0; i < layerDataList.Count; i++)
        {
            accumulatedHeight += layerDataList[i].layerHeight;
            if (playerY >= maincamStartY - accumulatedHeight)
            {
                return i;
            }
        }

        return layerDataList.Count - 1;
    }

    private int CalculateTilemapLayer(float currentViewportY)
    {
        float accumulatedHeight = 0f;
        for (int i = 0; i < layerDataList.Count; i++)
        {
            accumulatedHeight += layerDataList[i].layerHeight;
            if(currentViewportY >= maincamStartY - accumulatedHeight)
            {
                return i;
            }
        }
        return layerDataList.Count - 1;
    }


    private void UpdateCurrentLayerEndHeight()
    {
        currentLayerEndY = 0f;
        for (int i = 0; i <= CurrentTilemapLayer; i++)
        {
            currentLayerEndY += layerDataList[i].layerHeight;
        }
    }

#endregion
   

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

        if (CurrentTilemapLayer >= 0 && CurrentTilemapLayer < layerDataList.Count)
        {
            return layerDataList[CurrentTilemapLayer].tileIndex;
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

    #region Layer Calculation
    // 플레이어 위치와 카메라 위치를 기반으로 레이어 상태를 계산하는 메서드
    private void CalculateLayerFromPlayerPosition(Vector3 playerPosition, Vector3 cameraPosition)
    {
        InitializeLayerData();
        // 플레이어 레이어 계산
        float playerY = playerPosition.y;
        CurrentPlayerLayer = CalculatePlayerLayer(playerY);
        Debug.Log($"LayerManager: CurrentPlayerLayer: {CurrentPlayerLayer}");
        
        // 플레이어 레이어 상태 설정
        if (CurrentPlayerLayer >= 0 && CurrentPlayerLayer < layerDataList.Count)
        {
            CurrentPlayerLayerState = layerDataList[CurrentPlayerLayer].layerState;
        }
        
        // 카메라 위치를 기반으로 타일맵 레이어 계산
        float viewPortY = mainCam.ViewportToWorldPoint(new Vector3(0.5f, 0f, mainCam.nearClipPlane)).y;
        Debug.Log($"LayerManager: viewPortY: {viewPortY}");
        CurrentTilemapLayer = CalculateTilemapLayer(viewPortY);
        Debug.Log($"LayerManager: CurrentTilemapLayer: {CurrentTilemapLayer}");
        // 타일맵 레이어 상태 설정
        if (CurrentTilemapLayer >= 0 && CurrentTilemapLayer < layerDataList.Count)
        {
            CurrentLayerState = layerDataList[CurrentTilemapLayer].layerState;
        }
        
        // 현재 레이어 끝 높이 계산
        UpdateCurrentLayerEndHeight();
    }

    #endregion


}
