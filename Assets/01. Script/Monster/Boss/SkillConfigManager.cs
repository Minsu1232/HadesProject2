using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using UnityEngine;

/// <summary>
/// 스킬 구성 데이터를 로드하고 관리하는 매니저 클래스
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
            Debug.Log($"보스 스킬 구성 CSV 파일 복사 완료: BossSkillConfigs.csv");
        }
        else
        {
            Debug.LogError($"StreamingAssets에서 파일을 찾을 수 없습니다: BossSkillConfigs.csv");
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
            Debug.LogError($"스킬 구성 CSV 파일을 찾을 수 없습니다: {skillConfigPath}");
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

                // 버프 관련 데이터 (있을 경우)
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
                Debug.Log($"스킬 구성 로드: ID {configId}, 이름 {configName}");
            }
            catch (Exception e)
            {
                Debug.LogError($"스킬 구성 로드 중 오류 발생 (ID: {configId}): {e.Message}");
            }
        }

        Debug.Log($"총 {skillConfigs.Count}개의 스킬 구성 로드 완료");
    }

    /// <summary>
    /// 스킬 구성 ID로 스킬 구성 데이터를 가져옵니다.
    /// </summary>
    /// <param name="configId">구성 ID</param>
    /// <returns>스킬 구성 데이터 또는 해당 ID가 없으면 null</returns>
    public SkillConfig GetSkillConfig(int configId)
    {
        if (skillConfigs.TryGetValue(configId, out SkillConfig config))
        {
            return config;
        }

        Debug.LogWarning($"스킬 구성을 찾을 수 없음: ID {configId}");
        return null;
    }

    /// <summary>
    /// 모든 로드된 스킬 구성을 가져옵니다.
    /// </summary>
    /// <returns>스킬 구성 딕셔너리</returns>
    public Dictionary<int, SkillConfig> GetAllSkillConfigs()
    {
        return skillConfigs;
    }

    /// <summary>
    /// 모든 리소스를 해제합니다.
    /// </summary>
    public void ReleaseAllResources()
    {
        skillConfigs.Clear();
    }
}