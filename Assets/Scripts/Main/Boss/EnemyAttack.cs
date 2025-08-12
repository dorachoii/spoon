using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Tilemaps;

public enum EnemyType
{
    Enemy01,
    Enemy02,
    Enemy03
}

public class EnemyAttack : MonoBehaviour
{
    public GameObject[] bulletPrefab;
    private float fireRate = 4.0f;
    public int numOfDirections = 4;

    
    [SerializeField] private float extendDuration = 0.5f;
    [SerializeField] private float holdDuration = 0.2f;
    [SerializeField] private float lineWidth = 0.05f;
    [SerializeField] private Color warningColor = Color.red;


    float timer;
    private Tilemap tilemap;

    public EnemyType type;

    private bool isOnCooldown = false;
    private Transform playerTransform;
    private float minDistance = 5f;

    void Awake()
    {
        playerTransform = GameObject.FindGameObjectWithTag("Player").transform;
        tilemap = GameObject.FindGameObjectWithTag("Tilemap").GetComponent<Tilemap>();
    }

    // Start is called before the first frame update
    void Update()
    {
        float dist = Vector3.Distance(transform.position, playerTransform.position);
        switch (type)
        {
            case EnemyType.Enemy01:
                Shoot();
                break;
            case EnemyType.Enemy02:
                Shoot2();
                break;
            case EnemyType.Enemy03:
                if(!isOnCooldown && dist >= minDistance)
                PlayWarnLines();
                break;
        }

    }

    void Shoot2()
    {
        timer += Time.deltaTime;
        if (timer >= fireRate)
        {
            Instantiate(bulletPrefab[(int)EnemyType.Enemy02], transform.position, Quaternion.identity);
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

    void Shoot3()
    {
        Debug.Log("Shoot03");

            GameObject bullet = Instantiate(bulletPrefab[(int)EnemyType.Enemy03], transform.position, Quaternion.identity);
            bullet.transform.up = bullet.transform.right;

            GameObject bullet2 = Instantiate(bulletPrefab[(int)EnemyType.Enemy03], transform.position, Quaternion.identity);
            bullet2.transform.up = Vector2.left;

    }

    public void PlayWarnLines()
    {
        StartCoroutine(WarnCooldownRoutine());
    }

    private IEnumerator WarnCooldownRoutine()
    {
        isOnCooldown = true;
        yield return StartCoroutine(DrawWarnLines());
        yield return new WaitForSeconds(0.3f);
        yield return StartCoroutine(DrawWarnLines());
         yield return new WaitForSeconds(0.3f);
        Shoot3();
        yield return new WaitForSeconds(fireRate);
        isOnCooldown = false;
    }

    private IEnumerator DrawWarnLines()
    {
        Vector3 enemyPos = transform.position;
        Camera cam = Camera.main;
        if (cam == null)
        {
            Debug.LogWarning("Main Camera not found.");
            yield break;
        }

        // 현재 위치의 뷰포트 Y 를 구해서, 좌우 끝 좌표를 같은 Y로 잡음
        Vector3 viewportPos = cam.WorldToViewportPoint(enemyPos); // z는 카메라-월드 거리 포함
        float depth = Mathf.Abs(cam.transform.position.z - enemyPos.z);
        Vector3 leftViewport = new Vector3(0f, viewportPos.y, viewportPos.z);
        Vector3 rightViewport = new Vector3(1f, viewportPos.y, viewportPos.z);

        Vector3 leftWorldTarget = cam.ViewportToWorldPoint(leftViewport);
        Vector3 rightWorldTarget = cam.ViewportToWorldPoint(rightViewport);
        Vector3 origin = enemyPos;

        // 방향 벡터 (정규화)
        Vector3 leftDir = (leftWorldTarget - origin);
        float leftDist = leftDir.magnitude;
        leftDir.Normalize();

        Vector3 rightDir = (rightWorldTarget - origin);
        float rightDist = rightDir.magnitude;
        rightDir.Normalize();

        // 라인 생성
        LineRenderer leftLine = CreateWarningLine("WarnLine_Left");
        LineRenderer rightLine = CreateWarningLine("WarnLine_Right");

        float elapsed = 0f;
        while (elapsed < extendDuration)
        {
            float t = elapsed / extendDuration;
            float lenL = Mathf.Lerp(0f, leftDist, t);
            float lenR = Mathf.Lerp(0f, rightDist, t);

            leftLine.SetPosition(0, origin);
            leftLine.SetPosition(1, origin + leftDir * lenL);

            rightLine.SetPosition(0, origin);
            rightLine.SetPosition(1, origin + rightDir * lenR);

            elapsed += Time.deltaTime;
            yield return null;
        }

        // 최대 길이 고정
        leftLine.SetPosition(0, origin);
        leftLine.SetPosition(1, origin + leftDir * leftDist);
        rightLine.SetPosition(0, origin);
        rightLine.SetPosition(1, origin + rightDir * rightDist);

        // 잠깐 유지
        yield return new WaitForSeconds(holdDuration);

        Destroy(leftLine.gameObject);
        Destroy(rightLine.gameObject);
    }

    private LineRenderer CreateWarningLine(string name)
    {
        GameObject go = new GameObject(name);
        var lr = go.AddComponent<LineRenderer>();
        lr.positionCount = 2;
        lr.startWidth = lineWidth;
        lr.endWidth = lineWidth;
        lr.useWorldSpace = true;
        lr.material = new Material(Shader.Find("Sprites/Default"));
        lr.startColor = warningColor;
        lr.endColor = warningColor;
        lr.numCapVertices = 4;
        return lr;
    }


}
