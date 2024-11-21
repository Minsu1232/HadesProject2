using UnityEngine;
using UnityEditor;

public class MaterialUpdater : Editor
{
    [MenuItem("Tools/Replace Materials with Flat Kit Shader")]
    static void ReplaceMaterials()
    {
        // 모든 머티리얼을 검색합니다.
        string[] guids = AssetDatabase.FindAssets("t:Material");
        foreach (string guid in guids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            Material mat = AssetDatabase.LoadAssetAtPath<Material>(path);

            // URP Standard Shader를 사용하는 머티리얼만 선택하여 교체합니다.
            if (mat.shader.name == "Universal Render Pipeline/Lit")
            {
                mat.shader = Shader.Find("FlatKit/Stylized Surface With Outline"); // Flat Kit의 Toon 셰이더로 교체
            }
        }
        Debug.Log("머티리얼 교체 완료!");
    }
    [MenuItem("Tools/Replace Materials with Flat Kit Shader (FlatKit/Stylized Surface) > FlatKit/Stylized Surface With Outline")]
    static void ReplaceMaterials2()
    {
        // 모든 머티리얼을 검색합니다.
        string[] guids = AssetDatabase.FindAssets("t:Material");
        foreach (string guid in guids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            Material mat = AssetDatabase.LoadAssetAtPath<Material>(path);

            // URP Standard Shader를 사용하는 머티리얼만 선택하여 교체합니다.
            if (mat.shader.name == "FlatKit/Stylized Surface")
            {
                mat.shader = Shader.Find("FlatKit/Stylized Surface With Outline"); // Flat Kit의 Toon 셰이더로 교체
            }
        }
        Debug.Log("머티리얼 교체 완료!");
    }
}
