// DashImpactComponent.cs - 대시 충돌 시 데미지 기능 컴포넌트
using UnityEngine;
using System.Collections.Generic;

public class DashImpactComponent : MonoBehaviour
{
    private float damageMultiplier = 0f;   // 플레이어 공격력 대비 데미지 비율
    private PlayerClass playerClass;
    private PlayerDashComponent dashComponent;

    [SerializeField] private LayerMask monsterLayer; // 몬스터 레이어 마스크
    [SerializeField] private float impactRadius = 1.5f; // 충돌 판정 반경

    private void Awake()
    {
        playerClass = GameInitializer.Instance.GetPlayerClass();
        dashComponent = GetComponent<PlayerDashComponent>();

        if (playerClass == null || dashComponent == null)
        {
            Debug.LogError("DashImpactComponent: 필요한 컴포넌트가 없습니다.");
            enabled = false;
            return;
        }

        // 몬스터 레이어 설정
        if (monsterLayer == 0)
        {
            monsterLayer = LayerMask.GetMask("Monster");
        }

        // 대시 이벤트 구독
        dashComponent.OnDashStart.AddListener(OnDashStart);
    }

    private void OnDestroy()
    {
        // 이벤트 구독 해제
        if (dashComponent != null)
        {
            dashComponent.OnDashStart.RemoveListener(OnDashStart);
        }
    }

    // 대시 시작 시 충돌 체크
    private void OnDashStart()
    {
        if (damageMultiplier > 0f)
        {
            Debug.Log("#@@");
            CheckImpact();
        }
    }

    // 충돌 체크 및 데미지 적용
    private void CheckImpact()
    {
        // 주변 몬스터 탐지
        Collider[] hitColliders = Physics.OverlapSphere(transform.position, impactRadius, monsterLayer);

        // 이미 처리한 몬스터 객체 추적용
        HashSet<ICreatureStatus> processedCreatures = new HashSet<ICreatureStatus>();

        foreach (var hitCollider in hitColliders)
        {
            // 몬스터의 ICreatureStatus 컴포넌트 찾기
            ICreatureStatus creature = hitCollider.GetComponentInParent<ICreatureStatus>();

            // 유효한 몬스터이고 아직 처리하지 않은 경우에만 데미지 적용
            if (creature != null && !processedCreatures.Contains(creature))
            {
                // 처리한 몬스터로 기록
                processedCreatures.Add(creature);

                // 플레이어 공격력 기준 데미지 계산
                int damage = Mathf.RoundToInt(playerClass.PlayerStats.AttackPower * damageMultiplier);

                // 데미지 적용
                creature.TakeDamage(damage);

                Debug.Log($"대시 충격 데미지 적용: {damage} 데미지 (공격력 {playerClass.PlayerStats.AttackPower}의 {damageMultiplier * 100}%)");
            }
        }
    }

    // 충돌 데미지 설정 (공격력 대비 비율)
    public void SetDamageMultiplier(float multiplier)
    {
        damageMultiplier = Mathf.Max(0f, multiplier);
        Debug.Log($"대시 충격 데미지 설정: 공격력의 {damageMultiplier * 100}%");
    }

    // 현재 충돌 데미지 비율 반환
    public float GetDamageMultiplier()
    {
        return damageMultiplier;
    }

    // 충돌 반경 설정
    public void SetImpactRadius(float radius)
    {
        impactRadius = Mathf.Max(0.5f, radius);
    }

    // 디버그용 시각화
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, impactRadius);
    }
}