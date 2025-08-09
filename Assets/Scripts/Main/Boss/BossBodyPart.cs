using System.Collections;
using UnityEngine;

public class BossBodyPart : MonoBehaviour
{
    private BossHP bossHP;

    private Animator animator;
    private float damageTriggerCooldown = 0.3f;
    private bool damagedTriggered = false;

    void Awake()
    {
        bossHP = GetComponentInParent<BossHP>();
        animator = GetComponent<Animator>();
    }
    void Start()
    {
        bossHP.OnDeath += HandleDeath;
    }

    void Oestroy()
    {
        bossHP.OnDeath -= HandleDeath;
    }

    void HandleDeath()
    {
        animator.SetTrigger("Death");
    }

    public void Damage(int damage)
    {
        if (!damagedTriggered)
        {
            animator.SetTrigger("Damaged");
            StartCoroutine(DamagedCooldownRoutine());
        }

        bossHP.TakeDamage(damage);
    }

    private IEnumerator DamagedCooldownRoutine()
    {
        damagedTriggered = true;
        yield return new WaitForSeconds(damageTriggerCooldown);
        damagedTriggered = false;
    }
    
      public void TriggerBounceY()
    {
        StartCoroutine(BounceY(1f, 0.3f));
    }

    private IEnumerator BounceY(float height, float duration)
    {
        Vector3 startPos = transform.position;
        Vector3 apex = startPos + new Vector3(0, height, 0);

        float elapsed = 0f;

        while (elapsed < duration)
        {
            float t = elapsed / duration;
            float y = Mathf.Lerp(startPos.y, apex.y, Mathf.Sin(t * Mathf.PI));
            transform.position = new Vector3(startPos.x, y, startPos.z);

            elapsed += Time.deltaTime;
            yield return null;
        }
        transform.position = startPos;
    }
}
