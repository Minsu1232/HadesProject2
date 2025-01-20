using System;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

public abstract class MonsterFactoryBase
{
    protected abstract IMonsterClass CreateMonsterInstance(ICreatureData data);
    protected abstract string GetMonsterDataKey();
    protected abstract bool IsEliteAvailable();

    // ��ü���� ������ Ÿ���� ��� ���� �߻� �޼��� �߰�
    protected abstract Type GetDataType();

    public virtual IMonsterClass CreateMonster(Vector3 spawnPosition, Action<IMonsterClass> onMonsterCreated)
    {
        LoadMonsterData(spawnPosition, onMonsterCreated);
        return null;
    }

    private void LoadMonsterData(Vector3 spawnPosition, Action<IMonsterClass> onMonsterCreated)
    {
        // ��ü���� Ÿ������ �ε�
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
        if (data == null || string.IsNullOrEmpty(data.monsterPrefabKey))  // �빮�ڷ� ����
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
