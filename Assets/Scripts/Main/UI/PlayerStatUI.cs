using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

public class PlayerStatUI : MonoBehaviour
{
    [Header("Player Stat UI")]
    public Slider powerSlider;
    public Slider hpSlider;
    public Slider heatSlider;
    public TextMeshProUGUI layerHardnessText;

    int currentLayerHardness;
    private PlayerStat playerStat;

    void Start()
    {
        currentLayerHardness = 0;

        if (LayerManager.Instance != null)
        {
            LayerManager.Instance.OnLayerChangedForPlayer += UpdateLayerHardnessText;
            LayerManager.Instance.OnLavaLayerEntered += OnLavaLayerEntered;
            LayerManager.Instance.OnLavaLayerExited += OnLavaLayerExited;
        }

        // 플레이어를 찾을 때까지 코루틴으로 대기
        StartCoroutine(FindPlayerCoroutine());
    }

    private IEnumerator FindPlayerCoroutine()
    {

        while (playerStat == null)
        {
            if (GameObject.FindGameObjectWithTag("Player") != null)
            {
                playerStat = GameObject.FindGameObjectWithTag("Player").GetComponent<PlayerStat>();
                InitializePlayerUI(playerStat);
                break;
            }
            
            yield return null;
        }
    }

    private void InitializePlayerUI(PlayerStat playerStat)
    {
        // heatSlider 할당
        if (playerStat.heatSlider != null)
        {
            heatSlider = playerStat.heatSlider.GetComponent<Slider>();
        }

        hpSlider.maxValue = playerStat.MaxHP;
        powerSlider.maxValue = playerStat.MaxPower;
        heatSlider.maxValue = playerStat.MaxHeat;

        UpdateHPValue(playerStat.CurrentHP);
        UpdatePowerValue(playerStat.CurrentPower);
        UpdateHeatValue(playerStat.CurrentHeat);

        playerStat.OnHPChanged += UpdateHPValue;
        playerStat.OnDigPowerChanged += UpdatePowerValue;
        playerStat.OnHeatChanged += UpdateHeatValue;

        ToggleHeatSlider(false);
    }


    void OnDestroy()
    {
        if (playerStat != null)
        {
            playerStat.OnHPChanged -= UpdateHPValue;
            playerStat.OnDigPowerChanged -= UpdatePowerValue;
            playerStat.OnHeatChanged -= UpdateHeatValue;
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

int idx = 0;
    private void UpdateLayerHardnessText(int hardness)
    {
        int newidx = LayerManager.Instance.GetCurrentLayerTileIndex();
        if(newidx == -1) return;

        idx = newidx;
            layerHardnessText.text = $"Hardness: {idx}";
        
    }

    public void ToggleHeatSlider(bool isOn)
    {
        if(heatSlider.gameObject != null)
        heatSlider.gameObject.SetActive(isOn);

    }

    private void OnLavaLayerEntered()
    {
        if (heatSlider != null)
        {
            ToggleHeatSlider(true);
        }
    }

    private void OnLavaLayerExited()

    {
        ToggleHeatSlider(false);
    }
}
