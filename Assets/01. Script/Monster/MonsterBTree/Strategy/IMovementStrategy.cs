// 이동 전략
using UnityEngine;

public interface IMovementStrategy
{
    void Move(Transform transform, Transform target, MonsterClass monsterData);
    void StartMoving(Transform transform);  // 이동 시작시 호출
    void StopMoving();  // 이동 중단시 호출
    bool ShouldChangeState(float distanceToTarget, MonsterClass monsterData);
}