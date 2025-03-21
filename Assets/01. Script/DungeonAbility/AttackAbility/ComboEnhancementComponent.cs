// ComboEnhancementComponent.cs - 콤보 강화 컴포넌트
using UnityEngine;

public class ComboEnhancementComponent : MonoBehaviour
{
    private float enhancementAmount = 0f; // 콤보 데미지 증가량 (1.0 = 100% 증가)
    private PlayerClass playerClass;
    private WeaponBase weaponBase;

    private void Awake()
    {
        playerClass = GameInitializer.Instance.GetPlayerClass();
        if (playerClass == null)
        {
            Debug.LogError("ComboEnhancementComponent: PlayerClass를 찾을 수 없습니다.");
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
            Debug.Log($"콤보 강화 컴포넌트: {weaponBase.GetType().Name} 무기 참조 완료");

            // 콤보 데미지 증폭 배율 적용
            ApplyComboEnhancement();
        }
        else
        {
            Debug.LogWarning("ComboEnhancementComponent: WeaponBase를 찾을 수 없습니다.");
        }
    }

    // 콤보 강화 적용 메서드
    private void ApplyComboEnhancement()
    {
        if (weaponBase != null)
        {
            // WeaponBase에 추가된 SetComboDamageMultiplier 메서드를 사용하여 콤보 데미지 배율 설정
            float multiplier = 1.0f + enhancementAmount; // 기본값(1.0) + 강화량
            weaponBase.SetComboDamageMultiplier(multiplier);

            Debug.Log($"콤보 강화 적용: 배율 {multiplier}x (기본 + {enhancementAmount * 100}%)");
        }
    }

    // 콤보 강화 증가량 추가
    public void AddEnhancement(float amount)
    {
        enhancementAmount += amount;
        Debug.Log($"콤보 강화 효과 추가: +{amount * 100}%, 현재: +{enhancementAmount * 100}%");

        // 무기가 없다면 다시 찾기
        if (weaponBase == null)
        {
            FindWeapon();
        }
        else
        {
            ApplyComboEnhancement();
        }
    }

    // 콤보 강화 증가량 감소
    public void RemoveEnhancement(float amount)
    {
        enhancementAmount -= amount;
        enhancementAmount = Mathf.Max(0f, enhancementAmount); // 음수 방지
        Debug.Log($"콤보 강화 효과 감소: -{amount * 100}%, 현재: +{enhancementAmount * 100}%");

        // 효과 다시 적용
        ApplyComboEnhancement();
    }

    // 현재 콤보 강화 증가량 반환
    public float GetEnhancementAmount()
    {
        return enhancementAmount;
    }
}