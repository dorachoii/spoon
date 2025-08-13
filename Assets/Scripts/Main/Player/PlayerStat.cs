using System;
using System.Collections;
using UnityEngine;

public class PlayerStat : MonoBehaviour
{
    public static PlayerStat Instance { get; private set; }

    public GameObject heatSlider;

    // Player Move
    private float speed = 0.1f;
    private float jumpForce = 0.0005f;

    // Player HP
    private float maxHP = 200f;
    private float currentHP;

    // Player Power
    private float maxPower = 300f;
    //private float basePower = 100f;
    private float basePower = 150f;
    private float powerBonus = 0f;

    // Player Heat
    private float maxHeat = 100f;
    private float currentHeat;
    

    // Encapsulation
    public float Speed => speed;
    public float JumpForce => jumpForce;
    public float MaxHP => maxHP;
    public float MaxPower => maxPower;   
    public float MaxHeat => maxHeat;
    public float CurrentHP => currentHP;
    public float CurrentPower => basePower + powerBonus;
    public float CurrentHeat => currentHeat;

    // Events
    // HP Events
    public event Action<float> OnHPChanged;
    public event Action OnDamaged;
    public event Action OnDied;

    // Dig Power Events
    public event Action<float> OnDigPowerChanged;
    public event Action<float> OnPowerUp;

    // Heat Events
    public event Action<float> OnHeatChanged;

    // Invincible Events
    public event Action OnInvincibleStarted;
    public event Action OnInvincibleEnded;
    private Coroutine activeRecoverInvincible;

    // Poison Events
    public event Action OnPoisonedStarted;
    public event Action OnPoisonedEnded;

   

    private bool isDead = false;
    private bool isInvincible = false;
    public bool isPoisoned = false;
    private bool isInLavaZone = false; // Lava Zone 체크용


    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
       
        currentHP = maxHP;
        currentHeat = 0f;
        isDead = false;
        isInvincible = false;
        isPoisoned = false;
        isInLavaZone = false;
    }

    private void Start()
    {
        // LayerManager 이벤트 구독
        if (LayerManager.Instance != null)
        {
            LayerManager.Instance.OnLavaLayerEntered += HandleLavaLayerEntered;
            LayerManager.Instance.OnLavaLayerExited += HandleLavaLayerExited;
        }
    }

    private void Update()
    {
        // Lava Zone에 있을 때 지속적으로 열 증가
        if (isInLavaZone)
        {
            ChangeHeat(Time.deltaTime * 5f); 
        }
    }

    private void OnDestroy()
    {
        // 이벤트 구독 해제
        if (LayerManager.Instance != null)
        {
            LayerManager.Instance.OnLavaLayerEntered -= HandleLavaLayerEntered;
            LayerManager.Instance.OnLavaLayerExited -= HandleLavaLayerExited;
        }
    }

    private void HandleLavaLayerEntered()
    {
        isInLavaZone = true;
    }

    private void HandleLavaLayerExited()
    {
        isInLavaZone = false;
    }   

    #region HP
    public bool DamageHP(float damage)
    {
        if (isDead || isInvincible) return false;

        ChangeHP(-damage);
        OnDamaged?.Invoke();
        return true;
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
    #endregion

    #region Dig Power
    public float CalculateDigSpeed()
    {
        float hardness = LayerManager.Instance.GetCurrentHardness();
        return CurrentPower / Mathf.Max(1f, hardness) * 5f;
    }

    public float GetDigDelay()
    {
        return 1f / CalculateDigSpeed();
    }
    
    public void AddDigPowerBonus(float bonus)
    {
        powerBonus += bonus;
        OnDigPowerChanged?.Invoke(CurrentPower);
        OnPowerUp?.Invoke(bonus);
    }

    // 게임 로드 시 최소 파워 보장
    public void EnsureMinimumPower()
    {
        float hardness = LayerManager.Instance.GetCurrentHardness();
        float minRequiredPower = hardness * 2f;
        
        if (CurrentPower < minRequiredPower)
        {
            float neededBonus = minRequiredPower - CurrentPower;
            AddDigPowerBonus(neededBonus);
            
        }
    }
    #endregion

    #region Invincible Effect

    public void StartInvincible(float duration)
    {
        if(!isInvincible)
        {
            StartCoroutine(IInvincible(duration));
        }
    }

    public IEnumerator IInvincible(float duration)
    {
        if (activeRecoverInvincible != null)
        {
            StopCoroutine(activeRecoverInvincible);
        }

        activeRecoverInvincible = StartCoroutine(IRecoverHPAndInvincible(duration));
        yield return activeRecoverInvincible;
        activeRecoverInvincible = null;
    }

    private IEnumerator IRecoverHPAndInvincible(float duration)
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
        yield return new WaitForSeconds(duration);

        // 무적 종료
        isInvincible = false;
        OnInvincibleEnded?.Invoke();
    }
    #endregion

    #region Poison Effect
    public void StartPoisonEffect(float duration)
    {
        if (!isPoisoned)
        {
            StartCoroutine(IPoison(duration));
        }
    }

    public IEnumerator IPoison(float duration)
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
    #endregion

    #region Heat
    public void ChangeHeat(float amount)
    {
        currentHeat = Mathf.Clamp(currentHeat + amount, 0, maxHeat);
        OnHeatChanged?.Invoke(currentHeat);

        if (currentHeat >= maxHeat)
        {
            isDead = true;
            OnDied?.Invoke();
        }
    }

    public void CureHeat(float amount)
    {
        currentHeat = Mathf.Clamp(currentHeat - amount, 0, maxHeat);
        OnHeatChanged?.Invoke(currentHeat);
    }
    #endregion

}


