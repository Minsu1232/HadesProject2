using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

public class SpawnPointMarker : MonoBehaviour
{
    public enum SpawnType { Fixed, Random, Portal, Player }

    [Header("스폰 설정")]
    public SpawnType spawnType = SpawnType.Fixed;
    public int monsterID;
    public float spawnWeight = 1f;
    public bool isBoss = false;

    [Header("시각화 설정")]
    public Color gizmoColor = Color.red;
    public float gizmoSize = 0.5f;

    private void OnDrawGizmos()
    {
        Gizmos.color = gizmoColor;

        switch (spawnType)
        {
            case SpawnType.Fixed:
                DrawCube();
                break;
            case SpawnType.Random:
                DrawSphere();
                break;
            case SpawnType.Portal:
                DrawPortal();
                break;
            case SpawnType.Player:
                DrawPlayer();
                break;
        }

        // 몬스터 ID 또는 타입 표시
#if UNITY_EDITOR
        if (spawnType == SpawnType.Fixed || spawnType == SpawnType.Random)
        {
            Handles.Label(transform.position + Vector3.up * 0.5f,
                $"ID: {monsterID}" + (isBoss ? " (Boss)" : ""));
        }
        else
        {
            Handles.Label(transform.position + Vector3.up * 0.5f,
                spawnType.ToString());
        }
#endif
    }

    private void DrawCube()
    {
        Gizmos.DrawWireCube(transform.position, Vector3.one * gizmoSize);
    }

    private void DrawSphere()
    {
        Gizmos.DrawWireSphere(transform.position, gizmoSize);
    }

    private void DrawPortal()
    {
        Gizmos.DrawWireSphere(transform.position, gizmoSize);
        Gizmos.DrawRay(transform.position, Vector3.up * gizmoSize * 2);
        Gizmos.DrawWireCube(transform.position + Vector3.up * gizmoSize * 2,
            new Vector3(gizmoSize, gizmoSize * 0.1f, gizmoSize));
    }

    private void DrawPlayer()
    {
        Gizmos.DrawIcon(transform.position, "Player", true);
        Gizmos.DrawWireSphere(transform.position, gizmoSize * 0.5f);

        // 방향 표시
        Gizmos.DrawRay(transform.position, transform.forward * gizmoSize);
        Gizmos.DrawWireCube(transform.position + transform.forward * gizmoSize,
            new Vector3(gizmoSize * 0.3f, gizmoSize * 0.3f, gizmoSize * 0.1f));
    }
}