#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.IO;

public class SpawnPointEditorWindow : EditorWindow
{
    private SpawnPointContainer spawnContainer;
    private SpawnPointMarker.SpawnType currentSpawnType = SpawnPointMarker.SpawnType.Fixed;
    private int monsterID = 1;
    private float spawnWeight = 1f;
    private bool isBoss = false;
    private Color spawnColor = Color.red;

    private bool isPlacementMode = false; // 배치 모드 활성화 상태
    private Vector3 lastHitPosition; // 마지막으로 감지된 위치
    private bool hasValidHit = false; // 유효한 히트가 있는지

    [MenuItem("Tools/스폰 포인트 에디터")]
    public static void ShowWindow()
    {
        GetWindow<SpawnPointEditorWindow>("스폰 포인트 에디터");
    }

    private void OnEnable()
    {
        // 에디터 윈도우가 활성화될 때 씬 GUI 이벤트 등록
        SceneView.duringSceneGui += OnSceneGUI;
    }

    private void OnDisable()
    {
        // 에디터 윈도우가 비활성화될 때 이벤트 해제
        SceneView.duringSceneGui -= OnSceneGUI;

        // 배치 모드 강제 종료
        isPlacementMode = false;
    }

    private void OnGUI()
    {
        GUILayout.Label("스폰 포인트 에디터", EditorStyles.boldLabel);

        // 컨테이너 선택
        spawnContainer = (SpawnPointContainer)EditorGUILayout.ObjectField(
            "스폰 컨테이너", spawnContainer, typeof(SpawnPointContainer), true);

        if (spawnContainer == null)
        {
            EditorGUILayout.HelpBox("먼저 SpawnPointContainer를 선택하세요", MessageType.Warning);

            if (GUILayout.Button("새 스폰 컨테이너 생성"))
            {
                CreateNewContainer();
            }

            return;
        }

        EditorGUILayout.Space();

        // 스테이지 정보
        spawnContainer.stageID = EditorGUILayout.TextField("스테이지 ID", spawnContainer.stageID);
        spawnContainer.stageName = EditorGUILayout.TextField("스테이지 이름", spawnContainer.stageName);

        EditorGUILayout.Space();

        // 스폰 포인트 설정
        GUILayout.Label("스폰 포인트 추가", EditorStyles.boldLabel);
        currentSpawnType = (SpawnPointMarker.SpawnType)EditorGUILayout.EnumPopup("스폰 타입", currentSpawnType);

        if (currentSpawnType != SpawnPointMarker.SpawnType.Portal)
        {
            monsterID = EditorGUILayout.IntField("몬스터 ID", monsterID);

            if (currentSpawnType == SpawnPointMarker.SpawnType.Random)
            {
                spawnWeight = EditorGUILayout.FloatField("가중치", spawnWeight);
            }

            isBoss = EditorGUILayout.Toggle("보스", isBoss);
        }

        spawnColor = EditorGUILayout.ColorField("색상", spawnColor);

        // 기존 방식: 오브젝트 선택 후 스폰 추가
        GUI.enabled = Selection.activeGameObject != null;
        if (GUILayout.Button("선택한 오브젝트 위치에 스폰 포인트 추가"))
        {
            AddSpawnPointAtSelectedPosition();
        }
        GUI.enabled = true;

        EditorGUILayout.Space();

        // 새로운 방식: 콜라이더 감지 모드
        EditorGUILayout.BeginHorizontal();

        // 배치 모드 토글 버튼
        GUIStyle toggleStyle = new GUIStyle(GUI.skin.button);
        toggleStyle.normal.textColor = isPlacementMode ? Color.green : Color.white;
        toggleStyle.fontStyle = isPlacementMode ? FontStyle.Bold : FontStyle.Normal;

        if (GUILayout.Button(isPlacementMode ? "배치 모드 종료" : "배치 모드 시작", toggleStyle))
        {
            isPlacementMode = !isPlacementMode;
            SceneView.RepaintAll(); // 씬 뷰 갱신
        }

        // 배치 모드에서의 알림
        if (isPlacementMode)
        {
            EditorGUILayout.HelpBox("씬 뷰에서 Shift+클릭으로 스폰 포인트 배치", MessageType.Info);
        }

        EditorGUILayout.EndHorizontal();

        // 마지막 히트 위치 표시 (디버그용)
        if (hasValidHit)
        {
            EditorGUILayout.LabelField("마지막 히트 위치:", lastHitPosition.ToString("F2"));
        }

        EditorGUILayout.Space();

        // 내보내기/가져오기 버튼
        EditorGUILayout.BeginHorizontal();
        if (GUILayout.Button("CSV로 내보내기"))
        {
            spawnContainer.ExportToCSV();
        }

        if (GUILayout.Button("CSV에서 가져오기"))
        {
            spawnContainer.ImportFromCSV();
        }
        EditorGUILayout.EndHorizontal();

        // 스폰 포인트 현황 표시
        if (spawnContainer != null)
        {
            EditorGUILayout.Space();
            GUILayout.Label("스폰 포인트 현황", EditorStyles.boldLabel);

            Transform fixedParent = spawnContainer.transform.Find("FixedSpawns");
            Transform randomParent = spawnContainer.transform.Find("RandomSpawns");

            int fixedCount = fixedParent != null ? fixedParent.childCount : 0;
            int randomCount = randomParent != null ? randomParent.childCount : 0;

            EditorGUILayout.LabelField($"고정 스폰 포인트: {fixedCount}개");
            EditorGUILayout.LabelField($"랜덤 스폰 포인트: {randomCount}개");

            // 플레이어 위치 표시
            if (spawnContainer.playerSpawnPoint != null)
            {
                EditorGUILayout.LabelField("플레이어 스폰 위치:",
                    spawnContainer.playerSpawnPoint.position.ToString("F2"));
            }

            // 포탈 위치 표시
            if (spawnContainer.portalSpawnPoint != null)
            {
                EditorGUILayout.LabelField("포탈 스폰 위치:",
                    spawnContainer.portalSpawnPoint.position.ToString("F2"));
            }
        }
    }

    // 씬 GUI 이벤트 처리
    private void OnSceneGUI(SceneView sceneView)
    {
        if (!isPlacementMode || spawnContainer == null) return;

        Event e = Event.current;

        // 마우스 이동 시 레이캐스트로 콜라이더 감지
        if (e.type == EventType.MouseMove)
        {
            Ray ray = HandleUtility.GUIPointToWorldRay(e.mousePosition);
            if (Physics.Raycast(ray, out RaycastHit hit))
            {
                // 콜라이더 히트 감지됨
                lastHitPosition = hit.point;
                hasValidHit = true;
                sceneView.Repaint(); // 씬 뷰 갱신
            }
            else
            {
                hasValidHit = false;
            }
        }

        // 배치 모드에서 Shift + 클릭으로 스폰 포인트 배치
        if (e.type == EventType.MouseDown && e.button == 0 && e.shift && hasValidHit)
        {
            // 스폰 포인트 생성
            AddSpawnPointAtPosition(lastHitPosition);
            e.Use(); // 이벤트 사용 완료 처리
        }

        // 배치 모드 시각화
        if (hasValidHit)
        {
            // 에디터 핸들 그리기
            Handles.color = spawnColor;

            switch (currentSpawnType)
            {
                case SpawnPointMarker.SpawnType.Fixed:
                    Handles.DrawWireCube(lastHitPosition, Vector3.one * 0.5f);
                    break;
                case SpawnPointMarker.SpawnType.Random:
                    Handles.SphereHandleCap(0, lastHitPosition, Quaternion.identity, 0.5f, EventType.Repaint);
                    break;
                case SpawnPointMarker.SpawnType.Portal:
                    Handles.SphereHandleCap(0, lastHitPosition, Quaternion.identity, 0.5f, EventType.Repaint);
                    Handles.DrawLine(lastHitPosition, lastHitPosition + Vector3.up * 1.0f);
                    break;
            }

            // 라벨 표시
            Handles.Label(lastHitPosition + Vector3.up * 0.5f,
                currentSpawnType == SpawnPointMarker.SpawnType.Portal
                    ? "포탈"
                    : $"ID: {monsterID}");
        }
    }

    private void CreateNewContainer()
    {
        GameObject containerObj = new GameObject("SpawnPointContainer");
        spawnContainer = containerObj.AddComponent<SpawnPointContainer>();

        // 자식 컨테이너 생성
        GameObject fixedContainer = new GameObject("FixedSpawns");
        fixedContainer.transform.parent = containerObj.transform;

        GameObject randomContainer = new GameObject("RandomSpawns");
        randomContainer.transform.parent = containerObj.transform;

        // 플레이어 스폰 포인트
        GameObject playerSpawn = new GameObject("PlayerSpawn");
        playerSpawn.transform.parent = containerObj.transform;
        spawnContainer.playerSpawnPoint = playerSpawn.transform;

        // 포탈 스폰 포인트
        GameObject portalSpawn = new GameObject("PortalSpawn");
        portalSpawn.transform.parent = containerObj.transform;
        spawnContainer.portalSpawnPoint = portalSpawn.transform;

        Selection.activeGameObject = containerObj;
    }

    // 선택한 오브젝트 위치에 스폰 포인트 추가 (기존 방식)
    private void AddSpawnPointAtSelectedPosition()
    {
        if (Selection.activeGameObject == null)
        {
            EditorUtility.DisplayDialog("오류", "오브젝트를 선택해야 합니다", "확인");
            return;
        }

        Vector3 position = Selection.activeGameObject.transform.position;
        AddSpawnPointAtPosition(position);
    }

    // 지정한 위치에 스폰 포인트 추가 (공통 메서드)
    private void AddSpawnPointAtPosition(Vector3 position)
    {
        if (spawnContainer == null) return;

        GameObject spawnObj = new GameObject(GetSpawnName());
        spawnObj.transform.position = position;

        // 부모 설정
        Transform parent = null;

        switch (currentSpawnType)
        {
            case SpawnPointMarker.SpawnType.Fixed:
                parent = spawnContainer.transform.Find("FixedSpawns");
                break;
            case SpawnPointMarker.SpawnType.Random:
                parent = spawnContainer.transform.Find("RandomSpawns");
                break;
            case SpawnPointMarker.SpawnType.Portal:
                // 포탈 위치 업데이트
                if (spawnContainer.portalSpawnPoint != null)
                {
                    spawnContainer.portalSpawnPoint.position = position;
                    DestroyImmediate(spawnObj); // 임시 오브젝트 제거
                    return;
                }
                else
                {
                    // 포탈 스폰 포인트가 없으면 새로 생성
                    spawnObj.name = "PortalSpawn";
                    spawnContainer.portalSpawnPoint = spawnObj.transform;
                    parent = spawnContainer.transform;
                }
                break;
            case SpawnPointMarker.SpawnType.Player:
                // 플레이어 위치 업데이트
                if (spawnContainer.playerSpawnPoint != null)
                {
                    spawnContainer.playerSpawnPoint.position = position;
                    DestroyImmediate(spawnObj); // 임시 오브젝트 제거
                    return;
                }
                else
                {
                    // 플레이어 스폰 포인트가 없으면 새로 생성
                    spawnObj.name = "PlayerSpawn";
                    spawnContainer.playerSpawnPoint = spawnObj.transform;
                    parent = spawnContainer.transform;
                }
                break;
        }

        if (parent != null)
        {
            spawnObj.transform.parent = parent;
        }
        else
        {
            spawnObj.transform.parent = spawnContainer.transform;
        }

        // 마커 컴포넌트 추가 (포탈이나 플레이어가 아닌 경우)
        if (currentSpawnType != SpawnPointMarker.SpawnType.Portal &&
            currentSpawnType != SpawnPointMarker.SpawnType.Player)
        {
            SpawnPointMarker marker = spawnObj.AddComponent<SpawnPointMarker>();
            marker.spawnType = currentSpawnType;
            marker.monsterID = monsterID;
            marker.spawnWeight = spawnWeight;
            marker.isBoss = isBoss;
            marker.gizmoColor = spawnColor;
        }

        // 스폰 포인트 추가 성공 로그
        Debug.Log($"스폰 포인트 추가됨: {spawnObj.name}, 위치: {position}");

        // 에디터에서 선택
        Selection.activeGameObject = spawnObj;

        // 컨테이너가 변경됨을 기록 (Undo 지원)
        EditorUtility.SetDirty(spawnContainer);
    }

    private string GetSpawnName()
    {
        switch (currentSpawnType)
        {
            case SpawnPointMarker.SpawnType.Fixed:
                return isBoss ? $"Boss_{monsterID}" : $"Fixed_{monsterID}";
            case SpawnPointMarker.SpawnType.Random:
                return $"Random_{monsterID}";
            case SpawnPointMarker.SpawnType.Portal:
                return "Portal";
            case SpawnPointMarker.SpawnType.Player:
                return "PlayerSpawn";
            default:
                return "Spawn";
        }
    }
}
#endif