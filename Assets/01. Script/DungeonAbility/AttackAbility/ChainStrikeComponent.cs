// ChainStrikeComponent.cs
using UnityEngine;
using System.Collections.Generic;

public class ChainStrikeComponent : MonoBehaviour
{
    private float chainChance = 0f; // 연쇄 공격 확률 (1.0 = 100%)
    private PlayerClass playerClass;
    private IDamageDealer damageDealer;
    private List<ICreatureStatus> recentTargets = new List<ICreatureStatus>(); // 최근 타겟 추적

    private void Awake()
    {
        playerClass = GameInitializer.Instance.GetPlayerClass();
        if (playerClass == null)
        {
            Debug.LogError("ChainStrikeComponent: PlayerClass를 찾을 수 없습니다.");
            Destroy(this);
            return;
        }

        // 데미지 딜러 찾기
        FindDamageDealer();
    }

    private void OnDestroy()
    {
        // 이벤트 구독 해제
        UnregisterEvents();
    }

    // IDamageDealer 찾고 이벤트 등록
    private void FindDamageDealer()
    {
        // 기존 이벤트 해제
        UnregisterEvents();

        // 플레이어 게임오브젝트의 자식에서 IDamageDealer 찾기
        damageDealer = GameInitializer.Instance.gameObject.GetComponentInChildren<IDamageDealer>();

        if (damageDealer != null)
        {
            // 이벤트 구독
            damageDealer.OnFinalDamageCalculated += OnDamageCalculated;
            Debug.Log($"연쇄 타격 컴포넌트: {damageDealer.GetType().Name} 데미지 이벤트 구독 완료");
        }
        else
        {
            Debug.LogWarning("ChainStrikeComponent: IDamageDealer를 찾을 수 없습니다.");
        }
    }

    // 이벤트 구독 해제
    private void UnregisterEvents()
    {
        if (damageDealer != null)
        {
            damageDealer.OnFinalDamageCalculated -= OnDamageCalculated;
        }
    }

    // 데미지 계산 시 호출되는 이벤트 핸들러
    private void OnDamageCalculated(int finalDamage, ICreatureStatus target)
    {
        if (target == null || chainChance <= 0f)
            return;

        // 연쇄 공격 확률 체크
        if (Random.value <= chainChance)
        {
            // 같은 몬스터에게 동일한 데미지를 한 번 더 가함
            if (target is IDamageable damageable)
            {
                int chainDamage = Mathf.RoundToInt(finalDamage * 0.5f);
                damageable.TakeDamage(chainDamage);
                Debug.Log($"추가 타격 발동: 기본 데미지 {finalDamage} + 추가 데미지 {finalDamage} = 총 {finalDamage * 2}");
            }
        }
    }

    // 연쇄 확률 추가
    public void AddChainChance(float amount)
    {
        chainChance += amount;
        Debug.Log($"추가 타격 효과 추가: +{amount * 100}%, 현재: {chainChance * 100}%");

        // IDamageDealer를 찾지 못했다면 다시 시도
        if (damageDealer == null)
        {
            FindDamageDealer();
        }
    }

    // 연쇄 확률 감소
    public void RemoveChainChance(float amount)
    {
        chainChance -= amount;
        chainChance = Mathf.Max(0f, chainChance); // 음수 방지
        Debug.Log($"추가 타격 효과 감소: -{amount * 100}%, 현재: {chainChance * 100}%");
    }

    // 현재 연쇄 확률 반환
    public float GetChainChance()
    {
        return chainChance;
    }
}