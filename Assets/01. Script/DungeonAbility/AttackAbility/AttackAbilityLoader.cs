// AttackAbilityLoader.cs - CSV 데이터 로더 클래스
using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class AttackAbilityLoader : MonoBehaviour
{
    // Singleton 패턴
    public static AttackAbilityLoader Instance { get; private set; }

    // 공격 능력 데이터베이스
    private Dictionary<string, Dictionary<string, string>> attackAbilityData = new Dictionary<string, Dictionary<string, string>>();

    // CSV 파일 경로
    private string attackAbilitiesPath;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);

            // CSV 파일 경로 설정
            attackAbilitiesPath = Path.Combine(Application.persistentDataPath, "AttackAbilities.csv");

            // CSV 파일 복사 및 로드
            CopyCSVFromStreamingAssets();
            LoadAttackAbilitiesFromCSV();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    // StreamingAssets에서 CSV 파일 복사
    private void CopyCSVFromStreamingAssets()
    {
        string streamingPath = Path.Combine(Application.streamingAssetsPath, "AttackAbilities.csv");

        if (File.Exists(streamingPath))
        {
            File.Copy(streamingPath, attackAbilitiesPath, true);
            Debug.Log("공격 능력 CSV 파일 복사 완료");
        }
        else
        {
            Debug.LogWarning("StreamingAssets에서 AttackAbilities.csv 파일을 찾을 수 없습니다.");
        }
    }

    // CSV에서 공격 능력 데이터 로드
    private void LoadAttackAbilitiesFromCSV()
    {
        if (!File.Exists(attackAbilitiesPath))
        {
            Debug.LogError($"공격 능력 CSV 파일을 찾을 수 없습니다: {attackAbilitiesPath}");
            return;
        }

        string[] lines = File.ReadAllLines(attackAbilitiesPath);
        string[] headers = lines[0].Split(',');

        for (int i = 1; i < lines.Length; i++)
        {
            string line = lines[i].Trim();
            if (string.IsNullOrEmpty(line)) continue;

            string[] values = line.Split(',');

            // 값이 충분한지 확인
            if (values.Length < headers.Length)
            {
                Debug.LogWarning($"공격 능력 CSV 행 {i + 1}에 값이 부족합니다. 건너뜁니다.");
                continue;
            }

            Dictionary<string, string> abilityData = new Dictionary<string, string>();
            for (int j = 0; j < headers.Length; j++)
            {
                abilityData[headers[j]] = values[j];
            }

            string id = values[0];
            attackAbilityData[id] = abilityData;
        }

        Debug.Log($"공격 능력 데이터 로드 완료: {attackAbilityData.Count}개 능력");
    }

    // ID로 공격 능력 데이터 가져오기
    public Dictionary<string, string> GetAttackAbilityData(string id)
    {
        if (attackAbilityData.TryGetValue(id, out Dictionary<string, string> data))
        {
            return data;
        }

        Debug.LogWarning($"공격 능력 ID를 찾을 수 없음: {id}");
        return null;
    }

    // 모든 공격 능력 데이터 가져오기
    public List<Dictionary<string, string>> GetAllAttackAbilityData()
    {
        List<Dictionary<string, string>> allData = new List<Dictionary<string, string>>();
        foreach (var data in attackAbilityData.Values)
        {
            allData.Add(data);
        }
        return allData;
    }

    // ID로 공격 능력 인스턴스 생성
    public AttackAbility CreateAttackAbility(string id)
    {
        Dictionary<string, string> data = GetAttackAbilityData(id);
        if (data != null)
        {            
            return AttackAbility.FromCSVData(data);
        }
        return null;
    }

    // 모든 공격 능력 인스턴스 생성
    public List<AttackAbility> CreateAllAttackAbilities()
    {
        List<AttackAbility> abilities = new List<AttackAbility>();
        foreach (var data in attackAbilityData.Values)
        {
            AttackAbility ability = AttackAbility.FromCSVData(data);
            abilities.Add(ability);
            
        }
        return abilities;
    }
}