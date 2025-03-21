// ArmorCrushComponent.cs
using UnityEngine;

public class ArmorCrushComponent : MonoBehaviour
{
    private float crushAmount = 0f; // 방어력 감소량 (1.0 = 100% 감소)
    private PlayerClass playerClass;
    private IDamageDealer damageDealer;

    private void Awake()
    {
        playerClass = GameInitializer.Instance.GetPlayerClass();
        if (playerClass == null)
        {
            Debug.LogError("ArmorCrushComponent: PlayerClass를 찾을 수 없습니다.");
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
            Debug.Log($"방어력 파괴 컴포넌트: {damageDealer.GetType().Name} 데미지 이벤트 구독 완료");
        }
        else
        {
            Debug.LogWarning("ArmorCrushComponent: IDamageDealer를 찾을 수 없습니다.");
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
        if (target == null || crushAmount <= 0f)
            return;

        // 타겟의 방어력(Defense) 정보 가져오기
        IMonsterClass monster = target.GetMonsterClass();

        if (monster != null)
        {
            // 현재 방어력 가져오기
            int currentDefense = monster.CurrentDeffense;

            if (currentDefense > 0)
            {
                // 방어력 감소량 계산 (현재 방어력의 일정 비율)
                int defenseReduction = Mathf.RoundToInt(currentDefense * crushAmount);

                // 방어력 감소 적용 (ModifyStats 메서드를 통해)
                monster.ModifyStats(defenseAmount: -defenseReduction);

                Debug.Log($"방어력 파괴 발동: 원래 방어력 {currentDefense} → 감소된 방어력 {monster.CurrentDeffense} (감소량: {defenseReduction})");
            }
        }
    }

    // 방어력 감소량 추가
    public void AddCrushAmount(float amount)
    {
        crushAmount += amount;
        Debug.Log($"방어력 파괴 효과 추가: +{amount * 100}%, 현재: {crushAmount * 100}%");

        // IDamageDealer를 찾지 못했다면 다시 시도
        if (damageDealer == null)
        {
            FindDamageDealer();
        }
    }

    // 방어력 감소량 감소
    public void RemoveCrushAmount(float amount)
    {
        crushAmount -= amount;
        crushAmount = Mathf.Max(0f, crushAmount); // 음수 방지
        Debug.Log($"방어력 파괴 효과 감소: -{amount * 100}%, 현재: {crushAmount * 100}%");
    }

    // 현재 방어력 감소량 반환
    public float GetCrushAmount()
    {
        return crushAmount;
    }
}