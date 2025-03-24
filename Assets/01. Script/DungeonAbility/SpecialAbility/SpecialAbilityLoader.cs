// SpecialAbilityLoader.cs - CSV 데이터 로더 클래스
using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class SpecialAbilityLoader : MonoBehaviour
{
    // Singleton 패턴
    public static SpecialAbilityLoader Instance { get; private set; }

    // 특수 능력 데이터베이스
    private Dictionary<string, Dictionary<string, string>> specialAbilityData = new Dictionary<string, Dictionary<string, string>>();

    // CSV 파일 경로
    private string specialAbilitiesPath;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);

            // CSV 파일 경로 설정
            specialAbilitiesPath = Path.Combine(Application.persistentDataPath, "SpecialAbilities.csv");

            // CSV 파일 복사 및 로드
            CopyCSVFromStreamingAssets();
            LoadSpecialAbilitiesFromCSV();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    // StreamingAssets에서 CSV 파일 복사
    private void CopyCSVFromStreamingAssets()
    {
        string streamingPath = Path.Combine(Application.streamingAssetsPath, "SpecialAbilities.csv");

        if (File.Exists(streamingPath))
        {
            File.Copy(streamingPath, specialAbilitiesPath, true);
            Debug.Log("특수 능력 CSV 파일 복사 완료");
        }
        else
        {
            Debug.LogWarning("StreamingAssets에서 SpecialAbilities.csv 파일을 찾을 수 없습니다.");
        }
    }

    // CSV에서 특수 능력 데이터 로드
    private void LoadSpecialAbilitiesFromCSV()
    {
        if (!File.Exists(specialAbilitiesPath))
        {
            Debug.LogError($"특수 능력 CSV 파일을 찾을 수 없습니다: {specialAbilitiesPath}");
            return;
        }

        string[] lines = File.ReadAllLines(specialAbilitiesPath);
        string[] headers = lines[0].Split(',');

        for (int i = 1; i < lines.Length; i++)
        {
            string line = lines[i].Trim();
            if (string.IsNullOrEmpty(line)) continue;

            string[] values = line.Split(',');

            // 값이 충분한지 확인
            if (values.Length < headers.Length)
            {
                Debug.LogWarning($"특수 능력 CSV 줄 {i + 1}에 값이 부족합니다. 건너뜁니다.");
                continue;
            }

            Dictionary<string, string> abilityData = new Dictionary<string, string>();
            for (int j = 0; j < headers.Length; j++)
            {
                abilityData[headers[j]] = values[j];
            }

            string id = values[0];
            specialAbilityData[id] = abilityData;
        }

        Debug.Log($"특수 능력 데이터 로드 완료: {specialAbilityData.Count}개 능력");
    }

    // ID로 특수 능력 데이터 가져오기
    public Dictionary<string, string> GetSpecialAbilityData(string id)
    {
        if (specialAbilityData.TryGetValue(id, out Dictionary<string, string> data))
        {
            return data;
        }

        Debug.LogWarning($"특수 능력 ID를 찾을 수 없음: {id}");
        return null;
    }

    // 모든 특수 능력 데이터 가져오기
    public List<Dictionary<string, string>> GetAllSpecialAbilityData()
    {
        List<Dictionary<string, string>> allData = new List<Dictionary<string, string>>();
        foreach (var data in specialAbilityData.Values)
        {
            allData.Add(data);
        }
        return allData;
    }

    // ID로 특수 능력 인스턴스 생성
    public SpecialAbility CreateSpecialAbility(string id)
    {
        Dictionary<string, string> data = GetSpecialAbilityData(id);
        if (data != null)
        {
            return SpecialAbility.FromCSVData(data);
        }
        return null;
    }

    // 모든 특수 능력 인스턴스 생성
    public List<SpecialAbility> CreateAllSpecialAbilities()
    {
        List<SpecialAbility> abilities = new List<SpecialAbility>();
        foreach (var data in specialAbilityData.Values)
        {
            SpecialAbility ability = SpecialAbility.FromCSVData(data);
            abilities.Add(ability);
        }
        return abilities;
    }
}