using UnityEngine;

public interface IIdleStrategy
{
    void OnIdle(Transform transform, MonsterClass monsterData);
    bool ShouldChangeState(float distanceToPlayer, MonsterClass monsterData);
    void UpdateIdle();
}