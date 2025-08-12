using UnityEngine;

public class PowerItem : ItemBase
{
    private float powerBonus = 50f; 

    protected override void ApplyEffect(GameObject player)
    {
        PlayerStat playerStat = player.GetComponent<PlayerStat>();
        PlayerContoller playerController = player.GetComponent<PlayerContoller>();
        
        if (playerStat != null)
        {
            playerStat.AddDigPowerBonus(powerBonus);
            ShowStatusText("Power Up!", Color.cyan);
        }
        
        // 파워 부족 카운트 리셋
        if (playerController != null)
        {
            playerController.ResetInsufficientPowerCount();
        }
    }
}

