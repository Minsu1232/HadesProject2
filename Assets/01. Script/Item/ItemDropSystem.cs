// ItemDropSystem.cs - ����/���� óġ�� ������ ��� ó�� �ý��� (CSV ��� ���̺� ����)
using System.Collections.Generic;
using UnityEngine;

public class ItemDropSystem : MonoBehaviour
{
    public static ItemDropSystem Instance { get; private set; }

    [Header("��� ����")]
    [SerializeField] private GameObject itemPickupPrefab; // ������ ��� ������
    [SerializeField] private float dropForce = 3.0f;      // ��ӽ� ƨ��� ��
    [SerializeField] private float dropUpForce = 2.0f;    // ���� ƨ��� ��
    [SerializeField] private float dropRadius = 1.0f;     // ��� �ݰ�
    [SerializeField] private LayerMask groundLayer;       // �ٴ� ���̾�

    [Header("��ӷ� ����")]
    [SerializeField] private float commonDropRateMultiplier = 1.0f;
    [SerializeField] private float uncommonDropRateMultiplier = 1.0f;
    [SerializeField] private float rareDropRateMultiplier = 1.0f;
    [SerializeField] private float epicDropRateMultiplier = 1.0f;
    [SerializeField] private float legendaryDropRateMultiplier = 1.0f;

    [Header("�߰� ��� ����")]
    [SerializeField] private bool enableBonusDrops = true;    // �߰� ��� Ȱ��ȭ
    [SerializeField] private float bonusDropChance = 0.05f;   // �߰� ��� Ȯ�� (5%)
    [SerializeField] private int maxBonusDrops = 3;           // �ִ� �߰� ��� ��

    // ������ ��� �̺�Ʈ
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

    // ���� óġ�� ȣ��Ǵ� �޼���
    public void DropItemFromMonster(ICreatureData monsterData, Vector3 dropPosition)
    {
        if (monsterData == null) return;

        int monsterId = monsterData.MonsterID; // ���� ID �ʵ� - BossID�� ���� ����

        // DropTableManager���� ��� ���̺� ��������
        List<ItemDropEntry> dropTable = DropTableManager.Instance.GetMonsterDropTable(monsterId);

        if (dropTable != null && dropTable.Count > 0)
        {
            // ��� ���̺� ó��
            ProcessDropTable(dropTable, dropPosition);
        }
        else
        {
            // ���� ������� fallback (���� �ڵ� ����)
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

        // �߰� ��� ó��
        HandleBonusDrops(monsterData, dropPosition);
    }

    // ���� óġ�� ȣ��Ǵ� �޼���
    public void DropItemFromBoss(BossData bossData, Vector3 dropPosition)
    {
        if (bossData == null) return;

        int bossId = bossData.BossID;

        // DropTableManager���� ��� ���̺� ��������
        List<ItemDropEntry> dropTable = DropTableManager.Instance.GetBossDropTable(bossId);

        if (dropTable != null && dropTable.Count > 0)
        {
            // ��� ���̺� ó��
            ProcessDropTable(dropTable, dropPosition, true);
        }
        else
        {
            // ���� ������� fallback
            Item itemToDrop = ItemDataManager.Instance.GetItem(bossData.dropItem);
            if (itemToDrop != null)
            {
                SpawnItemDrop(itemToDrop, 1, dropPosition, true);
            }
        }

        // ���� �߰� ������ ��� ó��
        DropBossAdditionalItems(bossData, dropPosition);
    }

    // ��� ���̺� ó�� �޼���
    private void ProcessDropTable(List<ItemDropEntry> dropTable, Vector3 dropPosition, bool isBoss = false)
    {
        Vector3 basePosition = dropPosition;

        foreach (var entry in dropTable)
        {
            // �� �����ۺ� ��� Ȯ�� ���
            float dropChance = entry.dropChance / 100f;

            // Ȯ�� üũ
            if (Random.value <= dropChance)
            {
                // ������ ȹ��
                Item itemToDrop = ItemDataManager.Instance.GetItem(entry.itemId);
                if (itemToDrop != null)
                {
                    // ��͵� ���� ����
                    float rateMultiplier = GetRarityDropRateMultiplier(itemToDrop.rarity);

                    // ������ Ȯ���� �ٽ� üũ
                    if (Random.value <= dropChance * rateMultiplier)
                    {
                        // ���� ���� ����
                        int quantity = Random.Range(entry.minQuantity, entry.maxQuantity + 1);

                        // �ణ�� ��ġ ������ ����
                        Vector3 offset = new Vector3(
                            Random.Range(-dropRadius, dropRadius),
                            0.1f,
                            Random.Range(-dropRadius, dropRadius)
                        );
                        Debug.Log(itemToDrop.itemID + "���");
                        // ������ ��� (���� ������ ���� ����)
                        SpawnItemDrop(itemToDrop, quantity, basePosition + offset, isBoss && itemToDrop.rarity >= Item.ItemRarity.Rare);
                    }
                }
            }
        }
    }

    // ������ �߰� ������ ��� ó��
    private void DropBossAdditionalItems(BossData bossData, Vector3 dropPosition)
    {
        // ������ ���� ������ ��� Ȯ�� ����
        float fragmentDropChance = 0.5f; // 50% Ȯ��

        if (Random.value <= fragmentDropChance)
        {
            // ���� ID�� ���õ� ���� ã��
            FragmentItem fragment = null;

            // 1. ���� ���� ID ���� ���� ã�� �õ�
            FragmentItem bossFragment = ItemDataManager.Instance.GetFragmentByBossID(bossData.BossID.ToString());
            if (bossFragment != null)
            {
                fragment = bossFragment;
            }
            else
            {
                // 2. ���� ���� ����
                List<Item> fragments = ItemDataManager.Instance.GetItemsByType(Item.ItemType.Fragment);
                if (fragments != null && fragments.Count > 0)
                {
                    fragment = fragments[Random.Range(0, fragments.Count)] as FragmentItem;
                }
            }

            // ���� ���
            if (fragment != null)
            {
                SpawnItemDrop(fragment, 1, dropPosition + new Vector3(0, 0.5f, 0), true);
            }
        }

        // ��� ���̺� ���� �߰� ��� ������ ��� (�ɼ�)
        if (enableBonusDrops)
        {
            int extraMaterialCount = Random.Range(1, 4); // 1~3�� �߰� ���

            for (int i = 0; i < extraMaterialCount; i++)
            {
                List<Item> materials = ItemDataManager.Instance.GetItemsByType(Item.ItemType.Material);
                if (materials != null && materials.Count > 0)
                {
                    Item material = materials[Random.Range(0, materials.Count)];
                    int quantity = Random.Range(1, 4); // 1~3�� ���

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

    // �߰� ��� ó��
    private void HandleBonusDrops(ICreatureData monsterData, Vector3 dropPosition)
    {
        if (!enableBonusDrops) return;

        // �⺻ ������ �ܿ� �߰� ������ ��� ó��
        int bonusCount = 0;

        // �ִ� �߰� ��� ������ Ȯ�� üũ
        while (bonusCount < maxBonusDrops && Random.value <= bonusDropChance)
        {
            // ���� ���� �Ǵ� ü�� ���� ���� �⺻ �������� ���⼭ ����� �� ����
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

    // ��͵��� ���� ��ӷ� ���� ��� ��ȯ
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

    // ������ ��� ���� ������Ʈ ����
    private void SpawnItemDrop(Item item, int quantity, Vector3 position, bool isBossItem = false)
    {
        if (itemPickupPrefab == null)
        {
            Debug.LogError("������ ��� �������� �������� �ʾҽ��ϴ�.");
            return;
        }

        // �ٴ� ��ġ ã��
        Vector3 finalPosition = FindGroundPosition(position);

        // ������ ������Ʈ ����
        GameObject itemObj = Instantiate(itemPickupPrefab, finalPosition, Quaternion.identity);

        // ������ ��� ������Ʈ ����
        ItemPickupObject itemPickup = itemObj.GetComponent<ItemPickupObject>();
        if (itemPickup == null)
        {
            Debug.LogError("������ ��� �����տ� ItemPickupObject ������Ʈ�� �����ϴ�.");
            Destroy(itemObj);
            return;
        }

        // ������ ���� ����
        itemPickup.Initialize(item, quantity, isBossItem);

        // ���� ������Ʈ ����
        Rigidbody rb = itemObj.GetComponent<Rigidbody>();
        if (rb != null)
        {
            // ������ �������� ƨ��� ȿ��
            Vector3 randomDir = new Vector3(
                Random.Range(-1f, 1f),
                1,
                Random.Range(-1f, 1f)
            ).normalized;

            rb.AddForce(randomDir * dropForce + Vector3.up * dropUpForce, ForceMode.Impulse);
        }

        // �̺�Ʈ �߻�
        OnItemDropped?.Invoke(itemObj, item, quantity);
    }

    // �ٴ� ��ġ ã��
    private Vector3 FindGroundPosition(Vector3 position)
    {
        // �ٴ��� ����ĳ��Ʈ�� ã��
        if (Physics.Raycast(position + Vector3.up * 5f, Vector3.down, out RaycastHit hit, 10f, groundLayer))
        {
            // �ٴ� ���� �ణ ����� ��ġ��Ŵ
            return hit.point + Vector3.up * 0.1f;
        }

        // ����ĳ��Ʈ ���н� ���� ��ġ �״�� ���
        return position;
    }
}