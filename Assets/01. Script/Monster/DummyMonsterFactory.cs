using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

public class DummyMonsterFactory : MonsterFactoryBase
{
    private const float ELITE_CHANCE = 0.999f;

    protected override MonsterClass CreateMonsterInstance(MonsterData data)
    {
        return UnityEngine.Random.value < ELITE_CHANCE && IsEliteAvailable()
            ? new EliteMonster(data)
            : new DummyMonster(data);
    }

    protected override string GetMonsterDataKey() => "MonsterData_2";
    protected override bool IsEliteAvailable() => true;
}



