
using System;
public class RayMonsterFactory : MonsterFactoryBase
{

    private const float ELITE_CHANCE = 0.25f;
    protected override Type GetDataType()
    {
        return typeof(MonsterData);  // 일반 몬스터는 MonsterData 사용
    }
    protected override IMonsterClass CreateMonsterInstance(ICreatureData data)
    {
        return UnityEngine.Random.value < ELITE_CHANCE && IsEliteAvailable()
            ? new EliteMonster(data)
            : new DummyMonster(data);
    }

    protected override string GetMonsterDataKey() => "MonsterData_6";
    protected override bool IsEliteAvailable() => true;
}
