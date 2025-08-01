using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HPInfiniteItem : ItemBase
{
    public GameObject hpFX;
    SpriteColorEffect effector;

    protected override void Awake()
    {
        base.Awake();
        effector = GetComponent<SpriteColorEffect>();
        StartCoroutine(effector.IRainbowEffect(GetComponent<SpriteRenderer>(), -1));
    }

    protected override void ApplyEffect(GameObject player)
    {
        InstantiateFX();
        PlayerStat playerStat = player.GetComponent<PlayerStat>();
        if (playerStat != null)
        {
            StartCoroutine(playerStat.RecoverHPAndInvincible(5f));
        }
    }

    void InstantiateFX()
    {
        GameObject fx = Instantiate(hpFX, gameObject.transform.position, Quaternion.identity);
        Destroy(fx, 1);
    }

}
