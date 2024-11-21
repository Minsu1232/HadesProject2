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
        // CSV 파일을 Addressables로 로드
        var csvHandle = Addressables.LoadAssetAsync<TextAsset>("monsters_csv");
        TextAsset csvFile = await csvHandle.Task;

        if (csvFile == null)
        {
            Debug.LogError("몬스터 데이터 CSV 파일을 찾을 수 없습니다!");
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

            // ScriptableObject를 Addressables로 로드
            string monsterDataPath = $"MonsterData_{values[0]}"; // Addressables에 등록된 ScriptableObject 키
            var monsterDataHandle = Addressables.LoadAssetAsync<MonsterData>(monsterDataPath);
            MonsterData monsterData = await monsterDataHandle.Task;
            Debug.Log(values[0]);
             Debug.Log($"찾으려는 ScriptableObject 주소: {monsterDataPath}");

            if (monsterData != null)
            {
                // CSV 데이터를 ScriptableObject에 할당
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

                // Addressable 프리팹 참조 설정
                string prefabPath = values[11]; // Addressables에 등록된 프리팹 키
                monsterData.monsterPrefab = new AssetReferenceGameObject(prefabPath);

                // Dictionary에 추가
                int monsterId = int.Parse(values[0]);
                monsterDatabase.Add(monsterId, monsterData);

                Debug.Log($"몬스터 로드: ID {monsterId}, 이름 {monsterData.monsterName}");
            }
        }

        // CSV 파일 해제
        Addressables.Release(csvHandle);

        Debug.Log($"몬스터 데이터 로드 완료: {monsterDatabase.Count}개의 몬스터");
    }

    public MonsterData GetMonsterData(int monsterId)
    {
        if (monsterDatabase.TryGetValue(monsterId, out MonsterData data))
        {
            return data;
        }
        Debug.LogWarning($"몬스터 데이터를 찾을 수 없습니다: ID {monsterId}");
        return null;
    }

    // 선택적: 모든 리소스 해제
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
