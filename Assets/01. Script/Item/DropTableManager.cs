// DropTableManager.cs - CSV에서 드롭 테이블 로드 및 관리
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class DropTableManager : MonoBehaviour
{
    public static DropTableManager Instance { get; private set; }

    // 드롭 테이블 데이터 저장소
    private Dictionary<int, List<ItemDropEntry>> monsterDropTables = new Dictionary<int, List<ItemDropEntry>>();
    private Dictionary<int, List<ItemDropEntry>> bossDropTables = new Dictionary<int, List<ItemDropEntry>>();

    private string dropTablePath;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        // CSV 파일 경로 설정
        dropTablePath = Path.Combine(Application.persistentDataPath, "DropTables.csv");

        // StreamingAssets에서 CSV 파일 복사
        CopyCSVFromStreamingAssets();

        // 드롭 테이블 로드
        LoadDropTablesFromCSV();

        Debug.Log("==== 몬스터 드롭 테이블 ====");
        foreach (var kvp in monsterDropTables)
        {
            Debug.Log($"몬스터 ID: {kvp.Key}, 아이템 수: {kvp.Value.Count}");
        }
    }

    // StreamingAssets에서 CSV 파일 복사
    private void CopyCSVFromStreamingAssets()
    {
        string streamingPath = Path.Combine(Application.streamingAssetsPath, "DropTables.csv");

        if (File.Exists(streamingPath))
        {
            File.Copy(streamingPath, dropTablePath, true);
            Debug.Log("드롭 테이블 CSV 파일 복사 완료");
        }
        else
        {
            Debug.LogWarning("StreamingAssets에서 드롭 테이블 CSV 파일을 찾을 수 없습니다.");
        }
    }

    // CSV에서 드롭 테이블 로드
    private void LoadDropTablesFromCSV()
    {
        if (!File.Exists(dropTablePath))
        {
            Debug.LogError("드롭 테이블 CSV 파일을 찾을 수 없습니다: " + dropTablePath);
            return;
        }

        // CSV 파일 읽기
        string[] lines = File.ReadAllLines(dropTablePath);

        // 헤더 검사
        if (lines.Length == 0)
        {
            Debug.LogError("드롭 테이블 CSV 파일이 비어 있습니다.");
            return;
        }

        string[] headers = lines[0].Split(',');

        // 필수 열 인덱스 찾기
        int entityIdIndex = FindColumnIndex(headers, "EntityID");
        int isBossIndex = FindColumnIndex(headers, "IsBoss");
        int itemIdIndex = FindColumnIndex(headers, "ItemID");
        int dropChanceIndex = FindColumnIndex(headers, "DropChance");
        int minQuantityIndex = FindColumnIndex(headers, "MinQuantity");
        int maxQuantityIndex = FindColumnIndex(headers, "MaxQuantity");
        int itemNameIndex = FindColumnIndex(headers, "ItemName");

        // 필수 열이 없으면 오류 메시지 표시
        if (entityIdIndex == -1 || isBossIndex == -1 || itemIdIndex == -1 || dropChanceIndex == -1)
        {
            Debug.LogError("드롭 테이블 CSV 파일에 필수 열이 없습니다. 필수 열: EntityID, IsBoss, ItemID, DropChance");
            return;
        }

        // 각 행 처리
        for (int i = 1; i < lines.Length; i++)
        {
            // 빈 줄 건너뛰기
            if (string.IsNullOrWhiteSpace(lines[i])) continue;

            string[] values = lines[i].Split(',');

            // 값이 충분한지 확인
            if (values.Length <= System.Math.Max(entityIdIndex, System.Math.Max(isBossIndex, System.Math.Max(itemIdIndex, dropChanceIndex))))
            {
                Debug.LogWarning($"드롭 테이블 CSV 행 {i + 1}에 값이 부족합니다. 건너뜁니다.");
                continue;
            }

            try
            {
                // 기본 데이터 파싱
                int entityId = int.Parse(values[entityIdIndex]);
                bool isBoss = bool.Parse(values[isBossIndex]);
                int itemId = int.Parse(values[itemIdIndex]);
                float dropChance = float.Parse(values[dropChanceIndex]);

                // 선택적 데이터 파싱 (기본값 제공)
                int minQuantity = 1;
                int maxQuantity = 1;
                string itemName = "";

                // 수량 파싱 (있는 경우)
                if (minQuantityIndex != -1 && values.Length > minQuantityIndex)
                {
                    int.TryParse(values[minQuantityIndex], out minQuantity);
                }

                if (maxQuantityIndex != -1 && values.Length > maxQuantityIndex)
                {
                    int.TryParse(values[maxQuantityIndex], out maxQuantity);
                    // 최소값보다 작으면 최소값으로 조정
                    maxQuantity = Mathf.Max(minQuantity, maxQuantity);
                }

                // 아이템 이름 파싱 (있는 경우)
                if (itemNameIndex != -1 && values.Length > itemNameIndex)
                {
                    itemName = values[itemNameIndex];
                }

                // 드롭 엔트리 생성
                ItemDropEntry entry = new ItemDropEntry
                {
                    itemId = itemId,
                    dropChance = dropChance,
                    minQuantity = minQuantity,
                    maxQuantity = maxQuantity,
                    itemName = itemName
                };

                // 적절한 테이블에 추가
                if (isBoss)
                {
                    if (!bossDropTables.ContainsKey(entityId))
                    {
                        bossDropTables[entityId] = new List<ItemDropEntry>();
                    }
                    bossDropTables[entityId].Add(entry);
                }
                else
                {
                    if (!monsterDropTables.ContainsKey(entityId))
                    {
                        monsterDropTables[entityId] = new List<ItemDropEntry>();
                    }
                    monsterDropTables[entityId].Add(entry);
                }
            }
            catch (System.Exception e)
            {
                Debug.LogError($"드롭 테이블 CSV 행 {i + 1} 처리 중 오류 발생: {e.Message}");
            }
        }

        Debug.Log($"드롭 테이블 로드 완료: {monsterDropTables.Count}개 몬스터, {bossDropTables.Count}개 보스");
    }

    // CSV 컬럼 인덱스 찾기 헬퍼 함수
    private int FindColumnIndex(string[] headers, string columnName)
    {
        for (int i = 0; i < headers.Length; i++)
        {
            if (headers[i].Trim().Equals(columnName, System.StringComparison.OrdinalIgnoreCase))
            {
                return i;
            }
        }
        return -1; // 찾지 못함
    }

    // 몬스터 드롭 테이블 가져오기
    public List<ItemDropEntry> GetMonsterDropTable(int monsterId)
    {
        Debug.Log($"[DropTableManager] GetMonsterDropTable 호출: ID = {monsterId}");

        if (monsterDropTables.TryGetValue(monsterId, out List<ItemDropEntry> entries))
        {
            Debug.Log($"[DropTableManager] 드롭 테이블 찾음: {entries.Count}개 항목");
            return entries;
        }

        Debug.LogWarning($"[DropTableManager] ID {monsterId}에 대한 드롭 테이블을 찾을 수 없습니다.");
        return null;
    }

    // 보스 드롭 테이블 가져오기
    public List<ItemDropEntry> GetBossDropTable(int bossId)
    {
        if (bossDropTables.TryGetValue(bossId, out List<ItemDropEntry> entries))
        {
            return entries;
        }
        return null;
    }

    // 디버그용 - 모든 드롭 테이블 정보 출력
    public void DebugPrintAllDropTables()
    {
        Debug.Log("==== 몬스터 드롭 테이블 ====");
        foreach (var kvp in monsterDropTables)
        {
            Debug.Log($"몬스터 ID: {kvp.Key}, 아이템 수: {kvp.Value.Count}");
            foreach (var entry in kvp.Value)
            {
                Debug.Log($"  아이템 ID: {entry.itemId}, 이름: {entry.itemName}, 드롭률: {entry.dropChance}%, 수량: {entry.minQuantity}-{entry.maxQuantity}");
            }
        }

        Debug.Log("==== 보스 드롭 테이블 ====");
        foreach (var kvp in bossDropTables)
        {
            Debug.Log($"보스 ID: {kvp.Key}, 아이템 수: {kvp.Value.Count}");
            foreach (var entry in kvp.Value)
            {
                Debug.Log($"  아이템 ID: {entry.itemId}, 이름: {entry.itemName}, 드롭률: {entry.dropChance}%, 수량: {entry.minQuantity}-{entry.maxQuantity}");
            }
        }
    }
}