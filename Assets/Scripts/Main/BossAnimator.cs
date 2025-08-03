using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BossAnimator : MonoBehaviour
{
    public Animator[] bodies;
    private float interval = 0.5f;
    public string animationName = "Blue Idle - Animation";

    // Start is called before the first frame update
    void Start()
    {
        StartCoroutine(PlayStaggered());
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
}
