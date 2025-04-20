// 스테이지 클리어 시 체력 회복 컴포넌트
using UnityEngine;

public class StageHealComponent : MonoBehaviour
{
    private float healPercent = 0f;
    private PlayerClass playerClass;

    private void Awake()
    {
        playerClass = GameInitializer.Instance.GetPlayerClass();
        if (playerClass == null)
        {
            Debug.LogError("StageHealComponent: 플레이어 클래스를 찾을 수 없습니다.");
            Destroy(this);
            return;
        }

        // 스테이지 클리어 이벤트 구독
        DungeonManager.OnStageCleared += HandleStageCleared;
        Debug.Log("스테이지 회복 컴포넌트: 이벤트 구독 완료");
    }

    // 스테이지 클리어 시 호출되는 이벤트 핸들러
    private void HandleStageCleared()
    {
        if (healPercent > 0f && playerClass != null)
        {
            // 최대 체력의 비율만큼 회복
            int maxHealth = playerClass.PlayerStats.MaxHealth;
            int healthToRestore = Mathf.RoundToInt(maxHealth * healPercent);

            // 최소 회복량 설정 (1 이상)
            healthToRestore = Mathf.Max(1, healthToRestore);

            // 플레이어 체력 회복
            playerClass.ModifyPower(healthToRestore);
            Debug.Log($"스테이지 클리어 회복 발동: 최대 체력 {maxHealth}의 {healPercent * 100f}%인 {healthToRestore} 회복");

            // 필요하다면 여기에 회복 이펙트 추가
            // PlayHealEffect();
        }
    }

    private void OnDestroy()
    {
        // 이벤트 구독 해제
        DungeonManager.OnStageCleared -= HandleStageCleared;
        Debug.Log("스테이지 회복 컴포넌트: 이벤트 구독 해제");
    }

    // 회복 퍼센트 추가
    public void AddHealPercent(float percent)
    {
        healPercent += percent;
        Debug.Log($"스테이지 클리어 회복 능력 추가: 현재 {healPercent * 100f}%");
    }

    // 회복 퍼센트 감소
    public void RemoveHealPercent(float percent)
    {
        healPercent -= percent;
        healPercent = Mathf.Max(0f, healPercent);
        Debug.Log($"스테이지 클리어 회복 능력 감소: 현재 {healPercent * 100f}%");
    }

    // 현재 회복 퍼센트 반환
    public float GetHealPercent()
    {
        return healPercent;
    }
}