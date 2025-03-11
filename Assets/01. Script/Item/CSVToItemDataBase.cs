// CSVToItemDatabase.cs - CSV에서 아이템 데이터베이스 생성 에디터 스크립트
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
        GUILayout.Label("CSV 파일을 아이템 데이터베이스로 변환", EditorStyles.boldLabel);

        itemCSV = (TextAsset)EditorGUILayout.ObjectField("아이템 CSV", itemCSV, typeof(TextAsset), false);
        targetDatabase = (ItemDatabase)EditorGUILayout.ObjectField("대상 데이터베이스", targetDatabase, typeof(ItemDatabase), false);

        if (GUILayout.Button("변환"))
        {
            if (itemCSV == null || targetDatabase == null)
            {
                EditorUtility.DisplayDialog("오류", "CSV 파일과 대상 데이터베이스를 모두 지정해주세요!", "확인");
                return;
            }

            ConvertCSVToDatabase();
        }
    }

    private void ConvertCSVToDatabase()
    {
        // CSV 파싱 로직
        // 이 부분은 CSV 구조에 맞게 구현해야 함

        EditorUtility.SetDirty(targetDatabase);
        AssetDatabase.SaveAssets();

        EditorUtility.DisplayDialog("성공", "CSV 데이터가 데이터베이스로 변환되었습니다!", "확인");
    }
}
#endif