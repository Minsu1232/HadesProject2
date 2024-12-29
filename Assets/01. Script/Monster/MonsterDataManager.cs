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
    private string persistentFilePath; // ���� ����� CSV ���
    private string streamingFilePath;  // �ʱ� CSV ���

    private void Awake()
    {
        // ��� �ʱ�ȭ
        persistentFilePath = Path.Combine(Application.persistentDataPath, "Monsters.csv");
        streamingFilePath = Path.Combine(Application.streamingAssetsPath, "Monsters.csv");

        //// CSV ���� ���� ���� Ȯ��
        //if (!File.Exists(persistentFilePath))
        //{
        //    Debug.LogWarning($"CSV ������ �����ϴ�. StreamingAssets���� �����մϴ�: {persistentFilePath}");
        //}
        CopyCSVFromStreamingAssets(); // ���� ���� �ܰ��̱⿡ �������� ���� > �����Ϳ��� ������ ����
    
    }

    public void InitializeMonsters()
    {
        LoadMonstersFromCSV();
    }

    private void CopyCSVFromStreamingAssets()
    {
        if (File.Exists(streamingFilePath))
        {
            // overwrite �Ķ���͸� true�� ����
            File.Copy(streamingFilePath, persistentFilePath, true);
            Debug.Log($"StreamingAssets���� CSV ���� ���� �Ϸ�: {persistentFilePath}");
        }
        else
        {
            Debug.LogError("StreamingAssets���� Monsters.csv ������ ã�� �� �����ϴ�.");
        }
    }

    private async void LoadMonstersFromCSV()
    {
        if (!File.Exists(persistentFilePath))
        {
            Debug.LogError($"���� ������ CSV ������ ã�� �� �����ϴ�: {persistentFilePath}");
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
            string monsterDataPath = $"MonsterData_{values[0]}"; // Addressables�� ��ϵ� ScriptableObject Ű
            var monsterDataHandle = Addressables.LoadAssetAsync<MonsterData>(monsterDataPath);
            MonsterData monsterData = await monsterDataHandle.Task;

            if (monsterData != null)
            {
                UpdateMonsterData(monsterData, values);
                int monsterId = int.Parse(values[0]);
                monsterDatabase[monsterId] = monsterData;
                Debug.Log($"���� �ε�: ID {monsterId}, �̸� {monsterData.monsterName}");
            }
            else
            {
                Debug.LogError($"���� �����͸� ã�� �� �����ϴ�: {monsterDataPath}");
            }
        }

        Debug.Log($"���� ������ �ε� �Ϸ�: {monsterDatabase.Count}���� ����");
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
        monsterData.useHealthRetreat = bool.Parse(values[28]);           // ü�� ��� ���� ��� ����
        monsterData.healthRetreatThreshold = float.Parse(values[29]);    // ���� ���� ü�� ����
        monsterData.isPhaseChange = bool.Parse(values[30]);             // ������ ��ȯ�� ��������
                                                                        // ���� �߰��Ǵ� �κ�
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
        Debug.LogWarning($"���� �����͸� ã�� �� �����ϴ�: ID {monsterId}");
        return null;
    }

    // CSV�� ���� ������ ����
    public void SaveMonsterDataToCSV()
    {
        using (StreamWriter writer = new StreamWriter(persistentFilePath))
        {
            // ��� �ۼ�
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
        Debug.Log($"���� ������ ���� �Ϸ�: {persistentFilePath}");
    }

    public void ReleaseAllResources()
    {
       
        monsterDatabase.Clear();
    }
}
