using UnityEngine;

public class BasicSpawnStrategy : ISpawnStrategy
{
    private bool spawnComplete;
    private float spawnDuration;

    public bool IsSpawnComplete => spawnComplete;

    public void OnSpawn(Transform transform, MonsterClass monsterData)
    {
        spawnDuration = monsterData.CurrentSpawnDuration;
        // �⺻ ���� ����
        // ���߿� ���� �ִϸ��̼��̳� ����Ʈ �߰� ����
        spawnComplete = true;
    }
}
