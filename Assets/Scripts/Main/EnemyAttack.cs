using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyAttack : MonoBehaviour
{
    public GameObject bulletPrefab;
    private float fireRate = 3.0f;
    public int numOfDirections = 8;

    float timer;

    // Start is called before the first frame update
    void Update()
    {
        timer += Time.deltaTime;
        if (timer >= fireRate)
        {
            ShootInDirections(numOfDirections);
            timer = 0f;
        }
    }

    void ShootInDirections(int count)
    {
        float angleStep = 360 / count;

        for (int i = 0; i < count; i++)
        {
            float angle = angleStep * i;
            Vector2 direction = AngleToDirection(angle);

            GameObject bullet = Instantiate(bulletPrefab, transform.position, Quaternion.identity);
            bullet.GetComponent<EnemyBullet>().direction = direction;
        }
    }

    Vector2 AngleToDirection(float angle)
    {
        float rad = angle * Mathf.Deg2Rad;
        return new Vector2(Mathf.Cos(rad), Mathf.Sin(rad));
    }
}
