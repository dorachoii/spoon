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
    public LayerState state;
    public float layerHeight;
    public int bossIndex; // 보스 종류 구분 (-1이면 일반 층, 전환 층)
    public string layerName; // 레이어 이름

    public LayerData(int index, LayerState layerState, float height, int boss = -1)
    {
        layerIndex = index;
        state = layerState;
        layerHeight = height;
        bossIndex = boss;
        layerName = GetLayerName(index);
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
    public int CurrentLayer { get; private set; } = -1;
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
    public event Action<LayerState> OnLayerStateChanged;
    public event Action<int> OnTransitionLayerEntered; // 전환 층 진입
    public event Action<int> OnBossLayerEntered; // 보스 층 진입
    public event Action<int> OnBossLayerExited;  // 보스 층 퇴장


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
        InitializeLayerData();
    }

    void Start()
    {
        tilemap = TileGenerator.Instance.tilemap;
    }

    private void Update()
    {
        UpdateTilemapLayer();
        UpdatePlayerLayer();
    }   

    private void InitializeLayerData()
    {
        mainCamStartY =  mainCam.ViewportToWorldPoint(new Vector3(0.5f, 0f, mainCam.nearClipPlane)).y;
        layerDataList.Clear();

        LayerData layer1 = new LayerData(0, LayerState.Normal, 20f, -1);
        LayerData layer2 = new LayerData(1, LayerState.Normal, 20f, -1);

        LayerData boss1_transition = new LayerData(2, LayerState.Transition, 12f, -1);
        LayerData boss1 = new LayerData(3, LayerState.Boss, 80f, 0);

        LayerData layer3 = new LayerData(4, LayerState.Normal, 20f, -1);
        LayerData layer4 = new LayerData(5, LayerState.Normal, 20f, -1);
        LayerData layer5 = new LayerData(6, LayerState.Normal, 20f, -1);

        LayerData boss2_transition = new LayerData(7, LayerState.Transition, 12f, -1);
        LayerData boss2 = new LayerData(8, LayerState.Boss, 80f, 1);


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

    private void UpdateTilemapLayer()
    {
        Vector3 bottomCenterWorldPos = mainCam.ViewportToWorldPoint(new Vector3(0.5f, 0f, mainCam.nearClipPlane));
        float currentViewportY = bottomCenterWorldPos.y;

        // 카메라가 현재 층을 넘어갔는지 체크 (캐시된 값 사용)
        if (currentViewportY <= mainCamStartY - currentLayerEndY)
        {
            // 다음 층으로 이동
            if (CurrentLayer < layerDataList.Count - 1)
            {
                CurrentLayer++;
                LayerData newLayerData = layerDataList[CurrentLayer];
                CurrentLayerState = newLayerData.state;

                // 새로운 층의 끝 높이 계산 (한 번만)
                UpdateCurrentLayerEndHeight();

                // 이벤트 발생
                OnLayerChangedForTilemapGeneration?.Invoke(CurrentLayer);
                OnLayerStateChanged?.Invoke(CurrentLayerState);

                // 전환 층 진입/퇴장 이벤트
                if (CurrentLayerState == LayerState.Transition)
                {
                    Debug.Log($"전환 층 진입: {newLayerData.bossIndex}");
                    OnTransitionLayerEntered?.Invoke(newLayerData.bossIndex);
                    
                }

                // 보스 층 진입/퇴장 이벤트
                if (CurrentLayerState == LayerState.Boss)
                {
                    OnBossLayerEntered?.Invoke(newLayerData.bossIndex);
                }
            }
        }
    }

    private void UpdatePlayerLayer()
    {
        Vector3 playerPos = GameObject.FindWithTag("Player").transform.position;
        float playerY = playerPos.y;
        
        // 플레이어의 현재 레이어 계산
        int newPlayerLayer = CalculatePlayerLayer(playerY);
        
        // 플레이어 레이어가 변경되었는지 확인
        if (newPlayerLayer != CurrentPlayerLayer)
        {
            CurrentPlayerLayer = newPlayerLayer;
            
            if (CurrentPlayerLayer >= 0 && CurrentPlayerLayer < layerDataList.Count)
            {
                LayerData playerLayerData = layerDataList[CurrentPlayerLayer];
                CurrentPlayerLayerState = playerLayerData.state;
                
                // 플레이어 레이어 변경 이벤트 발생
                OnLayerChangedForPlayer?.Invoke(CurrentPlayerLayer);
                
                Debug.Log($"Player Layer: {CurrentPlayerLayer}, State: {CurrentPlayerLayerState}, Name: {playerLayerData.layerName}");
            }
        }
    }
    
    // 플레이어 Y 위치를 기반으로 현재 레이어 계산
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
        
        return layerDataList.Count - 1; // 가장 아래 레이어
    }

    // 현재 층의 끝 높이를 계산하여 캐시 (층 변경 시에만 호출)
    private void UpdateCurrentLayerEndHeight()
    {
        currentLayerEndY = 0f;
        for (int i = 0; i <= CurrentLayer; i++)
        {
            currentLayerEndY += layerDataList[i].layerHeight;
        }
    }

    

    // 현재 층의 하드니스 계산 (타일맵 기준)
    public float GetCurrentHardness()
    {
        int layer = Mathf.Max(0, CurrentPlayerLayer);
        return CurrentLayerHardness = 40f + layer * Mathf.Sqrt(layer) * 20f;
    }
    
   
    
    // 현재 레이어 이름 반환 (타일맵 기준)
    public string GetCurrentLayerName()
    {
        if (CurrentLayer >= 0 && CurrentLayer < layerDataList.Count)
        {
            return layerDataList[CurrentLayer].layerName;
        }
        return "Unknown Layer";
    }


    // 타일맵 총 높이
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

    // 보스 층 진입 시 호출 (보스 타일맵 생성)
    public void HandleBossLayerEntered(int bossIndex)
    {
        Debug.Log($"보스 층 {bossIndex} 진입 - 보스 타일맵 생성");

        // 보스 타일맵 생성
        if (TileGenerator.Instance != null)
        {
            TileGenerator.Instance.SpawnBossTilemap(bossIndex);
        }
    }
    
    // 보스가 죽어서 보스 층이 완료될 때 호출
    public void HandleBossLayerCompleted(int bossIndex)
    {
        Debug.Log($"보스 층 {bossIndex} 완료 - 현재 높이 확정 및 다음 층으로 진행");
        
        // 현재 층의 끝 높이를 현재 카메라 위치로 확정
        Vector3 bottomCenterWorldPos = mainCam.ViewportToWorldPoint(new Vector3(0.5f, 0f, mainCam.nearClipPlane));
        float currentViewportY = bottomCenterWorldPos.y;
        
        // 현재 층의 높이를 현재 카메라 위치까지로 확정
        float currentLayerHeight = mainCamStartY - currentViewportY;
        if (CurrentLayer >= 0 && CurrentLayer < layerDataList.Count)
        {
            layerDataList[CurrentLayer].layerHeight = currentLayerHeight;
        }
        
        // 현재 층의 끝 높이 재계산
        UpdateCurrentLayerEndHeight();
        
        // 다음 층으로 진행
        if (CurrentLayer < layerDataList.Count - 1)
        {
            CurrentLayer++;
            LayerData newLayerData = layerDataList[CurrentLayer];
            CurrentLayerState = newLayerData.state;

            // 새로운 층의 끝 높이 계산
            UpdateCurrentLayerEndHeight();

            // 이벤트 발생
            OnLayerChangedForTilemapGeneration?.Invoke(CurrentLayer);
            OnLayerStateChanged?.Invoke(CurrentLayerState);

        }
    }
}
