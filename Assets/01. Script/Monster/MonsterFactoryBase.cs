using System;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

public abstract class MonsterFactoryBase
{
    protected abstract MonsterClass CreateMonsterInstance(MonsterData data);
    protected abstract string GetMonsterDataKey();
    protected abstract bool IsEliteAvailable();

    public virtual MonsterClass CreateMonster(Vector3 spawnPosition, Action<MonsterClass> onMonsterCreated)
    {
        LoadMonsterData(spawnPosition, onMonsterCreated);
        return null;
    }

    private void LoadMonsterData(Vector3 spawnPosition, Action<MonsterClass> onMonsterCreated)
    {
        Addressables.LoadAssetAsync<MonsterData>(GetMonsterDataKey()).Completed += handle =>
        {
            if (handle.Status == AsyncOperationStatus.Succeeded)
            {
                InstantiatePrefab(handle.Result, spawnPosition, onMonsterCreated);
            }
            else
            {
                Debug.LogError($"Failed to load MonsterData with Key: {GetMonsterDataKey()}");
            }
        };
    }

    private void InstantiatePrefab(MonsterData data, Vector3 position, Action<MonsterClass> onMonsterCreated)
    {
        if (data == null || string.IsNullOrEmpty(data.monsterPrefabKey))
        {
            Debug.LogError("MonsterData is null or PrefabKey is missing.");
            return;
        }

        Addressables.InstantiateAsync(data.monsterPrefabKey, position, Quaternion.identity)
            .Completed += handle =>
            {
                if (handle.Status == AsyncOperationStatus.Succeeded)
                {
                    FinalizeMonsterCreation(handle.Result, data, onMonsterCreated);
                }
            };
    }

    private void FinalizeMonsterCreation(GameObject monsterObject, MonsterData data, Action<MonsterClass> onMonsterCreated)
    {
        MonsterClass monster = CreateMonsterInstance(data);
        MonsterStatus status = monsterObject.AddComponent<MonsterStatus>();
        status.Initialize(monster);

        if (monster is EliteMonster)
        {
            monsterObject.AddComponent<EliteMonsterController>();
        }

        onMonsterCreated?.Invoke(monster);
    }
}
