// using UnityEngine;
// using UnityEngine.UI;
// using System.Collections;
// using System.IO;
// using System;

// public class IrisEffectController : MonoBehaviour
// {
//     public RectTransform irisCircle;  // 마스크용 원 Image RectTransform
//     public CanvasGroup canvasGame;    // canvas_game 전체 제어용 CanvasGroup
//     private Transform playerTransform; // 플레이어 Transform

//     [Header("Iris Settings")]
//     public float startRadius = 200f;   // 시작 원 크기 (화면 꽉 차는 크기)
//     public float endRadius = 0f;       // 최종 원 크기 (0에 가깝게)
//     public float duration = 1f;        // iris in 시간

//     private bool isAnimating = false;

//     private void Awake()
//     {
//         // 초기에는 UI 투명, 비활성 상태로 설정
//         canvasGame.alpha = 0;
//         canvasGame.blocksRaycasts = false;
//         canvasGame.interactable = false;
//         irisCircle.sizeDelta = new Vector2(startRadius, startRadius);

//         playerTransform = GameObject.FindGameObjectWithTag("Player").transform;
//     }

//     private void OnEnable()
//     {
//         Debug.Log("IrisEffectController enabled");
//     }

//     void Update()
//     {
//         Debug.Log("Update 호출 중...");
//         if (Input.GetKeyDown(KeyCode.Space))
//         {
//             Debug.Log("Enter 키 눌림, IrisIn 시작");
//             PlayIrisIn();
//         }
//     }

//     public void PlayIrisIn()
//     {
//         if (isAnimating) return;

//         Debug.Log("iris in 시작");

//         // UI 활성화
//         canvasGame.alpha = 1;
//         canvasGame.blocksRaycasts = true;
//         canvasGame.interactable = true;
//         gameObject.SetActive(true); // 활성화 (만약 비활성 상태였다면)

//         // 플레이어 위치를 캔버스 로컬 좌표로 변환 후 원 위치 지정
//         Vector2 irisPos;
//         RectTransform canvasRect = canvasGame.GetComponent<RectTransform>();
//         Vector3 screenPos = Camera.main.WorldToScreenPoint(playerTransform.position);
//         RectTransformUtility.ScreenPointToLocalPointInRectangle(canvasRect, screenPos, null, out irisPos);
//         irisCircle.localPosition = irisPos;

//         irisCircle.sizeDelta = new Vector2(startRadius, startRadius); // 초기 크기 설정

//         StartCoroutine(IrisInCoroutine());
//     }

//     private IEnumerator IrisInCoroutine()
//     {
//         isAnimating = true;

//         float elapsed = 0f;
//         while (elapsed < duration)
//         {
//             float t = elapsed / duration;
//             float size = Mathf.Lerp(startRadius, endRadius, t);
//             irisCircle.sizeDelta = new Vector2(size, size);

//             elapsed += Time.deltaTime;
//             yield return null;
//         }

//         irisCircle.sizeDelta = new Vector2(endRadius, endRadius);

//         // 애니메이션 종료 후 UI 비활성화 및 상태 초기화
//         canvasGame.alpha = 0;
//         canvasGame.blocksRaycasts = false;
//         canvasGame.interactable = false;
//         gameObject.SetActive(false);

//         isAnimating = false;

//         Debug.Log("iris in 완료");

//         // 여기에 리스타트 UI 활성화 같은 후속 작업 호출 가능
//     }
// }
