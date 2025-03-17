using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class PortalManager : MonoBehaviour
{
    public static PortalManager Instance { get; private set; }

    [Header("포탈 설정")]
    [SerializeField] private GameObject portalPrefab;
    [SerializeField] private float portalAppearDuration = 1.5f;
    [SerializeField] private float portalRotationSpeed = 30f;

    [Header("이펙트 설정")]
    [SerializeField] private GameObject portalAppearEffectPrefab;
    [SerializeField] private GameObject portalActiveEffectPrefab;

    [Header("탐지 설정")]
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

    // 포탈 생성
    public void SpawnPortal(Vector3 position, string targetStageID)
    {
        if (currentPortal != null)
        {
            Destroy(currentPortal);
        }

        StartCoroutine(SpawnPortalWithEffect(position, targetStageID));
    }

    // 포탈 생성 애니메이션
    private IEnumerator SpawnPortalWithEffect(Vector3 position, string targetStageID)
    {
        // 이펙트 생성
        if (portalAppearEffectPrefab != null)
        {
            Instantiate(portalAppearEffectPrefab, position, Quaternion.identity);
        }

        yield return new WaitForSeconds(2f);

        // 포탈 생성
        currentPortal = Instantiate(portalPrefab, position, Quaternion.identity);

        // 포탈 크기 애니메이션
        currentPortal.transform.localScale = Vector3.zero;
        currentPortal.transform.DOScale(Vector3.one, portalAppearDuration)
            .SetEase(Ease.OutBack);

        // 회전 애니메이션 적용
        StartCoroutine(RotatePortal(currentPortal.transform));

        // 포탈 컴포넌트 설정
        StagePortal portalComponent = currentPortal.GetComponent<StagePortal>();
        if (portalComponent != null)
        {
            portalComponent.targetStageID = targetStageID; 
        }
        else
        {
            Debug.LogError("생성된 포탈에 DungeonPortal 컴포넌트가 없습니다.");
        }

        // 액티브 이펙트 생성
        if (portalActiveEffectPrefab != null)
        {
            GameObject activeEffect = Instantiate(portalActiveEffectPrefab, position, Quaternion.identity);
            activeEffect.transform.SetParent(currentPortal.transform);
        }
    }

    // 포탈 회전 애니메이션
    private IEnumerator RotatePortal(Transform portalTransform)
    {
        while (portalTransform != null)
        {
            portalTransform.Rotate(Vector3.up, portalRotationSpeed * Time.deltaTime);
            yield return null;
        }
    }

    // 아이템이 남아있는지 확인 후 포탈 생성
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

        // 약간의 지연
        yield return new WaitForSeconds(checkDelay);

        // 해당 영역에 아이템이 있는지 확인
        Collider[] hitColliders = Physics.OverlapSphere(centerPosition, detectionRadius, itemLayer);

        if (hitColliders.Length == 0)
        {
            // 아이템이 없으면 포탈 생성
            SpawnPortal(centerPosition, targetStageID);
        }
        else
        {
            Debug.Log($"아직 획득하지 않은 아이템이 {hitColliders.Length}개 남아있습니다. 포탈 생성 대기중...");

            // 아이템이 있으면 일정 시간 후 다시 확인
            yield return new WaitForSeconds(3f);
            isCheckingForItems = false;
            CheckAreaForItemsAndSpawnPortal(centerPosition, 0.1f, targetStageID);
        }
    }

    // 포탈 비활성화
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