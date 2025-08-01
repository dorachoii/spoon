using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyBullet : ItemBase
{
    private float speed = 3f;
    private float lifeTime = 4f;
    public Vector2 direction = Vector2.up;

    // Start is called before the first frame update
    void Start()
    {
        Destroy(gameObject, lifeTime);
    }

    // Update is called once per frame
    protected override void Update()
    {
        base.Update();
        transform.Translate(direction.normalized * speed * Time.deltaTime);
    }

    protected override void ApplyEffect(GameObject player)
    {
         PlayerStat.Instance.DamageHP(5);
    }

}
