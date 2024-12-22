using UnityEngine;

public interface ISpawnStrategy
{
    void OnSpawn(Transform transform, MonsterClass monsterData);
    bool IsSpawnComplete { get; }
}