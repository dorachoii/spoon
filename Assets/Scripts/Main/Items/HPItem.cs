using UnityEngine;

public class HPItem : ItemBase
{
    [SerializeField] private float healAmount = 10f;
    
    protected override void ApplyEffect(GameObject player)
    {
        PlayerStat playerStat = player.GetComponent<PlayerStat>();
        if (playerStat != null)
        {
            playerStat.HealHP(healAmount);
            ShowStatusText("Healed", Color.green);
        }
    }
}
