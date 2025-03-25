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

    protected override string GetMonsterDataKey() => $"BossData_1002";
    protected override bool IsEliteAvailable() => false;

    protected override void FinalizeMonsterCreation(GameObject bossObject, ICreatureData data, Action<ICreatureStatus> onMonsterCreated)
    {
        if (data is BossData bossData)
        {
            // 스폰 효과 적용
            if (bossData.spawnEffect != null)
            {
                GameObject.Instantiate(bossData.spawnEffect, bossObject.transform.position, Quaternion.identity);
            }
            ICreatureStatus status = bossObject.AddComponent<BossStatus>();
            IMonsterClass boss = CreateMonsterInstance(data);
            if (boss == null)
            {
                Debug.LogError("AlexanderBoss 인스턴스 생성 실패");
                return;
            }

          
            status.Initialize(boss);
            Debug.Log($"{bossData.MonsterName} 소환 (AlexanderBoss)");

            // AlexanderBoss의 UI 연동 처리
            var essenceUI = GameObject.FindObjectOfType<BossEssenceUIManager>();
            if (essenceUI != null && boss is AlexanderBoss alexanderBoss && boss is IBossWithEssenceSystem bossWithEssence)
            {
                Debug.Log($"{bossData.MonsterName} 챕터보스 소환");
                essenceUI.Initialize(alexanderBoss.GetEssenceSystem());

                // 광기 균열 시스템 초기화
                if (data is AlexanderBossData alexanderBossData && alexanderBossData.enableMadnessCrack)
                {
                    // 광기 균열 시스템 추가
                    var hazardManager = bossObject.AddComponent<BossEssenceHazardManager>();
                    hazardManager.Initialize(
                        bossWithEssence.GetEssenceSystem(),
                        alexanderBossData.crackCooldownMin,
                        alexanderBossData.crackCooldownMax
                    );




                    // 광기 균열 위험요소 생성 및 등록
                    var madnessCrack = new MadnessCrackHazard(
                 alexanderBossData.crackPrefab,
    alexanderBossData.crackIndicatorPrefab,
    alexanderBossData.crackExplosionPrefab,
    status,
    alexanderBossData.essenceThreshold,
    alexanderBossData.crackWarningDuration,
    alexanderBossData.crackRadius,
    alexanderBossData.crackDamage,
    alexanderBossData.crackDamageMultiplier
                    );

                    hazardManager.RegisterHazard(madnessCrack);
                    Debug.Log($"{bossData.MonsterName}의 광기 균열 시스템 초기화 완료");
                }
            }

            var soulStones = GameObject.FindObjectsOfType<SoulStone>();
            foreach (var stone in soulStones)
            {
                stone.InitializeWithBoss(boss as AlexanderBoss);
            }

            onMonsterCreated?.Invoke(status);
        }
    }
}
