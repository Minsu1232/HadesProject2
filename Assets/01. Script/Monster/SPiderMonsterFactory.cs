using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

public class SPiderMonsterFactory : MonsterFactoryBase
{
    public override MonsterClass CreateMonster(Vector3 spawnPosition, Action<MonsterClass> onMonsterCreated)
    {
        // 1. MonsterData �ε�
        Addressables.LoadAssetAsync<MonsterData>("MonsterData_3").Completed += dataHandle =>
        {
            if (dataHandle.Status == AsyncOperationStatus.Succeeded)
            {
                MonsterData monsterData = dataHandle.Result;

                if (monsterData == null || string.IsNullOrEmpty(monsterData.monsterPrefabKey))
                {
                    Debug.LogError("MonsterData is null or PrefabKey is missing.");
                    return;
                }

                // 2. Prefab InstantiateAsync
                Addressables.InstantiateAsync(monsterData.monsterPrefabKey, spawnPosition, Quaternion.identity).Completed += prefabHandle =>
                {
                    if (prefabHandle.Status == AsyncOperationStatus.Succeeded)
                    {
                        GameObject monsterObject = prefabHandle.Result;

                        // 3. MonsterClass �� Status �ʱ�ȭ
                        MonsterClass monster = new DummyMonster(monsterData);
                        MonsterStatus monsterStatus = monsterObject.AddComponent<MonsterStatus>();
                        monsterStatus.Initialize(monster);

                        // 4. Callback ȣ��
                        onMonsterCreated?.Invoke(monster);
                    }
                    else
                    {
                        Debug.LogError($"Failed to instantiate monster prefab with Key: {monsterData.monsterPrefabKey}");
                    }
                };
            }
            else
            {
                Debug.LogError("Failed to load MonsterData with Key: MonsterData_1");
            }
        };

        return null; // �񵿱� ó���� �Ϸ�� ������ ��ȯ�� �� ����
    }
}
