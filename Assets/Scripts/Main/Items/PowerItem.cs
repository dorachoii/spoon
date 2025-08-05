using UnityEngine;

public class PowerItem : ItemBase
{
    private float powerIncrease = 50f; // 파워 증가량

    protected override void ApplyEffect(GameObject player)
    {
        PlayerStat playerStat = player.GetComponent<PlayerStat>();
        if (playerStat != null)
        {
            playerStat.AddDigPowerBonus(powerIncrease);
            ShowStatusText("Power Up!", Color.cyan);
        }

    }
}

