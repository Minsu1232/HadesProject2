using System;
using System.Collections.Generic;
using UnityEngine;

public class CompositeMonsterFactory : MonsterFactoryBase
{
    private List<MonsterFactoryBase> factories = new List<MonsterFactoryBase>();

    // ���� ���丮�� ���ÿ� �����ڿ� ������ �� �ֵ��� params ���
    public CompositeMonsterFactory(params MonsterFactoryBase[] factoriesToAdd)
    {
        foreach (var factory in factoriesToAdd)
        {
            factories.Add(factory);
        }
    }

    // ��Ÿ�� �� Ư�� ���丮�� �߰��� �� ���
    public void AddFactory(MonsterFactoryBase factory)
    {
        if (factory != null && !factories.Contains(factory))
        {
            factories.Add(factory);
        }
    }

    // ��Ÿ�� �� Ư�� ���丮�� ������ �� ���
    public void RemoveFactory(MonsterFactoryBase factory)
    {
        if (factories.Contains(factory))
        {
            factories.Remove(factory);
        }
    }

    // �ϳ��� Spawn ��û�� ���� ���� ���͸� ��� �������� ����
    public override MonsterClass CreateMonster(Vector3 spawnPosition, Action<MonsterClass> onMonsterCreated)
    {
        // [Option 1] ��� ���丮���� ���� ����
        MonsterClass lastCreatedMonster = null;
        foreach (var factory in factories)
        {
            // �� ���͸��� �ٸ� ��ġ�� ������ �ο��ϰ� �ʹٸ�
            // ���⿡�� ��ġ���� �����ϰų� �߰� �Ű������� ���� ���� ����
            lastCreatedMonster = factory.CreateMonster(spawnPosition,onMonsterCreated);
        }
        return lastCreatedMonster;

        /*
        // [Option 2] ���� ���丮 �� �ϳ��� ���������� ����
        // �ʿ� �� Ȯ��, ���� ���� ������ 0~n��° ���丮�� ��� ��� ����
        if (factories.Count > 0)
        {
            int randomIndex = Random.Range(0, factories.Count);
            return factories[randomIndex].CreateMonster(spawnPosition);
        }
        return null;
        */
    }

}
