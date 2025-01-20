using System;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

public abstract class MonsterFactoryBase
{
    protected abstract IMonsterClass CreateMonsterInstance(ICreatureData data);
    protected abstract string GetMonsterDataKey();
    protected abstract bool IsEliteAvailable();

    // 구체적인 데이터 타입을 얻기 위한 추상 메서드 추가
    protected abstract Type GetDataType();

    public virtual IMonsterClass CreateMonster(Vector3 spawnPosition, Action<IMonsterClass> onMonsterCreated)
    {
        LoadMonsterData(spawnPosition, onMonsterCreated);
        return null;
    }

    private void LoadMonsterData(Vector3 spawnPosition, Action<IMonsterClass> onMonsterCreated)
    {
        // 구체적인 타입으로 로드
        var loadOperation = Addressables.LoadAssetAsync<MonsterData>(GetMonsterDataKey());
        loadOperation.Completed += handle =>
        {
            if (handle.Status == AsyncOperationStatus.Succeeded &&
                handle.Result is ICreatureData data)
            {
                InstantiatePrefab(data, spawnPosition, onMonsterCreated);
            }
            else
            {
                Debug.LogError($"Failed to load MonsterData with Key: {GetMonsterDataKey()}");
            }
        };
    }

    private void InstantiatePrefab(ICreatureData data, Vector3 position, Action<IMonsterClass> onMonsterCreated)
    {
        if (data == null || string.IsNullOrEmpty(data.monsterPrefabKey))  // 대문자로 수정
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

    protected virtual void FinalizeMonsterCreation(GameObject monsterObject, ICreatureData data, Action<IMonsterClass> onMonsterCreated)
    {
        IMonsterClass monster = CreateMonsterInstance(data);
        ICreatureStatus status = monsterObject.AddComponent<MonsterStatus>();
        status.Initialize(monster);
        if (monster is EliteMonster)
        {
            monsterObject.AddComponent<EliteMonsterController>();
        }
        onMonsterCreated?.Invoke(monster);
    }
}
