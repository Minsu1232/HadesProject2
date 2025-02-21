using System;
using UnityEngine;

public class CrabBossFactory : MonsterFactoryBase
{
    protected override Type GetDataType() => typeof(BossData);

    protected override IMonsterClass CreateMonsterInstance(ICreatureData data)
    {
        if (data is BossData bossData)
        {
            // CrabBoss�� ���� BossMonster �ν��Ͻ��� ���
            return new BossMonster(bossData);
        }
        Debug.LogError("CrabBossFactory: ���޵� �����Ͱ� BossData Ÿ���� �ƴմϴ�.");
        return null;
    }

    protected override string GetMonsterDataKey() => "BossData_1";
    protected override bool IsEliteAvailable() => false;

    protected override void FinalizeMonsterCreation(GameObject monsterObject, ICreatureData data, Action<IMonsterClass> onMonsterCreated)
    {
        if (data is BossData bossData)
        {
            if (bossData.spawnEffect != null)
            {
                GameObject.Instantiate(bossData.spawnEffect, monsterObject.transform.position, Quaternion.identity);
            }

            IMonsterClass boss = CreateMonsterInstance(data);
            if (boss == null)
            {
                Debug.LogError("CrabBoss �ν��Ͻ� ���� ����");
                return;
            }

            ICreatureStatus status = monsterObject.AddComponent<BossStatus>();
            status.Initialize(boss);
            Debug.Log($"{bossData.MonsterName} ��ȯ (CrabBoss)");

            onMonsterCreated?.Invoke(boss);
        }
    }
}
