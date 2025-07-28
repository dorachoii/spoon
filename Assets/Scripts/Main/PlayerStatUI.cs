using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayerStatUI : MonoBehaviour
{
    public Slider powerSlider;
    public Slider HPSlider;


    // Start is called before the first frame update
    void Start()
    {
        HPSlider.maxValue = PlayerStat.Instance.MaxHP;
        powerSlider.maxValue = 300f;

        UpdateHPUI(PlayerStat.Instance.CurrentHP);
        UpdateDigUI(PlayerStat.Instance.DigPower);

        PlayerStat.Instance.OnHPChanged += UpdateHPUI;
        PlayerStat.Instance.OnDigPowerChanged += UpdateDigUI;
    }


    void OnDisable()
    {
        PlayerStat.Instance.OnHPChanged -= UpdateHPUI;
        PlayerStat.Instance.OnDigPowerChanged -= UpdateDigUI;
    }

    private void UpdateDigUI(float digPower)
    {
        Debug.Log($"Updating dig power UI: {digPower}");
        powerSlider.value = digPower;
    }

    private void UpdateHPUI(float hp)
    {
        HPSlider.value = hp;
    }
}
