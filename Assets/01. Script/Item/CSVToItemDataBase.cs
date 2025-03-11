// CSVToItemDatabase.cs - CSV���� ������ �����ͺ��̽� ���� ������ ��ũ��Ʈ
#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using System.IO;
using System.Collections.Generic;

public class CSVToItemDatabase : EditorWindow
{
    private TextAsset itemCSV;
    private ItemDatabase targetDatabase;

    [MenuItem("Tools/CSV to Item Database")]
    public static void ShowWindow()
    {
        GetWindow<CSVToItemDatabase>("CSV to Item Database");
    }

    private void OnGUI()
    {
        GUILayout.Label("CSV ������ ������ �����ͺ��̽��� ��ȯ", EditorStyles.boldLabel);

        itemCSV = (TextAsset)EditorGUILayout.ObjectField("������ CSV", itemCSV, typeof(TextAsset), false);
        targetDatabase = (ItemDatabase)EditorGUILayout.ObjectField("��� �����ͺ��̽�", targetDatabase, typeof(ItemDatabase), false);

        if (GUILayout.Button("��ȯ"))
        {
            if (itemCSV == null || targetDatabase == null)
            {
                EditorUtility.DisplayDialog("����", "CSV ���ϰ� ��� �����ͺ��̽��� ��� �������ּ���!", "Ȯ��");
                return;
            }

            ConvertCSVToDatabase();
        }
    }

    private void ConvertCSVToDatabase()
    {
        // CSV �Ľ� ����
        // �� �κ��� CSV ������ �°� �����ؾ� ��

        EditorUtility.SetDirty(targetDatabase);
        AssetDatabase.SaveAssets();

        EditorUtility.DisplayDialog("����", "CSV �����Ͱ� �����ͺ��̽��� ��ȯ�Ǿ����ϴ�!", "Ȯ��");
    }
}
#endif