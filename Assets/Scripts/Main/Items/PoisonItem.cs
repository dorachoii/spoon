using System.Collections.Generic;
using UnityEngine;

public class PoisonItem : ItemBase
{
    [Header("Poison Item Settings")]
    [SerializeField] private float poisonDuration = 20f;
    
    protected override void ApplyEffect(GameObject player)
    {
        PlayerStat playerStat = player.GetComponent<PlayerStat>();
        if (playerStat != null)
        {
            playerStat.StartPoisonEffect(poisonDuration);
            ShowStatusText("Poisoned", Color.red);
        }
    }
}
