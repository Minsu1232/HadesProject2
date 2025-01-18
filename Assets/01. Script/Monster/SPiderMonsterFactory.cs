using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

public class SPiderMonsterFactory : MonsterFactoryBase
{
    private const float ELITE_CHANCE = 0.99f;

    protected override MonsterClass CreateMonsterInstance(MonsterData data)
    {
        return UnityEngine.Random.value < ELITE_CHANCE && IsEliteAvailable()
            ? new EliteMonster(data)
            : new DummyMonster(data);
    }

    protected override string GetMonsterDataKey() => "MonsterData_3";
    protected override bool IsEliteAvailable() => true;
}
