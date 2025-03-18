// 흡혈 처리 컴포넌트
using UnityEngine;

public class LifeStealComponent : MonoBehaviour
{
    private float lifeStealAmount = 0f;
    private PlayerClass playerClass;
    private CharacterAttackBase attackComponent;

    private void Awake()
    {
        playerClass = GetComponent<PlayerClass>();
        attackComponent = GetComponent<CharacterAttackBase>();

        if (playerClass == null || attackComponent == null)
        {
            Debug.LogError("LifeStealComponent: 필요한 컴포넌트가 없습니다.");
            Destroy(this);
            return;
        }

        // 공격에 리스너 추가
        PlayerAttack playerAttack = GetComponent<PlayerAttack>();
        if (playerAttack != null)
        {
            playerAttack.OnAttackInput += OnPlayerAttack;
        }
    }

    private void OnDestroy()
    {
        // 리스너 제거
        PlayerAttack playerAttack = GetComponent<PlayerAttack>();
        if (playerAttack != null)
        {
            playerAttack.OnAttackInput -= OnPlayerAttack;
        }
    }

    // 플레이어 공격 이벤트 핸들러
    private void OnPlayerAttack(AttackData.AttackType attackType)
    {
        // 능력이 활성화되어 있고 플레이어가 유효한 상태일 때만 처리
        if (lifeStealAmount > 0f && playerClass != null && playerClass.GetCurrentWeapon() != null)
        {
            // 기본 무기 데미지 기반으로 계산
            int weaponDamage = playerClass.PlayerStats.AttackPower;

            // 치명타 확률 고려 (선택 사항)
            bool isCritical = UnityEngine.Random.value < playerClass.PlayerStats.CriticalChance / 100f;
            int damageDealt = isCritical ? weaponDamage * 2 : weaponDamage;

            // 흡혈 효과로 회복될 체력 계산
            int healthToRestore = Mathf.RoundToInt(damageDealt * lifeStealAmount);

            // 최소 회복량 설정 (1)
            healthToRestore = Mathf.Max(1, healthToRestore);

            // 플레이어 체력 회복
            if (healthToRestore > 0)
            {
                playerClass.ModifyPower(healthToRestore);
                Debug.Log($"흡혈 발동: {healthToRestore} 체력 회복");
            }
        }
    }

    // 흡혈량 추가
    public void AddLifeSteal(float amount)
    {
        lifeStealAmount += amount;
    }

    // 흡혈량 감소
    public void RemoveLifeSteal(float amount)
    {
        lifeStealAmount -= amount;
        lifeStealAmount = Mathf.Max(0f, lifeStealAmount);
    }

    // 현재 흡혈량 반환
    public float GetLifeStealAmount()
    {
        return lifeStealAmount;
    }
}