using UnityEngine;

public interface IIdleStrategy
{
    void OnIdle(Transform transform, IMonsterClass monsterData);
    bool ShouldChangeState(float distanceToPlayer, IMonsterClass monsterData);
    void UpdateIdle();
}