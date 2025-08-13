using UnityEngine;

public class PoisonItem : ItemBase
{
    private float poisonDuration = 7f;
    
    protected override void ApplyEffect(GameObject player)
    {
        PlayerStat playerStat = player.GetComponent<PlayerStat>();
        if (playerStat != null)
        {
            playerStat.StartPoisonEffect(poisonDuration);
            ShowStatusText("Poisoned!", PlayerColor.Green.ToColor());
        }
    }
}
