using UnityEngine;

public interface IDieStrategy
{
    void OnDie(Transform transform, MonsterClass monsterData);
    bool IsDeathComplete { get; }
    void UpdateDeath();
}