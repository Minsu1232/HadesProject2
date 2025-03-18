// 3. CSV 데이터 로더 클래스
using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class PassiveAbilityLoader : MonoBehaviour
{
    // Singleton 패턴
    public static PassiveAbilityLoader Instance { get; private set; }

    // 패시브 능력 데이터베이스
    private Dictionary<string, Dictionary<string, string>> passiveAbilityData = new Dictionary<string, Dictionary<string, string>>();

    // CSV 파일 경로
    private string passiveAbilitiesPath;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);

            // CSV 파일 경로 설정
            passiveAbilitiesPath = Path.Combine(Application.persistentDataPath, "PassiveAbilities.csv");

            // CSV 파일 복사 및 로드
            CopyCSVFromStreamingAssets();
            LoadPassiveAbilitiesFromCSV();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    // StreamingAssets에서 CSV 파일 복사
    private void CopyCSVFromStreamingAssets()
    {
        string streamingPath = Path.Combine(Application.streamingAssetsPath, "PassiveAbilities.csv");

        if (File.Exists(streamingPath))
        {
            File.Copy(streamingPath, passiveAbilitiesPath, true);
            Debug.Log("패시브 능력 CSV 파일 복사 완료");
        }
        else
        {
            Debug.LogWarning("StreamingAssets에서 PassiveAbilities.csv 파일을 찾을 수 없습니다.");
        }
    }

    // CSV에서 패시브 능력 데이터 로드
    private void LoadPassiveAbilitiesFromCSV()
    {
        if (!File.Exists(passiveAbilitiesPath))
        {
            Debug.LogError($"패시브 능력 CSV 파일을 찾을 수 없습니다: {passiveAbilitiesPath}");
            return;
        }

        string[] lines = File.ReadAllLines(passiveAbilitiesPath);
        string[] headers = lines[0].Split(',');

        for (int i = 1; i < lines.Length; i++)
        {
            string line = lines[i].Trim();
            if (string.IsNullOrEmpty(line)) continue;

            string[] values = line.Split(',');

            // 값이 충분한지 확인
            if (values.Length < headers.Length)
            {
                Debug.LogWarning($"패시브 능력 CSV 행 {i + 1}에 값이 부족합니다. 건너뜁니다.");
                continue;
            }

            Dictionary<string, string> abilityData = new Dictionary<string, string>();
            for (int j = 0; j < headers.Length; j++)
            {
                abilityData[headers[j]] = values[j];
            }

            string id = values[0];
            passiveAbilityData[id] = abilityData;
        }

        Debug.Log($"패시브 능력 데이터 로드 완료: {passiveAbilityData.Count}개 능력");
    }

    // ID로 패시브 능력 데이터 가져오기
    public Dictionary<string, string> GetPassiveAbilityData(string id)
    {
        if (passiveAbilityData.TryGetValue(id, out Dictionary<string, string> data))
        {
            return data;
        }

        Debug.LogWarning($"패시브 능력 ID를 찾을 수 없음: {id}");
        return null;
    }

    // 모든 패시브 능력 데이터 가져오기
    public List<Dictionary<string, string>> GetAllPassiveAbilityData()
    {
        List<Dictionary<string, string>> allData = new List<Dictionary<string, string>>();
        foreach (var data in passiveAbilityData.Values)
        {
            allData.Add(data);
        }
        return allData;
    }

    // ID로 패시브 능력 인스턴스 생성
    public PassiveAbility CreatePassiveAbility(string id)
    {
        Dictionary<string, string> data = GetPassiveAbilityData(id);
        if (data != null)
        {
            return PassiveAbility.FromCSVData(data);
        }
        return null;
    }

    // 모든 패시브 능력 인스턴스 생성
    public List<PassiveAbility> CreateAllPassiveAbilities()
    {
        List<PassiveAbility> abilities = new List<PassiveAbility>();
        foreach (var data in passiveAbilityData.Values)
        {
            PassiveAbility ability = PassiveAbility.FromCSVData(data);
            abilities.Add(ability);
        }
        return abilities;
    }
}