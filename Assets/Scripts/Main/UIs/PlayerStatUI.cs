using UnityEngine;
using UnityEngine.UI;

public class PlayerStatUI : MonoBehaviour
{
    [Header("Player Stat UI")]
    public Slider powerSlider;
    public Slider hpSlider;

    // OnEnable:  PlayerStat.Instanceがまだnullの可能性がある
    // Start: 安全な初期化    
    void Start()
    {
        hpSlider.maxValue = PlayerStat.Instance.MaxHP;
        powerSlider.maxValue = PlayerStat.Instance.MaxPower;

        UpdateHPValue(PlayerStat.Instance.CurrentHP);
        UpdatePowerValue(PlayerStat.Instance.CurrentPower);

        PlayerStat.Instance.OnHPChanged += UpdateHPValue;
        PlayerStat.Instance.OnDigPowerChanged += UpdatePowerValue;
    }

    void OnDestroy()
    {
        PlayerStat.Instance.OnHPChanged -= UpdateHPValue;
        PlayerStat.Instance.OnDigPowerChanged -= UpdatePowerValue;
    }

    private void UpdatePowerValue(float digPower)
    {
        powerSlider.value = digPower;
    }

    private void UpdateHPValue(float hp)
    {
        hpSlider.value = hp;
    }
}
