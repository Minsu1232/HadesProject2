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

    protected override string GetMonsterDataKey() => $"BossData_2";
    protected override bool IsEliteAvailable() => false;

    protected override void FinalizeMonsterCreation(GameObject bossObject, ICreatureData data, Action<IMonsterClass> onMonsterCreated)
    {
        if (data is BossData bossData)
        {
            // ���� ȿ�� ����
            if (bossData.spawnEffect != null)
            {
                GameObject.Instantiate(bossData.spawnEffect, bossObject.transform.position, Quaternion.identity);
            }

            IMonsterClass boss = CreateMonsterInstance(data);
            if (boss == null)
            {
                Debug.LogError("AlexanderBoss �ν��Ͻ� ���� ����");
                return;
            }

            ICreatureStatus status = bossObject.AddComponent<BossStatus>();
            status.Initialize(boss);
            Debug.Log($"{bossData.MonsterName} ��ȯ (AlexanderBoss)");

            // AlexanderBoss�� UI ���� ó��
            var essenceUI = GameObject.FindObjectOfType<BossEssenceUIManager>();
            if (essenceUI != null && boss is AlexanderBoss alexanderBoss)
            {
                Debug.Log($"{bossData.MonsterName} é�ͺ��� ��ȯ");
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
