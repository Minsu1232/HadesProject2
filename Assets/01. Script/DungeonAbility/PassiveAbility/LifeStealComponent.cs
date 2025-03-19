// 흡혈 처리 컴포넌트
using System;
using UnityEngine;

public class LifeStealComponent : MonoBehaviour
{
    private float lifeStealAmount = 0f;
    private PlayerClass playerClass;
    private IWeaponDamageDealer damageDealer;

    private void Awake()
    {
        playerClass = GameInitializer.Instance.GetPlayerClass();
        // 자식 오브젝트 중에서 IWeaponDamageDealer 찾기
        damageDealer = GetComponentInChildren<IWeaponDamageDealer>();

        if (playerClass == null || damageDealer == null)
        {
            Debug.LogError("LifeStealComponent: 필요한 컴포넌트가 없습니다.");
            Destroy(this);
            return;
        }

        // 무기 데미지 딜러의 최종 데미지 계산 이벤트 구독
        if (damageDealer != null)
        {
            // MeleeDamageDealer에서 발생하는 이벤트 구독
            damageDealer.OnFinalDamageCalculated += HandleFinalDamageCalculated;
            Debug.Log("흡혈 컴포넌트: 데미지 이벤트 구독 완료");
        }
    }

    // 최종 데미지 계산 시 호출되는 이벤트 핸들러
    private void HandleFinalDamageCalculated(int finalDamage, ICreatureStatus targetMonster)
    {
        // 능력이 활성화되어 있고 플레이어가 유효한 상태일 때만 처리
        if (lifeStealAmount > 0f && playerClass != null)
        {
            // 흡혈 효과로 회복될 체력 계산 (최종 데미지 기준)
            int healthToRestore = Mathf.RoundToInt(finalDamage * lifeStealAmount);

            // 최소 회복량 설정 (0 이상)
            healthToRestore = Mathf.Max(0, healthToRestore);

            // 플레이어 체력 회복
            if (healthToRestore > 0)
            {
                playerClass.ModifyPower(healthToRestore);
                Debug.Log($"흡혈 발동: 데미지 {finalDamage}에서 {healthToRestore} 체력 회복");

                // 필요하다면 여기에 흡혈 이펙트 추가
                // PlayLifeStealEffect();
            }
        }
    }

    private void OnDestroy()
    {
        // 이벤트 구독 해제
        if (damageDealer != null)
        {
            damageDealer.OnFinalDamageCalculated -= HandleFinalDamageCalculated;
            Debug.Log("흡혈 컴포넌트: 이벤트 구독 해제");
        }
    }

    // 흡혈량 추가
    public void AddLifeSteal(float amount)
    {
        lifeStealAmount += amount;
        Debug.Log($"흡혈 능력 추가: 현재 {lifeStealAmount * 100f}%");
    }

    // 흡혈량 감소
    public void RemoveLifeSteal(float amount)
    {
        lifeStealAmount -= amount;
        lifeStealAmount = Mathf.Max(0f, lifeStealAmount);
        Debug.Log($"흡혈 능력 감소: 현재 {lifeStealAmount * 100f}%");
    }

    // 현재 흡혈량 반환
    public float GetLifeStealAmount()
    {
        return lifeStealAmount;
    }

    // 흡혈량 설정
    public void SetLifeStealAmount(float amount)
    {
        lifeStealAmount = Mathf.Max(0f, amount);
        Debug.Log($"흡혈 능력 설정: {lifeStealAmount * 100f}%");
    }
}