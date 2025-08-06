using UnityEngine;

public class PoisonItem : ItemBase
{
    [SerializeField] private float poisonDuration = 20f;
    
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
