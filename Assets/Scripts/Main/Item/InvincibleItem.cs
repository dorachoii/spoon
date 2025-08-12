using UnityEngine;

// 무적 (無敵)
public class InvincibleItem : ItemBase
{
    SpriteColorEffector effector;

    protected override void Awake()
    {
        base.Awake();
        effector = GetComponent<SpriteColorEffector>();
        StartCoroutine(effector.IRainbow(GetComponent<SpriteRenderer>(), loop: true));
    }

    protected override void ApplyEffect(GameObject player)
    {
        PlayerStat playerStat = player.GetComponent<PlayerStat>();
        if (playerStat != null)
        {
            playerStat.StartInvincible(10f);
            ShowStatusText("Invincible!", Color.blue);
        }
    }
}
