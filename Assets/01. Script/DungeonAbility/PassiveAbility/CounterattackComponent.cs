using UnityEngine;

public class CounterattackComponent : MonoBehaviour
{
    private float counterDamageAmount = 0f;
    private PlayerClass playerClass;

    private void Awake()
    {
        playerClass = GameInitializer.Instance.GetPlayerClass();

        if (playerClass == null)
        {
            Debug.LogError("CounterattackComponent: PlayerClass를 찾을 수 없습니다.");
            Destroy(this);
            return;
        }

        // 이벤트에 리스너 등록
        playerClass.OnDamageReceived += OnPlayerDamageReceived;
    }

    private void OnDestroy()
    {
        // 이벤트 리스너 제거
        if (playerClass != null)
        {
            playerClass.OnDamageReceived -= OnPlayerDamageReceived;
        }
    }

    // 데미지 이벤트 핸들러
    private void OnPlayerDamageReceived(int damage, MonoBehaviour attacker)
    {
        if (counterDamageAmount <= 0f || attacker == null)
            return;

        // 입은 데미지의 일정 비율로 반격
        int counterDamage = Mathf.RoundToInt(damage * counterDamageAmount);
        counterDamage = Mathf.Max(1, counterDamage); // 최소 1 데미지

        // 공격자에게 데미지 적용
        IDamageable damageable = attacker as IDamageable;
        if (damageable != null)
        {
            damageable.TakeDamage(counterDamage);
            Debug.Log($"반격 발동: {counterDamage} 데미지");
        }
    }

    // 반격 데미지 증가
    public void AddCounterDamage(float amount)
    {
        counterDamageAmount += amount;
    }

    // 반격 데미지 감소
    public void RemoveCounterDamage(float amount)
    {
        counterDamageAmount -= amount;
        counterDamageAmount = Mathf.Max(0f, counterDamageAmount);
    }

    // 현재 반격 데미지 비율 반환
    public float GetCounterDamageAmount()
    {
        return counterDamageAmount;
    }
}