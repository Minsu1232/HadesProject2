// MovementAbilityLoader.cs - CSV 데이터 로더 클래스
using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class MovementAbilityLoader : MonoBehaviour
{
    // Singleton 패턴
    public static MovementAbilityLoader Instance { get; private set; }

    // 이동 능력 데이터베이스
    private Dictionary<string, Dictionary<string, string>> movementAbilityData = new Dictionary<string, Dictionary<string, string>>();

    // CSV 파일 경로
    private string movementAbilitiesPath;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);

            // CSV 파일 경로 설정
            movementAbilitiesPath = Path.Combine(Application.persistentDataPath, "MovementAbilities.csv");

            // CSV 파일 복사 및 로드
            CopyCSVFromStreamingAssets();
            LoadMovementAbilitiesFromCSV();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    // StreamingAssets에서 CSV 파일 복사
    private void CopyCSVFromStreamingAssets()
    {
        string streamingPath = Path.Combine(Application.streamingAssetsPath, "MovementAbilities.csv");

        if (File.Exists(streamingPath))
        {
            File.Copy(streamingPath, movementAbilitiesPath, true);
            Debug.Log("이동 능력 CSV 파일 복사 완료");
        }
        else
        {
            Debug.LogWarning("StreamingAssets에서 MovementAbilities.csv 파일을 찾을 수 없습니다.");
        }
    }

    // CSV에서 이동 능력 데이터 로드
    private void LoadMovementAbilitiesFromCSV()
    {
        if (!File.Exists(movementAbilitiesPath))
        {
            Debug.LogError($"이동 능력 CSV 파일을 찾을 수 없습니다: {movementAbilitiesPath}");
            return;
        }

        string[] lines = File.ReadAllLines(movementAbilitiesPath);
        string[] headers = lines[0].Split(',');

        for (int i = 1; i < lines.Length; i++)
        {
            string line = lines[i].Trim();
            if (string.IsNullOrEmpty(line)) continue;

            string[] values = line.Split(',');

            // 값이 충분한지 확인
            if (values.Length < headers.Length)
            {
                Debug.LogWarning($"이동 능력 CSV 줄 {i + 1}에 값이 부족합니다. 건너뜁니다.");
                continue;
            }

            Dictionary<string, string> abilityData = new Dictionary<string, string>();
            for (int j = 0; j < headers.Length; j++)
            {
                abilityData[headers[j]] = values[j];
            }

            string id = values[0];
            movementAbilityData[id] = abilityData;
        }

        Debug.Log($"이동 능력 데이터 로드 완료: {movementAbilityData.Count}개 능력");
    }

    // ID로 이동 능력 데이터 가져오기
    public Dictionary<string, string> GetMovementAbilityData(string id)
    {
        if (movementAbilityData.TryGetValue(id, out Dictionary<string, string> data))
        {
            return data;
        }

        Debug.LogWarning($"이동 능력 ID를 찾을 수 없음: {id}");
        return null;
    }

    // 모든 이동 능력 데이터 가져오기
    public List<Dictionary<string, string>> GetAllMovementAbilityData()
    {
        List<Dictionary<string, string>> allData = new List<Dictionary<string, string>>();
        foreach (var data in movementAbilityData.Values)
        {
            allData.Add(data);
        }
        return allData;
    }

    // ID로 이동 능력 인스턴스 생성
    public MovementAbility CreateMovementAbility(string id)
    {
        Dictionary<string, string> data = GetMovementAbilityData(id);
        if (data != null)
        {
            return MovementAbility.FromCSVData(data);
        }
        return null;
    }

    // 모든 이동 능력 인스턴스 생성
    public List<MovementAbility> CreateAllMovementAbilities()
    {
        List<MovementAbility> abilities = new List<MovementAbility>();
        foreach (var data in movementAbilityData.Values)
        {
            MovementAbility ability = MovementAbility.FromCSVData(data);
            abilities.Add(ability);
        }
        return abilities;
    }
}