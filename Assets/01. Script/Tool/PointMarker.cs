#if UNITY_EDITOR
using UnityEngine;
using Sirenix.OdinInspector;
using System.Collections.Generic;
using UnityEditor;

[ExecuteInEditMode]
public class PointMarker : MonoBehaviour
{
    [PropertyOrder(0)]
    [InfoBox("점(Point)을 Scene에서 직접 찍고, Collider를 생성/삭제할 수 있습니다.")]
    [Button("Start Placing Points", ButtonSizes.Medium)]
    public void StartPlacingPoints()
    {
        isPlacingPoints = true;
        SceneView.duringSceneGui += OnSceneGUI;
    }

    [PropertyOrder(0)]
    [Button("Stop Placing Points", ButtonSizes.Medium)]
    public void StopPlacingPoints()
    {
        isPlacingPoints = false;
        SceneView.duringSceneGui -= OnSceneGUI;
    }

    [PropertyOrder(0)]
    [Button("Clear Points", ButtonSizes.Medium)]
    public void ClearPoints()
    {
        points.Clear();
        Debug.Log("All points cleared.");
    }

    [PropertyOrder(0)]
    [Button("Generate Colliders", ButtonSizes.Medium)]
    public void GenerateColliders()
    {
        if (boundaryPrefab == null)
        {
            Debug.LogError("Boundary Prefab is not assigned!");
            return;
        }
        if (points.Count < 2)
        {
            Debug.LogError("Not enough points to create boundaries! At least 2 points are required.");
            return;
        }

        // 기존 Collider 제거 후 새로 생성
        ClearColliders();

        for (int i = 0; i < points.Count - 1; i++)
        {
            Vector3 start = points[i];
            Vector3 end = points[i + 1];

            CreateBoundarySegment(start, end);
        }

        Debug.Log("Colliders generated successfully!");
    }

    [PropertyOrder(0)]
    [Button("Clear Colliders", ButtonSizes.Medium)]
    public void ClearColliders()
    {
        foreach (Transform child in transform)
        {
            DestroyImmediate(child.gameObject);
        }
        colliders.Clear(); // 생성된 Collider 리스트도 초기화
        Debug.Log("All colliders cleared.");
    }

    [PropertyOrder(1)]
    [InlineEditor(Expanded = true)]
    [Title("Boundary Prefab", "Collider 생성에 사용할 Prefab")]
    public GameObject boundaryPrefab;

    [PropertyOrder(2)]
    [TableList(ShowIndexLabels = true, AlwaysExpanded = true)]
    [Title("Points", "저장된 점 리스트")]
    public List<Vector3> points = new List<Vector3>();

    [PropertyOrder(3)]
    [Title("Placing Mode")]
    public bool isPlacingPoints { get; set; } // Getter와 Setter로 정의

    [PropertyOrder(4)]
    [Range(0.1f, 1f)]
    [Title("Gizmo Settings", "점 시각화 설정")]
    public float gizmoPointSize = 0.2f;

    private List<Collider> colliders = new List<Collider>(); // 생성된 Collider 리스트

    private void OnDrawGizmos()
    {
        if (points.Count == 0) return;

        Gizmos.color = Color.green;

        foreach (Vector3 point in points)
        {
            Gizmos.DrawSphere(point, gizmoPointSize); // 점 표시
        }

        Gizmos.color = Color.red;
        for (int i = 0; i < points.Count - 1; i++)
        {
            Gizmos.DrawLine(points[i], points[i + 1]); // 점 연결 선 표시
        }
    }

    private void CreateBoundarySegment(Vector3 start, Vector3 end)
    {
        Vector3 midPoint = (start + end) / 2; // 두 점의 중간 위치
        float distance = Vector3.Distance(start, end); // 두 점 사이 거리
        Quaternion rotation = Quaternion.FromToRotation(Vector3.right, end - start); // 방향 설정

        GameObject segment = Instantiate(boundaryPrefab, midPoint, rotation, transform); // 자식으로 생성
        BoxCollider collider = segment.GetComponent<BoxCollider>();
        if (collider != null)
        {
            collider.size = new Vector3(distance, collider.size.y, collider.size.z); // Collider 크기 설정
            AddCollider(collider); // 새 Collider 추가 및 충돌 무시 처리
        }
    }

    private void AddCollider(Collider newCollider)
    {
        // 기존 Collider와 충돌 무시
        foreach (var existingCollider in colliders)
        {
            Physics.IgnoreCollision(existingCollider, newCollider);
        }

        // 새 Collider를 리스트에 추가
        colliders.Add(newCollider);
    }

    private void OnSceneGUI(SceneView sceneView)
    {
        if (!isPlacingPoints) return;

        Event e = Event.current;

        // 마우스 클릭 감지
        if (e.type == EventType.MouseDown && e.button == 0) // 좌클릭
        {
            Ray ray = HandleUtility.GUIPointToWorldRay(e.mousePosition);
            if (Physics.Raycast(ray, out RaycastHit hit))
            {
                Undo.RecordObject(this, "Add Point");
                points.Add(hit.point); // 클릭한 위치를 점 리스트에 추가
                e.Use(); // 이벤트 소비
                SceneView.RepaintAll(); // Scene 뷰 갱신
            }
        }
    }
}
#endif
