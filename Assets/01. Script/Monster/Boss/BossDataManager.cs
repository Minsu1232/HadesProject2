using GSpawn_Pro;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using static MonsterData;

public class BossDataManager : Singleton<BossDataManager>
{
    private Dictionary<int, BossData> bossDatabase = new Dictionary<int, BossData>();
    private Dictionary<int, Dictionary<string, string>> bossBaseData = new Dictionary<int, Dictionary<string, string>>();
    private Dictionary<int, List<Dictionary<string, string>>> bossPhaseData = new Dictionary<int, List<Dictionary<string, string>>>();
    private Dictionary<int, List<Dictionary<string, string>>> bossSkillData = new Dictionary<int, List<Dictionary<string, string>>>();
    private Dictionary<int, List<Dictionary<string, string>>> bossCutsceneData = new Dictionary<int, List<Dictionary<string, string>>>();
    private Dictionary<int, List<Dictionary<string, string>>> bossPatternData = new Dictionary<int, List<Dictionary<string, string>>>();
    private Dictionary<int, List<Dictionary<string, string>>> bossPatternStepData = new Dictionary<int, List<Dictionary<string, string>>>();
    private Dictionary<int, List<Dictionary<string, string>>> bossSkillPrefabData = new Dictionary<int, List<Dictionary<string, string>>>();
    private Dictionary<int, List<Dictionary<string, string>>> bossGimmickData = new Dictionary<int, List<Dictionary<string, string>>>();

    private string bossBasePath;
    private string bossPhasePath;
    private string bossSkillPath;
    private string bossCutscenePath;
    private string bossPatternPath;
    private string bossPatternStepPath;
    private string bossPrefabPath;
    private string bossGimmickPath;

    private void Awake()
    {
        bossBasePath = Path.Combine(Application.persistentDataPath, "BossBase.csv");
        bossPhasePath = Path.Combine(Application.persistentDataPath, "BossPhases.csv");
        bossSkillPath = Path.Combine(Application.persistentDataPath, "BossSkills.csv");
        bossCutscenePath = Path.Combine(Application.persistentDataPath, "BossCutscenes.csv");
        bossPatternPath = Path.Combine(Application.persistentDataPath, "BossPatterns.csv");
        bossPatternStepPath = Path.Combine(Application.persistentDataPath, "BossPatternSteps.csv");
        bossPrefabPath = Path.Combine(Application.persistentDataPath, "BossPrefabs.csv");
        bossGimmickPath = Path.Combine(Application.persistentDataPath, "BossGimmicks.csv");

        CopyCSVFromStreamingAssets();
    }

    private void CopyCSVFromStreamingAssets()
    {
        string[] csvFiles = new string[] { "BossBase.csv", "BossPhases.csv", "BossSkills.csv", "BossCutscenes.csv","BossPatterns.csv","BossPrefabs.csv",
    "BossPatternSteps.csv","BossGimmicks.csv"  };
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
        await LoadBossPrefabs();
        await LoadBossCutsceneData();
        await LoadBossPatternData();      // 추가
        await LoadBossPatternStepData();  // 추가
        await LoadBossGimmickData();
        await InitializeBossData();
    }
    private async Task LoadBossGimmickData()
    {
        string bossGimmickPath = Path.Combine(Application.persistentDataPath, "BossGimmicks.csv");
        if (!File.Exists(bossGimmickPath)) return;

        string[] lines = File.ReadAllLines(bossGimmickPath);
        string[] headers = lines[0].Split(',');

        for (int i = 1; i < lines.Length; i++)
        {
            string[] values = lines[i].Split(',');
            int bossId = int.Parse(values[0]);

            if (!bossGimmickData.ContainsKey(bossId))
            {
                bossGimmickData[bossId] = new List<Dictionary<string, string>>();
            }

            var gimmickDict = new Dictionary<string, string>();
            for (int j = 0; j < headers.Length; j++)
            {
                gimmickDict[headers[j]] = values[j];
            }

            bossGimmickData[bossId].Add(gimmickDict);
        }
    }
    private async Task LoadBossPatternData()
    {
        if (!File.Exists(bossPatternPath)) return;

        string[] lines = File.ReadAllLines(bossPatternPath);
        string[] headers = lines[0].Split(',');

        for (int i = 1; i < lines.Length; i++)
        {
            string[] values = lines[i].Split(',');
            int bossId = int.Parse(values[0]);

            if (!bossPatternData.ContainsKey(bossId))
            {
                bossPatternData[bossId] = new List<Dictionary<string, string>>();
            }

            var patternDict = new Dictionary<string, string>();
            for (int j = 0; j < headers.Length; j++)
            {
                patternDict[headers[j]] = values[j];
            }

            bossPatternData[bossId].Add(patternDict);

            Debug.Log($"[BossDataManager] Loaded BossPatternData: {bossPatternData.Count} bosses.");
        }
    }

    private async Task LoadBossPatternStepData()
    {
        if (!File.Exists(bossPatternStepPath)) return;

        string[] lines = File.ReadAllLines(bossPatternStepPath);
        string[] headers = lines[0].Split(',');

        for (int i = 1; i < lines.Length; i++)
        {
            string[] values = lines[i].Split(',');
            int patternId = int.Parse(values[0]);

            if (!bossPatternStepData.ContainsKey(patternId))
            {
                bossPatternStepData[patternId] = new List<Dictionary<string, string>>();
            }

            var stepDict = new Dictionary<string, string>();
            for (int j = 0; j < headers.Length; j++)
            {
                stepDict[headers[j]] = values[j];
            }

            bossPatternStepData[patternId].Add(stepDict);
        }
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
    private async Task LoadBossPrefabs()
    {
        if (!File.Exists(bossPrefabPath))
        {
            Debug.LogError($"BossPrefab 파일이 존재하지 않습니다: {bossPrefabPath}");
            return;
        }

        string[] lines = File.ReadAllLines(bossPrefabPath);
        string[] headers = lines[0].Split(',');

        for (int i = 1; i < lines.Length; i++)
        {
            string[] values = lines[i].Split(',');
            int bossId = int.Parse(values[0]);

            if (!bossSkillPrefabData.ContainsKey(bossId))
            {
                bossSkillPrefabData[bossId] = new List<Dictionary<string, string>>();
            }

            var prefabDict = new Dictionary<string, string>();
            for (int j = 0; j < headers.Length; j++)
            {
                prefabDict[headers[j]] = values[j];
            }

            bossSkillPrefabData[bossId].Add(prefabDict);
        }

        Debug.Log($"bossSkillPrefabData 로드 완료: {bossSkillPrefabData.Count}개의 보스 프리팹 데이터 로드됨");
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
                Debug.Log($"보스 로드: ID {bossId}, 이름 {bossData.MonsterName}");
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
        if (bossPatternData.TryGetValue(bossId, out var patterns))
        {
            UpdateBossPatternData(bossData, patterns);
        }
        if(bossSkillPrefabData.TryGetValue(bossId, out var skillsPrefab))
        {
            UpdateBossPrefab(bossData, skillsPrefab);
        }
    }
    private void UpdateBossPatternData(BossData bossData, List<Dictionary<string, string>> patterns)
    {
        foreach (var pattern in patterns)
        {
            int patternId = int.Parse(pattern["PatternID"]);
            AttackPatternData patternData = new AttackPatternData
            {
                patternName = pattern["PatternName"],
                patternType = (BossPatternType)Enum.Parse(typeof(BossPatternType), pattern["PatternType"]),
                phaseNumber = int.Parse(pattern["PhaseNumber"]),
                patternCooldown = float.Parse(pattern["PatternCooldown"]),
                warningDuration = float.Parse(pattern["WarningDuration"]),
                healthThresholdMin = float.Parse(pattern["HealthThresholdMin"]),
                healthThresholdMax = float.Parse(pattern["HealthThresholdMax"]),
                warningMessage = pattern["WarningMessage"],
                  // 새로 추가한 필드들
                isDisabled = false,  // 초기값은 false
                requiredSuccessCount = int.Parse(pattern["RequiredSuccessCount"]),
                currentSuccessCount = 0,  // 초기값은 0
                baseDifficulty = float.Parse(pattern["baseDifficulty"]),
                maxDifficulty = float.Parse(pattern["maxDifficulty"]),
                difficultyIncreaseStep = float.Parse(pattern["difficultyIncreaseStep"])
            };

            if (bossPatternStepData.TryGetValue(patternId, out var steps))
            {
                foreach (var step in steps)
                {
                    AttackStepData stepData = new AttackStepData
                    {   
                        
                        attackType = (AttackStrategyType)Enum.Parse(typeof(AttackStrategyType), step["AttackType"]),
                        stepDelay = float.Parse(step["StepDelay"]),
                        hasMiniGame = bool.Parse(step["HasMiniGame"]),
                        miniGameType = (MiniGameType)Enum.Parse(typeof(MiniGameType), step["MiniGameType"]),
                        waitForMiniGame = bool.Parse(step["WaitForMiniGame"]),
                        miniGameDifficulty = float.Parse(step["MiniGameDifficulty"]),
                        isTransitionAnim = bool.Parse(step["IsAnim"])
                    };

                    patternData.steps.Add(stepData);
                }
            }

            // PhaseData에 패턴 추가
            if (patternData.phaseNumber <= bossData.phaseData.Count)
            {
                bossData.phaseData[patternData.phaseNumber].availablePatterns.Add(patternData);
            }

            Debug.Log($"[UpdateBossPatternData] Boss ID: {bossData.MonsterName}, Total Patterns: {patterns.Count}");
            Debug.Log($"[UpdateBossPatternData] Pattern '{patternData.patternName}' added to Phase {patternData.phaseNumber}");
        }


    }
    private void UpdateBossBaseData(BossData bossData, Dictionary<string, string> baseData)
    {
        bossData.MonsterName = baseData["Name"];
        bossData.initialHp = int.Parse(baseData["HP"]);
        bossData.initialAttackPower = int.Parse(baseData["AttackPower"]);
        bossData.initialAttackSpeed = float.Parse(baseData["AttackSpeed"]);
        bossData.initialSpeed = int.Parse(baseData["Speed"]);
        bossData.attackRange = float.Parse(baseData["BasicAttackRange"]);
        bossData.Grade = (MonsterGrade)Enum.Parse(typeof(MonsterGrade), baseData["Grade"]);
        bossData.monsterPrefabKey = baseData["PrefabPath"];
        bossData.moveRange = int.Parse(baseData["MoveRange"]);
        bossData.chaseRange = int.Parse(baseData["ChaseRange"]);
        bossData.initialDeffense = int.Parse(baseData["InitialDefense"]);
        bossData.armorValue = int.Parse(baseData["ArmorValue"]);
        bossData.aggroDropRange = int.Parse(baseData["AggroRange"]);
        bossData.dropChance = int.Parse(baseData["DropItemID"]);
        bossData.dropItem = int.Parse(baseData["DropRate"]);
        bossData.phaseCount = int.Parse(baseData["PhaseCount"]);
        bossData.groggyTime = float.Parse(baseData["GroggyTime"]);     
       
        bossData.rageModeThreshold = float.Parse(baseData["RageModeThreshold"]);    
        
        bossData.showPhaseNames = bool.Parse(baseData["ShowPhaseNames"]);


       //사운드
        string roarSoundKey = baseData["RoarSound"];
        if (!string.IsNullOrEmpty(roarSoundKey))
        {
            Addressables.LoadAssetAsync<AudioClip>(roarSoundKey).Completed += handle =>
            {
                if (handle.Status == AsyncOperationStatus.Succeeded)
                {
                    bossData.roarSound = handle.Result;
                    Debug.Log($"RoarSound 로드 완료: {roarSoundKey}");
                }
                else
                {
                    Debug.LogError($"RoarSound 로드 실패: {roarSoundKey}");
                }
            };
        }

    }
    private void UpdateBossPrefab(BossData bossData, List<Dictionary<string, string>> baseData)
    {foreach(var baseDataes in baseData)
        {
            bossData.areaEffectPrefab = LoadPrefab(baseDataes["AreaEffectPrefab"]);
            bossData.shorckEffectPrefab = LoadPrefab(baseDataes["ShockEffectPrefab"]);
            bossData.buffEffectPrefab = LoadPrefab(baseDataes["BuffEffectPrefab"]);
            bossData.summonPrefab = LoadPrefab(baseDataes["SummonPrefab"]);
            bossData.projectilePrefab = LoadPrefab(baseDataes["ProjectilePrefab"]);
            bossData.hitEffect = LoadPrefab(baseDataes["HitEffect"]);
        }
     
       

    }
    private GameObject LoadPrefab(string prefabKey)
    {
        if (string.IsNullOrEmpty(prefabKey)) return null;

        var handle = Addressables.LoadAssetAsync<GameObject>(prefabKey);

        return handle.Task.Result;
    }
    private void UpdateBossPhaseData(BossData bossData, List<Dictionary<string, string>> phases)
    {
        bossData.phaseData = new List<PhaseData>();
        foreach (var phase in phases)
        {
            PhaseData phaseData = new PhaseData
            {
                phaseName = phase["PhaseName"],
                phaseTransitionThreshold = float.Parse(phase["HealthThreshold"]),
                transitionDuration = float.Parse(phase["TransitionDuration"]),
                isInvulnerableDuringTransition = bool.Parse(phase["IsInvulnerableDuringTransition"]),                
                patternChangeTime = float.Parse(phase["PatternChangeTime"]),

                moveType = (MovementStrategyType)Enum.Parse(typeof(MovementStrategyType), phase["MoveStrategy"]),
                attackType = (AttackStrategyType)Enum.Parse(typeof(AttackStrategyType), phase["AttackStrategy"]),
                skillType = (SkillStrategyType)Enum.Parse(typeof(SkillStrategyType), phase["SkillStrategy"]),
                phaseTransitionType = (PhaseTransitionType)Enum.Parse(typeof(PhaseTransitionType), phase["PhaseTransitionType"]),

                damageMultiplier = float.Parse(phase["DamageMultiplier"]),
                speedMultiplier = float.Parse(phase["SpeedMultiplier"]),
                defenseMultiplier = float.Parse(phase["DefenseMultiplier"]),
                attackSpeedMultiplier = float.Parse(phase["AttackSpeedMultiplier"]),

                canBeInterrupted = bool.Parse(phase["CanBeInterrupted"]),
                stunResistance = float.Parse(phase["StunResistance"]),
                useHealthRetreat = bool.Parse(phase["UseHealthRetreat"]),
                healthRetreatThreshold = float.Parse(phase["HealthRetreatThreshold"])     ,

                phaseAttackStrategies = new List<AttackStrategyWeight>()

            };
            // 콤마로 구분된 전략 타입과 가중치 파싱
            string[] strategyTypes = phase["AttackStrategies"].Split('|');
            string[] strategyWeights = phase["StrategyWeights"].Split('|');

            for (int i = 0; i < strategyTypes.Length; i++)
            {
                if (!string.IsNullOrEmpty(strategyTypes[i]))
                {
                    var strategyWeight = new AttackStrategyWeight
                    {
                        type = (AttackStrategyType)Enum.Parse(typeof(AttackStrategyType), strategyTypes[i]),
                        weight = float.Parse(strategyWeights[i])
                    };
                    phaseData.phaseAttackStrategies.Add(strategyWeight);
                }
            }

            // 나머지 기존 코드...
            bossData.phaseData.Add(phaseData);
            // 해당 페이즈의 기믹 데이터 찾기
            if (bossGimmickData.TryGetValue(int.Parse(phase["BossID"]), out var gimmickList))
            {
                foreach (var gimmickData in gimmickList)
                {
                    // 페이즈 번호가 일치하는 기믹만 추가
                    if (int.Parse(gimmickData["PhaseNumber"]) == int.Parse(phase["PhaseNumber"]))
                    {
                        GimmickData gimmick = new GimmickData
                        {
                            gimmickName = gimmickData["GimmickName"],
                            type = (GimmickType)Enum.Parse(typeof(GimmickType), gimmickData["Type"]),
                            triggerHealthThreshold = float.Parse(gimmickData["TriggerHealth"]),
                            duration = float.Parse(gimmickData["Duration"]),
                            requirePlayerAction = bool.Parse(gimmickData["RequireAction"]),
                            isInterruptible = bool.Parse(gimmickData["Interruptible"]),                           
                            makeInvulnerable = bool.Parse(gimmickData["MakeInvulnerable"]),
                            damageMultiplier = float.Parse(gimmickData["DamageMultiplier"]),
                            failDamage = float.Parse(gimmickData["FailDamage"]),
                            damage = float.Parse(gimmickData["Damage"]),
                            useCustomPosition = bool.Parse(gimmickData["UseCustomPosition"]),
                            gimmickPosition = new Vector3(
                                float.Parse(gimmickData["PosX"]),
                                float.Parse(gimmickData["PosY"]),
                                float.Parse(gimmickData["PosZ"])
                            ),
                            areaRadius = float.Parse(gimmickData["Radius"]),
                            preparationTime = float.Parse(gimmickData["PreparationTime"]),
                            repeatInterval = float.Parse(gimmickData["RepeatInterval"]),
                            affectStatusEffects = bool.Parse(gimmickData["affectStatusEffects"]),
                            followTarget = bool.Parse(gimmickData["FollowTarget"]),
                            collisionMask = ParseLayerMask(gimmickData["CollisionMask"]),
                            successCount = int.Parse(gimmickData["SuccessCount"]),
                            moveSpeed = float.Parse(gimmickData["MoveSpeed"]),
                            hazardSpawnType = (HazardSpawnType)Enum.Parse(typeof(HazardSpawnType), gimmickData["HazardSpawnType"]),
                            targetType = (TargetType)Enum.Parse(typeof(TargetType), gimmickData["TargetType"]),
                            hazardPrefabKey = gimmickData["hazardPrefabKey"]
                        };

                        Addressables.LoadAssetAsync<GameObject>(gimmick.hazardPrefabKey).Completed += handle =>
                        {
                            if (handle.Status == AsyncOperationStatus.Succeeded)
                            {
                                gimmick.hazardPrefab = handle.Result;  // GetComponent 제거하고 직접 저장

                                Debug.Log($"[GimmickData] {gimmick.gimmickName}의 Prefab 로드 완료: {gimmick.hazardPrefabKey}");
                            }
                            else
                            {
                                Debug.LogError($"[GimmickData] {gimmick.gimmickName}의 Prefab 로드 실패: {gimmick.hazardPrefabKey}");
                            }
                        };
                        phaseData.gimmicks.Add(gimmick);
                    }
                }
            }

            bossData.phaseData.Add(phaseData);
        }
       
    }
    // LayerMask 파싱 함수
    private LayerMask ParseLayerMask(string layerString)
    {
        if (string.IsNullOrEmpty(layerString))
            return 0; // 기본값

        LayerMask mask = 0;
        string[] layers = layerString.Split('|');

        foreach (string layer in layers)
        {
            string trimmedLayer = layer.Trim();
            if (!string.IsNullOrEmpty(trimmedLayer))
            {
                mask |= (1 << LayerMask.NameToLayer(trimmedLayer));
            }
        }

        return mask;
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
        bossPatternData.Clear();
        bossPatternStepData.Clear();
    }
}