using UnityEngine;
using System.Collections;
using DG.Tweening;

public class ObjectiveMarker : MonoBehaviour
{
    [Header("�⺻ ����")]
    [SerializeField] private GameObject arrowPrefab;
    [SerializeField] private float arrowHeight = 2f;
    [SerializeField] private float arrowBobAmount = 0.3f;
    [SerializeField] private float arrowBobSpeed = 1f;
    [SerializeField] private Color arrowColor = Color.green;

    [Header("�Ÿ� ����")]
    [SerializeField] private float minDistance = 2f;  // �� �Ÿ� ���ϸ� ȭ��ǥ ����
    [SerializeField] private float maxDistance = 50f; // �ִ� ǥ�� �Ÿ� 

    private GameObject arrowInstance;
    private Transform playerTransform;
    private Transform targetTransform;
    private bool isActive = false;

    // �̱��� �ν��Ͻ�
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
        // �÷��̾� ĳ���� ���� ��������
        playerTransform = GameInitializer.Instance.GetPlayerClass().playerTransform;
    }
    // ObjectiveMarker.cs
    private void OnEnable()
    {
        // DialogSystem �̺�Ʈ ����
        if (DialogSystem.Instance != null)
        {
            DialogSystem.OnDialogEvent += HandleDialogEvent;
            Debug.Log("ObjectiveMarker: DialogSystem �̺�Ʈ ���� �Ϸ�");
        }
    }

    private void OnDisable()
    {
        // ���� ����
        if (DialogSystem.Instance != null)
        {
            DialogSystem.OnDialogEvent -= HandleDialogEvent;
        }
    }    



    // ȭ��ǥ Ȱ��ȭ
    public void ShowArrow(Transform target)
    {
        targetTransform = target;

        if (arrowInstance == null && arrowPrefab != null)
        {
            arrowInstance = Instantiate(arrowPrefab, Vector3.zero, Quaternion.identity);
            arrowInstance.transform.parent = transform;

            // ȭ��ǥ ���� ����
            Renderer[] renderers = arrowInstance.GetComponentsInChildren<Renderer>();
            foreach (Renderer r in renderers)
            {
                if (r.material.HasProperty("_Color"))
                {
                    r.material.color = arrowColor;
                }
            }

            // ȭ��ǥ �ִϸ��̼�
            StartBobAnimation();
        }

        isActive = true;
    }

    // ȭ��ǥ ��Ȱ��ȭ
    public void HideArrow()
    {
        isActive = false;

        if (arrowInstance != null)
        {
            // DOTween �ִϸ��̼� ����
            DOTween.Kill(arrowInstance.transform);

            // ȭ��ǥ ����
            Destroy(arrowInstance);
            arrowInstance = null;
        }
    }

    // ���Ʒ��� �����̴� �ִϸ��̼�
    private void StartBobAnimation()
    {
        if (arrowInstance != null)
        {
            // �ʱ� ��ġ ����
            Vector3 startPos = arrowInstance.transform.localPosition;

            // ������ �ݺ��Ǵ� ���Ʒ� �ִϸ��̼�
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
        // �÷��̾�� Ÿ�� ������ �Ÿ� ���
        float distance = Vector3.Distance(playerTransform.position, targetTransform.position);

        // �Ÿ��� �ּ� �Ÿ����� ������ ȭ��ǥ ����
        if (distance < minDistance)
        {
            arrowInstance.SetActive(false);
            return;
        }
        else
        {
            arrowInstance.SetActive(true);
        }

        // Ÿ�� ��ġ�� ȭ��ǥ ��ġ
        Vector3 arrowPos = targetTransform.position + Vector3.up * arrowHeight;
        arrowInstance.transform.position = arrowPos;

        // ȭ��ǥ�� �׻� �÷��̾ ���ϵ��� ȸ��
        arrowInstance.transform.LookAt(2 * arrowInstance.transform.position - playerTransform.position);

        // �Ÿ��� ���� ũ�� ���� (���� ����)
        float scaleFactor = Mathf.Clamp(1f - (distance / maxDistance), 0.5f, 1f);
        arrowInstance.transform.localScale = Vector3.one * scaleFactor;
    }

    // ȭ��ǥ ���� �� Ư�� ��� ���� (��: ���� �Ǵ� ����)
    public void HighlightObject(string objectTag, string playerFlagName = "")
    {
        // �±׷� ������Ʈ ã��
        GameObject targetObj = GameObject.FindGameObjectWithTag(objectTag);

        if (targetObj != null)
        {
            ShowArrow(targetObj.transform);

            // �÷��� ������ �ʿ��ϸ� ����
            if (!string.IsNullOrEmpty(playerFlagName) && GameProgressManager.Instance != null)
            {
                GameProgressManager.Instance.SetFlag(playerFlagName, true);
            }
        }
    }

    // �÷��̾ ���̾�α׿��� ���� Ư�� �̺�Ʈ�� �����Ͽ� ȭ��ǥ ǥ��
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