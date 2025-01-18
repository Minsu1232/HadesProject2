using UnityEngine;

public interface IGroggyStrategy
{
    void OnGroggy(Transform transform, MonsterClass monsterData);
    bool IsGroggyComplete { get; }
    void UpdateGroggy();
}