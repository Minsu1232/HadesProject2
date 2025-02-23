using System;
using UnityEngine;

public class AlexanderBossFactory : MonsterFactoryBase
{
    protected override Type GetDataType() => typeof(BossData);

    protected override IMonsterClass CreateMonsterInstance(ICreatureData data)
    {
        if (data is AlexanderBossData bossData)
        {
            // AlexanderBoss 전용 인스턴스 생성
            return new AlexanderBoss(bossData);
        }
        Debug.LogError("AlexanderBossFactory: 전달된 데이터가 BossData 타입이 아닙니다.");
        return null;
    }

    protected override string GetMonsterDataKey() => $"BossData_2";
    protected override bool IsEliteAvailable() => false;

    protected override void FinalizeMonsterCreation(GameObject bossObject, ICreatureData data, Action<IMonsterClass> onMonsterCreated)
    {
        if (data is BossData bossData)
        {
            // 스폰 효과 적용
            if (bossData.spawnEffect != null)
            {
                GameObject.Instantiate(bossData.spawnEffect, bossObject.transform.position, Quaternion.identity);
            }

            IMonsterClass boss = CreateMonsterInstance(data);
            if (boss == null)
            {
                Debug.LogError("AlexanderBoss 인스턴스 생성 실패");
                return;
            }

            ICreatureStatus status = bossObject.AddComponent<BossStatus>();
            status.Initialize(boss);
            Debug.Log($"{bossData.MonsterName} 소환 (AlexanderBoss)");

            // AlexanderBoss의 UI 연동 처리
            var essenceUI = GameObject.FindObjectOfType<BossEssenceUIManager>();
            if (essenceUI != null && boss is AlexanderBoss alexanderBoss)
            {
                Debug.Log($"{bossData.MonsterName} 챕터보스 소환");
                essenceUI.Initialize(alexanderBoss.GetEssenceSystem());
            }
            var soulStones = GameObject.FindObjectsOfType<SoulStone>();
            foreach (var stone in soulStones)
            {
                stone.InitializeWithBoss(boss as AlexanderBoss);
            }

            onMonsterCreated?.Invoke(boss);
        }
    }
}
