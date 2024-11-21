using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

public class DummyMonsterFactory : MonsterFactoryBase
{
    public override MonsterClass CreateMonster(Vector3 spawnPosition, Action<MonsterClass> onMonsterCreated)
    {
        Addressables.LoadAssetAsync<MonsterData>("MonsterData_1").Completed += handle =>
        {
            if (handle.Status == AsyncOperationStatus.Succeeded)
            {
                MonsterData monsterData = handle.Result;
                monsterData.monsterPrefab.InstantiateAsync(spawnPosition, Quaternion.identity).Completed += prefabHandle =>
                {
                    if (prefabHandle.Status == AsyncOperationStatus.Succeeded)
                    {
                        GameObject monsterObject = prefabHandle.Result;

                        // MonsterClass �ν��Ͻ� ����
                        MonsterClass monster = new DummyMonster(monsterData);

                        // MonsterStatus ������Ʈ �߰� �� �ʱ�ȭ
                        MonsterStatus monsterStatus = monsterObject.AddComponent<MonsterStatus>();
                        monsterStatus.Initialize(monster); // MonsterClass�� �ʱ�ȭ

                        onMonsterCreated?.Invoke(monster);
                    }
                };
            } 
        };

        return null; // �񵿱� ó���� �Ϸ�� ������ ��ȯ�� �� ����
    }
}



