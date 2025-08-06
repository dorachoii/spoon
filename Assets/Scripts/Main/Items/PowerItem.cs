using UnityEngine;

public class PowerItem : ItemBase
{
    private float powerBonus = 50f; 

    protected override void ApplyEffect(GameObject player)
    {
        PlayerStat playerStat = player.GetComponent<PlayerStat>();
        if (playerStat != null)
        {
            playerStat.AddDigPowerBonus(powerBonus);
            ShowStatusText("Power Up!", Color.cyan);
        }

    }
}

