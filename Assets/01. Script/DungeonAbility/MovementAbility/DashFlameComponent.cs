// DashFlameComponent.cs - 대시 경로에 따른 불꽃 생성 컴포넌트 (완성본)
using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class DashFlameComponent : MonoBehaviour
{
    private float damageMultiplier = 0f;    // 플레이어 공격력 대비 데미지 비율
    private PlayerClass playerClass;
    private PlayerDashComponent dashComponent;
    private Rarity flameRarity = Rarity.Common; // 기본 희귀도

    [SerializeField] private float flameDuration = 3f;      // 불꽃 지속 시간
    [SerializeField] private float flameRadius = 2f;        // 불꽃 효과 반경
    [SerializeField] private float flameHeight = 0.1f;     // 불꽃이 바닥에서 얼마나 떠 있을지

    private Vector3 dashStartPosition;      // 대시 시작 위치
    private List<GameObject> activeFlames = new List<GameObject>();
    // 불꽃 이펙트 프리팹
    private GameObject flameEffectPrefab;

    private void Start()
    {
        playerClass = GameInitializer.Instance.GetPlayerClass();
        dashComponent = GetComponent<PlayerDashComponent>();

        // 필요한 참조 체크
        if (playerClass == null || dashComponent == null)
        {
            Debug.LogError("DashFlameComponent: 필요한 컴포넌트가 없습니다.");
            enabled = false;
            return;
        }

        // 불꽃 프리팹 참조
        if (GameInitializer.Instance.flameWall != null)
        {
            flameEffectPrefab = GameInitializer.Instance.flameWall;
        }

        // 이벤트 구독
        dashComponent.OnDashStart.AddListener(OnDashStart);
        dashComponent.OnDashEnd.AddListener(OnDashEnd);
    }

    private void OnDestroy()
    {
        // 이벤트 구독 해제
        if (dashComponent != null)
        {
            dashComponent.OnDashStart.RemoveListener(OnDashStart);
            dashComponent.OnDashEnd.RemoveListener(OnDashEnd);
        }

        // 활성화된 불꽃 제거
        foreach (var flame in activeFlames)
        {
            if (flame != null)
            {
                Destroy(flame);
            }
        }
        activeFlames.Clear();
    }

    // 대시 시작 시 위치 저장
    private void OnDashStart()
    {
        dashStartPosition = transform.position;
    }

    // 대시 종료 시 불꽃 생성
    private void OnDashEnd()
    {
        if (damageMultiplier > 0f && flameEffectPrefab != null)
        {
            CreateFlame();
        }
    }

    // 대시 경로에 불꽃 생성
    private void CreateFlame()
    {
        Vector3 dashEndPosition = transform.position;
        Vector3 dashVector = dashEndPosition - dashStartPosition;

        // y축 성분 제거
        dashVector.y = 0;

        // 너무 짧은 대시는 무시
        float dashDistance = dashVector.magnitude;
        if (dashDistance < 0.5f)
            return;

        // 대시 방향 계산 (XZ 평면에서)
        Vector3 dashDirection = dashVector.normalized;

        // 불꽃 생성 위치 (대시 경로의 중간)
        Vector3 flamePosition = dashStartPosition + (dashVector * 0.5f);
        flamePosition.y += flameHeight; // 바닥에서 약간 위로 올림

        // Quaternion 회전 - 모델의 방향에 맞게 90도 회전 적용
        Quaternion rotation = Quaternion.LookRotation(dashDirection) * Quaternion.Euler(0, 90, 0);

        GameObject flameObj = Instantiate(flameEffectPrefab, flamePosition, rotation);

        // 기본 스케일 가져오기 (프리팹의 원본 크기)
        Vector3 originalScale = flameEffectPrefab.transform.localScale;

        // 스케일 계산 (대시 거리의 50%로 조정)
        float scaleFactor = dashDistance * 0.5f / originalScale.x;  // X축이 전방 방향이라고 가정

        // 새 스케일 적용 - X축으로 스케일링
        Vector3 newScale = new Vector3(
            scaleFactor * originalScale.x,
            originalScale.y,
           scaleFactor* originalScale.z
        );

        // 스케일 적용
        flameObj.transform.localScale = newScale;

        // 희귀도에 따른 색상 설정
        SetFlameColorByRarity(flameObj, flameRarity);

        // 데미지 계산
        float damage = playerClass.PlayerStats.AttackPower * damageMultiplier;

        // 불꽃 데미지 영역 컴포넌트 추가
        FlameAreaEffect flameEffect = flameObj.AddComponent<FlameAreaEffect>();
        flameEffect.Initialize(damage, flameDuration, dashDistance / 2.0f);

        // 활성화된 불꽃 목록에 추가
        activeFlames.Add(flameObj);

        // 지속 시간 후 제거
        StartCoroutine(RemoveFlameAfterDuration(flameObj, flameDuration));

        Debug.Log($"대시 불꽃 생성: 방향 {dashDirection}, 거리 {dashDistance}m, 데미지 {damage}, 희귀도: {flameRarity}");
    }

    // 지속 시간 후 불꽃 제거
    private IEnumerator RemoveFlameAfterDuration(GameObject flame, float duration)
    {
        yield return new WaitForSeconds(duration);
        if (activeFlames.Contains(flame))
        {
            activeFlames.Remove(flame);
        }
        Destroy(flame);
    }

    // 희귀도에 따른 색상 설정 (온도 기반)
    private void SetFlameColorByRarity(GameObject flameObj, Rarity rarity)
    {
        // 온도 기반 색상 (낮은 희귀도 = 낮은 온도, 높은 희귀도 = 높은 온도)
        Color flameColor;
        Debug.Log($"현재 불 희귀도 {rarity}");
        switch (rarity)
        {
            case Rarity.Common:
                flameColor = new Color(1f, 0.3f, 0f, 0.7f); // 붉은 주황색 (낮은 온도)
                break;
            case Rarity.Uncommon:
                flameColor = new Color(1f, 0.7f, 0f, 0.7f); // 주황색
                break;
            case Rarity.Rare:
                flameColor = new Color(1f, 0.9f, 0.2f, 0.7f); // 노란색
                break;
            case Rarity.Epic:
                flameColor = new Color(0.9f, 0.9f, 0.9f, 0.7f); // 백색
                break;
            case Rarity.Legendary:
                flameColor = new Color(0.3f, 0.5f, 1f, 0.7f); // 파란색 (가장 높은 온도)
                break;
            default:
                flameColor = new Color(1f, 0.5f, 0f, 0.7f); // 기본 주황색
                break;
        }

        // 파티클 시스템 찾기
        ParticleSystem[] particleSystems = flameObj.GetComponentsInChildren<ParticleSystem>();
        foreach (ParticleSystem ps in particleSystems)
        {
            var main = ps.main;
            main.startColor = flameColor;
        }
    }

    // 데미지 비율 설정 (공격력 대비 비율)
    public void SetDamageMultiplier(float multiplier)
    {
        damageMultiplier = Mathf.Max(0f, multiplier);
        Debug.Log($"대시 불꽃 데미지 설정: 공격력의 {damageMultiplier * 100}%");
    }

    // 현재 데미지 비율 반환
    public float GetDamageMultiplier()
    {
        return damageMultiplier;
    }

    // 불꽃 지속 시간 설정
    public void SetFlameDuration(float duration)
    {
        flameDuration = Mathf.Max(1f, duration);
    }

    // 불꽃 반경 설정
    public void SetFlameRadius(float radius)
    {
        flameRadius = Mathf.Max(1f, radius);
    }

    // 희귀도 설정 메서드
    public void SetFlameRarity(Rarity rarity)
    {
        flameRarity = rarity;
        Debug.Log($"대시 불꽃 희귀도 설정: {rarity}");
    }
}