using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class LayerManager : MonoBehaviour
{
    public static LayerManager Instance { get; private set; }

    public int CurrentLayer { get; private set; } = 0;
    public float CurrentLayerHardness { get; private set; } = 1f;
    public float layerHeight = 20f;

    private Camera mainCam;

    public Action<int> OnLayerChanged;
    private int lastLayer = 0;

    private int maxTilesPerFrame = 40;

    private Tilemap tilemap;
    private GameObject player;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        tilemap = FindObjectOfType<Tilemap>();
    }

    // Start is called before the first frame update
    void Start()
    {
        mainCam = Camera.main;
    }

    // Update is called once per frame
    void Update()
    {
        int newLayer = GetPlayerGroundLevel();

        if (newLayer != lastLayer)
        {
            CurrentLayer = newLayer;
            lastLayer = newLayer;

            OnLayerChanged?.Invoke(CurrentLayer);
        }
    }

    public float GetLevelStartY(int layer)
    {
        return -layer * layerHeight;
    }

    public float GetLevelEndY(int layer)
    {
        return -(layer + 1) * layerHeight;
    }


    int GetPlayerGroundLevel()
    {
        float originY = 0f;
        Vector3 centerWorldPos = mainCam.ViewportToWorldPoint(new Vector3(0.5f, 0.5f, mainCam.nearClipPlane));
        Vector3Int centerCell = FindObjectOfType<Tilemap>().WorldToCell(centerWorldPos);

        int level = Mathf.FloorToInt((originY - tilemap.CellToWorld(centerCell).y) / layerHeight);
        return Mathf.Max(0, level);
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
        //return CurrentLevelHardness = 1f + Mathf.Pow(CurrentLevel, 2.2f) * 21.6f;
        return CurrentLayerHardness = 1f + Mathf.Pow(CurrentLayer, 2.2f) * 1f;
    }


}
