using UnityEngine;
using System.Collections;
using DG.Tweening;

/// <summary>
/// CinematicCameraController - 다이얼로그 시스템과 연동되는 시네마틱 카메라 효과 컨트롤러
/// 
/// 사용법:
/// 1. 다이얼로그 이벤트에서 다음 형식으로 카메라 효과를 트리거합니다:
///    - "FocusCamera:타겟태그:딜레이" 
///      예: "FocusCamera:CameraPoint01:1.5"
///      (CameraPoint01 태그의 오브젝트 위치와 로테이션으로 카메라 이동, 1.5초 후 실행)
///      
///    - "ShakeCamera:강도:지속시간" 
///      예: "ShakeCamera:0.5:0.3" (중간 강도로 0.3초간 카메라 흔들림)
///      
///    - "ResetCamera:딜레이" 
///      예: "ResetCamera:1.0" (1초 후 원래 카메라로 복귀)
/// </summary>
public class CinematicCameraController : MonoBehaviour
{
    public static CinematicCameraController Instance { get; private set; }

    [SerializeField] private Camera mainCamera;
    [SerializeField] private float transitionDuration = 1.5f;
    [SerializeField] private Ease pathEase = Ease.InOutSine;
    [SerializeField] private PathType pathType = PathType.CatmullRom;
    [SerializeField] private int pathResolution = 10;
    [SerializeField] private float defaultDelay = 0.5f;

    private CameraFollow cameraFollow;
    private Transform originalTarget;
    private Vector3 originalOffset;
    private Vector3 originalPosition;
    private Quaternion originalRotation;
    private bool isCinematicActive = false;

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

        if (mainCamera == null)
            mainCamera = Camera.main;

        cameraFollow = mainCamera.GetComponent<CameraFollow>();
    }

    private void OnEnable()
    {
        if (DialogSystem.Instance != null)
        {
            DialogSystem.OnDialogEvent += HandleDialogEvent;
        }
    }

    private void OnDisable()
    {
        if (DialogSystem.Instance != null)
        {
            DialogSystem.OnDialogEvent -= HandleDialogEvent;
        }
    }

    private void HandleDialogEvent(string eventName)
    {
        // 여러 이벤트 처리 (콤마로 구분)
        if (eventName.Contains(","))
        {
            string[] events = eventName.Split(',');
            foreach (string evt in events)
            {
                HandleDialogEvent(evt.Trim());
            }
            return;
        }

        // FocusCamera 이벤트 처리
        if (eventName.StartsWith("FocusCamera:"))
        {
            string[] parts = eventName.Split(':');
            if (parts.Length >= 2)
            {
                string targetTag = parts[1];
                float delay = defaultDelay;

                if (parts.Length >= 3 && !string.IsNullOrEmpty(parts[2])) float.TryParse(parts[2], out delay);

                GameObject targetObject = GameObject.FindGameObjectWithTag(targetTag);

                if (targetObject != null)
                {
                    DOVirtual.DelayedCall(delay, () => {
                        MoveCameraToTarget(targetObject.transform);
                    });
                }
            }
        }
        // ResetCamera 이벤트 처리
        else if (eventName.StartsWith("ResetCamera"))
        {
            float delay = defaultDelay;

            if (eventName.Contains(":"))
            {
                string[] parts = eventName.Split(':');
                if (parts.Length >= 2 && !string.IsNullOrEmpty(parts[1])) float.TryParse(parts[1], out delay);
            }

            DOVirtual.DelayedCall(delay, () => {
                ResetCamera();
            });
        }
        // 카메라 쉐이크 이벤트 처리
        else if (eventName.StartsWith("ShakeCamera"))
        {
            float intensity = 0.5f;
            float duration = 0.3f;

            if (eventName.Contains(":"))
            {
                string[] parts = eventName.Split(':');
                if (parts.Length >= 2 && !string.IsNullOrEmpty(parts[1])) float.TryParse(parts[1], out intensity);
                if (parts.Length >= 3 && !string.IsNullOrEmpty(parts[2])) float.TryParse(parts[2], out duration);
            }

            // CameraShakeManager를 사용하여 쉐이크 효과 적용
            CameraShakeManager.TriggerShake(intensity, duration);
        }
        // FadeToBlack 이벤트 처리
        else if (eventName == "FadeToBlack")
        {
            if (SceneTransitionManager.Instance != null)
            {
                SceneTransitionManager.Instance.FadeIn();
            }
        }
        // FadeFromBlack 이벤트 처리
        else if (eventName == "FadeFromBlack")
        {
            if (SceneTransitionManager.Instance != null)
            {
                SceneTransitionManager.Instance.FadeOut();
            }
        }
        // DelayedCall 이벤트 처리
        else if (eventName.StartsWith("DelayedCall:"))
        {
            string[] parts = eventName.Split(':');
            if (parts.Length >= 3)
            {
                float delay;
                if (float.TryParse(parts[1], out delay))
                {
                    string delayedEvent = eventName.Substring(eventName.IndexOf(':', parts[0].Length + 1) + 1);
                    DOVirtual.DelayedCall(delay, () => {
                        HandleDialogEvent(delayedEvent);
                    });
                }
            }
        }
    }

    // 타겟 오브젝트의 위치와 로테이션으로 카메라 이동
    public void MoveCameraToTarget(Transform target)
    {
        if (target == null || mainCamera == null) return;

        // 현재 시네마틱이 활성화되어 있지 않으면 원래 상태 저장
        if (!isCinematicActive)
        {
            // CameraFollow가 있으면 비활성화
            if (cameraFollow != null)
            {
                cameraFollow.enabled = false;
                originalTarget = cameraFollow.GetPlayerTransform();
                originalOffset = cameraFollow.GetOffset();
            }

            // 현재 카메라 위치/회전 저장
            originalPosition = mainCamera.transform.position;
            originalRotation = mainCamera.transform.rotation;

            isCinematicActive = true;
        }
        else
        {
            // 이미 진행 중인 트윈 중단
            DOTween.Kill(mainCamera.transform);
        }

        // 타겟 위치와 로테이션 가져오기
        Vector3 targetPosition = target.position;
        Quaternion targetRotation = target.rotation;

        // 중간 경로 포인트 계산
        Vector3 midPoint = Vector3.Lerp(mainCamera.transform.position, targetPosition, 0.5f);
        midPoint.y += Mathf.Min(5f, Vector3.Distance(mainCamera.transform.position, targetPosition) * 0.2f);

        // 경로 포인트 배열 생성
        Vector3[] pathPoints = new Vector3[] {
            mainCamera.transform.position, // 시작점
            midPoint, // 중간점
            targetPosition // 종료점
        };

        // 카메라 이동
        mainCamera.transform.DOPath(
            pathPoints,
            transitionDuration,
            pathType,
            pathMode: PathMode.Full3D,
            resolution: pathResolution
        ).SetEase(pathEase);

        // 카메라 회전
        mainCamera.transform.DORotateQuaternion(
            targetRotation,
            transitionDuration * 0.8f
        ).SetEase(Ease.OutSine);
    }

    // 원래 카메라로 리셋
    public void ResetCamera()
    {
        if (!isCinematicActive || mainCamera == null) return;

        // 진행 중인 트윈 중단
        DOTween.Kill(mainCamera.transform);

        // 원래 위치 계산 (CameraFollow가 사용하는 위치)
        Vector3 returnPosition = originalTarget.position + originalOffset;

        // 중간 경로 포인트 계산
        Vector3 midPoint = Vector3.Lerp(mainCamera.transform.position, returnPosition, 0.5f);
        midPoint.y += Mathf.Min(5f, Vector3.Distance(mainCamera.transform.position, returnPosition) * 0.2f);

        // 경로 포인트 배열 생성
        Vector3[] pathPoints = new Vector3[] {
            mainCamera.transform.position, // 시작점
            midPoint, // 중간점
            returnPosition // 종료점
        };

        // 카메라 이동 및 회전
        mainCamera.transform.DOPath(
            pathPoints,
            transitionDuration,
            pathType,
            pathMode: PathMode.Full3D,
            resolution: pathResolution
        ).SetEase(pathEase).OnComplete(() => {
            // 애니메이션 완료 후 원래 카메라 로직 복원
            if (cameraFollow != null)
            {
                cameraFollow.enabled = true;
            }

            isCinematicActive = false;
        });

        // 카메라 회전
        mainCamera.transform.DORotateQuaternion(
            originalRotation,
            transitionDuration * 0.8f
        ).SetEase(Ease.OutSine);
    }
}