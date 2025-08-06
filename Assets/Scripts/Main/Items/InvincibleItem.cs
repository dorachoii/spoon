using UnityEngine;

// 무적 (無敵)
public class InvincibleItem : ItemBase
{
    SpriteColorEffect effector;

    protected override void Awake()
    {
        base.Awake();
        effector = GetComponent<SpriteColorEffect>();
        StartCoroutine(effector.IRainbowEffect(GetComponent<SpriteRenderer>(), -1));
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
