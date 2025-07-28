using System.Collections;
using System.Collections.Generic;
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
            Debug.Log($"[PowerItem] Increased dig power by {powerIncrease}. New dig power: {playerStat.DigPower}");
        }
        else
        {
            Debug.LogWarning("[PowerItem] PlayerStat component not found on player.");
        }
    }
}

