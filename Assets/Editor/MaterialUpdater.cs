using UnityEngine;
using UnityEditor;

public class MaterialUpdater : Editor
{
    [MenuItem("Tools/Replace Materials with Flat Kit Shader")]
    static void ReplaceMaterials()
    {
        // ��� ��Ƽ������ �˻��մϴ�.
        string[] guids = AssetDatabase.FindAssets("t:Material");
        foreach (string guid in guids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            Material mat = AssetDatabase.LoadAssetAtPath<Material>(path);

            // URP Standard Shader�� ����ϴ� ��Ƽ���� �����Ͽ� ��ü�մϴ�.
            if (mat.shader.name == "Universal Render Pipeline/Lit")
            {
                mat.shader = Shader.Find("FlatKit/Stylized Surface With Outline"); // Flat Kit�� Toon ���̴��� ��ü
            }
        }
        Debug.Log("��Ƽ���� ��ü �Ϸ�!");
    }
    [MenuItem("Tools/Replace Materials with Flat Kit Shader (FlatKit/Stylized Surface) > FlatKit/Stylized Surface With Outline")]
    static void ReplaceMaterials2()
    {
        // ��� ��Ƽ������ �˻��մϴ�.
        string[] guids = AssetDatabase.FindAssets("t:Material");
        foreach (string guid in guids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            Material mat = AssetDatabase.LoadAssetAtPath<Material>(path);

            // URP Standard Shader�� ����ϴ� ��Ƽ���� �����Ͽ� ��ü�մϴ�.
            if (mat.shader.name == "FlatKit/Stylized Surface")
            {
                mat.shader = Shader.Find("FlatKit/Stylized Surface With Outline"); // Flat Kit�� Toon ���̴��� ��ü
            }
        }
        Debug.Log("��Ƽ���� ��ü �Ϸ�!");
    }
}
