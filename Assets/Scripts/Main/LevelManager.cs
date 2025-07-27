using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class LevelManager : MonoBehaviour
{
    public static LevelManager Instance { get; private set; }

    public int CurrentLevel { get; private set; } = 0;
    public float CurrentLevelHardness { get; private set; } = 1f;
    public float levelHeight = 20f;

    private Camera mainCam;

    public Action<int> OnLevelChanged;
    private int lastLevel = 0;

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
        int newLevel = GetPlayerGroundLevel();

        if (newLevel != lastLevel)
        {
            CurrentLevel = newLevel;
            lastLevel = newLevel;

            OnLevelChanged?.Invoke(CurrentLevel);
        }
    }

    public float GetLevelStartY(int level)
    {
        return -level * levelHeight;
    }

    public float GetLevelEndY(int level)
    {
        return -(level + 1) * levelHeight;
    }


    int GetPlayerGroundLevel()
    {
        float originY = 0f;
        Vector3 centerWorldPos = mainCam.ViewportToWorldPoint(new Vector3(0.5f, 0.5f, mainCam.nearClipPlane));
        Vector3Int centerCell = FindObjectOfType<Tilemap>().WorldToCell(centerWorldPos);

        int level = Mathf.FloorToInt((originY - tilemap.CellToWorld(centerCell).y) / levelHeight);
        return Mathf.Max(0, level);
    }

    public float GetTilemapTotalHeight()
    {
        return levelHeight * 5f + 10f;
    }

    public int GetMaxTile()
    {
        return maxTilesPerFrame;
    }

    public float GetCurrentHardness()
    {
        //return CurrentLevelHardness = 1f + Mathf.Pow(CurrentLevel, 2.2f) * 21.6f;
        return CurrentLevelHardness = 1f + Mathf.Pow(CurrentLevel, 2.2f) * 1f;
    }


}
