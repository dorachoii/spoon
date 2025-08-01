using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DetoxItem : ItemBase
{
    protected override void ApplyEffect(GameObject player)
    {
        PlayerStat playerStat = player.GetComponent<PlayerStat>();
        if (playerStat != null)
        {
            playerStat.CurePoision();
        }
    }
}
