using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum enemyType
{
    Enemy01,
    Enemy02,
    Enemy03
}

public class EnemyAttack : MonoBehaviour
{
    public GameObject[] bulletPrefab;
    private float fireRate = 3.0f;
    public int numOfDirections = 4;

    float timer;

    public enemyType type;

    // Start is called before the first frame update
    void Update()
    {
        switch (type)
        {
            case enemyType.Enemy01:
                Shoot();
                break;
            case enemyType.Enemy02:
                Shoot2();
                break;
            case enemyType.Enemy03:
                break;
        }
        
    }

    void Shoot2()
    {
        timer += Time.deltaTime;
        if (timer >= fireRate)
        {
            Instantiate(bulletPrefab[(int)enemyType.Enemy02], transform.position, Quaternion.identity);
            timer = 0f;
        }
    }


    void Shoot()
    {
        timer += Time.deltaTime;
        if (timer >= fireRate)
        {
            ShootInDirections(numOfDirections, (int)type);
            timer = 0f;
        }
    }

    void ShootInDirections(int count, int idx)
    {
        float angleStep = 360 / count;

        for (int i = 0; i < count; i++)
        {
            float angle = 45f + angleStep * i;
            Vector2 direction = AngleToDirection(angle);

            GameObject bullet = Instantiate(bulletPrefab[idx], transform.position, Quaternion.identity);
            bullet.GetComponent<EnemyBullet>().direction = direction;
        }
    }

    Vector2 AngleToDirection(float angle)
    {
        float rad = angle * Mathf.Deg2Rad;
        return new Vector2(Mathf.Cos(rad), Mathf.Sin(rad));
    }
}
