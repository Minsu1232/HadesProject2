// SpecialResourceRetentionComponent.cs - 게이지 보존 컴포넌트
using UnityEngine;

public class SpecialResourceRetentionComponent : MonoBehaviour
{
    private float retentionRate = 0f; // 게이지 보존율 (1.0 = 100% 보존)
    private PlayerClass playerClass;

    private void Awake()
    {
        playerClass = GameInitializer.Instance.GetPlayerClass();
        if (playerClass == null)
        {
            Debug.LogError("SpecialResourceRetentionComponent: PlayerClass를 찾을 수 없습니다.");
            Destroy(this);
            return;
        }

        // MonoBehaviour 시작 시 WeaponManager에 게이지 보존율 설정
        SetWeaponRetentionRate();
    }

    // 무기에 게이지 보존율 설정
    private void SetWeaponRetentionRate()
    {
        // CharacterAttackBase를 통해 현재 무기에 접근
        CharacterAttackBase characterAttack = GetComponent<CharacterAttackBase>();
        if (characterAttack != null && characterAttack.currentWeapon != null)
        {
            // IWeapon 인터페이스를 통해 게이지 보존율 설정
            characterAttack.currentWeapon.SetGageRetentionRate(retentionRate);
            Debug.Log($"현재 무기에 게이지 보존율 {retentionRate * 100}% 설정됨");
        }
    }

    // 보존율 추가
    public void AddRetentionRate(float rate)
    {
        retentionRate += rate;
        retentionRate = Mathf.Clamp01(retentionRate); // 0.0-1.0 범위로 제한
        SetWeaponRetentionRate(); // 새 보존율 적용
        Debug.Log($"게이지 보존율 증가: +{rate * 100}%, 현재: {retentionRate * 100}%");
    }

    // 보존율 감소
    public void RemoveRetentionRate(float rate)
    {
        retentionRate -= rate;
        retentionRate = Mathf.Max(0f, retentionRate); // 음수 방지
        SetWeaponRetentionRate(); // 새 보존율 적용
        Debug.Log($"게이지 보존율 감소: -{rate * 100}%, 현재: {retentionRate * 100}%");
    }

    // 현재 보존율 반환
    public float GetRetentionRate()
    {
        return retentionRate;
    }
}