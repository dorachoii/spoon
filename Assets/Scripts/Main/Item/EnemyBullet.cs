using UnityEngine;

public class EnemyBullet : ItemBase
{
    private float speed = 3f;
    private float lifeTime = 4f;
    public Vector2 direction = Vector2.up;

    void Start()
    {
        Destroy(gameObject, lifeTime);
    }


    protected override void Update()
    {
        base.Update();
        transform.Translate(direction.normalized * speed * Time.deltaTime);
    }

    protected override void ApplyEffect(GameObject player)
    {
         PlayerStat.Instance.DamageHP(5);
         ShowStatusText("Damaged!", Color.red);
    }

}
