using UnityEngine;
using UnityEngine.SceneManagement;

public class CameraFollow : MonoBehaviour
{
    public Transform target;      // 따라갈 대상 (옵션: 인스펙터로 없을 땐 자동 탐색)
    public Vector3 offset;
    public float smoothSpeed = 0.125f;

    private void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    void LateUpdate()
    {
        // target이 없거나 이미 파괴된 경우 재획득 시도
        if (target == null)
        {
            TryAcquireTarget();
            if (target == null) return;
        }

        Vector3 desiredPosition = new Vector3(transform.position.x, target.position.y, transform.position.z) + offset;
        Vector3 smoothedPosition = Vector3.Lerp(transform.position, desiredPosition, smoothSpeed);
        transform.position = smoothedPosition;
    }

    private void OnSceneLoaded(UnityEngine.SceneManagement.Scene scene, UnityEngine.SceneManagement.LoadSceneMode mode)
    {
        // 씬 바뀌면 타겟을 다시 찾아본다
        TryAcquireTarget();
    }

    private void TryAcquireTarget()
    {
        // 예: 플레이어에 "Player" 태그가 붙어 있다면
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
            target = player.transform;
    }
}
