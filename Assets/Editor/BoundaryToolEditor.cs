using UnityEngine;
using UnityEditor;

public class BoundaryToolEditor : EditorWindow
{
    // 경계 점 리스트
    private Vector3[] boundaryPoints;
    private GameObject boundaryPrefab;

    [MenuItem("Tools/Boundary Tool")]
    public static void ShowWindow()
    {
        GetWindow<BoundaryToolEditor>("Boundary Tool");
    }

    private void OnGUI()
    {
        GUILayout.Label("Boundary Tool", EditorStyles.boldLabel);

        // 경계 Prefab 설정
        boundaryPrefab = (GameObject)EditorGUILayout.ObjectField("Boundary Prefab", boundaryPrefab, typeof(GameObject), false);

        // 경계 점 개수 설정
        int pointCount = EditorGUILayout.IntField("Number of Points", boundaryPoints != null ? boundaryPoints.Length : 0);

        if (boundaryPoints == null || boundaryPoints.Length != pointCount)
        {
            boundaryPoints = new Vector3[pointCount];
        }

        // 경계 점 위치 입력
        for (int i = 0; i < pointCount; i++)
        {
            boundaryPoints[i] = EditorGUILayout.Vector3Field($"Point {i + 1}", boundaryPoints[i]);
        }

        // 버튼: 경계 생성
        if (GUILayout.Button("Generate Boundaries"))
        {
            if (ValidateInputs())
                GenerateBoundaries();
        }

        // 버튼: 경계 삭제
        if (GUILayout.Button("Clear Boundaries"))
        {
            ClearBoundaries();
        }
    }

    private bool ValidateInputs()
    {
        if (boundaryPrefab == null)
        {
            Debug.LogError("Boundary Prefab is not assigned!");
            return false;
        }

        if (boundaryPoints == null || boundaryPoints.Length < 2)
        {
            Debug.LogError("Not enough points to create boundaries! At least 2 points are required.");
            return false;
        }

        return true;
    }

    private void GenerateBoundaries()
    {
        for (int i = 0; i < boundaryPoints.Length - 1; i++) // 마지막 점에서 첫 번째 점으로 돌아가지 않음
        {
            Vector3 start = boundaryPoints[i];
            Vector3 end = boundaryPoints[i + 1];

            CreateBoundarySegment(start, end);
        }

        Debug.Log("Boundaries generated successfully!");
    }

    private void CreateBoundarySegment(Vector3 start, Vector3 end)
    {
        if (boundaryPrefab == null)
            return;

        // 두 점의 중앙 위치
        Vector3 midPoint = (start + end) / 2;

        // 두 점 사이의 거리
        float distance = Vector3.Distance(start, end);

        // 방향 계산: X축 기준으로 회전하도록 설정
        Vector3 direction = (end - start).normalized;
        Quaternion rotation = Quaternion.FromToRotation(Vector3.right, direction); // X축을 기준으로 방향 계산

        // Boundary Prefab 생성
        GameObject boundary = Instantiate(boundaryPrefab, midPoint, rotation);

        // Box Collider 크기 조정
        BoxCollider collider = boundary.GetComponent<BoxCollider>();
        if (collider != null)
        {
            collider.size = new Vector3(distance, collider.size.y, collider.size.z); // X축 크기만 변경
                                                                                     // 오브젝트 이름과 태그 설정
            boundary.name = "Boundary Segment";
            boundary.tag = "Boundary"; // 태그를 설정하여 Clear 기능과 연동
        }

      
      
    }

    private void ClearBoundaries()
    {
        GameObject[] boundaries = GameObject.FindGameObjectsWithTag("Boundary");
        foreach (GameObject boundary in boundaries)
        {
            DestroyImmediate(boundary); // 즉시 삭제
        }

        Debug.Log("Boundaries cleared!");
    }
}
