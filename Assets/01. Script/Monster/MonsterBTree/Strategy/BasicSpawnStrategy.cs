using UnityEngine;

public class BasicSpawnStrategy : ISpawnStrategy
{
    private bool spawnComplete;
    private float spawnDuration;

    public bool IsSpawnComplete => spawnComplete;

    public void OnSpawn(Transform transform, MonsterClass monsterData)
    {
        spawnDuration = monsterData.CurrentSpawnDuration;
        // 기본 스폰 로직
        // 나중에 스폰 애니메이션이나 이펙트 추가 가능
        spawnComplete = true;
    }
}
