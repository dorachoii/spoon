using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerStat : MonoBehaviour
{
    public static PlayerStat Instance { get; private set; }
    private float baseDigPower = 100f;
    private float jumpForce = 0.0005f;

    private float maxHP = 100f;
    private float currentHP;

    public event Action<float> OnDigPowerChanged;
    public event Action<float> OnHPChanged;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
        currentHP = maxHP;
    }

    public float DigPower
    {
        get
        {
            float hardness = LayerManager.Instance.GetCurrentHardness();
            return baseDigPower / Mathf.Max(1f, hardness);
        }
    }

    public float JumpForce => jumpForce;
    public float MaxHP => maxHP;
    public float CurrentHP => currentHP;

}
