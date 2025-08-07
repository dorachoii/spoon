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
    public float startY;
    public float endY;
    public int bossIndex; // 보스 종류 구분 (-1이면 일반 층, 전환 층층)
    
    public LayerData(int index, LayerState layerState, float start, float end, int boss = -1)
    {
        layerIndex = index;
        state = layerState;
        startY = start;
        endY = end;
        bossIndex = boss;
    }
}

public class LayerManager : MonoBehaviour
{
    public static LayerManager Instance { get; private set; }
    private Camera mainCam;

    [Header("Layer Settings")]
    [SerializeField] private float layerHeight = 50f; // 한 층 당 높이 (1層あたりの高さ)
    [SerializeField] private float transitionLayerHeight = 20f; // 전환 층 높이 (遷移層の高さ)
    [SerializeField] private float bossLayerHeight = 100f; // 보스 층 높이 (ボス層の高さ)
    
    private int lastLayer = -1;
    public int CurrentLayer { get; private set; } = 0;
    public float CurrentLayerHardness { get; private set; } = 1f;
    public LayerState CurrentLayerState { get; private set; } = LayerState.Normal;
    
    // 층 데이터 관리
    private List<LayerData> layerDataList = new List<LayerData>();
    private LayerData currentLayerData;

    // 이벤트
    public event Action<int> OnLayerChanged;
    public event Action<LayerState> OnLayerStateChanged;
    public event Action<int> OnTransitionLayerEntered; // 전환 층 진입
    public event Action<int> OnTransitionLayerExited;  // 전환 층 퇴장
    public event Action<int> OnBossLayerEntered; // 보스 층 진입
    public event Action<int> OnBossLayerExited;  // 보스 층 퇴장

    [Header("Tilemap")]
    private Tilemap tilemap;
    private int maxTilesPerFrame = 40;  // 한 프레임에 처리할 최대 타일 수 (1Frameに処理する最大タイル数)

    // 보스 층 설정
    [Header("Boss Layer Configuration")]
    [SerializeField] private int[] bossLayerIndices = { 2, 5 }; // 보스가 등장할 일반 층 인덱스들

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
        UpdateLayer();
    }

    private void InitializeLayerData()
    {
        layerDataList.Clear();
        float currentY = 0f;
        
        for (int i = 0; i < 6; i++) 
        {
            bool isBossLayer = Array.Exists(bossLayerIndices, x => x == i);
            
            if (isBossLayer)
            {
                // 일반 층 추가
                LayerData normalLayer = new LayerData(i, LayerState.Normal, currentY, currentY + layerHeight);
                layerDataList.Add(normalLayer);
                currentY += layerHeight;
                
                // 전환 층 추가
                LayerData transitionLayer = new LayerData(i, LayerState.Transition, currentY, currentY + transitionLayerHeight, i);
                layerDataList.Add(transitionLayer);
                currentY += transitionLayerHeight;
                
                // 보스 층 추가
                LayerData bossLayer = new LayerData(i, LayerState.Boss, currentY, currentY + bossLayerHeight, i);
                layerDataList.Add(bossLayer);
                currentY += bossLayerHeight;
            }
            else
            {
                // 일반 층만 추가
                LayerData normalLayer = new LayerData(i, LayerState.Normal, currentY, currentY + layerHeight);
                layerDataList.Add(normalLayer);
                currentY += layerHeight;
            }
        }
    }

    private void UpdateLayer()
    {
        int newLayer = CalculateCurrentLayer();
        LayerData newLayerData = GetLayerDataAtPosition(GetCameraBottomY());
        
        bool layerChanged = false;
        bool stateChanged = false;

        // 레이어 변경 체크
        
        if (newLayer != lastLayer)
        {
            CurrentLayer = newLayer;
            lastLayer = newLayer;
            OnLayerChanged?.Invoke(CurrentLayer);
            layerChanged = true;
        }

        // 레이어 상태 변경 체크
        if (newLayerData != null && (currentLayerData == null || newLayerData.layerIndex != currentLayerData.layerIndex))
        {
            LayerState previousState = CurrentLayerState;
            CurrentLayerState = newLayerData.state;
            currentLayerData = newLayerData;
            
            if (previousState != CurrentLayerState)
            {
                OnLayerStateChanged?.Invoke(CurrentLayerState);
                stateChanged = true;
                
                                 // 전환 층 진입/퇴장 이벤트
                 if (CurrentLayerState == LayerState.Transition)
                 {
                    Debug.Log($"전환 층 진입: {currentLayerData.bossIndex}");
                     OnTransitionLayerEntered?.Invoke(currentLayerData.bossIndex);
                     HandleTransitionLayerEntered(currentLayerData.bossIndex);
                 }
                 else if (previousState == LayerState.Transition)
                 {
                     OnTransitionLayerExited?.Invoke(currentLayerData.bossIndex);
                 }
                 
                 // 보스 층 진입/퇴장 이벤트
                 if (CurrentLayerState == LayerState.Boss)
                 {
                     OnBossLayerEntered?.Invoke(currentLayerData.bossIndex);
                     HandleBossLayerEntered(currentLayerData.bossIndex);
                 }
                 else if (previousState == LayerState.Boss)
                 {
                     OnBossLayerExited?.Invoke(currentLayerData.bossIndex);
                 }
            }
        }

        if (layerChanged || stateChanged)
        {
            Debug.Log($"Layer: {CurrentLayer}, State: {CurrentLayerState}");
        }
    }

    private int CalculateCurrentLayer()
    {
        if (tilemap == null) return Mathf.Max(0, lastLayer);
        if (mainCam == null) return Mathf.Max(0, lastLayer);

        Vector3Int bottomLeftCell = tilemap.WorldToCell(mainCam.ViewportToWorldPoint(new Vector3(0, 0, mainCam.nearClipPlane)));
        Vector3 cellWorldPos = tilemap.CellToWorld(bottomLeftCell);

        float originY = 0f;
        int layer = Mathf.FloorToInt((originY - cellWorldPos.y) / layerHeight);
        return Mathf.Max(0, layer);
    }

    private LayerData GetLayerDataAtPosition(float yPosition)
    {
        foreach (var layerData in layerDataList)
        {
            if (yPosition >= layerData.startY && yPosition <= layerData.endY)
            {
                return layerData;
            }
        }
        return null;
    }

    // 보스가 죽었을 때 호출 (BossManager에서 호출)
    public void OnBossDefeated(int bossIndex)
    {
        Debug.Log($"보스 {bossIndex} 처치됨!");
        
        // 보스 층을 일반 층으로 변경
        var bossLayer = layerDataList.Find(l => l.bossIndex == bossIndex && l.state == LayerState.Boss);
        if (bossLayer != null)
        {
            bossLayer.state = LayerState.Normal;
            bossLayer.bossIndex = -1;
        }
        
        // 타일 생성 재개
        if (TileGenerator.Instance != null)
        {
            TileGenerator.Instance.ResumeTileGeneration();
        }
    }

    // 전환 층 진입 시 호출 (타일 생성 중단)
    public void HandleTransitionLayerEntered(int bossIndex)
    {
        Debug.Log($"전환 층 {bossIndex} 진입 - 타일 생성 중단");
        
        // 타일 생성 중단
        if (TileGenerator.Instance != null)
        {
            TileGenerator.Instance.PauseTileGeneration();
        }
        
        // 전환 효과 (예: 화면 페이드, 사운드 등)
        // if (TransitionManager.Instance != null)
        // {
        //     TransitionManager.Instance.PlayTransitionEffect(bossIndex);
        // }
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
        
        // 보스 생성 (BossManager가 있다면)
        // if (BossManager.Instance != null)
        // {
        //     BossManager.Instance.SpawnBoss(bossIndex);
        // }
    }

    // 현재 층이 보스 층인지 확인
    public bool IsCurrentLayerBoss()
    {
        return CurrentLayerState == LayerState.Boss;
    }

    // 현재 층의 하드니스 계산
    public float GetCurrentHardness()
    {
        int layer = Mathf.Max(0, CurrentLayer);
        return CurrentLayerHardness = 40f + layer * Mathf.Sqrt(layer) * 20f;
    }

    // 타일맵 총 높이
    public float GetTilemapTotalHeight()
    {
        return layerHeight * 5f + bossLayerHeight * bossLayerIndices.Length + 10f;
    }

    public int GetMaxTile()
    {
        return maxTilesPerFrame;
    }

    private float GetCameraBottomY()
    {
        Vector3 bottomCenterWorldPos = mainCam.ViewportToWorldPoint(new Vector3(0.5f, 0f, mainCam.nearClipPlane));
        return bottomCenterWorldPos.y;
    }
    
    // 보스 층 데이터 가져오기
    public LayerData GetBossLayerData(int bossIndex)
    {
        return layerDataList.Find(l => l.bossIndex == bossIndex && l.state == LayerState.Boss);
    }
}
