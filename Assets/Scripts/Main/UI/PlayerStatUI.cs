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
        hpSlider.maxValue = PlayerStat.Instance.MaxHP;
        powerSlider.maxValue = PlayerStat.Instance.MaxPower;
        heatSlider.maxValue = PlayerStat.Instance.MaxHeat;

        UpdateHPValue(PlayerStat.Instance.CurrentHP);
        UpdatePowerValue(PlayerStat.Instance.CurrentPower);
        UpdateHeatValue(PlayerStat.Instance.CurrentHeat);

        PlayerStat.Instance.OnHPChanged += UpdateHPValue;
        PlayerStat.Instance.OnDigPowerChanged += UpdatePowerValue;
        PlayerStat.Instance.OnHeatChanged += UpdateHeatValue;
        LayerManager.Instance.OnLayerChangedForPlayer += UpdateLayerHardnessText;
        ToggleHeatSlider(false);
    }

    void OnDestroy()
    {
        PlayerStat.Instance.OnHPChanged -= UpdateHPValue;
        PlayerStat.Instance.OnDigPowerChanged -= UpdatePowerValue;
        PlayerStat.Instance.OnHeatChanged -= UpdateHeatValue;
        LayerManager.Instance.OnLayerChangedForPlayer -= UpdateLayerHardnessText;
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
        if(LayerManager.Instance.CurrentLayerState == LayerState.Normal)
        {
            currentLayerHardness++;
            layerHardnessText.text = $"Hardness: {currentLayerHardness}";
        }
    }

    public void ToggleHeatSlider(bool isOn){
        heatSlider.gameObject.SetActive(isOn);
    }
}
