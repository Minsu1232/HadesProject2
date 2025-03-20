// ItemDropSystem.cs - 몬스터/보스 처치시 아이템 드롭 처리 시스템
using System.Collections.Generic;
using UnityEngine;

public class ItemDropSystem : MonoBehaviour
{
    public static ItemDropSystem Instance { get; private set; }

    [Header("드롭 설정")]
    [SerializeField] private GameObject itemPickupPrefab; // 아이템 드롭 프리팹
    [SerializeField] private float dropRadius = 1.0f;     // 드롭 반경
    [SerializeField] private LayerMask groundLayer;       // 바닥 레이어

    [Header("드롭률 보정")]
    [SerializeField] private float commonDropRateMultiplier = 1.0f;
    [SerializeField] private float uncommonDropRateMultiplier = 1.0f;
    [SerializeField] private float rareDropRateMultiplier = 1.0f;
    [SerializeField] private float epicDropRateMultiplier = 1.0f;
    [SerializeField] private float legendaryDropRateMultiplier = 1.0f;

    [Header("추가 드롭 설정")]
    [SerializeField] private bool enableBonusDrops = true;    // 추가 드롭 활성화
    [SerializeField] private float bonusDropChance = 0.05f;   // 추가 드롭 확률 (5%)
    [SerializeField] private int maxBonusDrops = 3;           // 최대 추가 드롭 수

    // 아이템 드롭 이벤트
    public System.Action<GameObject, Item, int> OnItemDropped;

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
        }
    }

    // 몬스터 처치시 호출되는 메서드
    public void DropItemFromMonster(ICreatureData monsterData, Vector3 dropPosition)
    {
        if (monsterData == null) return;

        int monsterId = monsterData.MonsterID;

        // DropTableManager에서 드롭 테이블 가져오기
        List<ItemDropEntry> dropTable = DropTableManager.Instance.GetMonsterDropTable(monsterId);

        if (dropTable != null && dropTable.Count > 0)
        {
            // 드롭 테이블 처리
            ProcessDropTable(dropTable, dropPosition);
        }
        else
        {
            // 기존 방식으로 fallback
            float dropChance = monsterData.dropChance / 100f;

            if (Random.value <= dropChance)
            {
                Item itemToDrop = ItemDataManager.Instance.GetItem(monsterData.dropItem);
                if (itemToDrop != null)
                {
                    SpawnItemDrop(itemToDrop, 1, dropPosition);
                }
            }
        }

        // 추가 드롭 처리
        HandleBonusDrops(monsterData, dropPosition);
    }

    // 보스 처치시 호출되는 메서드
    public void DropItemFromBoss(BossData bossData, Vector3 dropPosition)
    {
        if (bossData == null) return;

        int bossId = bossData.BossID;

        // DropTableManager에서 드롭 테이블 가져오기
        List<ItemDropEntry> dropTable = DropTableManager.Instance.GetBossDropTable(bossId);

        if (dropTable != null && dropTable.Count > 0)
        {
            // 드롭 테이블 처리
            ProcessDropTable(dropTable, dropPosition, true);
        }
        else
        {
            // 기존 방식으로 fallback
            Item itemToDrop = ItemDataManager.Instance.GetItem(bossData.dropItem);
            if (itemToDrop != null)
            {
                SpawnItemDrop(itemToDrop, 1, dropPosition, true);
            }
        }

        // 보스 추가 아이템 드롭 처리
        DropBossAdditionalItems(bossData, dropPosition);
    }

    // 드롭 테이블 처리 메서드
    private void ProcessDropTable(List<ItemDropEntry> dropTable, Vector3 dropPosition, bool isBoss = false)
    {
        Vector3 basePosition = dropPosition;

        foreach (var entry in dropTable)
        {
            // 기본 드롭 확률 계산
            float baseDropChance = entry.dropChance / 100f;
            float finalDropChance = baseDropChance;

            // 아이템 찾기 보너스 적용
            float itemFindBonus = 0f;
            if (GlobalItemFindManager.Instance != null)
            {
                itemFindBonus = GlobalItemFindManager.Instance.GetGlobalItemFindBonus();
                finalDropChance *= (1f + itemFindBonus);
            }

            // 아이템 정보 가져오기
            Item itemToDrop = ItemDataManager.Instance.GetItem(entry.itemId);
            if (itemToDrop == null) continue;

            // 희귀도 보정 적용
            float rateMultiplier = GetRarityDropRateMultiplier(itemToDrop.rarity);
            float finalAdjustedChance = finalDropChance * rateMultiplier;

            // 디버그 로그로 드롭 확률 표시
            Debug.Log($"[아이템 드롭] {itemToDrop.itemName} - " +
                      $"기본 확률: {baseDropChance * 100:F2}%, " +
                      $"찾기 보너스: +{itemFindBonus * 100:F2}%, " +
                      $"희귀도 보정: x{rateMultiplier:F2}, " +
                      $"최종 확률: {finalAdjustedChance * 100:F2}%");

            // 실제 확률 계산
            float randomValue = Random.value;
            bool willDrop = randomValue <= finalAdjustedChance;

            Debug.Log($"[아이템 드롭 판정] {itemToDrop.itemName} - " +
                      $"랜덤 값: {randomValue:F4}, " +
                      $"판정 결과: {(willDrop ? "드롭됨" : "드롭 실패")}");

            // 확률에 따라 아이템 드롭
            if (willDrop)
            {
                // 랜덤 수량 결정
                int quantity = Random.Range(entry.minQuantity, entry.maxQuantity + 1);

                // 약간의 위치 오프셋 적용
                Vector3 offset = new Vector3(
                    Random.Range(-dropRadius, dropRadius),
                    0.1f,
                    Random.Range(-dropRadius, dropRadius)
                );

                Debug.Log($"[아이템 드롭 성공] {itemToDrop.itemName} x{quantity}개 드롭!");

                // 아이템 드롭 (보스 아이템 여부 적용)
                SpawnItemDrop(itemToDrop, quantity, basePosition + offset, isBoss && itemToDrop.rarity >= Item.ItemRarity.Rare);
            }
        }
    }

    // 보스의 추가 아이템 드롭 처리
    private void DropBossAdditionalItems(BossData bossData, Vector3 dropPosition)
    {
        // 보스는 파편 아이템 드롭 확률 증가
        float fragmentDropChance = 0.5f; // 50% 확률

        if (Random.value <= fragmentDropChance)
        {
            // 보스 ID와 관련된 파편 찾기
            FragmentItem fragment = null;

            // 1. 먼저 보스 ID 관련 파편 찾기 시도
            FragmentItem bossFragment = ItemDataManager.Instance.GetFragmentByBossID(bossData.BossID.ToString());
            if (bossFragment != null)
            {
                fragment = bossFragment;
            }
            else
            {
                // 2. 랜덤 파편 선택
                List<Item> fragments = ItemDataManager.Instance.GetItemsByType(Item.ItemType.Fragment);
                if (fragments != null && fragments.Count > 0)
                {
                    fragment = fragments[Random.Range(0, fragments.Count)] as FragmentItem;
                }
            }

            // 파편 드롭
            if (fragment != null)
            {
                SpawnItemDrop(fragment, 1, dropPosition + new Vector3(0, 0.5f, 0), true);
            }
        }

        // 드롭 테이블에 없는 추가 재료 아이템 드롭 (옵션)
        if (enableBonusDrops)
        {
            int extraMaterialCount = Random.Range(1, 4); // 1~3개 추가 재료

            for (int i = 0; i < extraMaterialCount; i++)
            {
                List<Item> materials = ItemDataManager.Instance.GetItemsByType(Item.ItemType.Material);
                if (materials != null && materials.Count > 0)
                {
                    Item material = materials[Random.Range(0, materials.Count)];
                    int quantity = Random.Range(1, 4); // 1~3개 드롭

                    Vector3 randomOffset = new Vector3(
                        Random.Range(-dropRadius, dropRadius),
                        0.1f,
                        Random.Range(-dropRadius, dropRadius)
                    );

                    SpawnItemDrop(material, quantity, dropPosition + randomOffset);
                }
            }
        }
    }

    // 추가 드롭 처리
    private void HandleBonusDrops(ICreatureData monsterData, Vector3 dropPosition)
    {
        if (!enableBonusDrops) return;

        // 기본 아이템 외에 추가 아이템 드롭 처리
        int bonusCount = 0;

        // 최대 추가 드롭 수까지 확률 체크
        while (bonusCount < maxBonusDrops && Random.value <= bonusDropChance)
        {
            // 마나 포션 또는 체력 포션 등의 기본 아이템을 여기서 드롭할 수 있음
            List<Item> potions = ItemDataManager.Instance.GetItemsByType(Item.ItemType.Potion);
            if (potions != null && potions.Count > 0)
            {
                Item potion = potions[Random.Range(0, potions.Count)];

                Vector3 randomOffset = new Vector3(
                    Random.Range(-dropRadius, dropRadius),
                    0.1f,
                    Random.Range(-dropRadius, dropRadius)
                );

                SpawnItemDrop(potion, 1, dropPosition + randomOffset);
            }

            bonusCount++;
        }
    }

    // 희귀도에 따른 드롭률 보정 계수 반환
    private float GetRarityDropRateMultiplier(Item.ItemRarity rarity)
    {
        switch (rarity)
        {
            case Item.ItemRarity.Common:
                return commonDropRateMultiplier;
            case Item.ItemRarity.Uncommon:
                return uncommonDropRateMultiplier;
            case Item.ItemRarity.Rare:
                return rareDropRateMultiplier;
            case Item.ItemRarity.Epic:
                return epicDropRateMultiplier;
            case Item.ItemRarity.Legendary:
                return legendaryDropRateMultiplier;
            default:
                return 1.0f;
        }
    }

    // 아이템 드롭 월드 오브젝트 생성 (간소화된 버전)
    private void SpawnItemDrop(Item item, int quantity, Vector3 position, bool isBossItem = false)
    {
        if (itemPickupPrefab == null)
        {
            Debug.LogError("아이템 드롭 프리팹이 설정되지 않았습니다.");
            return;
        }

        // 바닥 위치 찾기
        Vector3 finalPosition = FindGroundPosition(position);

        // 아이템 오브젝트 생성
        GameObject itemObj = Instantiate(itemPickupPrefab, finalPosition, Quaternion.identity);

        // 아이템 드롭 컴포넌트 설정
        ItemPickupObject itemPickup = itemObj.GetComponent<ItemPickupObject>();
        if (itemPickup == null)
        {
            Debug.LogError("아이템 드롭 프리팹에 ItemPickupObject 컴포넌트가 없습니다.");
            Destroy(itemObj);
            return;
        }

        // 아이템 정보 설정 - 이제 ItemPickupObject가 팝업 애니메이션 처리
        itemPickup.Initialize(item, quantity, isBossItem);

        // 이벤트 발생
        OnItemDropped?.Invoke(itemObj, item, quantity);
    }

    // 바닥 위치 찾기
    private Vector3 FindGroundPosition(Vector3 position)
    {
        // 바닥을 레이캐스트로 찾기
        if (Physics.Raycast(position + Vector3.up * 5f, Vector3.down, out RaycastHit hit, 10f, groundLayer))
        {
            // 바닥 위에 약간 띄워서 위치시킴
            return hit.point + Vector3.up * 0.1f;
        }

        // 레이캐스트 실패시 원래 위치 그대로 사용
        return position;
    }
}