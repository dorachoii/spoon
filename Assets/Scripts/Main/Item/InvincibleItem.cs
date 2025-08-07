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
            StartCoroutine(playerStat.RecoverHPAndInvincible(5f));
            ShowStatusText("Invincible!", Color.blue);
        }
    }
}
