using UnityEngine;

public abstract class MonsterFactoryBase
{
    public abstract MonsterClass CreateMonster(Vector3 spawnPosition, System.Action<MonsterClass> onMonsterCreated);
}
