using UnityEngine;
using System.Collections;
using DG.Tweening;

/// <summary>
/// CinematicCameraController - ���̾�α� �ý��۰� �����Ǵ� �ó׸�ƽ ī�޶� ȿ�� ��Ʈ�ѷ�
/// 
/// ����:
/// 1. ���̾�α� �̺�Ʈ���� ���� �������� ī�޶� ȿ���� Ʈ�����մϴ�:
///    - "FocusCamera:Ÿ���±�:������" 
///      ��: "FocusCamera:CameraPoint01:1.5"
///      (CameraPoint01 �±��� ������Ʈ ��ġ�� �����̼����� ī�޶� �̵�, 1.5�� �� ����)
///      
///    - "ShakeCamera:����:���ӽð�" 
///      ��: "ShakeCamera:0.5:0.3" (�߰� ������ 0.3�ʰ� ī�޶� ��鸲)
///      
///    - "ResetCamera:������" 
///      ��: "ResetCamera:1.0" (1�� �� ���� ī�޶�� ����)
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
        // ���� �̺�Ʈ ó�� (�޸��� ����)
        if (eventName.Contains(","))
        {
            string[] events = eventName.Split(',');
            foreach (string evt in events)
            {
                HandleDialogEvent(evt.Trim());
            }
            return;
        }

        // FocusCamera �̺�Ʈ ó��
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
        // ResetCamera �̺�Ʈ ó��
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
        // ī�޶� ����ũ �̺�Ʈ ó��
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

            // CameraShakeManager�� ����Ͽ� ����ũ ȿ�� ����
            CameraShakeManager.TriggerShake(intensity, duration);
        }
        // FadeToBlack �̺�Ʈ ó��
        else if (eventName == "FadeToBlack")
        {
            if (SceneTransitionManager.Instance != null)
            {
                SceneTransitionManager.Instance.FadeIn();
            }
        }
        // FadeFromBlack �̺�Ʈ ó��
        else if (eventName == "FadeFromBlack")
        {
            if (SceneTransitionManager.Instance != null)
            {
                SceneTransitionManager.Instance.FadeOut();
            }
        }
        // DelayedCall �̺�Ʈ ó��
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

    // Ÿ�� ������Ʈ�� ��ġ�� �����̼����� ī�޶� �̵�
    public void MoveCameraToTarget(Transform target)
    {
        if (target == null || mainCamera == null) return;

        // ���� �ó׸�ƽ�� Ȱ��ȭ�Ǿ� ���� ������ ���� ���� ����
        if (!isCinematicActive)
        {
            // CameraFollow�� ������ ��Ȱ��ȭ
            if (cameraFollow != null)
            {
                cameraFollow.enabled = false;
                originalTarget = cameraFollow.GetPlayerTransform();
                originalOffset = cameraFollow.GetOffset();
            }

            // ���� ī�޶� ��ġ/ȸ�� ����
            originalPosition = mainCamera.transform.position;
            originalRotation = mainCamera.transform.rotation;

            isCinematicActive = true;
        }
        else
        {
            // �̹� ���� ���� Ʈ�� �ߴ�
            DOTween.Kill(mainCamera.transform);
        }

        // Ÿ�� ��ġ�� �����̼� ��������
        Vector3 targetPosition = target.position;
        Quaternion targetRotation = target.rotation;

        // �߰� ��� ����Ʈ ���
        Vector3 midPoint = Vector3.Lerp(mainCamera.transform.position, targetPosition, 0.5f);
        midPoint.y += Mathf.Min(5f, Vector3.Distance(mainCamera.transform.position, targetPosition) * 0.2f);

        // ��� ����Ʈ �迭 ����
        Vector3[] pathPoints = new Vector3[] {
            mainCamera.transform.position, // ������
            midPoint, // �߰���
            targetPosition // ������
        };

        // ī�޶� �̵�
        mainCamera.transform.DOPath(
            pathPoints,
            transitionDuration,
            pathType,
            pathMode: PathMode.Full3D,
            resolution: pathResolution
        ).SetEase(pathEase);

        // ī�޶� ȸ��
        mainCamera.transform.DORotateQuaternion(
            targetRotation,
            transitionDuration * 0.8f
        ).SetEase(Ease.OutSine);
    }

    // ���� ī�޶�� ����
    public void ResetCamera()
    {
        if (!isCinematicActive || mainCamera == null) return;

        // ���� ���� Ʈ�� �ߴ�
        DOTween.Kill(mainCamera.transform);

        // ���� ��ġ ��� (CameraFollow�� ����ϴ� ��ġ)
        Vector3 returnPosition = originalTarget.position + originalOffset;

        // �߰� ��� ����Ʈ ���
        Vector3 midPoint = Vector3.Lerp(mainCamera.transform.position, returnPosition, 0.5f);
        midPoint.y += Mathf.Min(5f, Vector3.Distance(mainCamera.transform.position, returnPosition) * 0.2f);

        // ��� ����Ʈ �迭 ����
        Vector3[] pathPoints = new Vector3[] {
            mainCamera.transform.position, // ������
            midPoint, // �߰���
            returnPosition // ������
        };

        // ī�޶� �̵� �� ȸ��
        mainCamera.transform.DOPath(
            pathPoints,
            transitionDuration,
            pathType,
            pathMode: PathMode.Full3D,
            resolution: pathResolution
        ).SetEase(pathEase).OnComplete(() => {
            // �ִϸ��̼� �Ϸ� �� ���� ī�޶� ���� ����
            if (cameraFollow != null)
            {
                cameraFollow.enabled = true;
            }

            isCinematicActive = false;
        });

        // ī�޶� ȸ��
        mainCamera.transform.DORotateQuaternion(
            originalRotation,
            transitionDuration * 0.8f
        ).SetEase(Ease.OutSine);
    }
}