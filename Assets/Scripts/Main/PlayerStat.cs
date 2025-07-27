using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerStat : MonoBehaviour
{
    public static PlayerStat Instance { get; private set; }
    private float baseDigPower = 100f;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    public float DigPower
    {
        get
        {
            float hardness = LevelManager.Instance.GetCurrentHardness();
            return baseDigPower / Mathf.Max(1f, hardness);
        }
    }
}
