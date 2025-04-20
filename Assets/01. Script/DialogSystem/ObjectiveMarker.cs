using UnityEngine;
using System.Collections;
using DG.Tweening;

public class ObjectiveMarker : MonoBehaviour
{
    [Header("기본 설정")]
    [SerializeField] private GameObject arrowPrefab;
    [SerializeField] private float arrowHeight = 2f;
    [SerializeField] private float arrowBobAmount = 0.3f;
    [SerializeField] private float arrowBobSpeed = 1f;
    [SerializeField] private Color arrowColor = Color.green;

    [Header("거리 설정")]
    [SerializeField] private float minDistance = 2f;  // 이 거리 이하면 화살표 숨김
    [SerializeField] private float maxDistance = 50f; // 최대 표시 거리 

    private GameObject arrowInstance;
    private Transform playerTransform;
    private Transform targetTransform;
    private bool isActive = false;

    // 싱글톤 인스턴스
    public static ObjectiveMarker Instance { get; private set; }

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        // 플레이어 캐릭터 참조 가져오기
        playerTransform = GameInitializer.Instance.GetPlayerClass().playerTransform;
    }
    // ObjectiveMarker.cs
    private void OnEnable()
    {
        // DialogSystem 이벤트 구독
        if (DialogSystem.Instance != null)
        {
            DialogSystem.OnDialogEvent += HandleDialogEvent;
            Debug.Log("ObjectiveMarker: DialogSystem 이벤트 구독 완료");
        }
    }

    private void OnDisable()
    {
        // 구독 해제
        if (DialogSystem.Instance != null)
        {
            DialogSystem.OnDialogEvent -= HandleDialogEvent;
        }
    }    



    // 화살표 활성화
    public void ShowArrow(Transform target)
    {
        targetTransform = target;

        if (arrowInstance == null && arrowPrefab != null)
        {
            arrowInstance = Instantiate(arrowPrefab, Vector3.zero, Quaternion.identity);
            arrowInstance.transform.parent = transform;

            // 화살표 색상 설정
            Renderer[] renderers = arrowInstance.GetComponentsInChildren<Renderer>();
            foreach (Renderer r in renderers)
            {
                if (r.material.HasProperty("_Color"))
                {
                    r.material.color = arrowColor;
                }
            }

            // 화살표 애니메이션
            StartBobAnimation();
        }

        isActive = true;
    }

    // 화살표 비활성화
    public void HideArrow()
    {
        isActive = false;

        if (arrowInstance != null)
        {
            // DOTween 애니메이션 중지
            DOTween.Kill(arrowInstance.transform);

            // 화살표 제거
            Destroy(arrowInstance);
            arrowInstance = null;
        }
    }

    // 위아래로 움직이는 애니메이션
    private void StartBobAnimation()
    {
        if (arrowInstance != null)
        {
            // 초기 위치 저장
            Vector3 startPos = arrowInstance.transform.localPosition;

            // 영원히 반복되는 위아래 애니메이션
            arrowInstance.transform.DOLocalMoveY(startPos.y + arrowBobAmount, arrowBobSpeed)
                .SetEase(Ease.InOutSine)
                .SetLoops(-1, LoopType.Yoyo);
        }
    }

    private void Update()
    {
        if (isActive && targetTransform != null && playerTransform != null && arrowInstance != null)
        {
            UpdateArrowPosition();
        }
    }

    private void UpdateArrowPosition()
    {
        // 플레이어와 타겟 사이의 거리 계산
        float distance = Vector3.Distance(playerTransform.position, targetTransform.position);

        // 거리가 최소 거리보다 작으면 화살표 숨김
        if (distance < minDistance)
        {
            arrowInstance.SetActive(false);
            return;
        }
        else
        {
            arrowInstance.SetActive(true);
        }

        // 타겟 위치에 화살표 배치
        Vector3 arrowPos = targetTransform.position + Vector3.up * arrowHeight;
        arrowInstance.transform.position = arrowPos;

        // 화살표가 항상 플레이어를 향하도록 회전
        arrowInstance.transform.LookAt(2 * arrowInstance.transform.position - playerTransform.position);

        // 거리에 따른 크기 조절 (선택 사항)
        float scaleFactor = Mathf.Clamp(1f - (distance / maxDistance), 0.5f, 1f);
        arrowInstance.transform.localScale = Vector3.one * scaleFactor;
    }

    // 화살표 생성 및 특정 대상 강조 (예: 무기 또는 더미)
    public void HighlightObject(string objectTag, string playerFlagName = "")
    {
        // 태그로 오브젝트 찾기
        GameObject targetObj = GameObject.FindGameObjectWithTag(objectTag);

        if (targetObj != null)
        {
            ShowArrow(targetObj.transform);

            // 플래그 설정이 필요하면 설정
            if (!string.IsNullOrEmpty(playerFlagName) && GameProgressManager.Instance != null)
            {
                GameProgressManager.Instance.SetFlag(playerFlagName, true);
            }
        }
    }

    // 플레이어가 다이얼로그에서 받은 특정 이벤트에 반응하여 화살표 표시
    public void HandleDialogEvent(string eventName)
    {
        switch (eventName)
        {
            case "HighlightWeapon":
                HighlightObject("Weapon", "weapon_highlighted");
                break;

            case "HighlightDummy":
                HighlightObject("Dummy", "dummy_highlighted");
                break;

            case "DisableMarker":
                HideArrow();
                break;
        }
    }
}