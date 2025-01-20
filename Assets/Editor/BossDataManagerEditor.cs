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
                Debug.Log($"CSV ���� ���� �Ϸ�: {persistentPath}");
            }
            else
            {
                Debug.LogError($"StreamingAssets���� {fileName} ������ ã�� �� �����ϴ�.");
            }
        }
    }

    private void LoadBossDataInEditor(BossDataManager manager)
    {
        string bossBasePath = GetCSVPath("BossBase.csv");
        if (!File.Exists(bossBasePath))
        {
            Debug.LogError($"���� ������ CSV ������ ã�� �� �����ϴ�: {bossBasePath}");
            return;
        }

        EditorUtility.DisplayProgressBar("Loading Boss Data", "���� �����͸� �ε��ϴ� ��...", 0f);

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
                    $"���� �����͸� �ε��ϴ� ��... ({currentLine}/{totalLines})",
                    currentLine / totalLines);

                string[] values = line.Trim().Split(',');
                if (values.Length == 0)
                {
                    Debug.LogWarning($"�߸��� CSV ���� ����: {line}");
                    continue;
                }

                string bossDataKey = $"BossData_{values[0]}";

                // GUID �˻� ��� Ȯ��
                string[] guids = AssetDatabase.FindAssets($"t:BossData {bossDataKey}");
                if (guids.Length == 0)
                {
                    Debug.LogError($"������ ã�� �� �����ϴ�: {bossDataKey}");
                    continue;
                }

                // ���� ��� Ȯ��
                string assetPath = AssetDatabase.GUIDToAssetPath(guids[0]);
                if (string.IsNullOrEmpty(assetPath))
                {
                    Debug.LogError($"���� ��θ� ã�� �� �����ϴ�: {bossDataKey}");
                    continue;
                }

                // ���� �ε� Ȯ��
                var bossData = AssetDatabase.LoadAssetAtPath<BossData>(assetPath);
                if (bossData == null)
                {
                    Debug.LogError($"���� �����͸� �ε��� �� �����ϴ�: {bossDataKey} (���: {assetPath})");
                    continue;
                }

                try
                {
                    UpdateBossDataInEditor(bossData, values);
                    EditorUtility.SetDirty(bossData);
                }
                catch (Exception ex)
                {
                    Debug.LogError($"���� ������ ������Ʈ �� ���� �߻� - {bossDataKey}: {ex.Message}");
                    continue;
                }

                currentLine++;
            }

            AssetDatabase.SaveAssets();
            Debug.Log("��� ���� ������ �ε� �Ϸ�");
        }
        catch (Exception e)
        {
            Debug.LogError($"���� ������ �ε� �� ���� �߻�: {e.Message}\n{e.StackTrace}");
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
            Debug.LogError("Scene���� BossDataManager�� ã�� �� �����ϴ�.");
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
            Debug.LogError("Scene���� BossDataManager�� ã�� �� �����ϴ�.");
        }
    }
}