using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class PortalManager : MonoBehaviour
{
    public static PortalManager Instance { get; private set; }

    [Header("��Ż ����")]
    [SerializeField] private GameObject portalPrefab;
    [SerializeField] private float portalAppearDuration = 1.5f;
    [SerializeField] private float portalRotationSpeed = 30f;

    [Header("����Ʈ ����")]
    [SerializeField] private GameObject portalAppearEffectPrefab;
    [SerializeField] private GameObject portalActiveEffectPrefab;

    [Header("Ž�� ����")]
    [SerializeField] private LayerMask itemLayer;
    [SerializeField] private float detectionRadius = 15f;

    private GameObject currentPortal;
    private bool isCheckingForItems = false;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    // ��Ż ����
    public void SpawnPortal(Vector3 position, string targetStageID)
    {
        if (currentPortal != null)
        {
            Destroy(currentPortal);
        }

        StartCoroutine(SpawnPortalWithEffect(position, targetStageID));
    }

    // ��Ż ���� �ִϸ��̼�
    private IEnumerator SpawnPortalWithEffect(Vector3 position, string targetStageID)
    {
        // ����Ʈ ����
        if (portalAppearEffectPrefab != null)
        {
            Instantiate(portalAppearEffectPrefab, position, Quaternion.identity);
        }

        yield return new WaitForSeconds(2f);

        // ��Ż ����
        currentPortal = Instantiate(portalPrefab, position, Quaternion.identity);

        // ��Ż ũ�� �ִϸ��̼�
        currentPortal.transform.localScale = Vector3.zero;
        currentPortal.transform.DOScale(Vector3.one, portalAppearDuration)
            .SetEase(Ease.OutBack);

        // ȸ�� �ִϸ��̼� ����
        StartCoroutine(RotatePortal(currentPortal.transform));

        // ��Ż ������Ʈ ����
        StagePortal portalComponent = currentPortal.GetComponent<StagePortal>();
        if (portalComponent != null)
        {
            portalComponent.targetStageID = targetStageID; 
        }
        else
        {
            Debug.LogError("������ ��Ż�� DungeonPortal ������Ʈ�� �����ϴ�.");
        }

        // ��Ƽ�� ����Ʈ ����
        if (portalActiveEffectPrefab != null)
        {
            GameObject activeEffect = Instantiate(portalActiveEffectPrefab, position, Quaternion.identity);
            activeEffect.transform.SetParent(currentPortal.transform);
        }
    }

    // ��Ż ȸ�� �ִϸ��̼�
    private IEnumerator RotatePortal(Transform portalTransform)
    {
        while (portalTransform != null)
        {
            portalTransform.Rotate(Vector3.up, portalRotationSpeed * Time.deltaTime);
            yield return null;
        }
    }

    // �������� �����ִ��� Ȯ�� �� ��Ż ����
    public void CheckAreaForItemsAndSpawnPortal(Vector3 centerPosition, float checkDelay, string targetStageID)
    {
        if (!isCheckingForItems)
        {
            StartCoroutine(CheckItemsWithDelay(centerPosition, checkDelay, targetStageID));
        }
    }

    private IEnumerator CheckItemsWithDelay(Vector3 centerPosition, float checkDelay, string targetStageID)
    {
        isCheckingForItems = true;

        // �ణ�� ����
        yield return new WaitForSeconds(checkDelay);

        // �ش� ������ �������� �ִ��� Ȯ��
        Collider[] hitColliders = Physics.OverlapSphere(centerPosition, detectionRadius, itemLayer);

        if (hitColliders.Length == 0)
        {
            // �������� ������ ��Ż ����
            SpawnPortal(centerPosition, targetStageID);
        }
        else
        {
            Debug.Log($"���� ȹ������ ���� �������� {hitColliders.Length}�� �����ֽ��ϴ�. ��Ż ���� �����...");

            // �������� ������ ���� �ð� �� �ٽ� Ȯ��
            yield return new WaitForSeconds(3f);
            isCheckingForItems = false;
            CheckAreaForItemsAndSpawnPortal(centerPosition, 0.1f, targetStageID);
        }
    }

    // ��Ż ��Ȱ��ȭ
    public void HidePortal()
    {
        if (currentPortal != null)
        {
            currentPortal.transform.DOScale(Vector3.zero, 0.5f).OnComplete(() => {
                Destroy(currentPortal);
                currentPortal = null;
            });
        }
    }
}