using UnityEngine;

public interface IDieStrategy
{
    void OnDie(Transform transform, IMonsterClass monsterData);
    bool IsDeathComplete { get; }
    void UpdateDeath();
}