using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class PlayerStatUI : MonoBehaviour
{
    [Header("Player Stat UI")]
    public Slider powerSlider;
    public Slider hpSlider;
    public TextMeshProUGUI layerHardnessText;

    int currentLayerHardness;

    // OnEnable:  PlayerStat.Instanceがまだnullの可能性がある
    // Start: 安全な初期化    
    void Start()
    {
        currentLayerHardness = 0;
        hpSlider.maxValue = PlayerStat.Instance.MaxHP;
        powerSlider.maxValue = PlayerStat.Instance.MaxPower;

        UpdateHPValue(PlayerStat.Instance.CurrentHP);
        UpdatePowerValue(PlayerStat.Instance.CurrentPower);

        PlayerStat.Instance.OnHPChanged += UpdateHPValue;
        PlayerStat.Instance.OnDigPowerChanged += UpdatePowerValue;
        LayerManager.Instance.OnLayerChangedForPlayer += UpdateLayerHardnessText;
    }

    void OnDestroy()
    {
        PlayerStat.Instance.OnHPChanged -= UpdateHPValue;
        PlayerStat.Instance.OnDigPowerChanged -= UpdatePowerValue;
        LayerManager.Instance.OnLayerChangedForPlayer -= UpdateLayerHardnessText;
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
        if(LayerManager.Instance.CurrentLayerState == LayerState.Normal)
        {
            currentLayerHardness++;
            layerHardnessText.text = $"Hardness: {currentLayerHardness}";
        }
    }
}
