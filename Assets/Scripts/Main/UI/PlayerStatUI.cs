using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class PlayerStatUI : MonoBehaviour
{
    [Header("Player Stat UI")]
    public Slider powerSlider;
    public Slider hpSlider;
    public Slider heatSlider;
    public TextMeshProUGUI layerHardnessText;

    int currentLayerHardness;

    // OnEnable:  PlayerStat.Instanceがまだnullの可能性がある
    // Start: 安全な初期化    
    void Start()
    {
        currentLayerHardness = 0;
        
        // 플레이어 준비 이벤트 구독
        GameManager.OnPlayerReady += OnPlayerReady;
        
       
    }
    
    private void OnPlayerReady()
    {
        Debug.Log("[PlayerStatUI] OnPlayerReady");
        // 플레이어가 준비된 후에 UI 초기화
        if (PlayerStat.Instance != null)
        {
            // heatSlider 할당
            if (PlayerStat.Instance.heatSlider != null)
            {
                Debug.Log("[PlayerStatUI] heatSlider found");
                heatSlider = PlayerStat.Instance.heatSlider.GetComponent<Slider>();
            }
            
            hpSlider.maxValue = PlayerStat.Instance.MaxHP;
            powerSlider.maxValue = PlayerStat.Instance.MaxPower;
            heatSlider.maxValue = PlayerStat.Instance.MaxHeat;

            UpdateHPValue(PlayerStat.Instance.CurrentHP);
            UpdatePowerValue(PlayerStat.Instance.CurrentPower);
            UpdateHeatValue(PlayerStat.Instance.CurrentHeat);

            PlayerStat.Instance.OnHPChanged += UpdateHPValue;
            PlayerStat.Instance.OnDigPowerChanged += UpdatePowerValue;
            PlayerStat.Instance.OnHeatChanged += UpdateHeatValue;

            ToggleHeatSlider(false);
        }
        else
        {
            Debug.LogWarning("[PlayerStatUI] PlayerStat.Instance is null - UI may not work properly");
        }
        
        // LayerManager 이벤트도 플레이어 준비 후에 구독
        if (LayerManager.Instance != null)
        {
            LayerManager.Instance.OnLayerChangedForPlayer += UpdateLayerHardnessText;
            LayerManager.Instance.OnLavaLayerEntered += OnLavaLayerEntered;
            LayerManager.Instance.OnLavaLayerExited += OnLavaLayerExited;
        }
        else
        {
            Debug.LogWarning("[PlayerStatUI] LayerManager.Instance is null - layer updates may not work");
        }

        
    }

    void OnDestroy()
    {
        // 플레이어 준비 이벤트 구독 해제
        GameManager.OnPlayerReady -= OnPlayerReady;
        
        // PlayerStat.Instance가 null인지 체크
        if (PlayerStat.Instance != null)
        {
            PlayerStat.Instance.OnHPChanged -= UpdateHPValue;
            PlayerStat.Instance.OnDigPowerChanged -= UpdatePowerValue;
            PlayerStat.Instance.OnHeatChanged -= UpdateHeatValue;
        }
        
        // LayerManager.Instance가 null인지 체크
        if (LayerManager.Instance != null)
        {
            LayerManager.Instance.OnLayerChangedForPlayer -= UpdateLayerHardnessText;
            LayerManager.Instance.OnLavaLayerEntered -= OnLavaLayerEntered;
            LayerManager.Instance.OnLavaLayerExited -= OnLavaLayerExited;
        }
    }

    private void UpdateHeatValue(float heat)
    {
        heatSlider.value = heat;
    }

    private void UpdatePowerValue(float digPower)
    {
        powerSlider.value = digPower;
    }

    private void UpdateHPValue(float hp)
    {
        hpSlider.value = hp;
    }

    private void UpdateLayerHardnessText(int hardness)
    {
        if(LayerManager.Instance != null && LayerManager.Instance.CurrentLayerState == LayerState.Normal)
        {
            currentLayerHardness++;
            if (layerHardnessText != null)
            {
                layerHardnessText.text = $"Hardness: {currentLayerHardness}";
            }
        }
    }

    public void ToggleHeatSlider(bool isOn){
        if (heatSlider != null)
        {
            heatSlider.gameObject.SetActive(isOn);
        }
        else
        {
            Debug.LogWarning("[PlayerStatUI] heatSlider is null - cannot toggle heat slider");
        }
    }
    
    private void OnLavaLayerEntered()
    {
        ToggleHeatSlider(true);
    }
    
    private void OnLavaLayerExited()
    
    {
        ToggleHeatSlider(false);
    }
}
