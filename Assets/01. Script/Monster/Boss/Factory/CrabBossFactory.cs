using System;
using UnityEngine;

public class CrabBossFactory : MonsterFactoryBase
{
    protected override Type GetDataType() => typeof(BossData);

    protected override IMonsterClass CreateMonsterInstance(ICreatureData data)
    {
        if (data is BossData bossData)
        {
            // CrabBoss는 기존 BossMonster 인스턴스를 사용
            return new BossMonster(bossData);
        }
        Debug.LogError("CrabBossFactory: 전달된 데이터가 BossData 타입이 아닙니다.");
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
                Debug.LogError("CrabBoss 인스턴스 생성 실패");
                return;
            }

            ICreatureStatus status = monsterObject.AddComponent<BossStatus>();
            status.Initialize(boss);
            Debug.Log($"{bossData.MonsterName} 소환 (CrabBoss)");

            onMonsterCreated?.Invoke(boss);
        }
    }
}
