using System;
using System.Collections.Generic;
using UnityEngine;

public class CompositeMonsterFactory : MonsterFactoryBase
{
    private readonly MonsterFactoryBase[] factories;

    public CompositeMonsterFactory(params MonsterFactoryBase[] factories)
    {
        this.factories = factories;
    }
    protected override Type GetDataType()
    {
        return typeof(MonsterData);  // �Ϲ� ���ʹ� MonsterData ���
    }
    public override IMonsterClass CreateMonster(Vector3 spawnPosition, Action<IMonsterClass> onMonsterCreated)
    {
        if (factories.Length > 0)
        {
            int randomIndex = UnityEngine.Random.Range(0, factories.Length);
            return factories[randomIndex].CreateMonster(spawnPosition, onMonsterCreated);
        }
        return null;
    }

    protected override IMonsterClass CreateMonsterInstance(ICreatureData data) => null;
    protected override string GetMonsterDataKey() => "";
    protected override bool IsEliteAvailable() => false;

}
