using UnityEngine;

public class PassiveAbilityTester : MonoBehaviour
{
    private PlayerClass playerClass;
    private PassiveAbility testAbility;

    void Start()
    {
        // 플레이어 클래스 가져오기
        playerClass = GameInitializer.Instance.GetPlayerClass();

        if (playerClass == null)
        {
            Debug.LogError("플레이어 클래스를 찾을 수 없습니다.");
            return;
        }

        // 테스트용 패시브 능력 생성 (피해 감소 능력)
        testAbility = new PassiveAbility();
        testAbility.Initialize(
            PassiveAbility.PassiveType.DamageReduction,
            20f, // 20% 피해 감소
            "테스트 방어력",
            "피해를 20% 감소시킵니다.",
            Rarity.Common
        );

        // 현재 데미지 계수 로그
        Debug.Log($"패시브 적용 전 데미지 계수: {playerClass.PlayerStats.DamageReceiveRate}");

        // 패시브 적용
        testAbility.OnAcquire(playerClass);

        // 적용 후 데미지 계수 로그
        Debug.Log($"패시브 적용 후 데미지 계수: {playerClass.PlayerStats.DamageReceiveRate}");

        // 5초 후에 패시브 제거 예약
        Invoke("RemovePassive", 5f);
    }

    void RemovePassive()
    {
        // 패시브 제거
        testAbility.OnReset(playerClass);

        // 제거 후 데미지 계수 로그
        Debug.Log($"패시브 제거 후 데미지 계수: {playerClass.PlayerStats.DamageReceiveRate}");
    }
}