// GaugeBoostComponent.cs
using UnityEngine;

public class GaugeBoostComponent : MonoBehaviour
{
    private float gaugeBoost = 0f; // 게이지 충전량 증가 (1.0 = 100% 증가)
    private PlayerClass playerClass;
    private WeaponBase weaponBase;

    private void Awake()
    {
        playerClass = GameInitializer.Instance.GetPlayerClass();
        if (playerClass == null)
        {
            Debug.LogError("GaugeBoostComponent: PlayerClass를 찾을 수 없습니다.");
            Destroy(this);
            return;
        }

        // 현재 무기 찾기
        FindWeapon();
    }

    private void OnEnable()
    {
        // 컴포넌트가 활성화될 때마다 무기 찾기 시도
        FindWeapon();
    }

    // 무기 찾기 메서드
    private void FindWeapon()
    {
        // 현재 플레이어가 들고 있는 무기 찾기
        weaponBase = playerClass.GetCurrentWeapon() as WeaponBase;

        if (weaponBase != null)
        {
            // 무기 찾았을 때 처리
            Debug.Log($"게이지 강화 컴포넌트: {weaponBase.GetType().Name} 무기 참조 완료");

            // 게이지 충전량 증가 적용
            ApplyGaugeBoost();
        }
        else
        {
            Debug.LogWarning("GaugeBoostComponent: WeaponBase를 찾을 수 없습니다.");
        }
    }

    // 게이지 충전량 증가 적용 메서드
    private void ApplyGaugeBoost()
    {
        if (weaponBase != null)
        {
            // WeaponBase에 추가된 SetGaugeChargeMultiplier 메서드를 사용하여 게이지 충전량 배율 설정
            float multiplier = 1.0f + gaugeBoost;
            weaponBase.SetGaugeChargeMultiplier(multiplier);

            Debug.Log($"게이지 충전량 증가 적용: 배율 {multiplier}x (기본 + {gaugeBoost * 100}%)");
        }
    }

    // 게이지 충전량 증가치 추가
    public void AddGaugeBoost(float amount)
    {
        gaugeBoost += amount;
        Debug.Log($"게이지 충전량 효과 추가: +{amount * 100}%, 현재: {gaugeBoost * 100}%");

        // 무기가 없다면 다시 찾기
        if (weaponBase == null)
        {
            FindWeapon();
        }
        else
        {
            ApplyGaugeBoost();
        }
    }

    // 게이지 충전량 증가치 감소
    public void RemoveGaugeBoost(float amount)
    {
        gaugeBoost -= amount;
        gaugeBoost = Mathf.Max(0f, gaugeBoost); // 음수 방지
        Debug.Log($"게이지 충전량 효과 감소: -{amount * 100}%, 현재: {gaugeBoost * 100}%");

        // 효과 다시 적용
        ApplyGaugeBoost();
    }

    // 현재 게이지 충전량 증가치 반환
    public float GetGaugeBoost()
    {
        return gaugeBoost;
    }
}