using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

public class ItemDataManager : MonoBehaviour
{
    public static ItemDataManager Instance { get; private set; }
    // ItemDataManager.cs
    public event Action OnAnyItemIconLoaded;
    // ������ �����ͺ��̽�
    private Dictionary<int, Item> itemDatabase = new Dictionary<int, Item>();

    // CSV ������ ����� ��ųʸ�
    private Dictionary<int, Dictionary<string, string>> itemBaseData = new Dictionary<int, Dictionary<string, string>>();
    private Dictionary<int, Dictionary<string, string>> fragmentData = new Dictionary<int, Dictionary<string, string>>();
    private Dictionary<int, Dictionary<string, string>> potionData = new Dictionary<int, Dictionary<string, string>>();
    private Dictionary<int, Dictionary<string, string>> materialData = new Dictionary<int, Dictionary<string, string>>();

    // CSV ���� ���
    private string itemBasePath;
    private string fragmentItemsPath;
    private string potionItemsPath;
    private string materialItemsPath;

    private void Awake()
    {
        // �̱��� ���� ����
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

        // ���� ��� ����
        itemBasePath = Path.Combine(Application.persistentDataPath, "ItemBase.csv");
        fragmentItemsPath = Path.Combine(Application.persistentDataPath, "FragmentItems.csv");
        potionItemsPath = Path.Combine(Application.persistentDataPath, "PotionItems.csv");
        materialItemsPath = Path.Combine(Application.persistentDataPath, "MaterialItems.csv");

        // StreamingAssets���� CSV ���� ����
        CopyCSVFromStreamingAssets();
    }

    private void CopyCSVFromStreamingAssets()
    {
        string[] csvFiles = { "ItemBase.csv", "FragmentItems.csv", "PotionItems.csv", "MaterialItems.csv" };

        foreach (string fileName in csvFiles)
        {
            string streamingPath = Path.Combine(Application.streamingAssetsPath, fileName);
            string persistentPath = Path.Combine(Application.persistentDataPath, fileName);

            if (File.Exists(streamingPath))
            {
                File.Copy(streamingPath, persistentPath, true);
                Debug.Log($"������ CSV ���� ���� �Ϸ�: {fileName}");
            }
            else
            {
                Debug.LogWarning($"StreamingAssets���� {fileName} ������ ã�� �� �����ϴ�.");
            }
        }
    }

    public async Task InitializeItems()
    {
        await LoadAllItemData();
        Debug.Log($"������ �����ͺ��̽� �ʱ�ȭ �Ϸ�: {itemDatabase.Count}�� ������ �ε��");
    }

    private async Task LoadAllItemData()
    {
        // �⺻ ������ ������ �ε�
        await LoadItemBaseData();

        // ������ Ư�� �Ӽ� ������ �ε�
        await LoadFragmentItemData();
        await LoadPotionItemData();
        await LoadMaterialItemData();

        // ������ �ν��Ͻ� ����
        await CreateItemInstances();
    }

    private async Task LoadItemBaseData()
    {
        if (!File.Exists(itemBasePath))
        {
            Debug.LogError($"������ �⺻ ������ CSV ������ ã�� �� �����ϴ�: {itemBasePath}");
            return;
        }

        string[] lines = File.ReadAllLines(itemBasePath);
        string[] headers = lines[0].Split(',');

        for (int i = 1; i < lines.Length; i++)
        {
            string[] values = lines[i].Split(',');
            if (values.Length < 2) continue; // �� �� ����

            if (int.TryParse(values[0], out int itemId))
            {
                var baseDict = new Dictionary<string, string>();
                for (int j = 0; j < headers.Length && j < values.Length; j++)
                {
                    baseDict[headers[j]] = values[j];
                }

                itemBaseData[itemId] = baseDict;
            }
            else
            {
                Debug.LogWarning($"������ ID �Ľ� ���� (�� {i + 1}): {values[0]}");
            }
        }

        Debug.Log($"�⺻ ������ ������ �ε� �Ϸ�: {itemBaseData.Count}�� �׸�");
    }

    private async Task LoadFragmentItemData()
    {
        if (!File.Exists(fragmentItemsPath))
        {
            Debug.LogWarning($"���� ������ ������ CSV ������ ã�� �� �����ϴ�: {fragmentItemsPath}");
            return;
        }

        string[] lines = File.ReadAllLines(fragmentItemsPath);
        string[] headers = lines[0].Split(',');

        for (int i = 1; i < lines.Length; i++)
        {
            string[] values = lines[i].Split(',');
            if (values.Length < 2) continue; // �� �� ����

            if (int.TryParse(values[0], out int itemId))
            {
                var fragDict = new Dictionary<string, string>();
                for (int j = 0; j < headers.Length && j < values.Length; j++)
                {
                    fragDict[headers[j]] = values[j];
                }

                fragmentData[itemId] = fragDict;
            }
            else
            {
                Debug.LogWarning($"���� ������ ID �Ľ� ���� (�� {i + 1}): {values[0]}");
            }
        }

        Debug.Log($"���� ������ ������ �ε� �Ϸ�: {fragmentData.Count}�� �׸�");
    }

    private async Task LoadPotionItemData()
    {
        if (!File.Exists(potionItemsPath))
        {
            Debug.LogWarning($"���� ������ ������ CSV ������ ã�� �� �����ϴ�: {potionItemsPath}");
            return;
        }

        string[] lines = File.ReadAllLines(potionItemsPath);
        string[] headers = lines[0].Split(',');

        for (int i = 1; i < lines.Length; i++)
        {
            string[] values = lines[i].Split(',');
            if (values.Length < 2) continue; // �� �� ����

            if (int.TryParse(values[0], out int itemId))
            {
                var potionDict = new Dictionary<string, string>();
                for (int j = 0; j < headers.Length && j < values.Length; j++)
                {
                    potionDict[headers[j]] = values[j];
                }

                potionData[itemId] = potionDict;
            }
            else
            {
                Debug.LogWarning($"���� ������ ID �Ľ� ���� (�� {i + 1}): {values[0]}");
            }
        }

        Debug.Log($"���� ������ ������ �ε� �Ϸ�: {potionData.Count}�� �׸�");
    }

    private async Task LoadMaterialItemData()
    {
        if (!File.Exists(materialItemsPath))
        {
            Debug.LogWarning($"��� ������ ������ CSV ������ ã�� �� �����ϴ�: {materialItemsPath}");
            return;
        }

        string[] lines = File.ReadAllLines(materialItemsPath);
        string[] headers = lines[0].Split(',');

        for (int i = 1; i < lines.Length; i++)
        {
            string[] values = lines[i].Split(',');
            if (values.Length < 2) continue; // �� �� ����

            if (int.TryParse(values[0], out int itemId))
            {
                var materialDict = new Dictionary<string, string>();
                for (int j = 0; j < headers.Length && j < values.Length; j++)
                {
                    materialDict[headers[j]] = values[j];
                }

                materialData[itemId] = materialDict;
            }
            else
            {
                Debug.LogWarning($"��� ������ ID �Ľ� ���� (�� {i + 1}): {values[0]}");
            }
        }

        Debug.Log($"��� ������ ������ �ε� �Ϸ�: {materialData.Count}�� �׸�");
    }

    private async Task CreateItemInstances()
    {
        foreach (var kvp in itemBaseData)
        {
            int itemId = kvp.Key;
            var baseData = kvp.Value;

            string itemType = baseData["ItemType"];
            Item item = null;

            // ������ ������ ���� ������ �ν��Ͻ� ����
            switch (itemType)
            {
                case "Fragment":
                    item = CreateFragmentItem(itemId, baseData);
                    break;

                case "Potion":
                    item = CreatePotionItem(itemId, baseData);
                    break;

                case "Material":
                    item = CreateMaterialItem(itemId, baseData);
                    break;

                default:
                    item = CreateBasicItem(itemId, baseData);
                    break;
            }

            if (item != null)
            {
                itemDatabase[itemId] = item;
            }
        }
    }

    // �⺻ ������ ����
    private Item CreateBasicItem(int itemId, Dictionary<string, string> baseData)
    {
        Item item = new Item();

        // �⺻ �Ӽ� ����
        item.itemID = itemId.ToString();
        item.itemName = baseData["ItemName"];
        item.description = baseData["Description"];

        // ������ ���� �Ӽ�
        item.isStackable = bool.Parse(baseData["IsStackable"]);
        if (int.TryParse(baseData["MaxStackSize"], out int maxStack))
        {
            item.maxStackSize = maxStack;
        }

        // ������ ���� ����
        if (Enum.TryParse<Item.ItemType>(baseData["ItemType"], out var type))
        {
            item.itemType = type;
        }

        // ������ ��͵� ����
        if (Enum.TryParse<Item.ItemRarity>(baseData["Rarity"], out var rarity))
        {
            item.rarity = rarity;
        }

        // ������ �ε�
        LoadItemIcon(item, baseData["Icon"]);

        return item;
    }

    // ���� ������ ����
    private FragmentItem CreateFragmentItem(int itemId, Dictionary<string, string> baseData)
    {
        FragmentItem item = new FragmentItem();

        // �⺻ �Ӽ� ����
        item.itemID = itemId.ToString();
        item.itemName = baseData["ItemName"];
        item.description = baseData["Description"];
        item.isStackable = bool.Parse(baseData["IsStackable"]);
        if (int.TryParse(baseData["MaxStackSize"], out int maxStack))
        {
            item.maxStackSize = maxStack;
        }

        // ������ ���� �� ��͵� ����
        item.itemType = Item.ItemType.Fragment;
        if (Enum.TryParse<Item.ItemRarity>(baseData["Rarity"], out var rarity))
        {
            item.rarity = rarity;
        }

        // ������ �ε�
        LoadItemIcon(item, baseData["Icon"]);

        // Ư�� �Ӽ� ���� (���� �����Ͱ� �ִ� ���)
        if (fragmentData.TryGetValue(itemId, out var fragData))
        {
            // ���� ���ʽ� ����
            if (float.TryParse(fragData["AttackBonus"], out float attackBonus))
            {
                item.attackBonus = attackBonus;
            }

            if (float.TryParse(fragData["DefenseBonus"], out float defenseBonus))
            {
                item.defenseBonus = defenseBonus;
            }

            if (float.TryParse(fragData["HealthBonus"], out float healthBonus))
            {
                item.healthBonus = healthBonus;
            }

            if (float.TryParse(fragData["SpeedBonus"], out float speedBonus))
            {
                item.speedBonus = speedBonus;
            }

            // ���� ���� ����
            item.isResonated = bool.Parse(fragData["IsResonated"]);
            item.associatedBossID = fragData["AssociatedBossID"];

            // ���� Ÿ�� ����
            if (Enum.TryParse<FragmentItem.FragmentType>(fragData["FragmentType"], out var fragType))
            {
                item.fragType = fragType;
            }
        }

        return item;
    }

    // ���� ������ ����
    private PotionItem CreatePotionItem(int itemId, Dictionary<string, string> baseData)
    {
        PotionItem item = new PotionItem();

        // �⺻ �Ӽ� ����
        item.itemID = itemId.ToString();
        item.itemName = baseData["ItemName"];
        item.description = baseData["Description"];
        item.isStackable = bool.Parse(baseData["IsStackable"]);
        if (int.TryParse(baseData["MaxStackSize"], out int maxStack))
        {
            item.maxStackSize = maxStack;
        }

        // ������ ���� �� ��͵� ����
        item.itemType = Item.ItemType.Potion;
        if (Enum.TryParse<Item.ItemRarity>(baseData["Rarity"], out var rarity))
        {
            item.rarity = rarity;
        }

        // ������ �ε�
        LoadItemIcon(item, baseData["Icon"]);

        // Ư�� �Ӽ� ���� (���� �����Ͱ� �ִ� ���)
        if (potionData.TryGetValue(itemId, out var potData))
        {
            // ���� ȿ�� �� ����
            if (float.TryParse(potData["PotionValue"], out float value))
            {
                item.potionValue = value;
            }

            // ���� ���� �ð� ����
            if (float.TryParse(potData["Duration"], out float duration))
            {
                item.duration = duration;
            }

            // ���� Ÿ�� ����
            if (Enum.TryParse<PotionItem.PotionType>(potData["PotionType"], out var potType))
            {
                item.potionType = potType;
            }
        }

        return item;
    }

    // ��� ������ ����
    private MaterialItem CreateMaterialItem(int itemId, Dictionary<string, string> baseData)
    {
        MaterialItem item = new MaterialItem();

        // �⺻ �Ӽ� ����
        item.itemID = itemId.ToString();
        item.itemName = baseData["ItemName"];
        item.description = baseData["Description"];
        item.isStackable = bool.Parse(baseData["IsStackable"]);
        if (int.TryParse(baseData["MaxStackSize"], out int maxStack))
        {
            item.maxStackSize = maxStack;
        }

        // ������ ���� �� ��͵� ����
        item.itemType = Item.ItemType.Material;
        if (Enum.TryParse<Item.ItemRarity>(baseData["Rarity"], out var rarity))
        {
            item.rarity = rarity;
        }

        // ������ �ε�
        LoadItemIcon(item, baseData["Icon"]);

        // Ư�� �Ӽ� ���� (��� �����Ͱ� �ִ� ���)
        if (materialData.TryGetValue(itemId, out var matData))
        {
            // ��� ī�װ� ����
            item.materialCategory = matData["MaterialCategory"];
        }

        return item;
    }

    // ������ ������ �ε� (Addressables ���)
    private void LoadItemIcon(Item item, string iconPath)
    {
        if (!string.IsNullOrEmpty(iconPath))
        {
            Addressables.LoadAssetAsync<Sprite>(iconPath).Completed += (op) =>
            {
                if (op.Status == AsyncOperationStatus.Succeeded)
                {
                    item.icon = op.Result;
                    // ������ �ε� �Ϸ� �̺�Ʈ �߻�
                    OnAnyItemIconLoaded?.Invoke();
                    Debug.Log($"������ ������ �ε� ����: {iconPath}");
                }
                else
                {
                    Debug.LogWarning($"������ ������ �ε� ����: {iconPath}");
                }
            };
        }
    }

    // ������ ID�� ������ ��������
    public Item GetItem(int itemId)
    {
        if (itemDatabase.TryGetValue(itemId, out Item item))
        {
            return item;
        }

        Debug.LogWarning($"�������� ã�� �� �����ϴ�: ID {itemId}");
        return null;
    }

    // ���� ID�� ���� ���� ã��
    public FragmentItem GetFragmentByBossID(string bossId)
    {
        foreach (var item in itemDatabase.Values)
        {
            if (item is FragmentItem fragment && fragment.associatedBossID == bossId)
            {
                return fragment;
            }
        }

        Debug.LogWarning($"���� ID {bossId}�� �ش��ϴ� ������ ã�� �� �����ϴ�.");
        return null;
    }

    // ������ �������� ������ ��� ��������
    public List<Item> GetItemsByType(Item.ItemType type)
    {
        List<Item> result = new List<Item>();

        foreach (var item in itemDatabase.Values)
        {
            if (item.itemType == type)
            {
                result.Add(item);
            }
        }

        return result;
    }

    // ���� ���� �� ���ҽ� ����
    private void OnDestroy()
    {
        // �ʿ��� ��� ���ҽ� ���� ���� �߰�
        itemDatabase.Clear();
        itemBaseData.Clear();
        fragmentData.Clear();
        potionData.Clear();
        materialData.Clear();
    }
}