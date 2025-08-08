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

    public LayerData(int index, LayerState layerState, float height, int boss = -1)
    {
        layerIndex = index;
        state = layerState;
        layerHeight = height;
        bossIndex = boss;
    }
}
//보스 전환!
public class LayerManager : MonoBehaviour
{
    public static LayerManager Instance { get; private set; }
    private Camera mainCam;
    private float startY = 0f;


    private int lastLayer = -1;
    public int CurrentLayer { get; private set; } = 0;
    public float CurrentLayerHardness { get; private set; } = 1f;
    public LayerState CurrentLayerState { get; private set; } = LayerState.Normal;

    // 층 데이터 관리
    private List<LayerData> layerDataList = new List<LayerData>();
    private float currentLayerEndHeight = 0f; // 현재 층의 끝 높이 (캐시)

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
        startY = mainCam.transform.position.y;
        layerDataList.Clear();

        // 0층: Normal, 높이 50
        LayerData layer1 = new LayerData(0, LayerState.Normal, 20f, -1);
        // 1층: Transition, 높이 20  tilemap offset만큼
        LayerData layer2 = new LayerData(1, LayerState.Transition, 12f, 0);
        // 2층: Boss, 높이 100
        LayerData layer3 = new LayerData(2, LayerState.Boss, 100f, 0);

        layerDataList.Add(layer1);
        layerDataList.Add(layer2);
        layerDataList.Add(layer3);
        
        // 초기 층의 끝 높이 계산
        UpdateCurrentLayerEndHeight();
    }

    private void UpdateLayer()
    {
        Vector3 bottomCenterWorldPos = mainCam.ViewportToWorldPoint(new Vector3(0.5f, 0f, mainCam.nearClipPlane));
        float viewportY = bottomCenterWorldPos.y;

        // 카메라가 현재 층을 넘어갔는지 체크 (캐시된 값 사용)
        if (viewportY <= startY - currentLayerEndHeight)
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
                OnLayerChanged?.Invoke(CurrentLayer);
                OnLayerStateChanged?.Invoke(CurrentLayerState);

                // 전환 층 진입/퇴장 이벤트
                if (CurrentLayerState == LayerState.Transition)
                {
                    Debug.Log($"전환 층 진입: {newLayerData.bossIndex}");
                    OnTransitionLayerEntered?.Invoke(newLayerData.bossIndex);
                    HandleTransitionLayerEntered(newLayerData.bossIndex);
                }

                // 보스 층 진입/퇴장 이벤트
                if (CurrentLayerState == LayerState.Boss)
                {
                    OnBossLayerEntered?.Invoke(newLayerData.bossIndex);
                    HandleBossLayerEntered(newLayerData.bossIndex);
                }

                Debug.Log($"Layer: {CurrentLayer}, State: {CurrentLayerState}");
            }
        }
    }

    // 현재 층의 끝 높이를 계산하여 캐시 (층 변경 시에만 호출)
    private void UpdateCurrentLayerEndHeight()
    {
        currentLayerEndHeight = 0f;
        for (int i = 0; i <= CurrentLayer; i++)
        {
            currentLayerEndHeight += layerDataList[i].layerHeight;
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

}
