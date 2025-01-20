using UnityEngine;

public interface ISpawnStrategy
{
    void OnSpawn(Transform transform, IMonsterClass monsterData);
    bool IsSpawnComplete { get; }
}