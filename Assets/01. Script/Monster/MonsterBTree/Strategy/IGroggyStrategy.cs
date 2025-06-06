using UnityEngine;

public interface IGroggyStrategy
{
    void OnGroggy(Transform transform, IMonsterClass monsterData);
    bool IsGroggyComplete { get; }
    float GroggyDuration { get; }
    void UpdateGroggy();
}