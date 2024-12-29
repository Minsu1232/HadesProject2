using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.AddressableAssets;
using static MonsterData;

public class MonsterDataManager : Singleton<MonsterDataManager>
{
    private Dictionary<int, MonsterData> monsterDatabase = new Dictionary<int, MonsterData>();
    private string persistentFilePath; // 실제 사용할 CSV 경로
    private string streamingFilePath;  // 초기 CSV 경로

    private void Awake()
    {
        // 경로 초기화
        persistentFilePath = Path.Combine(Application.persistentDataPath, "Monsters.csv");
        streamingFilePath = Path.Combine(Application.streamingAssetsPath, "Monsters.csv");

        //// CSV 파일 존재 여부 확인
        //if (!File.Exists(persistentFilePath))
        //{
        //    Debug.LogWarning($"CSV 파일이 없습니다. StreamingAssets에서 복사합니다: {persistentFilePath}");
        //}
        CopyCSVFromStreamingAssets(); // 현재 개발 단계이기에 무조건적 복사 > 에디터에서 수정을 위함
    
    }

    public void InitializeMonsters()
    {
        LoadMonstersFromCSV();
    }

    private void CopyCSVFromStreamingAssets()
    {
        if (File.Exists(streamingFilePath))
        {
            // overwrite 파라미터를 true로 설정
            File.Copy(streamingFilePath, persistentFilePath, true);
            Debug.Log($"StreamingAssets에서 CSV 파일 복사 완료: {persistentFilePath}");
        }
        else
        {
            Debug.LogError("StreamingAssets에서 Monsters.csv 파일을 찾을 수 없습니다.");
        }
    }

    private async void LoadMonstersFromCSV()
    {
        if (!File.Exists(persistentFilePath))
        {
            Debug.LogError($"몬스터 데이터 CSV 파일을 찾을 수 없습니다: {persistentFilePath}");
            return;
        }

        string[] lines = File.ReadAllLines(persistentFilePath);
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
            string monsterDataPath = $"MonsterData_{values[0]}"; // Addressables에 등록된 ScriptableObject 키
            var monsterDataHandle = Addressables.LoadAssetAsync<MonsterData>(monsterDataPath);
            MonsterData monsterData = await monsterDataHandle.Task;

            if (monsterData != null)
            {
                UpdateMonsterData(monsterData, values);
                int monsterId = int.Parse(values[0]);
                monsterDatabase[monsterId] = monsterData;
                Debug.Log($"몬스터 로드: ID {monsterId}, 이름 {monsterData.monsterName}");
            }
            else
            {
                Debug.LogError($"몬스터 데이터를 찾을 수 없습니다: {monsterDataPath}");
            }
        }

        Debug.Log($"몬스터 데이터 로드 완료: {monsterDatabase.Count}개의 몬스터");
    }

    private void UpdateMonsterData(MonsterData monsterData, string[] values)
    {
        monsterData.monsterName = values[1];
        monsterData.initialHp = int.Parse(values[2]);
        monsterData.initialAttackPower = int.Parse(values[3]);
        monsterData.initialAttackSpeed = float.Parse(values[4]);
        monsterData.initialSpeed = int.Parse(values[5]);
        monsterData.attackRange = float.Parse(values[6]);
        monsterData.dropChance = float.Parse(values[7]);
        monsterData.dropItem = int.Parse(values[8]);
        monsterData.moveRange = int.Parse(values[9]);
        monsterData.chaseRange = int.Parse(values[10]);        
        monsterData.monsterPrefabKey = values[11];
        monsterData.skillCooldown = float.Parse(values[12]);
        monsterData.aggroDropRange = int.Parse(values[13]);
        monsterData.skillRange = float.Parse(values[14]);
        monsterData.skillDuration = float.Parse(values[15]);
        monsterData.hitStunDuration = float.Parse(values[16]);
        monsterData.deathDuration = float.Parse(values[17]);
        monsterData.spawnDuration = float.Parse(values[18]);
        monsterData.grade = (MonsterGrade)Enum.Parse(typeof(MonsterGrade), values[19]);
        monsterData.skillDamage = int.Parse(values[20]);
        monsterData.spawnStrategy = (SpawnStrategyType)Enum.Parse(typeof(SpawnStrategyType), values[21]);
        monsterData.moveStrategy = (MovementStrategyType)Enum.Parse(typeof(MovementStrategyType), values[22]);
        monsterData.attackStrategy = (AttackStrategyType)Enum.Parse(typeof(AttackStrategyType), values[23]);
        monsterData.idleStrategy = (IdleStrategyType)Enum.Parse(typeof(IdleStrategyType), values[24]);
        monsterData.skillStrategy = (SkillStrategyType)Enum.Parse(typeof(SkillStrategyType), values[25]);
        monsterData.dieStrategy = (DieStrategyType)Enum.Parse(typeof(DieStrategyType), values[26]);
        monsterData.hitStrategy = (HitStrategyType)Enum.Parse(typeof(HitStrategyType), values[27]);
        monsterData.useHealthRetreat = bool.Parse(values[28]);           // 체력 기반 도주 사용 여부
        monsterData.healthRetreatThreshold = float.Parse(values[29]);    // 도주 시작 체력 비율
        monsterData.isPhaseChange = bool.Parse(values[30]);             // 페이즈 전환용 도주인지
                                                                        // 새로 추가되는 부분
        monsterData.projectileSpeed = float.Parse(values[31]);
        monsterData.rotateSpeed = float.Parse(values[32]);
        monsterData.areaRadius = float.Parse(values[33]);
        monsterData.buffType = (BuffType)Enum.Parse(typeof(BuffType), values[34]);
        monsterData.buffDuration = float.Parse(values[35]);
        monsterData.buffValue = float.Parse(values[36]);
        monsterData.summonCount = int.Parse(values[37]);
        monsterData.summonRadius = float.Parse(values[38]);
        monsterData.projectileType = (ProjectileMovementType)Enum.Parse(typeof(ProjectileMovementType), values[39]);
        monsterData.skillEffectType = (SkillEffectType)Enum.Parse(typeof(SkillEffectType), values[40]);
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

    // CSV에 현재 데이터 저장
    public void SaveMonsterDataToCSV()
    {
        using (StreamWriter writer = new StreamWriter(persistentFilePath))
        {
            // 헤더 작성
            writer.WriteLine("ID,Name,HP,AttackPower,AttackSpeed,Speed,AttackRange,DropChance,DropItem,MoveRange,ChaseRange,PrefabPath");

            foreach (var pair in monsterDatabase)
            {
                MonsterData monster = pair.Value;
                string line = $"{pair.Key}," +
                            $"{monster.monsterName}," +
                            $"{monster.initialHp}," +
                            $"{monster.initialAttackPower}," +
                            $"{monster.initialAttackSpeed}," +
                            $"{monster.initialSpeed}," +
                            $"{monster.attackRange}," +
                            $"{monster.dropChance}," +
                            $"{monster.dropItem}," +
                            $"{monster.moveRange}," +
                            $"{monster.chaseRange}," +
                            //$"{monster.monsterPrefab.AssetGUID}" +
                            $"{monster.skillCooldown}" +
                            $"{monster.aggroDropRange}";

                writer.WriteLine(line);
            }
        }
        Debug.Log($"몬스터 데이터 저장 완료: {persistentFilePath}");
    }

    public void ReleaseAllResources()
    {
       
        monsterDatabase.Clear();
    }
}
