using UnityEngine;

public interface IGroggyStrategy
{
    void OnGroggy(Transform transform, IMonsterClass monsterData);
    bool IsGroggyComplete { get; }
    void UpdateGroggy();
}