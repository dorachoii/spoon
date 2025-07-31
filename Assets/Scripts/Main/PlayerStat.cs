using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerStat : MonoBehaviour
{
    public static PlayerStat Instance { get; private set; }
    private float baseDigPower = 100f;
    private float digPowerBonus = 0f;

    private float jumpForce = 0.0005f;

    private float maxHP = 100f;
    private float currentHP;

    private float maxStamina = 100f;
    private float currentStamina;

    public event Action<float> OnDigPowerChanged;
    public event Action<float> OnHPChanged;
    public event Action OnDamaged;
    public event Action<float> OnStaminaChanged;
    public event Action OnDied;

    private bool isDead = false;

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
        currentStamina = maxStamina;
    }

    public float DigPower => baseDigPower + digPowerBonus;

    public float JumpForce => jumpForce;
    public float MaxHP => maxHP;
    public float CurrentHP => currentHP;

    public float MaxStamina => maxStamina;
    public float CurrentStamina => currentStamina;

    public float CalculateDigSpeed()
    {
        float hardness = LayerManager.Instance.GetCurrentHardness();
        return (DigPower / Mathf.Max(1f, hardness)) * 5f;
    }

    public float GetDigDelay()
    {
        return 1f / CalculateDigSpeed(); // 더 이상 10f 안 곱함
    }


    public void AddDigPowerBonus(float bonus)
    {
        digPowerBonus += bonus;
        OnDigPowerChanged?.Invoke(DigPower);

    }

    public void TakeDamage(float damage)
    {
        if (isDead) return;
        
        currentHP -= damage;
        if (currentHP < 0)
        {
            currentHP = 0;
            isDead = true;
            OnDied?.Invoke();
        }

        OnDamaged?.Invoke();
        OnHPChanged?.Invoke(currentHP);
    }

    public void ConsumeStamina(float amount)
    {
        currentStamina = Mathf.Max(0, currentStamina - amount);
        OnStaminaChanged?.Invoke(currentStamina);
    }
    
    public void RecoverStamina(float amount)
    {
        currentStamina = Mathf.Min(maxStamina, currentStamina + amount);
        OnStaminaChanged?.Invoke(currentStamina);
    }
}


