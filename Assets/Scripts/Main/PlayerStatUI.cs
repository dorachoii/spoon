using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayerStatUI : MonoBehaviour
{
    public Slider powerSlider;
    public Slider HPSlider;

    private PlayerStat playerStat;

    // Start is called before the first frame update
    void Start()
    {
        playerStat = GameObject.FindGameObjectWithTag("Player").GetComponent<PlayerStat>();
        HPSlider.maxValue = playerStat.MaxHP;
        powerSlider.maxValue = playerStat.DigPower;

        UpdateHPUI(playerStat.CurrentHP);
        UpdateDigUI(playerStat.DigPower);
    }

    void OnEnable()
{
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
        powerSlider.value = digPower;
    }

    private void UpdateHPUI(float hp)
    {
        HPSlider.value = hp;
    }
}
