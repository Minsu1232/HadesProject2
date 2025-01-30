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
    // MonsterClass 대신 IMonsterClass 반환
    protected override IMonsterClass CreateMonsterInstance(ICreatureData data)
    {
        if (data is BossData bossData)
        {
            return new BossMonster(bossData);
        }
        Debug.LogError($"보스 데이터가 아닌 데이터가 전달되었습니다: {data.MonsterName}");
        return null;
    }

    protected override string GetMonsterDataKey() => $"BossData_{bossId}";
    protected override bool IsEliteAvailable() => false;  // 보스는 엘리트화 될 수 없음

    // LoadBossData, InstantiateBossPrefab은 제거하고 base의 구현 사용

    // FinalizeMonsterCreation만 보스 전용 로직이 필요한 경우 override
    protected override void FinalizeMonsterCreation(GameObject bossObject, ICreatureData data, Action<IMonsterClass> onMonsterCreated)
    {
        if (data is BossData bossData)
        {
            // 보스 스폰 이펙트 처리
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