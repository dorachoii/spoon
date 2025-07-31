using UnityEngine;
using UnityEngine.SceneManagement;

public class UIManager : MonoBehaviour
{
    public static UIManager Instance { get; private set; }

    [Header("References")]
    [SerializeField] private IrisEffectController irisEffectController; // 연결해두기
  
    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;

    }

    private void Start()
    {
        if (PlayerStat.Instance != null)
        {
            PlayerStat.Instance.OnDied += HandlePlayerDied;
        }
    }

    private void OnDestroy()
    {
        if (PlayerStat.Instance != null)
            PlayerStat.Instance.OnDied -= HandlePlayerDied;
    }

    private void HandlePlayerDied()
    {
        // iris in 실행
        if (irisEffectController != null)
        {
            irisEffectController.IrisIn();
        }

    }

  
}
