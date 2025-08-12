using UnityEngine;

public class Debris : MonoBehaviour
{
    private Camera mainCam;
    private float offscreenMargin = 1f;
    private bool isBossDead = false;

    // Start is called before the first frame update
    void Start()
    {
        mainCam = Camera.main;
    }

    // Update is called once per frame
    void Update()
    {
        CheckOffScreen();
    }

    private void CheckOffScreen()
    {
        Vector3 viewportPos = mainCam.WorldToViewportPoint(transform.position);

        if (viewportPos.x < -offscreenMargin || viewportPos.x > 1 + offscreenMargin || viewportPos.y < -offscreenMargin || viewportPos.y > 1 + offscreenMargin) Destroy(gameObject);
    }

    // 보스가 죽었을 때 호출되는 메서드
    public void SetBossDead(bool bossDead)
    {
        isBossDead = bossDead;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Boss"))
        {
            var part = collision.GetComponent<BossBodyPart>();

            if (part != null)
            {
                part.Damage(1);
                Destroy(gameObject);
            }
        }
        else if (collision.CompareTag("Player"))
        {
            // 보스가 죽었을 때는 플레이어에게 데미지를 주지 않음
            if (!isBossDead)
            {
                PlayerStat.Instance.DamageHP(1);
            }
        }
    }
}
