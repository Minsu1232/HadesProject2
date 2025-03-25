using System;
using UnityEngine;

public class AlexanderBossFactory : MonsterFactoryBase
{
    protected override Type GetDataType() => typeof(BossData);

    protected override IMonsterClass CreateMonsterInstance(ICreatureData data)
    {
        if (data is AlexanderBossData bossData)
        {
            // AlexanderBoss ���� �ν��Ͻ� ����
            return new AlexanderBoss(bossData);
        }
        Debug.LogError("AlexanderBossFactory: ���޵� �����Ͱ� BossData Ÿ���� �ƴմϴ�.");
        return null;
    }

    protected override string GetMonsterDataKey() => $"BossData_1002";
    protected override bool IsEliteAvailable() => false;

    protected override void FinalizeMonsterCreation(GameObject bossObject, ICreatureData data, Action<ICreatureStatus> onMonsterCreated)
    {
        if (data is BossData bossData)
        {
            // ���� ȿ�� ����
            if (bossData.spawnEffect != null)
            {
                GameObject.Instantiate(bossData.spawnEffect, bossObject.transform.position, Quaternion.identity);
            }
            ICreatureStatus status = bossObject.AddComponent<BossStatus>();
            IMonsterClass boss = CreateMonsterInstance(data);
            if (boss == null)
            {
                Debug.LogError("AlexanderBoss �ν��Ͻ� ���� ����");
                return;
            }

          
            status.Initialize(boss);
            Debug.Log($"{bossData.MonsterName} ��ȯ (AlexanderBoss)");

            // AlexanderBoss�� UI ���� ó��
            var essenceUI = GameObject.FindObjectOfType<BossEssenceUIManager>();
            if (essenceUI != null && boss is AlexanderBoss alexanderBoss && boss is IBossWithEssenceSystem bossWithEssence)
            {
                Debug.Log($"{bossData.MonsterName} é�ͺ��� ��ȯ");
                essenceUI.Initialize(alexanderBoss.GetEssenceSystem());

                // ���� �տ� �ý��� �ʱ�ȭ
                if (data is AlexanderBossData alexanderBossData && alexanderBossData.enableMadnessCrack)
                {
                    // ���� �տ� �ý��� �߰�
                    var hazardManager = bossObject.AddComponent<BossEssenceHazardManager>();
                    hazardManager.Initialize(
                        bossWithEssence.GetEssenceSystem(),
                        alexanderBossData.crackCooldownMin,
                        alexanderBossData.crackCooldownMax
                    );




                    // ���� �տ� ������ ���� �� ���
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
                    Debug.Log($"{bossData.MonsterName}�� ���� �տ� �ý��� �ʱ�ȭ �Ϸ�");
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
