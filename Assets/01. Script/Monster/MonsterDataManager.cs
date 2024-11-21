using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;

public class MonsterDataManager : Singleton<MonsterDataManager>
{
    private Dictionary<int, MonsterData> monsterDatabase = new Dictionary<int, MonsterData>();

    public void InitializeMonsters()
    {
        LoadMonstersFromCSV();
    }

    private async void LoadMonstersFromCSV()
    {
        // CSV ������ Addressables�� �ε�
        var csvHandle = Addressables.LoadAssetAsync<TextAsset>("monsters_csv");
        TextAsset csvFile = await csvHandle.Task;

        if (csvFile == null)
        {
            Debug.LogError("���� ������ CSV ������ ã�� �� �����ϴ�!");
            return;
        }

        string[] lines = csvFile.text.Split('\n');
        bool isFirstLine = true;

        foreach (string line in lines)
        {
            if (isFirstLine)
            {
                isFirstLine = false;
                continue;
            }

            if (string.IsNullOrWhiteSpace(line)) continue;

            string[] values = line.Trim().Split(',');

            // ScriptableObject�� Addressables�� �ε�
            string monsterDataPath = $"MonsterData_{values[0]}"; // Addressables�� ��ϵ� ScriptableObject Ű
            var monsterDataHandle = Addressables.LoadAssetAsync<MonsterData>(monsterDataPath);
            MonsterData monsterData = await monsterDataHandle.Task;
            Debug.Log(values[0]);
             Debug.Log($"ã������ ScriptableObject �ּ�: {monsterDataPath}");

            if (monsterData != null)
            {
                // CSV �����͸� ScriptableObject�� �Ҵ�
                monsterData.monsterName = values[1];
                monsterData.initialHp = int.Parse(values[2]);
                monsterData.initialAttackPower = int.Parse(values[3]);
                monsterData.initialAttackSpeed = float.Parse(values[4]);
                monsterData.initialSpeed = int.Parse(values[5]);
                monsterData.attackRange = int.Parse(values[6]);
                monsterData.dropChance = float.Parse(values[7]);
                monsterData.dropItem = int.Parse(values[8]);
                monsterData.moveRange = int.Parse(values[9]);
                monsterData.chaseRange = int.Parse(values[10]);

                // Addressable ������ ���� ����
                string prefabPath = values[11]; // Addressables�� ��ϵ� ������ Ű
                monsterData.monsterPrefab = new AssetReferenceGameObject(prefabPath);

                // Dictionary�� �߰�
                int monsterId = int.Parse(values[0]);
                monsterDatabase.Add(monsterId, monsterData);

                Debug.Log($"���� �ε�: ID {monsterId}, �̸� {monsterData.monsterName}");
            }
        }

        // CSV ���� ����
        Addressables.Release(csvHandle);

        Debug.Log($"���� ������ �ε� �Ϸ�: {monsterDatabase.Count}���� ����");
    }

    public MonsterData GetMonsterData(int monsterId)
    {
        if (monsterDatabase.TryGetValue(monsterId, out MonsterData data))
        {
            return data;
        }
        Debug.LogWarning($"���� �����͸� ã�� �� �����ϴ�: ID {monsterId}");
        return null;
    }

    // ������: ��� ���ҽ� ����
    public void ReleaseAllResources()
    {
        foreach (var monsterData in monsterDatabase.Values)
        {
            if (monsterData.monsterPrefab.IsValid())
            {
                Addressables.Release(monsterData.monsterPrefab);
            }
        }
        monsterDatabase.Clear();
    }
}
