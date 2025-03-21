// WeakPreyComponent.cs - 약자 공격 컴포넌트
using UnityEngine;

public class WeakPreyComponent : MonoBehaviour
{
    private float damageBonus = 0f; // 추가 데미지 비율 (1.0 = 100% 추가)
    private float healthThreshold = 0.3f; // 체력 임계점 (기본 30% 이하)
    private PlayerClass playerClass;
    private IDamageDealer damageDealer;

    private void Awake()
    {
        playerClass = GameInitializer.Instance.GetPlayerClass();
        if (playerClass == null)
        {
            Debug.LogError("WeakPreyComponent: PlayerClass를 찾을 수 없습니다.");
            Destroy(this);
            return;
        }

        // 플레이어의 무기에서 IDamageDealer 찾기
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
            Debug.Log($"약자 공격 컴포넌트: {damageDealer.GetType().Name} 데미지 이벤트 구독 완료");
        }
        else
        {
            Debug.LogWarning("WeakPreyComponent: IDamageDealer를 찾을 수 없습니다.");
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
        if (target == null || damageBonus <= 0f)
            return;

        // 타겟의 현재 체력 비율 확인
        float healthRatio = GetTargetHealthRatio(target);

        // 체력이 임계치 이하면 추가 데미지 적용
        if (healthRatio <= healthThreshold)
        {
            int bonusDamage = Mathf.RoundToInt(finalDamage * damageBonus);

            // 추가 데미지 적용
            if (bonusDamage > 0 && target is IDamageable damageable)
            {
                damageable.TakeDamage(bonusDamage);
                Debug.Log($"약자 공격: 기본 데미지 {finalDamage} + 추가 데미지 {bonusDamage} = 총 {finalDamage + bonusDamage} (타겟 체력 비율: {healthRatio * 100}%)");
            }
        }
    }

    // 타겟의 현재 체력 비율 계산
    private float GetTargetHealthRatio(ICreatureStatus target)
    {
        // MonsterClass나 다른 ICreatureStatus 구현체에서 체력 정보 가져오기
        if (target != null && target.GetMonsterClass() != null)
        {
            var monsterClass = target.GetMonsterClass();
            int currentHealth = monsterClass.CurrentHealth;
            int maxHealth = monsterClass.MaxHealth;

            if (maxHealth > 0)
            {
                return (float)currentHealth / maxHealth;
            }
        }

        return 1.0f; // 기본값: 풀 체력으로 가정
    }

    // 추가 데미지 설정
    public void AddDamageBonus(float amount)
    {
        damageBonus += amount;
        Debug.Log($"약자 공격 효과 추가: +{amount * 100}%, 현재: {damageBonus * 100}%");

        // 아직 IDamageDealer를 찾지 못했다면 다시 시도
        if (damageDealer == null)
        {
            FindDamageDealer();
        }
    }

    // 체력 임계치 설정 (희귀도가 높은 능력일수록 임계치도 높게 설정)
    public void SetHealthThreshold(float threshold)
    {
        healthThreshold = threshold;
        Debug.Log($"약자 공격 체력 임계치 설정: {threshold * 100}% 이하");
    }

    // 추가 데미지 감소
    public void RemoveDamageBonus(float amount)
    {
        damageBonus -= amount;
        damageBonus = Mathf.Max(0f, damageBonus); // 음수 방지
        Debug.Log($"약자 공격 효과 감소: -{amount * 100}%, 현재: {damageBonus * 100}%");
    }

    // 현재 추가 데미지 반환
    public float GetDamageBonus()
    {
        return damageBonus;
    }
}