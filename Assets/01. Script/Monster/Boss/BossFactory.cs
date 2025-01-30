using System;
using UnityEngine.AddressableAssets;
using UnityEngine;
using UnityEngine.ResourceManagement.AsyncOperations;

public class BossFactory : MonsterFactoryBase
{
    private readonly int bossId;
    public BossFactory(int bossId)
    {
        this.bossId = bossId;
    }
    protected override Type GetDataType()
    {
        return typeof(BossData);
    }
    // MonsterClass ��� IMonsterClass ��ȯ
    protected override IMonsterClass CreateMonsterInstance(ICreatureData data)
    {
        if (data is BossData bossData)
        {
            return new BossMonster(bossData);
        }
        Debug.LogError($"���� �����Ͱ� �ƴ� �����Ͱ� ���޵Ǿ����ϴ�: {data.MonsterName}");
        return null;
    }

    protected override string GetMonsterDataKey() => $"BossData_{bossId}";
    protected override bool IsEliteAvailable() => false;  // ������ ����Ʈȭ �� �� ����

    // LoadBossData, InstantiateBossPrefab�� �����ϰ� base�� ���� ���

    // FinalizeMonsterCreation�� ���� ���� ������ �ʿ��� ��� override
    protected override void FinalizeMonsterCreation(GameObject bossObject, ICreatureData data, Action<IMonsterClass> onMonsterCreated)
    {
        if (data is BossData bossData)
        {
            // ���� ���� ����Ʈ ó��
            if (bossData.spawnEffect != null)
            {
                GameObject.Instantiate(bossData.spawnEffect, bossObject.transform.position, Quaternion.identity);
            }

            IMonsterClass boss = new BossMonster(bossData);
            ICreatureStatus status = bossObject.AddComponent<BossStatus>();
            status.Initialize(boss);

            onMonsterCreated?.Invoke(boss);
        }
    }
}