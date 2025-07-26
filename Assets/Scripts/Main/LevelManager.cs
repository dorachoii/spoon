using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LevelManager : MonoBehaviour
{
    public static LevelManager Instance { get; private set; }

    public int CurrentLevel { get; private set; } = 0;
    private float levelHeight = 20f;
    private Camera mainCam;

    public Action<int> OnLevelChanged;
    private int lastLevel = 0;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    // Start is called before the first frame update
    void Start()
    {
        mainCam = Camera.main;
    }

    // Update is called once per frame
    void Update()
    {
        int newLevel = GetCameraLevel();

        if (newLevel != lastLevel)
        {
            Debug.Log($"current Level : {CurrentLevel}");
            CurrentLevel = newLevel;
            lastLevel = newLevel;

            OnLevelChanged?.Invoke(CurrentLevel);
        }
    }

    int GetCameraLevel()
    {
        float originY = 0f;
        float camY = mainCam.transform.position.y;
        return Mathf.Max(0, Mathf.FloorToInt( originY - camY / levelHeight));
    }
}
