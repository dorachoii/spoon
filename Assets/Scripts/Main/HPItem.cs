using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HPItem : ItemBase
{
    public GameObject hpFX;
    public float healamount = 10f;
    protected override void ApplyEffect(GameObject player)
    {
        InstantiateFX();
        PlayerStat playerStat = player.GetComponent<PlayerStat>();
        if (playerStat != null)
        {
            playerStat.HealHP(healamount);
        }
    }

     void InstantiateFX()
    {
        GameObject fx = Instantiate(hpFX, gameObject.transform.position, Quaternion.identity);
        Destroy(fx, 1);
    }
}
