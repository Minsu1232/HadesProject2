using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.AddressableAssets;
using static MonsterData;

public class MonsterDataManager : Singleton<MonsterDataManager>
{
    private Dictionary<int, MonsterData> monsterDatabase = new Dictionary<int, MonsterData>();
    private Dictionary<int, Dictionary<string, string>> strategyData = new Dictionary<int, Dictionary<string, string>>();
    private Dictionary<int, Dictionary<string, string>> skillData = new Dictionary<int, Dictionary<string, string>>();
    private Dictionary<int, MonsterData> monsterPrefabData = new Dictionary<int, MonsterData>();

    private string monsterFilePath;
    private string strategyFilePath;
    private string skillFilePath;
    private string prefabFilePath;
    private void Awake()
    {
        // 경로 초기화
        monsterFilePath = Path.Combine(Application.persistentDataPath, "Monsters.csv");
        strategyFilePath = Path.Combine(Application.persistentDataPath, "MonsterStrategies.csv");
        skillFilePath = Path.Combine(Application.persistentDataPath, "MonsterSkills.csv");
        prefabFilePath = Path.Combine(Application.persistentDataPath, "MonsterPrefabs.csv");
        CopyCSVFromStreamingAssets();
    }

    public async Task InitializeMonsters()
    {
        await LoadAllMonsterData();
    }

    private void CopyCSVFromStreamingAssets()
    {
        string[] csvFiles = new string[] { "Monsters.csv", "MonsterStrategies.csv", "MonsterSkills.csv","MonsterPrefabs.csv" };
        foreach (string fileName in csvFiles)
        {
            string streamingPath = Path.Combine(Application.streamingAssetsPath, fileName);
            string persistentPath = Path.Combine(Application.persistentDataPath, fileName);

            if (File.Exists(streamingPath))
            {
                File.Copy(streamingPath, persistentPath, true);
                Debug.Log($"CSV 파일 복사 완료: {fileName}");
            }
            else
            {
                Debug.LogError($"StreamingAssets에서 파일을 찾을 수 없습니다: {fileName}");
            }
        }
    }

    private async Task LoadAllMonsterData()
    {
        // 먼저 전략과 스킬 데이터를 로드
        await LoadStrategyData();
        await LoadSkillData();
        await LoadPrefabsFromCSV();
        // 마지막으로 몬스터 데이터를 로드하고 모든 데이터를 결합
        await LoadMonstersFromCSV();
    }
    private async Task LoadPrefabsFromCSV()
    {
        if (!File.Exists(prefabFilePath))
        {
            Debug.LogError($"프리팹 데이터 CSV 파일을 찾을 수 없습니다: {prefabFilePath}");
            return;
        }

        string[] lines = File.ReadAllLines(prefabFilePath);
        string[] headers = lines[0].Split(',');
        bool isFirstLine = true;

        foreach (string line in lines)
        {
            if (isFirstLine)
            {
                isFirstLine = false;
                continue;
            }

            if (string.IsNullOrWhiteSpace(line)) continue;

            string[] values = line.Split(',');
            int monsterId = int.Parse(values[0]);

            try
            {
                // ScriptableObject.CreateInstance를 사용하여 MonsterData 인스턴스 생성
                MonsterData prefabData = ScriptableObject.CreateInstance<MonsterData>();

                // BuffData 초기화
                prefabData.buffData = new BuffData
                {
                    buffTypes = new BuffType[0],
                    durations = new float[0],
                    values = new float[0]
                };

                // 프리팹 필드 매핑
                prefabData.areaEffectPrefab = await LoadPrefab(values[1]);
                prefabData.shorckEffectPrefab = await LoadPrefab(values[2]);
                prefabData.buffEffectPrefab = await LoadPrefab(values[3]);
                prefabData.summonPrefab = await LoadPrefab(values[4]);
                prefabData.projectilePrefab = await LoadPrefab(values[5]);
                prefabData.hitEffect = await LoadPrefab(values[6]);
                prefabData.eliteOutlineMaterial = await LoadMaterial(values[7]);

                // 프리팹 데이터를 별도 딕셔너리에 저장
                monsterPrefabData[monsterId] = prefabData;
                Debug.Log($"몬스터 프리팹 로드 완료: ID {monsterId}");
            }
            catch (Exception e)
            {
                Debug.LogError($"프리팹 로드 중 오류 발생 (ID: {monsterId}): {e.Message}");
            }
        }
    }

    // Addressables로 프리팹 로드
    private async Task<GameObject> LoadPrefab(string prefabKey)
    {
        if (string.IsNullOrEmpty(prefabKey)) return null;

        
        var handle = Addressables.LoadAssetAsync<GameObject>(prefabKey);
        Debug.Log(prefabKey + "로드완료");
        return await handle.Task;
    }

    // Addressables로 머티리얼 로드
    private async Task<Material> LoadMaterial(string materialKey)
    {
        if (string.IsNullOrEmpty(materialKey)) return null;

        var handle = Addressables.LoadAssetAsync<Material>(materialKey);
        return await handle.Task;
    }

    private async Task LoadStrategyData()
    {
        if (!File.Exists(strategyFilePath))
        {
            Debug.LogError($"전략 데이터 CSV 파일을 찾을 수 없습니다: {strategyFilePath}");
            return;
        }

        string[] lines = File.ReadAllLines(strategyFilePath);
        string[] headers = lines[0].Split(',');

        for (int i = 1; i < lines.Length; i++)
        {
            string[] values = lines[i].Split(',');
            int monsterId = int.Parse(values[0]);

            var strategyDict = new Dictionary<string, string>();
            for (int j = 1; j < headers.Length; j++)
            {
                strategyDict[headers[j]] = values[j];
            }

            strategyData[monsterId] = strategyDict;
        }
    }

    private async Task LoadSkillData()
    {
        if (!File.Exists(skillFilePath))
        {
            Debug.LogError($"스킬 데이터 CSV 파일을 찾을 수 없습니다: {skillFilePath}");
            return;
        }

        string[] lines = File.ReadAllLines(skillFilePath);
        string[] headers = lines[0].Split(',');

        for (int i = 1; i < lines.Length; i++)
        {
            string[] values = lines[i].Split(',');
            int monsterId = int.Parse(values[0]);

            var skillDict = new Dictionary<string, string>();
            for (int j = 1; j < headers.Length; j++)
            {
                skillDict[headers[j]] = values[j];
            }

            skillData[monsterId] = skillDict;
        }
    }

    private async Task LoadMonstersFromCSV()
    {
        if (!File.Exists(monsterFilePath))
        {
            Debug.LogError($"몬스터 데이터 CSV 파일을 찾을 수 없습니다: {monsterFilePath}");
            return;
        }

        string[] lines = File.ReadAllLines(monsterFilePath);
        bool isFirstLine = true;

        foreach (string line in lines)
        {
            if (isFirstLine)
            {
                isFirstLine = false;
                continue;
            }

            if (string.IsNullOrWhiteSpace(line)) continue;

            string[] values = line.Split(',');
            int monsterId = int.Parse(values[0]);
            string monsterDataPath = $"MonsterData_{monsterId}";

            try
            {
                var monsterDataHandle = Addressables.LoadAssetAsync<MonsterData>(monsterDataPath);
                MonsterData monsterData = await monsterDataHandle.Task;

                if (monsterData != null)
                {
                    UpdateMonsterData(monsterData, values, monsterId);

                    // 프리팹 데이터가 있으면 복사
                    if (monsterPrefabData.TryGetValue(monsterId, out MonsterData prefabData))
                    {
                        CopyPrefabData(monsterData, prefabData);
                    }

                    monsterDatabase[monsterId] = monsterData;
                    Debug.Log($"몬스터 로드: ID {monsterId}, 이름 {monsterData.MonsterName}");
                }
                else
                {
                    Debug.LogError($"몬스터 데이터를 찾을 수 없습니다: {monsterDataPath}");
                }
            }
            catch (Exception e)
            {
                Debug.LogError($"몬스터 데이터 로드 중 오류 발생 (ID: {monsterId}): {e.Message}");
            }
        }
    }

    // 프리팹 데이터 복사 헬퍼 메서드
    private void CopyPrefabData(MonsterData target, MonsterData source)
    {
        target.areaEffectPrefab = source.areaEffectPrefab;
        target.shorckEffectPrefab = source.shorckEffectPrefab;
        target.buffEffectPrefab = source.buffEffectPrefab;
        target.summonPrefab = source.summonPrefab;
        target.projectilePrefab = source.projectilePrefab;
        target.hitEffect = source.hitEffect;
        target.eliteOutlineMaterial = source.eliteOutlineMaterial;
    }

    private void UpdateMonsterData(MonsterData monsterData, string[] baseValues, int monsterId)
    {
        // 기본 데이터 업데이트
        monsterData.MonsterName = baseValues[1];
        monsterData.initialHp = int.Parse(baseValues[2]);
        monsterData.initialAttackPower = int.Parse(baseValues[3]);
        monsterData.initialAttackSpeed = float.Parse(baseValues[4]);
        monsterData.initialSpeed = int.Parse(baseValues[5]);
        monsterData.attackRange = float.Parse(baseValues[6]);
        monsterData.dropChance = float.Parse(baseValues[7]);
        monsterData.dropItem = int.Parse(baseValues[8]);
        monsterData.moveRange = int.Parse(baseValues[9]);
        monsterData.chaseRange = int.Parse(baseValues[10]);
        monsterData.monsterPrefabKey = baseValues[11];
        monsterData.Grade = (MonsterGrade)Enum.Parse(typeof(MonsterGrade), baseValues[12]);
        monsterData.armorValue = int.Parse(baseValues[13]);
        monsterData.initialDeffense = int.Parse(baseValues[14]);
        monsterData.aggroDropRange = int.Parse(baseValues[15]);
        monsterData.groggyTime = float.Parse(baseValues[16]);

        // 전략 데이터 업데이트
        if (strategyData.TryGetValue(monsterId, out var strategies))
        {
            monsterData.spawnStrategy = (SpawnStrategyType)Enum.Parse(typeof(SpawnStrategyType), strategies["SpawnStrategy"]);
            monsterData.moveStrategy = (MovementStrategyType)Enum.Parse(typeof(MovementStrategyType), strategies["MoveStrategy"]);
            monsterData.attackStrategy = (AttackStrategyType)Enum.Parse(typeof(AttackStrategyType), strategies["AttackStrategy"]);
            monsterData.idleStrategy = (IdleStrategyType)Enum.Parse(typeof(IdleStrategyType), strategies["IdleStrategy"]);
            monsterData.skillStrategy = (SkillStrategyType)Enum.Parse(typeof(SkillStrategyType), strategies["SkillStrategy"]);
            monsterData.dieStrategy = (DieStrategyType)Enum.Parse(typeof(DieStrategyType), strategies["DieStrategy"]);
            monsterData.hitStrategy = (HitStrategyType)Enum.Parse(typeof(HitStrategyType), strategies["HitStrategy"]);
            monsterData.useHealthRetreat = bool.Parse(strategies["UseHealthRetreat"]);
            monsterData.healthRetreatThreshold = float.Parse(strategies["HealthRetreatThreshold"]);
            monsterData.isPhaseChange = bool.Parse(strategies["IsPhaseChange"]);
        }

        // 스킬 데이터 업데이트
        if (skillData.TryGetValue(monsterId, out var skills))
        {
            monsterData.skillCooldown = float.Parse(skills["SkillCooldown"]);
            monsterData.skillRange = float.Parse(skills["SkillRange"]);
            monsterData.skillDuration = float.Parse(skills["SkillDuration"]);
            monsterData.skillDamage = int.Parse(skills["SkillDamage"]);
            monsterData.hitStunDuration = float.Parse(skills["HitStunDuration"]);
            monsterData.deathDuration = float.Parse(skills["DeathDuration"]);
            monsterData.spawnDuration = float.Parse(skills["SpawnDuration"]);
            monsterData.projectileSpeed = float.Parse(skills["ProjectileSpeed"]);
            monsterData.rotateSpeed = float.Parse(skills["RotateSpeed"]);
            monsterData.areaRadius = float.Parse(skills["AreaRadius"]);

            // 버프 관련 데이터
            string[] buffTypes = skills["BuffTypes"].Split('|');
            string[] buffDurations = skills["BuffDurations"].Split('|');
            string[] buffValues = skills["BuffValues"].Split('|');
           
            monsterData.buffData.buffTypes = new BuffType[buffTypes.Length];
            monsterData.buffData.durations = new float[buffDurations.Length];
            monsterData.buffData.values = new float[buffValues.Length];

            for (int i = 0; i < buffTypes.Length; i++)
            {
                if (buffTypes[i] != "None")
                {
                    monsterData.buffData.buffTypes[i] = (BuffType)Enum.Parse(typeof(BuffType), buffTypes[i]);
                    if (i < buffDurations.Length)
                        monsterData.buffData.durations[i] = float.Parse(buffDurations[i]);
                    if (i < buffValues.Length)
                        monsterData.buffData.values[i] = float.Parse(buffValues[i]);
                }
            }

            monsterData.summonCount = int.Parse(skills["SummonCount"]);
            monsterData.summonRadius = float.Parse(skills["SummonRadius"]);
            monsterData.projectileType = (ProjectileMovementType)Enum.Parse(typeof(ProjectileMovementType), skills["ProjectileType"]);
            monsterData.skillEffectType = (SkillEffectType)Enum.Parse(typeof(SkillEffectType), skills["SkillEffectType"]);
            monsterData.projectileImpactType = (ProjectileImpactType)Enum.Parse(typeof(ProjectileImpactType), skills["ProjectileImpactType"]);
            monsterData.areaDuration = float.Parse(skills["AreaDuration"]);
            monsterData.superArmorThreshold = float.Parse(skills["SuperArmorThreshold"]);
            monsterData.hitStunMultiplier = float.Parse(skills["HitStunMultiplier"]);
            monsterData.knockbackForce = float.Parse(skills["KnockbackForce"]);
            monsterData.cameraShakeIntensity = float.Parse(skills["CameraShakeIntensity"]);
            monsterData.cameraShakeDuration = float.Parse(skills["CameraShakeDuration"]);
            monsterData.shockwaveRadius = float.Parse(skills["ShockwaveRadius"]);
            monsterData.multiShotCount = int.Parse(skills["MultiShotCount"]);
            monsterData.multiShotInterval = float.Parse(skills["MultiShotInterval"]);
            monsterData.projectileRotationAxis = ParseVector3(skills["ProjectileRotationAxis"]);
            monsterData.projectileRotationSpeed = float.Parse(skills["ProjectileRotationSpeed"]);
        }
    }
    private Vector3 ParseVector3(string vectorString)
    {
        string[] components = vectorString.Split('|');
        if (components.Length == 3)
        {
            return new Vector3(
                float.Parse(components[0]),
                float.Parse(components[1]),
                float.Parse(components[2])
            );
        }
        return Vector3.forward; // 기본값 반환
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

    public void ReleaseAllResources()
    {
        monsterDatabase.Clear();
        strategyData.Clear();
        skillData.Clear();
    }
}