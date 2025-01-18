using UnityEngine;
using UnityEditor;
using System.IO;
using System.Threading.Tasks;
using System;
using System.Linq;

[CustomEditor(typeof(BossDataManager))]
public class BossDataManagerEditor : Editor
{
    private static string GetCSVPath(string fileName)
    {
        return Path.Combine(Application.persistentDataPath, fileName);
    }

    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        BossDataManager manager = (BossDataManager)target;

        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Boss Data Manager Tools", EditorStyles.boldLabel);

        EditorGUILayout.LabelField("CSV Paths:");
        EditorGUILayout.LabelField("BossBase:", GetCSVPath("BossBase.csv"));
        EditorGUILayout.LabelField("BossPhases:", GetCSVPath("BossPhases.csv"));
        EditorGUILayout.LabelField("BossSkills:", GetCSVPath("BossSkills.csv"));
        EditorGUILayout.LabelField("BossCutscenes:", GetCSVPath("BossCutscenes.csv"));

        if (GUILayout.Button("Copy CSVs from StreamingAssets"))
        {
            CopyAllCSVFiles();
            AssetDatabase.Refresh();
        }

        if (GUILayout.Button("Load Boss Data"))
        {
            LoadBossDataInEditor(manager);
        }
    }

    private void CopyAllCSVFiles()
    {
        string[] csvFiles = new string[] { "BossBase.csv", "BossPhases.csv", "BossSkills.csv", "BossCutscenes.csv" };
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
    }

    private void LoadBossDataInEditor(BossDataManager manager)
    {
        string bossBasePath = GetCSVPath("BossBase.csv");
        if (!File.Exists(bossBasePath))
        {
            Debug.LogError($"보스 데이터 CSV 파일을 찾을 수 없습니다: {bossBasePath}");
            return;
        }

        EditorUtility.DisplayProgressBar("Loading Boss Data", "보스 데이터를 로드하는 중...", 0f);

        try
        {
            string[] lines = File.ReadAllLines(bossBasePath);
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

                EditorUtility.DisplayProgressBar("Loading Boss Data",
                    $"보스 데이터를 로드하는 중... ({currentLine}/{totalLines})",
                    currentLine / totalLines);

                string[] values = line.Trim().Split(',');
                string bossDataKey = $"BossData_{values[0]}";

                var bossData = AssetDatabase.LoadAssetAtPath<BossData>(
                    AssetDatabase.FindAssets($"t:BossData {bossDataKey}")
                    .Select(guid => AssetDatabase.GUIDToAssetPath(guid))
                    .FirstOrDefault());

                if (bossData != null)
                {
                    UpdateBossDataInEditor(bossData, values);
                    EditorUtility.SetDirty(bossData);
                }
                else
                {
                    Debug.LogError($"보스 데이터를 찾을 수 없습니다: {bossDataKey}");
                }

                currentLine++;
            }

            AssetDatabase.SaveAssets();
            Debug.Log("모든 보스 데이터 로드 완료");
        }
        catch (Exception e)
        {
            Debug.LogError($"보스 데이터 로드 중 오류 발생: {e.Message}");
        }
        finally
        {
            EditorUtility.ClearProgressBar();
        }
    }

    private void UpdateBossDataInEditor(BossData bossData, string[] values)
    {
        var manager = (BossDataManager)target;
        manager.GetType().GetMethod("UpdateBossData",
            System.Reflection.BindingFlags.NonPublic |
            System.Reflection.BindingFlags.Instance)
            .Invoke(manager, new object[] { bossData, int.Parse(values[0]) });
    }

    [MenuItem("Tools/Boss Manager/Copy CSVs")]
    public static void CopyCSVs()
    {
        var manager = FindObjectOfType<BossDataManager>();
        if (manager != null)
        {
            var editor = CreateInstance<BossDataManagerEditor>();
            editor.CopyAllCSVFiles();
        }
        else
        {
            Debug.LogError("Scene에서 BossDataManager를 찾을 수 없습니다.");
        }
    }

    [MenuItem("Tools/Boss Manager/Load Boss Data")]
    public static void LoadBossData()
    {
        var manager = FindObjectOfType<BossDataManager>();
        if (manager != null)
        {
            var editor = CreateInstance<BossDataManagerEditor>();
            editor.LoadBossDataInEditor(manager);
        }
        else
        {
            Debug.LogError("Scene에서 BossDataManager를 찾을 수 없습니다.");
        }
    }
}