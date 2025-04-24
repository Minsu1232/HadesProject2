#if UNITY_EDITOR
using UnityEngine;
using Sirenix.OdinInspector;
using System.Collections.Generic;
using UnityEditor;

[ExecuteInEditMode]
public class PointMarker : MonoBehaviour
{
    [PropertyOrder(0)]
    [InfoBox("��(Point)�� Scene���� ���� ���, Collider�� ����/������ �� �ֽ��ϴ�.")]
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

        // ���� Collider ���� �� ���� ����
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
        colliders.Clear(); // ������ Collider ����Ʈ�� �ʱ�ȭ
        Debug.Log("All colliders cleared.");
    }

    [PropertyOrder(1)]
    [InlineEditor(Expanded = true)]
    [Title("Boundary Prefab", "Collider ������ ����� Prefab")]
    public GameObject boundaryPrefab;

    [PropertyOrder(2)]
    [TableList(ShowIndexLabels = true, AlwaysExpanded = true)]
    [Title("Points", "����� �� ����Ʈ")]
    public List<Vector3> points = new List<Vector3>();

    [PropertyOrder(3)]
    [Title("Placing Mode")]
    public bool isPlacingPoints { get; set; } // Getter�� Setter�� ����

    [PropertyOrder(4)]
    [Range(0.1f, 1f)]
    [Title("Gizmo Settings", "�� �ð�ȭ ����")]
    public float gizmoPointSize = 0.2f;

    private List<Collider> colliders = new List<Collider>(); // ������ Collider ����Ʈ

    private void OnDrawGizmos()
    {
        if (points.Count == 0) return;

        Gizmos.color = Color.green;

        foreach (Vector3 point in points)
        {
            Gizmos.DrawSphere(point, gizmoPointSize); // �� ǥ��
        }

        Gizmos.color = Color.red;
        for (int i = 0; i < points.Count - 1; i++)
        {
            Gizmos.DrawLine(points[i], points[i + 1]); // �� ���� �� ǥ��
        }
    }

    private void CreateBoundarySegment(Vector3 start, Vector3 end)
    {
        Vector3 midPoint = (start + end) / 2; // �� ���� �߰� ��ġ
        float distance = Vector3.Distance(start, end); // �� �� ���� �Ÿ�
        Quaternion rotation = Quaternion.FromToRotation(Vector3.right, end - start); // ���� ����

        GameObject segment = Instantiate(boundaryPrefab, midPoint, rotation, transform); // �ڽ����� ����
        BoxCollider collider = segment.GetComponent<BoxCollider>();
        if (collider != null)
        {
            collider.size = new Vector3(distance, collider.size.y, collider.size.z); // Collider ũ�� ����
            AddCollider(collider); // �� Collider �߰� �� �浹 ���� ó��
        }
    }

    private void AddCollider(Collider newCollider)
    {
        // ���� Collider�� �浹 ����
        foreach (var existingCollider in colliders)
        {
            Physics.IgnoreCollision(existingCollider, newCollider);
        }

        // �� Collider�� ����Ʈ�� �߰�
        colliders.Add(newCollider);
    }

    private void OnSceneGUI(SceneView sceneView)
    {
        if (!isPlacingPoints) return;

        Event e = Event.current;

        // ���콺 Ŭ�� ����
        if (e.type == EventType.MouseDown && e.button == 0) // ��Ŭ��
        {
            Ray ray = HandleUtility.GUIPointToWorldRay(e.mousePosition);
            if (Physics.Raycast(ray, out RaycastHit hit))
            {
                Undo.RecordObject(this, "Add Point");
                points.Add(hit.point); // Ŭ���� ��ġ�� �� ����Ʈ�� �߰�
                e.Use(); // �̺�Ʈ �Һ�
                SceneView.RepaintAll(); // Scene �� ����
            }
        }
    }
}
#endif
