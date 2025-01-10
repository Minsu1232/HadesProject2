using System;
using System.Collections.Generic;
using UnityEngine;

public class CompositeMonsterFactory : MonsterFactoryBase
{
    private List<MonsterFactoryBase> factories = new List<MonsterFactoryBase>();

    // 여러 팩토리를 동시에 생성자에 전달할 수 있도록 params 사용
    public CompositeMonsterFactory(params MonsterFactoryBase[] factoriesToAdd)
    {
        foreach (var factory in factoriesToAdd)
        {
            factories.Add(factory);
        }
    }

    // 런타임 중 특정 팩토리를 추가할 때 사용
    public void AddFactory(MonsterFactoryBase factory)
    {
        if (factory != null && !factories.Contains(factory))
        {
            factories.Add(factory);
        }
    }

    // 런타임 중 특정 팩토리를 제거할 때 사용
    public void RemoveFactory(MonsterFactoryBase factory)
    {
        if (factories.Contains(factory))
        {
            factories.Remove(factory);
        }
    }

    // 하나의 Spawn 요청에 대해 여러 몬스터를 어떻게 생성할지 정의
    public override MonsterClass CreateMonster(Vector3 spawnPosition, Action<MonsterClass> onMonsterCreated)
    {
        // [Option 1] 모든 팩토리에서 몬스터 생성
        MonsterClass lastCreatedMonster = null;
        foreach (var factory in factories)
        {
            // 각 몬스터마다 다른 위치나 로직을 부여하고 싶다면
            // 여기에서 위치값을 변형하거나 추가 매개변수를 넣을 수도 있음
            lastCreatedMonster = factory.CreateMonster(spawnPosition,onMonsterCreated);
        }
        return lastCreatedMonster;

        /*
        // [Option 2] 여러 팩토리 중 하나만 선택적으로 생성
        // 필요 시 확률, 조건 등을 적용해 0~n번째 팩토리를 골라 사용 가능
        if (factories.Count > 0)
        {
            int randomIndex = Random.Range(0, factories.Count);
            return factories[randomIndex].CreateMonster(spawnPosition);
        }
        return null;
        */
    }

}
