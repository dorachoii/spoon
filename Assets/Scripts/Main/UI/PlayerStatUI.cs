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
        
        // PlayerStat.Instance가 null인지 체크
        if (PlayerStat.Instance != null)
        {
            hpSlider.maxValue = PlayerStat.Instance.MaxHP;
            powerSlider.maxValue = PlayerStat.Instance.MaxPower;
            heatSlider.maxValue = PlayerStat.Instance.MaxHeat;

            UpdateHPValue(PlayerStat.Instance.CurrentHP);
            UpdatePowerValue(PlayerStat.Instance.CurrentPower);
            UpdateHeatValue(PlayerStat.Instance.CurrentHeat);

            PlayerStat.Instance.OnHPChanged += UpdateHPValue;
            PlayerStat.Instance.OnDigPowerChanged += UpdatePowerValue;
            PlayerStat.Instance.OnHeatChanged += UpdateHeatValue;
        }
        else
        {
            Debug.LogWarning("[PlayerStatUI] PlayerStat.Instance is null - UI may not work properly");
        }
        
        // LayerManager.Instance가 null인지 체크
        if (LayerManager.Instance != null)
        {
            LayerManager.Instance.OnLayerChangedForPlayer += UpdateLayerHardnessText;
        }
        else
        {
            Debug.LogWarning("[PlayerStatUI] LayerManager.Instance is null - layer updates may not work");
        }
        
        ToggleHeatSlider(false);
    }

    void OnDestroy()
    {
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
        heatSlider.gameObject.SetActive(isOn);
    }
}
