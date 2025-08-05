using UnityEngine;

// 해독제 (解毒アイテム)
public class AntidoteItem : ItemBase
{
    protected override void ApplyEffect(GameObject player)
    {
        PlayerStat playerStat = player.GetComponent<PlayerStat>();
        if (playerStat != null)
        {
            playerStat.CurePoision();
            ShowStatusText("Poison Cure", Color.green);
        }
    }
}
