using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using UnityEngine;

/// <summary>
/// ��ų ���� �����͸� �ε��ϰ� �����ϴ� �Ŵ��� Ŭ����
/// </summary>
public class SkillConfigManager : Singleton<SkillConfigManager>
{
    private Dictionary<int, SkillConfig> skillConfigs = new Dictionary<int, SkillConfig>();
    private string skillConfigPath;

   protected override void Awake()
    {
        //skillConfigPath = Path.Combine(Application.persistentDataPath, "BossSkillConfigs.csv");
        //CopyCSVFromStreamingAssets();
        Debug.Log("#@$@#($@#)($@#)($@#)$!)@#");
    }

    private void CopyCSVFromStreamingAssets()
    {
        string streamingPath = Path.Combine(Application.streamingAssetsPath, "BossSkillConfigs.csv");
        string persistentPath = Path.Combine(Application.persistentDataPath, "BossSkillConfigs.csv");

        if (File.Exists(streamingPath))
        {
            File.Copy(streamingPath, persistentPath, true);
            Debug.Log($"���� ��ų ���� CSV ���� ���� �Ϸ�: BossSkillConfigs.csv");
        }
        else
        {
            Debug.LogError($"StreamingAssets���� ������ ã�� �� �����ϴ�: BossSkillConfigs.csv");
        }
    }

    public async Task Initialize()
    {
        await LoadSkillConfigs();
    }

    private async Task LoadSkillConfigs()
    {
        skillConfigPath = Path.Combine(Application.persistentDataPath, "BossSkillConfigs.csv");
        if (!File.Exists(skillConfigPath))
        {
            Debug.LogError($"��ų ���� CSV ������ ã�� �� �����ϴ�: {skillConfigPath}");
            return;
        }

        string[] lines = File.ReadAllLines(skillConfigPath);
        string[] headers = lines[0].Split(',');

        for (int i = 1; i < lines.Length; i++)
        {
            string[] values = lines[i].Split(',');
            int configId = int.Parse(values[0]);

            try
            {
                SkillStrategyType strategyType = (SkillStrategyType)Enum.Parse(typeof(SkillStrategyType), values[1]);
                SkillEffectType effectType = (SkillEffectType)Enum.Parse(typeof(SkillEffectType), values[2]);
                ProjectileMovementType moveType = (ProjectileMovementType)Enum.Parse(typeof(ProjectileMovementType), values[3]);
                ProjectileImpactType impactType = (ProjectileImpactType)Enum.Parse(typeof(ProjectileImpactType), values[4]);
                string configName = values[5];

                // ���� ���� ������ (���� ���)
                string buffTypes = values.Length > 6 ? values[6] : "";
                string buffDurations = values.Length > 7 ? values[7] : "";
                string buffValues = values.Length > 8 ? values[8] : "";

                SkillConfig config = new SkillConfig(
                    configId,
                    configName,
                    strategyType,
                    effectType,
                    moveType,
                    impactType,
                    buffTypes,
                    buffDurations,
                    buffValues
                );
                string damageMultiplier = values.Length > 9 ? values[9] : "1.0";
                config.damageMultiplier = float.Parse(damageMultiplier);
                string speedMultiplier  = values.Length > 9 ? values[10] : "1.0";
                config.speedMultiplier = float.Parse(speedMultiplier);
                skillConfigs[configId] = config;
                Debug.Log($"��ų ���� �ε�: ID {configId}, �̸� {configName}");
            }
            catch (Exception e)
            {
                Debug.LogError($"��ų ���� �ε� �� ���� �߻� (ID: {configId}): {e.Message}");
            }
        }

        Debug.Log($"�� {skillConfigs.Count}���� ��ų ���� �ε� �Ϸ�");
    }

    /// <summary>
    /// ��ų ���� ID�� ��ų ���� �����͸� �����ɴϴ�.
    /// </summary>
    /// <param name="configId">���� ID</param>
    /// <returns>��ų ���� ������ �Ǵ� �ش� ID�� ������ null</returns>
    public SkillConfig GetSkillConfig(int configId)
    {
        if (skillConfigs.TryGetValue(configId, out SkillConfig config))
        {
            return config;
        }

        Debug.LogWarning($"��ų ������ ã�� �� ����: ID {configId}");
        return null;
    }

    /// <summary>
    /// ��� �ε�� ��ų ������ �����ɴϴ�.
    /// </summary>
    /// <returns>��ų ���� ��ųʸ�</returns>
    public Dictionary<int, SkillConfig> GetAllSkillConfigs()
    {
        return skillConfigs;
    }

    /// <summary>
    /// ��� ���ҽ��� �����մϴ�.
    /// </summary>
    public void ReleaseAllResources()
    {
        skillConfigs.Clear();
    }
}