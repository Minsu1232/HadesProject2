using System.Collections.Generic;
using System;
using UnityEditor;
using UnityEngine;
using System.IO;
using System.Linq;

[CustomEditor(typeof(MonsterDataManager))]
public class MonsterDataManagerEditor : Editor
{
    private static string GetCSVPath(string fileName)
    {
        return Path.Combine(Application.persistentDataPath, fileName);
    }

    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        MonsterDataManager manager = (MonsterDataManager)target;

        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Editor Tools", EditorStyles.boldLabel);

        EditorGUILayout.LabelField("CSV Paths:");
        EditorGUILayout.LabelField("Monsters:", GetCSVPath("Monsters.csv"));
        EditorGUILayout.LabelField("Strategies:", GetCSVPath("MonsterStrategies.csv"));
        EditorGUILayout.LabelField("Skills:", GetCSVPath("MonsterSkills.csv"));

        if (GUILayout.Button("Copy CSVs from StreamingAssets"))
        {
            CopyAllCSVFiles();
        }

        if (GUILayout.Button("Load Monsters from CSVs"))
        {
            LoadMonstersInEditor(manager);
        }
    }

    private void CopyAllCSVFiles()
    {
        string[] csvFiles = new string[] { "Monsters.csv", "MonsterStrategies.csv", "MonsterSkills.csv" };
        foreach (string fileName in csvFiles)
        {
            string streamingPath = Path.Combine(Application.streamingAssetsPath, fileName);
            string persistentPath = GetCSVPath(fileName);

            if (File.Exists(streamingPath))
            {
                File.Copy(streamingPath, persistentPath, true);
                Debug.Log($"CSV 파일 복사 완료: {persistentPath}");
            }
            else
            {
                Debug.LogError($"StreamingAssets에서 {fileName} 파일을 찾을 수 없습니다.");
            }
        }
        AssetDatabase.Refresh();
    }

    private void LoadMonstersInEditor(MonsterDataManager manager)
    {
        // 먼저 전략과 스킬 데이터를 Dictionary에 로드
        var strategyData = LoadStrategyData();
        var skillData = LoadSkillData();

        string monstersPath = GetCSVPath("Monsters.csv");
        if (!File.Exists(monstersPath))
        {
            Debug.LogError($"몬스터 데이터 CSV 파일을 찾을 수 없습니다: {monstersPath}");
            return;
        }

        EditorUtility.DisplayProgressBar("Loading Monsters", "몬스터 데이터를 로드하는 중...", 0f);

        try
        {
            string[] lines = File.ReadAllLines(monstersPath);
            float totalLines = lines.Length - 1;
            int currentLine = 0;
            bool isFirstLine = true;

            foreach (string line in lines)
            {
                if (isFirstLine)
                {
                    isFirstLine = false;
                    continue;
                }

                if (string.IsNullOrWhiteSpace(line))
                    continue;

                EditorUtility.DisplayProgressBar("Loading Monsters",
                    $"몬스터 데이터를 로드하는 중... ({currentLine}/{totalLines})",
                    currentLine / totalLines);

                string[] values = line.Trim().Split(',');
                string monsterDataKey = $"MonsterData_{values[0]}";

                var monsterData = AssetDatabase.LoadAssetAtPath<MonsterData>(
                    AssetDatabase.FindAssets($"t:MonsterData {monsterDataKey}")
                    .Select(guid => AssetDatabase.GUIDToAssetPath(guid))
                    .FirstOrDefault());

                if (monsterData != null)
                {
                    int monsterId = int.Parse(values[0]);
                    UpdateMonsterDataInEditor(monsterData, values, monsterId, strategyData, skillData);
                    EditorUtility.SetDirty(monsterData);
                }
                else
                {
                    Debug.LogError($"몬스터 데이터를 찾을 수 없습니다: {monsterDataKey}");
                }

                currentLine++;
            }

            AssetDatabase.SaveAssets();
            Debug.Log("모든 몬스터 데이터 로드 완료");
        }
        catch (Exception e)
        {
            Debug.LogError($"몬스터 로드 중 오류 발생: {e.Message}");
        }
        finally
        {
            EditorUtility.ClearProgressBar();
        }
    }

    private Dictionary<int, Dictionary<string, string>> LoadStrategyData()
    {
        var strategyDict = new Dictionary<int, Dictionary<string, string>>();
        string path = GetCSVPath("MonsterStrategies.csv");

        if (File.Exists(path))
        {
            string[] lines = File.ReadAllLines(path);
            string[] headers = lines[0].Split(',');

            for (int i = 1; i < lines.Length; i++)
            {
                string[] values = lines[i].Split(',');
                int monsterId = int.Parse(values[0]);

                var dataDict = new Dictionary<string, string>();
                for (int j = 1; j < headers.Length; j++)
                {
                    dataDict[headers[j]] = values[j];
                }
                strategyDict[monsterId] = dataDict;
            }
        }

        return strategyDict;
    }

    private Dictionary<int, Dictionary<string, string>> LoadSkillData()
    {
        var skillDict = new Dictionary<int, Dictionary<string, string>>();
        string path = GetCSVPath("MonsterSkills.csv");

        if (File.Exists(path))
        {
            string[] lines = File.ReadAllLines(path);
            string[] headers = lines[0].Split(',');

            for (int i = 1; i < lines.Length; i++)
            {
                string[] values = lines[i].Split(',');
                int monsterId = int.Parse(values[0]);

                var dataDict = new Dictionary<string, string>();
                for (int j = 1; j < headers.Length; j++)
                {
                    dataDict[headers[j]] = values[j];
                }
                skillDict[monsterId] = dataDict;
            }
        }

        return skillDict;
    }

    private void UpdateMonsterDataInEditor(MonsterData monsterData, string[] baseValues, int monsterId,
        Dictionary<int, Dictionary<string, string>> strategyData,
        Dictionary<int, Dictionary<string, string>> skillData)
    {
        // MonsterDataManager의 UpdateMonsterData와 동일한 로직...
        // 기존 UpdateMonsterData 메서드의 내용을 여기에 복사
    }

    [MenuItem("Tools/Monster Manager/Copy CSVs")]
    public static void CopyCSVs()
    {
        var manager = FindObjectOfType<MonsterDataManager>();
        if (manager != null)
        {
            var editor = CreateInstance<MonsterDataManagerEditor>();
            editor.CopyAllCSVFiles();
        }
        else
        {
            Debug.LogError("Scene에서 MonsterDataManager를 찾을 수 없습니다.");
        }
    }

    [MenuItem("Tools/Monster Manager/Load Monsters")]
    public static void LoadMonsters()
    {
        var manager = FindObjectOfType<MonsterDataManager>();
        if (manager != null)
        {
            var editor = CreateInstance<MonsterDataManagerEditor>();
            editor.LoadMonstersInEditor(manager);
        }
        else
        {
            Debug.LogError("Scene에서 MonsterDataManager를 찾을 수 없습니다.");
        }
    }
}