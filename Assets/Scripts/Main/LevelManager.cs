using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LevelManager : MonoBehaviour
{
    public static LevelManager Instance { get; private set; }

    public int CurrentLevel { get; private set; } = 0;
    public float levelHeight = 40f;
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
            CurrentLevel = newLevel;
            lastLevel = newLevel;

            OnLevelChanged?.Invoke(CurrentLevel);
        }
    }

    int GetCameraLevel()
    {
        float camY = mainCam.transform.position.y;
        return Mathf.FloorToInt(-camY / levelHeight);
    }
}
