using UnityEngine;

public class BasicIdleStrategy : IIdleStrategy
{
    private float detectionCheckInterval = 0.5f;  // 감지 체크 주기
    private float nextDetectionCheck;

    public void OnIdle(Transform transform, MonsterClass monsterData)
    {
        nextDetectionCheck = Time.time + detectionCheckInterval;
    }

    public void UpdateIdle()
    {
        // 대기 상태 업데이트
        // 나중에 Idle 애니메이션이나 대기 모션 추가 가능
    }

    public bool ShouldChangeState(float distanceToPlayer, MonsterClass monsterData)
    {
        if (Time.time >= nextDetectionCheck)
        {
            nextDetectionCheck = Time.time + detectionCheckInterval;

            // 플레이어가 감지 범위 안에 들어오면 상태 전환
            if (distanceToPlayer <= monsterData.CurrentChaseRange)
            {
                return true;
            }
        }
        return false;
    }
}