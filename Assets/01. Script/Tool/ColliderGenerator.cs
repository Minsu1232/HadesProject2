using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ColliderGenerator : MonoBehaviour
{
    public PointMarker pointMarker; // PointMarker 연결
    public GameObject boundaryPrefab; // Collider 프리팹

    public void GenerateColliders()
    {
        if (boundaryPrefab == null || pointMarker == null || pointMarker.points.Count < 2)
        {
            Debug.LogError("Boundary Prefab, PointMarker, or Points are not properly set.");
            return;
        }

        for (int i = 0; i < pointMarker.points.Count - 1; i++)
        {
            Vector3 start = pointMarker.points[i];
            Vector3 end = pointMarker.points[i + 1];

            CreateBoundarySegment(start, end);
        }
    }

    private void CreateBoundarySegment(Vector3 start, Vector3 end)
    {
        Vector3 midPoint = (start + end) / 2; // 중간 위치 계산
        float distance = Vector3.Distance(start, end); // 길이 계산
        Quaternion rotation = Quaternion.FromToRotation(Vector3.right, end - start); // 방향 계산

        GameObject segment = Instantiate(boundaryPrefab, midPoint, rotation);
        BoxCollider collider = segment.GetComponent<BoxCollider>();
        if (collider != null)
        {
            collider.size = new Vector3(distance, collider.size.y, collider.size.z); // 길이만 조정
        }
    }
}
