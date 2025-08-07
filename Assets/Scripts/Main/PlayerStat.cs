using System;
using System.Collections;
using UnityEngine;

public class PlayerStat : MonoBehaviour, ISaveable
{
    public static PlayerStat Instance { get; private set; }
    private float maxPower = 300f;
    private float basePower = 100f;
    private float powerBonus = 0f;
    private float currentPower;

    private float jumpForce = 0.0005f;

    private float maxHP = 100f;
    private float currentHP;


    public event Action<float> OnDigPowerChanged;
    public event Action<float> OnHPChanged;
    public event Action OnDamaged;
    public event Action<float> OnStaminaChanged;
    public event Action OnDied;
    public event Action OnInvincibleStarted;
    public event Action OnInvincibleEnded;
    public event Action OnPoisonedStarted;
    public event Action OnPoisonedEnded;
    public event Action<float> OnPowerUp;

    private bool isDead = false;
    private bool isInvincible = false;
    public bool isPoisoned = false;

    public void WriteData(GameData data)
    {
        data.playerPosition = gameObject.transform.position;
    }
    public void ReadData(GameData data)
    {
        gameObject.transform.position = data.playerPosition;
    }

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
       
        currentHP = maxHP;
        //currentHP = 10f;
        currentPower = basePower;
    }

    public float CurrentPower => basePower + powerBonus;

    public float JumpForce => jumpForce;
    public float MaxHP => maxHP;
    public float MaxPower => maxPower;   
    public float CurrentHP => currentHP;

    public float CalculateDigSpeed()
    {
        float hardness = LayerManager.Instance.GetCurrentHardness();
        return (CurrentPower / Mathf.Max(1f, hardness)) * 5f;
    }

    public float GetDigDelay()
    {
        return 1f / CalculateDigSpeed(); // 더 이상 10f 안 곱함
    }


    public void AddDigPowerBonus(float bonus)
    {
        powerBonus += bonus;
        OnDigPowerChanged?.Invoke(CurrentPower);
        OnPowerUp?.Invoke(bonus);

    }

    public void DamageHP(float damage)
    {
        if (isDead || isInvincible) return;

        ChangeHP(-damage);
        OnDamaged?.Invoke();
    }

    public void HealHP(float amount)
    {
        if (isDead) return;
        ChangeHP(amount);
    }

    private void ChangeHP(float amount)
    {
        currentHP = Mathf.Clamp(currentHP + amount, 0, maxHP);
        OnHPChanged?.Invoke(currentHP);

        if (currentHP <= 0 && !isDead)
        {
            isDead = true;
            OnDied?.Invoke();
        }
    }

    private Coroutine activeRecoverInvincible;

    public IEnumerator RecoverHPAndInvincible(float invincibleDuration)
    {
        if (activeRecoverInvincible != null)
        {
            StopCoroutine(activeRecoverInvincible);
        }

        activeRecoverInvincible = StartCoroutine(RecoverHPAndInvincibleImpl(invincibleDuration));
        yield return activeRecoverInvincible;
        activeRecoverInvincible = null;
    }

    private IEnumerator RecoverHPAndInvincibleImpl(float invincibleDuration)
    {
        float startHP = currentHP;
        float elapsed = 0f;

        bool wasAlreadyInvincible = isInvincible;
        isInvincible = true;

        if (!wasAlreadyInvincible) OnInvincibleStarted?.Invoke();


        // HP 회복 (1초)
        if (currentHP < maxHP)
        {
            while (elapsed < 1f)
            {
                elapsed += Time.deltaTime;
                currentHP = Mathf.Lerp(startHP, maxHP, elapsed / 1f);
                OnHPChanged?.Invoke(currentHP);
                yield return null;
            }
            currentHP = maxHP;
            OnHPChanged?.Invoke(currentHP);
        }

        // 무적 시작
        yield return new WaitForSeconds(invincibleDuration);

        // 무적 종료
        isInvincible = false;
        OnInvincibleEnded?.Invoke();
    }

    public void StartPoisonEffect(float duration)
    {
        if (!isPoisoned)
        {
            StartCoroutine(ApplyPoisionEffect(duration));
        }
    }


    public IEnumerator ApplyPoisionEffect(float duration)
    {
        Debug.Log("poison start  playerStat");
        if (isPoisoned) yield break;

        isPoisoned = true;
        OnPoisonedStarted?.Invoke();
        yield return new WaitForSeconds(duration);

        if (isPoisoned)
        {
            isPoisoned = false;
            OnPoisonedEnded?.Invoke();
        }
        
    }

    public void CurePoision()
    {
        if (!isPoisoned) return;
        isPoisoned = false;
        OnPoisonedEnded?.Invoke();
    }

}


