using UnityEngine;
using UnityEditor;
using System.IO;
using System.Threading.Tasks;
using System;
using UnityEditor.AddressableAssets;
using System.Linq;

[CustomEditor(typeof(MonsterDataManager))]
/// <summary>
/// Monster CSV 데이터 관리를 위한 에디터 툴
/// Editor 폴더 내에 위치해야 합니다.
/// </summary>


public class MonsterDataManagerEditor : Editor
{
    private static string GetMonstersCSVPath()
    {
        return Path.Combine(Application.persistentDataPath, "Monsters.csv");
    }

    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        MonsterDataManager manager = (MonsterDataManager)target;

        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Editor Tools", EditorStyles.boldLabel);

        EditorGUILayout.LabelField("CSV Path:", GetMonstersCSVPath());

        if (GUILayout.Button("Copy CSV from StreamingAssets"))
        {
            string streamingFilePath = Path.Combine(Application.streamingAssetsPath, "Monsters.csv");
            string persistentFilePath = GetMonstersCSVPath();

            if (File.Exists(streamingFilePath))
            {
                File.Copy(streamingFilePath, persistentFilePath, true);
                Debug.Log($"CSV 파일 복사 완료: {persistentFilePath}");
                AssetDatabase.Refresh();
            }
            else
            {
                Debug.LogError("StreamingAssets에서 Monsters.csv 파일을 찾을 수 없습니다.");
            }
        }

        if (GUILayout.Button("Load Monsters from CSV"))
        {
            LoadMonstersInEditor(manager);
        }
    }

    private void LoadMonstersInEditor(MonsterDataManager manager)
    {
        string csvPath = GetMonstersCSVPath();
        if (!File.Exists(csvPath))
        {
            Debug.LogError($"몬스터 데이터 CSV 파일을 찾을 수 없습니다: {csvPath}");
            return;
        }

        EditorUtility.DisplayProgressBar("Loading Monsters", "몬스터 데이터를 로드하는 중...", 0f);

        try
        {
            string[] lines = File.ReadAllLines(csvPath);
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

                // 에디터에서는 AssetReference로 직접 로드
                var monsterData = AssetDatabase.LoadAssetAtPath<MonsterData>(
                    AssetDatabase.FindAssets($"t:MonsterData {monsterDataKey}")
                    .Select(guid => AssetDatabase.GUIDToAssetPath(guid))
                    .FirstOrDefault());

                if (monsterData != null)
                {
                    // UpdateMonsterData 메서드 호출
                    manager.GetType().GetMethod("UpdateMonsterData",
                        System.Reflection.BindingFlags.NonPublic |
                        System.Reflection.BindingFlags.Instance)
                        .Invoke(manager, new object[] { monsterData, values });

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
        catch (System.Exception e)
        {
            Debug.LogError($"몬스터 로드 중 오류 발생: {e.Message}");
        }
        finally
        {
            EditorUtility.ClearProgressBar();
        }
    }

    [MenuItem("Tools/Monster Manager/Copy CSV")]
    public static void CopyCSV()
    {
        var manager = FindObjectOfType<MonsterDataManager>();
        if (manager != null)
        {
            string streamingFilePath = Path.Combine(Application.streamingAssetsPath, "Monsters.csv");
            string persistentFilePath = GetMonstersCSVPath();

            if (File.Exists(streamingFilePath))
            {
                File.Copy(streamingFilePath, persistentFilePath, true);
                Debug.Log($"CSV 파일 복사 완료: {persistentFilePath}");
                AssetDatabase.Refresh();
            }
            else
            {
                Debug.LogError("StreamingAssets에서 Monsters.csv 파일을 찾을 수 없습니다.");
            }
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