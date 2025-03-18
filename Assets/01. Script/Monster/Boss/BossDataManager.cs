using GSpawn_Pro;
using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
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


    // é�ͺ�������������
    private Dictionary<string, Dictionary<string, string>> alexanderBossData = new Dictionary<string, Dictionary<string, string>>();

    // ������ ��ų ������ ������ ���� ��ųʸ� �߰�
    private Dictionary<int, Dictionary<int, GameObject>> bossSkillPrefabMap = new Dictionary<int, Dictionary<int, GameObject>>();
    private Dictionary<int, Dictionary<int, GameObject>> bossSkillImpactPrefabMap = new Dictionary<int, Dictionary<int, GameObject>>();
    private Dictionary<int, Dictionary<int, GameObject>> bossSkillAreaPrefabMap = new Dictionary<int, Dictionary<int, GameObject>>();
    private Dictionary<int, Dictionary<int, GameObject>> howlEffectPrefabMap = new Dictionary<int, Dictionary<int, GameObject>>();
    private Dictionary<int, Dictionary<int, AudioClip>> bossSkillSoundMap = new Dictionary<int, Dictionary<int, AudioClip>>();
    // �ε������� ������ �� �߰�
    private Dictionary<int, Dictionary<int, GameObject>> indicatorPrefabMap = new Dictionary<int, Dictionary<int, GameObject>>();


    private string bossBasePath;
    private string bossPhasePath;
    private string bossSkillPath;
    private string bossCutscenePath;
    private string bossPatternPath;
    private string bossPatternStepPath;
    private string bossPrefabPath;
    private string bossGimmickPath;
    private string alexanderBossDataPath;

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
        alexanderBossDataPath = Path.Combine(Application.persistentDataPath, "AlexanderBossData.csv");


        CopyCSVFromStreamingAssets();
    }

    private void CopyCSVFromStreamingAssets()
    {
        string[] csvFiles = new string[] { "BossBase.csv", "BossPhases.csv", "BossSkills.csv", "BossCutscenes.csv","BossPatterns.csv","BossPrefabs.csv",
    "BossPatternSteps.csv","BossGimmicks.csv","AlexanderBossData.csv"};
        foreach (string fileName in csvFiles)
        {
            string streamingPath = Path.Combine(Application.streamingAssetsPath, fileName);
            string persistentPath = Path.Combine(Application.persistentDataPath, fileName);

            if (File.Exists(streamingPath))
            {
                File.Copy(streamingPath, persistentPath, true);
                Debug.Log($"���� CSV ���� ���� �Ϸ�: {fileName}");
            }
            else
            {
                Debug.LogError($"StreamingAssets���� {fileName} ������ ã�� �� �����ϴ�.");
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
        await LoadBossPatternData();      // �߰�
        await LoadBossPatternStepData();  // �߰�
        await LoadAlexanderBossData();    // �߰�
        await LoadBossGimmickData();
        await InitializeBossData();
    }
    private async Task LoadAlexanderBossData()
    {
        if (!File.Exists(alexanderBossDataPath))
        {
            Debug.LogError($"�˷���� ���� ������ CSV ������ ã�� �� �����ϴ�: {alexanderBossDataPath}");
            return;
        }

        string[] lines = File.ReadAllLines(alexanderBossDataPath);
        string[] headers = lines[0].Split(',');

        for (int i = 1; i < lines.Length; i++)
        {
            string[] values = lines[i].Split(',');
            string bossName = values[0];

            var dataDict = new Dictionary<string, string>();
            for (int j = 0; j < headers.Length; j++)
            {
                dataDict[headers[j]] = values[j];
            }

            alexanderBossData[bossName] = dataDict;
            Debug.Log($"�˷���� Ư�� ������ �ε�: {bossName}");
        }
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
            Debug.LogError($"���� �⺻ ������ CSV ������ ã�� �� �����ϴ�: {bossBasePath}");
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
        try
        {
            if (!File.Exists(bossPrefabPath))
            {
                Debug.LogError($"BossPrefab ������ �������� �ʽ��ϴ�: {bossPrefabPath}");
                return;
            }

            string[] lines = File.ReadAllLines(bossPrefabPath);
            if (lines.Length <= 1)
            {
                Debug.LogWarning("BossPrefabs.csv ���Ͽ� �����Ͱ� �����ϴ�.");
                return;
            }

            string[] headers = lines[0].Split(',');
            Debug.Log($"BossPrefabs.csv ���: {string.Join(", ", headers)}");

            for (int i = 1; i < lines.Length; i++)
            {
                try
                {
                    string[] values = lines[i].Split(',');
                    Debug.Log($"�м� ���� �� {i}: {lines[i]}");

                    // ���� ������ ��� ������ �ٸ� ��� ó��
                    if (values.Length != headers.Length)
                    {
                        Debug.LogWarning($"�� {i}�� �� ����({values.Length})�� ��� ����({headers.Length})�� �ٸ��ϴ�. �� ���� �ǳʶݴϴ�.");
                        continue;
                    }

                    // BossId �Ľ̿� �����ϸ� �ǳʶٱ�
                    if (!int.TryParse(values[0], out int bossId))
                    {
                        Debug.LogWarning($"�� {i}�� BossId �Ľ� ����: {values[0]}");
                        continue;
                    }

                    // ��ųʸ� �ʱ�ȭ
                    if (!bossSkillPrefabData.ContainsKey(bossId))
                    {
                        bossSkillPrefabData[bossId] = new List<Dictionary<string, string>>();
                    }

                    var prefabDict = new Dictionary<string, string>();
                    for (int j = 0; j < headers.Length; j++)
                    {
                        if (j < values.Length)
                        {
                            prefabDict[headers[j]] = values[j];
                        }
                        else
                        {
                            prefabDict[headers[j]] = ""; // ���� ������ �� ���ڿ��� ����
                        }
                    }

                    bossSkillPrefabData[bossId].Add(prefabDict);

                    // ��ų�� ������ ���� ó�� �߰�
                    await ProcessSkillPrefabMapping(bossId, prefabDict);
                }
                catch (Exception lineEx)
                {
                    Debug.LogError($"�� {i} ó�� �� ����: {lineEx.Message}\n{lineEx.StackTrace}");
                    // Ư�� ���� ������ ��ü �ε带 �ߴ����� �ʵ��� �մϴ�
                    continue;
                }
            }

            Debug.Log($"bossSkillPrefabData �ε� �Ϸ�: {bossSkillPrefabData.Count}���� ���� ������ ������ �ε��");
        }
        catch (Exception e)
        {
            Debug.LogError($"LoadBossPrefabs ��ü ����: {e.Message}\n{e.StackTrace}");
            File.WriteAllText(Path.Combine(Application.persistentDataPath, "prefab_load_error.log"),
                $"Error: {e.Message}\n{e.StackTrace}");
        }
    }

    private async Task ProcessSkillPrefabMapping(int bossId, Dictionary<string, string> prefabDict)
    {
        try
        {
            // SkillConfigID ��ȿ�� �˻�
            string skillConfigIdStr = prefabDict.ContainsKey("SkillConfigID") ? prefabDict["SkillConfigID"] : "";

            if (string.IsNullOrEmpty(skillConfigIdStr) || !int.TryParse(skillConfigIdStr, out int skillConfigId))
            {
                // SkillConfigID�� ��ȿ���� ������ �ǳʶݴϴ�
                return;
            }

            // ������Ÿ�� ������ ó��
            string projectilePrefabPath = prefabDict.ContainsKey("ProjectilePrefab") ? prefabDict["ProjectilePrefab"] : "";
            if (!string.IsNullOrEmpty(projectilePrefabPath))
            {
                try
                {
                    var handle = Addressables.LoadAssetAsync<GameObject>(projectilePrefabPath);
                    // �ڷ�ƾ ��� �ݹ� ���
                    handle.Completed += op =>
                    {
                        if (op.Status == AsyncOperationStatus.Succeeded)
                        {
                            if (!bossSkillPrefabMap.ContainsKey(bossId))
                                bossSkillPrefabMap[bossId] = new Dictionary<int, GameObject>();

                            bossSkillPrefabMap[bossId][skillConfigId] = op.Result;
                            Debug.Log($"���� {bossId}�� ��ų {skillConfigId}�� ������ {projectilePrefabPath} ���ε�");
                        }
                        else
                        {
                            Debug.LogWarning($"���� {bossId}�� ��ų {skillConfigId}�� ���� ������ �ε� ����: {projectilePrefabPath}");
                        }
                    };
                }
                catch (Exception e)
                {
                    Debug.LogError($"������ �ε� �� ����: {e.Message}");
                    // �ε� ������ �ִ��� ��� ����
                }
            }

            // ��Ʈ ����Ʈ ������ ó��
            string hitEffectPath = prefabDict.ContainsKey("HitEffect") ? prefabDict["HitEffect"] : "";
            if (!string.IsNullOrEmpty(hitEffectPath))
            {
                try
                {
                    var handle = Addressables.LoadAssetAsync<GameObject>(hitEffectPath);
                    // �ڷ�ƾ ��� �ݹ� ���
                    handle.Completed += op =>
                    {
                        if (op.Status == AsyncOperationStatus.Succeeded)
                        {
                            if (!bossSkillImpactPrefabMap.ContainsKey(bossId))
                                bossSkillImpactPrefabMap[bossId] = new Dictionary<int, GameObject>();

                            bossSkillImpactPrefabMap[bossId][skillConfigId] = op.Result;
                            Debug.Log($"���� {bossId}�� ��ų {skillConfigId}�� ��Ʈ ����Ʈ {hitEffectPath} ���ε�");
                        }
                        else
                        {
                            Debug.LogWarning($"���� {bossId}�� ��ų {skillConfigId}�� ���� ��Ʈ ����Ʈ �ε� ����: {hitEffectPath}");
                        }
                    };
                }
                catch (Exception e)
                {
                    Debug.LogError($"��Ʈ ����Ʈ �ε� �� ����: {e.Message}");
                    // �ε� ������ �ִ��� ��� ����
                }
            }
            string areaEffectPath = prefabDict.ContainsKey("AreaEffectPrefab") ? prefabDict["AreaEffectPrefab"] : "";
            if (!string.IsNullOrEmpty(areaEffectPath))
            {
                try
                {
                    var handle = Addressables.LoadAssetAsync<GameObject>(areaEffectPath);
                    // �ڷ�ƾ ��� �ݹ� ���
                    handle.Completed += op =>
                    {
                        if (op.Status == AsyncOperationStatus.Succeeded)
                        {
                            if (!bossSkillAreaPrefabMap.ContainsKey(bossId))
                                bossSkillAreaPrefabMap[bossId] = new Dictionary<int, GameObject>();

                            bossSkillAreaPrefabMap[bossId][skillConfigId] = op.Result;
                            Debug.Log($"���� {bossId}�� ��ų {skillConfigId}�� ����� ����Ʈ {areaEffectPath} ���ε�");
                        }
                        else
                        {
                            Debug.LogWarning($"���� {bossId}�� ��ų {skillConfigId}�� ���� ��Ʈ ����Ʈ �ε� ����: {hitEffectPath}");
                        }
                    };
                }
                catch (Exception e)
                {
                    Debug.LogError($"��Ʈ ����Ʈ �ε� �� ����: {e.Message}");
                    // �ε� ������ �ִ��� ��� ����
                }
            }
            // �Ͽ� ����Ʈ ������ ó��
            string howlEffectPrefabPath = prefabDict.ContainsKey("HowlEffectPrefabKey") ? prefabDict["HowlEffectPrefabKey"] : "";
            if (!string.IsNullOrEmpty(howlEffectPrefabPath))
            {
                try
                {
                    var handle = Addressables.LoadAssetAsync<GameObject>(howlEffectPrefabPath);
                    // �ڷ�ƾ ��� �ݹ� ���
                    handle.Completed += op =>
                    {
                        if (op.Status == AsyncOperationStatus.Succeeded)
                        {
                            if (!howlEffectPrefabMap.ContainsKey(bossId))
                                howlEffectPrefabMap[bossId] = new Dictionary<int, GameObject>();

                            howlEffectPrefabMap[bossId][skillConfigId] = op.Result;
                            Debug.Log($"���� {bossId}�� ��ų {skillConfigId}�� �Ͽ� ����Ʈ {howlEffectPrefabPath} ���ε�");
                        }
                        else
                        {
                            Debug.LogWarning($"���� {bossId}�� ��ų {skillConfigId}�� ���� �Ͽ� ����Ʈ �ε� ����: {howlEffectPrefabPath}");
                        }
                    };
                }
                catch (Exception e)
                {
                    Debug.LogError($"�Ͽ� ����Ʈ ������ �ε� �� ����: {e.Message}");
                    // �ε� ������ �ִ��� ��� ����
                }
            }
            // ProcessSkillPrefabMapping�� �ε������� ó�� ���� �߰�
            string indicatorPrefabPath = prefabDict.ContainsKey("CircleIndicatorPrefab") ? prefabDict["CircleIndicatorPrefab"] : "";
            if (!string.IsNullOrEmpty(indicatorPrefabPath))
            {
                try
                {
                    var handle = Addressables.LoadAssetAsync<GameObject>(indicatorPrefabPath);
                    handle.Completed += op =>
                    {
                        if (op.Status == AsyncOperationStatus.Succeeded)
                        {
                            if (!indicatorPrefabMap.ContainsKey(bossId))
                                indicatorPrefabMap[bossId] = new Dictionary<int, GameObject>();

                            indicatorPrefabMap[bossId][skillConfigId] = op.Result;
                            Debug.Log($"���� {bossId}�� ��ų {skillConfigId}�� �ε������� {indicatorPrefabPath} ���ε�");
                        }
                        else
                        {
                            Debug.LogWarning($"���� {bossId}�� ��ų {skillConfigId}�� ���� �ε������� �ε� ����: {indicatorPrefabPath}");
                        }
                    };
                }
                catch (Exception e)
                {
                    Debug.LogError($"�ε������� ������ �ε� �� ����: {e.Message}");
                }
            }
        }
        catch (Exception e)
        {
            Debug.LogError($"��ų ������ ���� ó�� �� ����: {e.Message}");
        }
    }
    // ��ų�� ������ �������� �޼���
    public GameObject GetSkillPrefab(int bossId, int skillConfigId)
    {
        if (bossSkillPrefabMap.TryGetValue(bossId, out var prefabMap) &&
            prefabMap.TryGetValue(skillConfigId, out var prefab))
        {
            return prefab;
        }

        // ������ ������ ������ �⺻ ������Ÿ�� ������ ��ȯ
        var bossData = GetBossData(bossId);
        return bossData?.projectilePrefab;
    }
    public GameObject GetSkillImpactPrefab(int bossId, int skillConfigId)
    {
        try
        {
            // �켱 ���ε� ��ų�� ��Ʈ ����Ʈ�� �ִ��� Ȯ��
            if (bossSkillImpactPrefabMap.TryGetValue(bossId, out var prefabMap) &&
                prefabMap.TryGetValue(skillConfigId, out var prefab) &&
                prefab != null)
            {
                return prefab;
            }

            // ������ ������ ������ �⺻ ����Ʈ ������ ��ȯ
            var bossData = GetBossData(bossId);
            if (bossData?.hitEffect != null)
            {
                return bossData.hitEffect;
            }
            else
            {
                Debug.LogWarning($"���� {bossId}�� ��ų {skillConfigId}�� ���� ��Ʈ ����Ʈ�� �����ϴ�. �⺻ ����Ʈ�� ����.");
                return null;
            }
        }
        catch (Exception e)
        {
            Debug.LogError($"GetSkillImpactPrefab ����: {e.Message}\n{e.StackTrace}");
            return null;
        }
    }
    public GameObject GetSkillAreaPrefab(int bossId, int skillConfigId)
    {
        try
        {
            // �켱 ���ε� ��ų�� ��Ʈ ����Ʈ�� �ִ��� Ȯ��
            if (bossSkillAreaPrefabMap.TryGetValue(bossId, out var prefabMap) &&
                prefabMap.TryGetValue(skillConfigId, out var prefab) &&
                prefab != null)
            {
                return prefab;
            }

            // ������ ������ ������ �⺻ ����Ʈ ������ ��ȯ
            var bossData = GetBossData(bossId);
            if (bossData?.areaEffectPrefab != null)
            {
                return bossData.areaEffectPrefab;
            }
            else
            {
                Debug.LogWarning($"���� {bossId}�� ��ų {skillConfigId}�� ���� ����� ����Ʈ�� �����ϴ�. �⺻ ����Ʈ�� ����.");
                return null;
            }
        }
        catch (Exception e)
        {
            Debug.LogError($"GetSkillImpactPrefab ����: {e.Message}\n{e.StackTrace}");
            return null;
        }
    }
    public GameObject GetHowlEffectPrefab(int bossId, int skillConfigId)
    {
        try
        {
            // �켱 ���ε� ��ų�� �Ͽ� ����Ʈ�� �ִ��� Ȯ��
            if (howlEffectPrefabMap.TryGetValue(bossId, out var prefabMap) &&
                prefabMap.TryGetValue(skillConfigId, out var prefab) &&
                prefab != null)
            {
                return prefab;
            }

            // ������ ������ ������ �⺻ �Ͽ� ����Ʈ ������ ��ȯ
            var bossData = GetBossData(bossId);
            if (bossData?.howlEffectPrefab != null)
            {
                return bossData.howlEffectPrefab;
            }
            else
            {
                Debug.LogWarning($"���� {bossId}�� ��ų {skillConfigId}�� ���� �Ͽ� ����Ʈ�� �����ϴ�. �⺻ ����Ʈ�� ����.");
                return null;
            }
        }
        catch (Exception e)
        {
            Debug.LogError($"GetHowlEffectPrefab ����: {e.Message}\n{e.StackTrace}");
            return null;
        }
    }
    // GetIndicatorPrefab �޼��� �߰�
    public GameObject GetIndicatorPrefab(int bossId, int skillConfigId)
    {
        try
        {
            if (indicatorPrefabMap.TryGetValue(bossId, out var prefabMap) &&
                prefabMap.TryGetValue(skillConfigId, out var prefab) &&
                prefab != null)
            {
                return prefab;
            }

            var bossData = GetBossData(bossId);
            if (bossData?.circleIndicatorPrefab != null) // ������ �⺻ �ε������ͷ� ����
            {
                return bossData.circleIndicatorPrefab;
            }

            Debug.LogWarning($"���� {bossId}�� ��ų {skillConfigId}�� ���� �ε������Ͱ� �����ϴ�.");
            return null;
        }
        catch (Exception e)
        {
            Debug.LogError($"GetIndicatorPrefab ����: {e.Message}");
            return null;
        }
    }
    // ��ų�� ���� �������� �޼���
    public AudioClip GetSkillSound(int bossId, int skillConfigId)
    {
        try
        {
            if (bossSkillSoundMap.TryGetValue(bossId, out var soundMap) &&
                soundMap.TryGetValue(skillConfigId, out var sound))
            {
                return sound;
            }

            // ������ ������ ������ �⺻ �����Ҹ� ��ȯ
            var bossData = GetBossData(bossId);
            return bossData?.howlSound;
        }
        catch (Exception e)
        {
            Debug.LogError($"GetSkillSound ����: {e.Message}");
            return null;
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
            Debug.Log(bossId);
            string bossDataPath = $"BossData_{bossId}";
            var bossDataHandle = Addressables.LoadAssetAsync<BossData>(bossDataPath);
            BossData bossData = await bossDataHandle.Task;

            if (bossData != null)
            {
                UpdateBossData(bossData, bossId);
                bossDatabase[bossId] = bossData;
                Debug.Log($"���� �ε�: ID {bossId}, �̸� {bossData.MonsterName}");
            }
            else
            {
                Debug.LogError($"���� �����͸� ã�� �� �����ϴ�: {bossDataPath}");
            }
        }
    }

    private void UpdateBossData(BossData bossData, int bossId)
    { // ���� ID ����
        bossData.BossID = bossId;
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
            _ = UpdateBossPrefab(bossData, skillsPrefab);
        }

        // é�ͺ���
        // �˷���� �����ʹ� ID�� �ƴ� �̸����� ã�ƾ� �ϹǷ� baseData���� �̸��� ������
        if (bossBaseData.TryGetValue(bossId, out var bData))
        {
            string bossName = bData["Name"];
            if (alexanderBossData.TryGetValue(bossName, out var alexanderData))
            {
                UpdateAlexanderBossData(bossData, alexanderData);
            }
        }
    }
    private void UpdateBossPatternData(BossData bossData, List<Dictionary<string, string>> patterns)
    {
        foreach (var pattern in patterns)
        {
          
            AttackPatternData patternData = new AttackPatternData
            {
                patternName = pattern["PatternName"],
                patternIndex = int.Parse(pattern["PatternIndex"]),
                patternType = (BossPatternType)Enum.Parse(typeof(BossPatternType), pattern["PatternType"]),
                phaseNumber = int.Parse(pattern["PhaseNumber"]),
                patternCooldown = float.Parse(pattern["PatternCooldown"]),
                warningDuration = float.Parse(pattern["WarningDuration"]),
                healthThresholdMin = float.Parse(pattern["HealthThresholdMin"]),
                healthThresholdMax = float.Parse(pattern["HealthThresholdMax"]),
                warningMessage = pattern["WarningMessage"],
                  // ���� �߰��� �ʵ��
                isDisabled = false,  // �ʱⰪ�� false
                requiredSuccessCount = int.Parse(pattern["RequiredSuccessCount"]),
                currentSuccessCount = 0,  // �ʱⰪ�� 0
                baseDifficulty = float.Parse(pattern["baseDifficulty"]),
                maxDifficulty = float.Parse(pattern["maxDifficulty"]),
                difficultyIncreaseStep = float.Parse(pattern["difficultyIncreaseStep"])
            };

           

            // BossDataManager���� ���� �ε� ��
            int phaseIndex = patternData.patternIndex; // �̹� 0���� �����ϹǷ� ��ȯ ���ʿ�
            if (phaseIndex >= 0 && phaseIndex < bossData.phaseData.Count)
            {
                bossData.phaseData[phaseIndex].availablePatterns.Add(patternData);
            }


            Debug.Log($"[UpdateBossPatternData] Boss ID: {bossData.MonsterName}, Total Patterns: {patterns.Count}");
            Debug.Log($"[UpdateBossPatternData] Pattern '{patternData.patternName}' added to Phase {patternData.phaseNumber}");
        }


    }
    private void UpdateBossBaseData(BossData bossData, Dictionary<string, string> baseData)
    {
        //bossData.BossID = int.Parse(baseData["BossID"]);
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
        bossData.chargeSpeed = float.Parse(baseData["ChargeSpeed"]);
        bossData.chargeDuration = float.Parse(baseData["ChargeDuration"]);
        bossData.prepareTime = float.Parse(baseData["ChargePrepareTime"]);
        bossData.multiShotCount = int.Parse(baseData["MultiShotCount"]);
        // ����Ʈ ������ �ε�
        LoadEffectPrefab(baseData, "ChargePrepareDustEffect", result => bossData.ChargePrepareDustEffect = result);
        LoadEffectPrefab(baseData, "ChargeStartEffect", result => bossData.ChargeStartEffect = result);
        LoadEffectPrefab(baseData, "ChargeTrailEffect", result => bossData.ChargeTrailEffect = result);
        LoadEffectPrefab(baseData, "WallImpactEffect", result => bossData.WallImpactEffect = result);
        LoadEffectPrefab(baseData, "PlayerImpactEffect", result => bossData.PlayerImpactEffect = result);

        bossData.showPhaseNames = bool.Parse(baseData["ShowPhaseNames"]);
        


       //����
        string roarSoundKey = baseData["RoarSound"];
        if (!string.IsNullOrEmpty(roarSoundKey))
        {
            Addressables.LoadAssetAsync<AudioClip>(roarSoundKey).Completed += handle =>
            {
                if (handle.Status == AsyncOperationStatus.Succeeded)
                {
                    bossData.roarSound = handle.Result;
                    Debug.Log($"RoarSound �ε� �Ϸ�: {roarSoundKey}");
                }
                else
                {
                    Debug.LogError($"RoarSound �ε� ����: {roarSoundKey}");
                }
            };
        }
        string HowlSoundKey = baseData["HowlSound"];
        if (!string.IsNullOrEmpty(HowlSoundKey))
        {
            Addressables.LoadAssetAsync<AudioClip>(HowlSoundKey).Completed += handle =>
            {
                if (handle.Status == AsyncOperationStatus.Succeeded)
                {
                    bossData.howlSound = handle.Result;
                    Debug.Log($"RoarSound �ε� �Ϸ�: {HowlSoundKey}");
                }
                else
                {
                    Debug.LogError($"RoarSound �ε� ����: {HowlSoundKey}");
                }
            };
        }
        string squreIndicator = baseData["ChargeIndicatorPrefab"];
        if(!string.IsNullOrEmpty(squreIndicator))
        {
            Addressables.LoadAssetAsync<GameObject>(squreIndicator).Completed += handle =>
            {
                if (handle.Status == AsyncOperationStatus.Succeeded)
                {
                    bossData.chargeIndicatorPrefab = handle.Result;
                    Debug.Log($"RoarSound �ε� �Ϸ�: {roarSoundKey}");
                }
                else
                {
                    Debug.LogError($"RoarSound �ε� ����: {roarSoundKey}");
                }
            };
        }


    }
    // ����Ʈ ������ �ε� ���� �޼���
    private void LoadEffectPrefab(Dictionary<string, string> data, string key, System.Action<GameObject> onLoaded)
    {
        if (data.TryGetValue(key, out string prefabPath) && !string.IsNullOrEmpty(prefabPath))
        {
            Addressables.LoadAssetAsync<GameObject>(prefabPath).Completed += handle =>
            {
                if (handle.Status == AsyncOperationStatus.Succeeded)
                {
                    onLoaded?.Invoke(handle.Result);
                    Debug.Log($"{key} �ε� �Ϸ�: {prefabPath}");
                }
                else
                {
                    Debug.LogError($"{key} �ε� ����: {prefabPath}");
                }
            };
        }
    }
    private async Task UpdateBossPrefab(BossData bossData, List<Dictionary<string, string>> baseData)
    {
        foreach(var baseDataes in baseData)
        {
            bossData.areaEffectPrefab = await LoadPrefabAsync(baseDataes["AreaEffectPrefab"]);
            bossData.shorckEffectPrefab = await LoadPrefabAsync(baseDataes["ShockEffectPrefab"]);
            bossData.buffEffectPrefab = await LoadPrefabAsync(baseDataes["BuffEffectPrefab"]);
            bossData.summonPrefab = await LoadPrefabAsync(baseDataes["SummonPrefab"]);
            bossData.projectilePrefab = await LoadPrefabAsync(baseDataes["ProjectilePrefab"]);
            bossData.hitEffect = await LoadPrefabAsync(baseDataes["HitEffect"]);
            bossData.circleIndicatorPrefab = await LoadPrefabAsync(baseDataes["CircleIndicatorPrefab"]);
            Debug.Log($"@@@@@@@@@@@@@@@@@@@@HowlEffectPrefabKey ��: {baseDataes["HowlEffectPrefabKey"]}");
            var howlPrefab = await LoadPrefabAsync(baseDataes["HowlEffectPrefabKey"]);
            Debug.Log($"�ε�� howlPrefab: {(howlPrefab != null ? howlPrefab.name : "null")}");
            bossData.howlEffectPrefab = howlPrefab;
            bossData.ExplosionEffect = await LoadPrefabAsync($"{baseDataes["ExplosionPrefab"]}");

        }
     
       

    }
    private async Task<GameObject> LoadPrefabAsync(string prefabKey)
    {
        if (string.IsNullOrEmpty(prefabKey)) return null;

        var handle = Addressables.LoadAssetAsync<GameObject>(prefabKey);
        await handle.Task;  //  �񵿱� �ε� �ϷḦ ��ٸ�

        if (handle.Status == AsyncOperationStatus.Succeeded)
        {
            return handle.Result;
        }
        else
        {
            Debug.LogError($"Prefab �ε� ����: {prefabKey}");
            return null;
        }
    }

    private void UpdateBossPhaseData(BossData bossData, List<Dictionary<string, string>> phases)
    {
        foreach (var gimmickSet in bossGimmickData)
        {
            Debug.Log($"[Gimmick Debug] Boss ID: {gimmickSet.Key}, Gimmick Count: {gimmickSet.Value.Count}");

            foreach (var gimmickData in gimmickSet.Value)
            {
                string bossId = gimmickData["BossID"];
                string phaseNumber = gimmickData["PhaseNumber"];

                Debug.Log($"[Gimmick Detail] Boss ID: {bossId}, Phase Number: {phaseNumber}");
            }
        }
        bossData.phaseData = new List<PhaseData>();
        foreach (var phase in phases)
        {
            int bossId = int.Parse(phase["BossID"]);
            int phaseNumber = int.Parse(phase["PhaseNumber"]);

            PhaseData phaseData = new PhaseData
            {
                phaseName = phase["PhaseName"],
                phaseTransitionThreshold = float.Parse(phase["HealthThreshold"]),
                transitionDuration = float.Parse(phase["TransitionDuration"]),
                isInvulnerableDuringTransition = bool.Parse(phase["IsInvulnerableDuringTransition"]),
                patternChangeTime = float.Parse(phase["PatternChangeTime"]),

                moveType = (MovementStrategyType)Enum.Parse(typeof(MovementStrategyType), phase["MoveStrategy"]),
                attackType = (AttackStrategyType)Enum.Parse(typeof(AttackStrategyType), phase["AttackStrategy"]),                
                phaseTransitionType = (PhaseTransitionType)Enum.Parse(typeof(PhaseTransitionType), phase["PhaseTransitionType"]),

                damageMultiplier = float.Parse(phase["DamageMultiplier"]),
                speedMultiplier = float.Parse(phase["SpeedMultiplier"]),
                defenseMultiplier = float.Parse(phase["DefenseMultiplier"]),
                attackSpeedMultiplier = float.Parse(phase["AttackSpeedMultiplier"]),

                canBeInterrupted = bool.Parse(phase["CanBeInterrupted"]),
                stunResistance = float.Parse(phase["StunResistance"]),
                useHealthRetreat = bool.Parse(phase["UseHealthRetreat"]),
                healthRetreatThreshold = float.Parse(phase["HealthRetreatThreshold"]),

                phaseAttackStrategies = new List<AttackStrategyWeight>(),
                skillConfigIds = new List<int>(),
                skillConfigWeights = new List<float>(),
                gimmicks = new List<GimmickData>() // ��������� �ʱ�ȭ
            };

            // ���� ���� �� ����ġ �Ľ�
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

            // ��ų ���� ID�� ����ġ �Ľ�
            string[] configIds = phase["SkillConfigIds"].Split('|');
            string[] configWeights = phase["SkillConfigWeights"].Split('|');

            for (int i = 0; i < configIds.Length; i++)
            {
                if (!string.IsNullOrEmpty(configIds[i]) && int.TryParse(configIds[i], out int configId))
                {
                    phaseData.skillConfigIds.Add(configId);

                    // ����ġ ���� (�⺻�� 1.0f)
                    float weight = 1.0f;
                    if (i < configWeights.Length && float.TryParse(configWeights[i], out float parsedWeight))
                    {
                        weight = parsedWeight;
                    }
                    phaseData.skillConfigWeights.Add(weight);
                }
            }

            // �ش� �������� ��� ������ ó��
            if (bossGimmickData.TryGetValue(bossId, out var gimmickList))
            {
                foreach (var gimmickData in gimmickList)
                {
                    // ���� ������ ���� ����� �ش��ϴ� ��͸� �߰�
                    if (int.Parse(gimmickData["PhaseNumber"]) == phaseNumber)
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

                        // �񵿱������� ������ ������ �ε�
                        Addressables.LoadAssetAsync<GameObject>(gimmick.hazardPrefabKey).Completed += handle =>
                        {
                            if (handle.Status == AsyncOperationStatus.Succeeded)
                            {
                                gimmick.hazardPrefab = handle.Result;
                                Debug.Log($"[GimmickData] {gimmick.gimmickName}�� Prefab �ε� �Ϸ�: {gimmick.hazardPrefabKey}");
                            }
                            else
                            {
                                Debug.LogError($"[GimmickData] {gimmick.gimmickName}�� Prefab �ε� ����: {gimmick.hazardPrefabKey}");
                            }
                        };

                        // ����� �α� �߰�
                        Debug.Log($"Adding Gimmick: {gimmick.gimmickName} to Boss {bossId}, Phase {phaseNumber}");

                        // ���� ������ �����Ϳ� ��� �߰�
                        phaseData.gimmicks.Add(gimmick);
                    }
                }
            }

            // ������ �����͸� ���� �����Ϳ� �߰�
            bossData.phaseData.Add(phaseData);

            // ����� �α׷� ������ ���� Ȯ��
            Debug.Log($"Completed Phase {phaseNumber} for Boss {bossId}: {phaseData.phaseName}, Gimmicks: {phaseData.gimmicks.Count}");
        }
    }
    // LayerMask �Ľ� �Լ�
    private LayerMask ParseLayerMask(string layerString)
    {
        if (string.IsNullOrEmpty(layerString))
            return 0; // �⺻��

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
              
                bossData.projectileSpeed = float.Parse(skill["ProjectileSpeed"]);
                bossData.areaRadius = float.Parse(skill["AreaRadius"]);
               
                bossData.hitStunDuration = float.Parse(skill["HitStunDuration"]);
                bossData.knockbackForce = float.Parse(skill["KnockbackForce"]);
                bossData.cameraShakeIntensity = float.Parse(skill["CameraShakeIntensity"]);
                bossData.cameraShakeDuration = float.Parse(skill["CameraShakeDuration"]);
                bossData.shockwaveRadius = float.Parse(skill["ShockwaveRadius"]);
                bossData.projectileRotationAxis = ParseVector3(skill["ProjectileRotationAxis"]);
                bossData.projectileRotationSpeed = float.Parse(skill["ProjectileRotationSpeed"]);
                bossData.heightFactor = float.Parse(skill["HeightFactor"]);
                bossData.howlRadius = float.Parse(skill["HowlRadius"]);
                bossData.EssenceAmount = float.Parse(skill["EssenceAmount"]); 
                bossData.howlDuration = float.Parse(skill["howlDuration"]);
                bossData.safeZoneRadius = float.Parse(skill["safeZoneRadius"]);
                bossData.dangerRadiusMultiplier = float.Parse(skill["dangerRadiusMultiplier"]);
                
            }
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
        return Vector3.forward; // �⺻�� ��ȯ
    }
    #region é�ͺ��� ������
    private void UpdateAlexanderBossData(BossData bossData, Dictionary<string, string> alexanderData)
    {
        if (bossData is AlexanderBossData alexanderBossData)
        {
            alexanderBossData.initialEssence = float.Parse(alexanderData["InitialEssence"]);
            alexanderBossData.essenceName = alexanderData["EssenceName"];  // �߰�
            alexanderBossData.maxEssence = float.Parse(alexanderData["MaxEssence"]);
            alexanderBossData.essenceThreshold = float.Parse(alexanderData["EssenceThreshold"]);
            alexanderBossData.playerAttackBuff = float.Parse(alexanderData["PlayerAttackBuff"]);
            alexanderBossData.playerDamageBuff = float.Parse(alexanderData["PlayerDamageBuff"]);
            alexanderBossData.maxEssenceStunTime = float.Parse(alexanderData["MaxEssenceStunTime"]);
            // ���� �տ� ���� �߰�
            if (alexanderData.ContainsKey("EnableMadnessCrack"))
            {
                alexanderBossData.enableMadnessCrack = bool.Parse(alexanderData["EnableMadnessCrack"]);
            }

            if (alexanderData.ContainsKey("CrackWarningDuration"))
            {
                alexanderBossData.crackWarningDuration = float.Parse(alexanderData["CrackWarningDuration"]);
            }

            if (alexanderData.ContainsKey("CrackRadius"))
            {
                alexanderBossData.crackRadius = float.Parse(alexanderData["CrackRadius"]);
            }

            if (alexanderData.ContainsKey("CrackDamage"))
            {
                alexanderBossData.crackDamage = float.Parse(alexanderData["CrackDamage"]);
            }

            if (alexanderData.ContainsKey("CrackDamageMultiplier"))
            {
                alexanderBossData.crackDamageMultiplier = float.Parse(alexanderData["CrackDamageMultiplier"]);
            }

            if (alexanderData.ContainsKey("CrackCooldownMin"))
            {
                alexanderBossData.crackCooldownMin = float.Parse(alexanderData["CrackCooldownMin"]);
            }

            if (alexanderData.ContainsKey("CrackCooldownMax"))
            {
                alexanderBossData.crackCooldownMax = float.Parse(alexanderData["CrackCooldownMax"]);
            }
            // ������ ��ΰ� ������ ������ �ε�
            if (alexanderData.ContainsKey("CrackPrefabPath") && !string.IsNullOrEmpty(alexanderData["CrackPrefabPath"]))
            {
                Addressables.LoadAssetAsync<GameObject>(alexanderData["CrackPrefabPath"]).Completed += op => {
                    if (op.Status == AsyncOperationStatus.Succeeded)
                    {
                        alexanderBossData.crackPrefab = op.Result;
                        Debug.Log($"���� �տ� ������ �ε� �Ϸ�: {alexanderData["CrackPrefabPath"]}");
                    }
                    else
                    {
                        Debug.LogError($"���� �տ� ������ �ε� ����: {alexanderData["CrackPrefabPath"]}");
                    }
                };
            }

            if (alexanderData.ContainsKey("CrackIndicatorPath") && !string.IsNullOrEmpty(alexanderData["CrackIndicatorPath"]))
            {
                Addressables.LoadAssetAsync<GameObject>(alexanderData["CrackIndicatorPath"]).Completed += op => {
                    if (op.Status == AsyncOperationStatus.Succeeded)
                    {
                        alexanderBossData.crackIndicatorPrefab = op.Result;
                        Debug.Log($"���� �տ� �ε������� ������ �ε� �Ϸ�: {alexanderData["CrackIndicatorPath"]}");
                    }
                    else
                    {
                        Debug.LogError($"���� �տ� �ε������� ������ �ε� ����: {alexanderData["CrackIndicatorPath"]}");
                    }
                };
            }

            if (alexanderData.ContainsKey("CrackExplosionPath") && !string.IsNullOrEmpty(alexanderData["CrackExplosionPath"]))
            {
                Addressables.LoadAssetAsync<GameObject>(alexanderData["CrackExplosionPath"]).Completed += op => {
                    if (op.Status == AsyncOperationStatus.Succeeded)
                    {
                        alexanderBossData.crackExplosionPrefab = op.Result;
                        Debug.Log($"���� �տ� ���� ����Ʈ ������ �ε� �Ϸ�: {alexanderData["CrackExplosionPath"]}");
                    }
                    else
                    {
                        Debug.LogError($"���� �տ� ���� ����Ʈ ������ �ε� ����: {alexanderData["CrackExplosionPath"]}");
                    }
                };
            }


            Debug.Log($"�˷���� Ư�� ������ ������Ʈ �Ϸ�: {bossData.MonsterName}");
        }
    }
    #endregion
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
        Debug.LogWarning($"���� �����͸� ã�� �� �����ϴ�: ID {bossId}");
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