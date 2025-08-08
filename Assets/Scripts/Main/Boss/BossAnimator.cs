using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum bossType
{
    Normal,
    Jumping
}

public class BossAnimator : MonoBehaviour
{
    public Animator[] bodies;
    private float interval = 0.5f;
    public string animationName = "Blue Idle - Animation";

    public bossType type;

    // Start is called before the first frame update
    void Start()
    {
        switch (type)
        {
            case bossType.Normal:
                StartCoroutine(PlayStaggered());
                break;
            case bossType.Jumping:
                StartCoroutine(PlayStaggeredJump());
                break;
        }

    }

    private IEnumerator PlayStaggered()
    {
        for (int i = 0; i < bodies.Length; i++)
        {
            var animator = bodies[i];
            animator.Play(animationName, 0, 0f); // 0초부터 재생
            animator.speed = 1f;
            yield return new WaitForSeconds(interval);
        }
    }

    private IEnumerator PlayStaggeredJump()
    {
        for (int i = 0; i < bodies.Length; i++)
        {
            var animator = bodies[i];
            animator.speed = 1f;

            animator.SetTrigger("Jump");

            yield return new WaitForSeconds(2f);
        }
    }


}
