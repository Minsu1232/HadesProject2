using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.AddressableAssets;
using static MonsterData;

public class BossDataManager : Singleton<BossDataManager>
{
    private Dictionary<int, BossData> bossDatabase = new Dictionary<int, BossData>();
    private Dictionary<int, Dictionary<string, string>> bossBaseData = new Dictionary<int, Dictionary<string, string>>();
    private Dictionary<int, List<Dictionary<string, string>>> bossPhaseData = new Dictionary<int, List<Dictionary<string, string>>>();
    private Dictionary<int, List<Dictionary<string, string>>> bossSkillData = new Dictionary<int, List<Dictionary<string, string>>>();
    private Dictionary<int, List<Dictionary<string, string>>> bossCutsceneData = new Dictionary<int, List<Dictionary<string, string>>>();

    private string bossBasePath;
    private string bossPhasePath;
    private string bossSkillPath;
    private string bossCutscenePath;

    private void Awake()
    {
        bossBasePath = Path.Combine(Application.persistentDataPath, "BossBase.csv");
        bossPhasePath = Path.Combine(Application.persistentDataPath, "BossPhases.csv");
        bossSkillPath = Path.Combine(Application.persistentDataPath, "BossSkills.csv");
        bossCutscenePath = Path.Combine(Application.persistentDataPath, "BossCutscenes.csv");

        CopyCSVFromStreamingAssets();
    }

    private void CopyCSVFromStreamingAssets()
    {
        string[] csvFiles = new string[] { "BossBase.csv", "BossPhases.csv", "BossSkills.csv", "BossCutscenes.csv" };
        foreach (string fileName in csvFiles)
        {
            string streamingPath = Path.Combine(Application.streamingAssetsPath, fileName);
            string persistentPath = Path.Combine(Application.persistentDataPath, fileName);

            if (File.Exists(streamingPath))
            {
                File.Copy(streamingPath, persistentPath, true);
                Debug.Log($"보스 CSV 파일 복사 완료: {fileName}");
            }
            else
            {
                Debug.LogError($"StreamingAssets에서 {fileName} 파일을 찾을 수 없습니다.");
            }
        }
    }

    public async Task InitializeBosses()
    {
        await LoadAllBossData();
    }

    private async Task LoadAllBossData()
    {
        await LoadBossBaseData();
        await LoadBossPhaseData();
        await LoadBossSkillData();
        await LoadBossCutsceneData();
        await InitializeBossData();
    }

    private async Task LoadBossBaseData()
    {
        if (!File.Exists(bossBasePath))
        {
            Debug.LogError($"보스 기본 데이터 CSV 파일을 찾을 수 없습니다: {bossBasePath}");
            return;
        }

        string[] lines = File.ReadAllLines(bossBasePath);
        string[] headers = lines[0].Split(',');

        for (int i = 1; i < lines.Length; i++)
        {
            string[] values = lines[i].Split(',');
            int bossId = int.Parse(values[0]);

            var baseDict = new Dictionary<string, string>();
            for (int j = 0; j < headers.Length; j++)
            {
                baseDict[headers[j]] = values[j];
            }

            bossBaseData[bossId] = baseDict;
        }
    }

    private async Task LoadBossPhaseData()
    {
        if (!File.Exists(bossPhasePath)) return;

        string[] lines = File.ReadAllLines(bossPhasePath);
        string[] headers = lines[0].Split(',');

        for (int i = 1; i < lines.Length; i++)
        {
            string[] values = lines[i].Split(',');
            int bossId = int.Parse(values[0]);

            if (!bossPhaseData.ContainsKey(bossId))
            {
                bossPhaseData[bossId] = new List<Dictionary<string, string>>();
            }

            var phaseDict = new Dictionary<string, string>();
            for (int j = 0; j < headers.Length; j++)
            {
                phaseDict[headers[j]] = values[j];
            }

            bossPhaseData[bossId].Add(phaseDict);
        }
    }

    private async Task LoadBossSkillData()
    {
        if (!File.Exists(bossSkillPath)) return;

        string[] lines = File.ReadAllLines(bossSkillPath);
        string[] headers = lines[0].Split(',');

        for (int i = 1; i < lines.Length; i++)
        {
            string[] values = lines[i].Split(',');
            int bossId = int.Parse(values[0]);

            if (!bossSkillData.ContainsKey(bossId))
            {
                bossSkillData[bossId] = new List<Dictionary<string, string>>();
            }

            var skillDict = new Dictionary<string, string>();
            for (int j = 0; j < headers.Length; j++)
            {
                skillDict[headers[j]] = values[j];
            }

            bossSkillData[bossId].Add(skillDict);
        }
    }

    private async Task LoadBossCutsceneData()
    {
        if (!File.Exists(bossCutscenePath)) return;

        string[] lines = File.ReadAllLines(bossCutscenePath);
        string[] headers = lines[0].Split(',');

        for (int i = 1; i < lines.Length; i++)
        {
            string[] values = lines[i].Split(',');
            int bossId = int.Parse(values[0]);

            if (!bossCutsceneData.ContainsKey(bossId))
            {
                bossCutsceneData[bossId] = new List<Dictionary<string, string>>();
            }

            var cutsceneDict = new Dictionary<string, string>();
            for (int j = 0; j < headers.Length; j++)
            {
                cutsceneDict[headers[j]] = values[j];
            }

            bossCutsceneData[bossId].Add(cutsceneDict);
        }
    }

    private async Task InitializeBossData()
    {
        foreach (var kvp in bossBaseData)
        {
            int bossId = kvp.Key;
            var baseData = kvp.Value;

            string bossDataPath = $"BossData_{bossId}";
            var bossDataHandle = Addressables.LoadAssetAsync<BossData>(bossDataPath);
            BossData bossData = await bossDataHandle.Task;

            if (bossData != null)
            {
                UpdateBossData(bossData, bossId);
                bossDatabase[bossId] = bossData;
                Debug.Log($"보스 로드: ID {bossId}, 이름 {bossData.monsterName}");
            }
            else
            {
                Debug.LogError($"보스 데이터를 찾을 수 없습니다: {bossDataPath}");
            }
        }
    }

    private void UpdateBossData(BossData bossData, int bossId)
    {
        if (bossBaseData.TryGetValue(bossId, out var baseData))
        {
            UpdateBossBaseData(bossData, baseData);
        }

        if (bossPhaseData.TryGetValue(bossId, out var phases))
        {
            UpdateBossPhaseData(bossData, phases);
        }

        if (bossSkillData.TryGetValue(bossId, out var skills))
        {
            UpdateBossSkillData(bossData, skills);
        }

        //if (bossCutsceneData.TryGetValue(bossId, out var cutscenes))
        //{
        //    UpdateBossCutsceneData(bossData, cutscenes);
        //}
    }

    private void UpdateBossBaseData(BossData bossData, Dictionary<string, string> baseData)
    {
        bossData.monsterName = baseData["Name"];
        bossData.initialHp = int.Parse(baseData["HP"]);
        bossData.initialAttackPower = int.Parse(baseData["AttackPower"]);
        bossData.initialAttackSpeed = float.Parse(baseData["AttackSpeed"]);
        bossData.initialSpeed = int.Parse(baseData["Speed"]);
        bossData.attackRange = float.Parse(baseData["BasicAttackRange"]);
        bossData.grade = (MonsterGrade)Enum.Parse(typeof(MonsterGrade), baseData["Grade"]);
        bossData.monsterPrefabKey = baseData["PrefabPath"];
        bossData.moveRange = int.Parse(baseData["MoveRange"]);
        bossData.chaseRange = int.Parse(baseData["ChaseRange"]);
        bossData.initialDeffense = int.Parse(baseData["InitialDefense"]);
        bossData.armorValue = int.Parse(baseData["ArmorValue"]);
        bossData.aggroDropRange = int.Parse(baseData["AggroRange"]);
        bossData.dropChance = int.Parse(baseData["DropItemID"]);
        bossData.dropItem = int.Parse(baseData["DropRate"]);
        bossData.phaseCount = int.Parse(baseData["PhaseCount"]);

        
        bossData.phaseTransitionDuration = float.Parse(baseData["PhaseTransitionDuration"]);
        bossData.rageModeThreshold = float.Parse(baseData["RageModeThreshold"]);    
        
        bossData.showPhaseNames = bool.Parse(baseData["ShowPhaseNames"]);
    }

    private void UpdateBossPhaseData(BossData bossData, List<Dictionary<string, string>> phases)
    {
        bossData.phaseData = new List<PhaseData>();
        foreach (var phase in phases)
        {
            PhaseData phaseData = new PhaseData
            {
                phaseName = phase["PhaseName"],
                healthThreshold = float.Parse(phase["HealthThreshold"]),
                transitionDuration = float.Parse(phase["TransitionDuration"]),
                isInvulnerableDuringTransition = bool.Parse(phase["IsInvulnerableDuringTransition"]),

                patternChangeTime = float.Parse(phase["PatternChangeTime"]),

                moveType = (MovementStrategyType)Enum.Parse(typeof(MovementStrategyType), phase["MoveStrategy"]),
                attackType = (AttackStrategyType)Enum.Parse(typeof(AttackStrategyType), phase["AttackStrategy"]),
                skillType = (SkillStrategyType)Enum.Parse(typeof(SkillStrategyType), phase["SkillStrategy"]),

                damageMultiplier = float.Parse(phase["DamageMultiplier"]),
                speedMultiplier = float.Parse(phase["SpeedMultiplier"]),
                defenseMultiplier = float.Parse(phase["DefenseMultiplier"]),
                attackSpeedMultiplier = float.Parse(phase["AttackSpeedMultiplier"]),

                canBeInterrupted = bool.Parse(phase["CanBeInterrupted"]),
                stunResistance = float.Parse(phase["StunResistance"]),
                useHealthRetreat = bool.Parse(phase["UseHealthRetreat"]),
                healthRetreatThreshold = float.Parse(phase["HealthRetreatThreshold"])

            };

            bossData.phaseData.Add(phaseData);
        }
    }

    private void UpdateBossSkillData(BossData bossData, List<Dictionary<string, string>> skills)
    {
        foreach (var skill in skills)
        {
            int phaseNumber = int.Parse(skill["PhaseNumber"]);
            if (phaseNumber - 1 < bossData.phaseData.Count)
            {
                PhaseData phaseData = bossData.phaseData[phaseNumber - 1];

                bossData.skillCooldown = float.Parse(skill["SkillCooldown"]);
                bossData.skillRange = float.Parse(skill["SkillRange"]);
                bossData.skillDuration = float.Parse(skill["SkillDuration"]);
                bossData.skillDamage = int.Parse(skill["SkillDamage"]);
                bossData.projectileType = (ProjectileMovementType)Enum.Parse(typeof(ProjectileMovementType), skill["ProjectileType"]);
                bossData.skillEffectType = (SkillEffectType)Enum.Parse(typeof(SkillEffectType), skill["SkillEffectType"]);
                bossData.projectileSpeed = float.Parse(skill["ProjectileSpeed"]);
                bossData.areaRadius = float.Parse(skill["AreaRadius"]);

                bossData.hitStunDuration = float.Parse(skill["HitStunDuration"]);
                bossData.knockbackForce = float.Parse(skill["KnockbackForce"]);
                bossData.cameraShakeIntensity = float.Parse(skill["CameraShakeIntensity"]);
                bossData.cameraShakeDuration = float.Parse(skill["CameraShakeDuration"]);
                bossData.shockwaveRadius = float.Parse(skill["ShockwaveRadius"]);
            }
        }
    }

    //private void UpdateBossCutsceneData(BossData bossData, List<Dictionary<string, string>> cutscenes)
    //{
    //    foreach (var cutscene in cutscenes)
    //    {
    //        int phaseNumber = int.Parse(cutscene["PhaseNumber"]);
    //        if (phaseNumber - 1 < bossData.phaseData.Count)
    //        {
    //            PhaseData phaseData = bossData.phaseData[phaseNumber - 1];

    //            string[] cameraPos = cutscene["CameraPosition"].Split('|');
    //            string[] cameraRot = cutscene["CameraRotation"].Split('|');
    //            float duration = float.Parse(cutscene["Duration"]);
    //            bool lookAtTarget = bool.Parse(cutscene["LookAtTarget"]);
    //            float shakeIntensity = float.Parse(cutscene["ShakeIntensity"]);
    //            float shakeDuration = float.Parse(cutscene["ShakeDuration"]);
    //            string bgm = cutscene["BackgroundMusic"];
    //            string sfx = cutscene["SoundEffect"];
    //            string uiEffect = cutscene["UIEffect"];
    //            string screenEffect = cutscene["ScreenEffect"];
    //        }
    //    }
    //}

    public BossData GetBossData(int bossId)
    {
        if (bossDatabase.TryGetValue(bossId, out BossData data))
        {
            return data;
        }
        Debug.LogWarning($"보스 데이터를 찾을 수 없습니다: ID {bossId}");
        return null;
    }

    public void ReleaseAllResources()
    {
        bossDatabase.Clear();
        bossBaseData.Clear();
        bossPhaseData.Clear();
        bossSkillData.Clear();
        bossCutsceneData.Clear();
    }
}