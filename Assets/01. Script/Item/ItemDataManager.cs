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
    // 아이템 데이터베이스
    private Dictionary<int, Item> itemDatabase = new Dictionary<int, Item>();

    // CSV 데이터 저장용 딕셔너리
    private Dictionary<int, Dictionary<string, string>> itemBaseData = new Dictionary<int, Dictionary<string, string>>();
    private Dictionary<int, Dictionary<string, string>> fragmentData = new Dictionary<int, Dictionary<string, string>>();
    private Dictionary<int, Dictionary<string, string>> potionData = new Dictionary<int, Dictionary<string, string>>();
    private Dictionary<int, Dictionary<string, string>> materialData = new Dictionary<int, Dictionary<string, string>>();

    // CSV 파일 경로
    private string itemBasePath;
    private string fragmentItemsPath;
    private string potionItemsPath;
    private string materialItemsPath;

    private void Awake()
    {
        // 싱글톤 패턴 구현
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

        // 파일 경로 설정
        itemBasePath = Path.Combine(Application.persistentDataPath, "ItemBase.csv");
        fragmentItemsPath = Path.Combine(Application.persistentDataPath, "FragmentItems.csv");
        potionItemsPath = Path.Combine(Application.persistentDataPath, "PotionItems.csv");
        materialItemsPath = Path.Combine(Application.persistentDataPath, "MaterialItems.csv");

        // StreamingAssets에서 CSV 파일 복사
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
                Debug.Log($"아이템 CSV 파일 복사 완료: {fileName}");
            }
            else
            {
                Debug.LogWarning($"StreamingAssets에서 {fileName} 파일을 찾을 수 없습니다.");
            }
        }
    }

    public async Task InitializeItems()
    {
        await LoadAllItemData();
        Debug.Log($"아이템 데이터베이스 초기화 완료: {itemDatabase.Count}개 아이템 로드됨");
    }

    private async Task LoadAllItemData()
    {
        // 기본 아이템 데이터 로드
        await LoadItemBaseData();

        // 유형별 특수 속성 데이터 로드
        await LoadFragmentItemData();
        await LoadPotionItemData();
        await LoadMaterialItemData();

        // 아이템 인스턴스 생성
        await CreateItemInstances();
    }

    private async Task LoadItemBaseData()
    {
        if (!File.Exists(itemBasePath))
        {
            Debug.LogError($"아이템 기본 데이터 CSV 파일을 찾을 수 없습니다: {itemBasePath}");
            return;
        }

        string[] lines = File.ReadAllLines(itemBasePath);
        string[] headers = lines[0].Split(',');

        for (int i = 1; i < lines.Length; i++)
        {
            string[] values = lines[i].Split(',');
            if (values.Length < 2) continue; // 빈 줄 무시

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
                Debug.LogWarning($"아이템 ID 파싱 실패 (행 {i + 1}): {values[0]}");
            }
        }

        Debug.Log($"기본 아이템 데이터 로드 완료: {itemBaseData.Count}개 항목");
    }

    private async Task LoadFragmentItemData()
    {
        if (!File.Exists(fragmentItemsPath))
        {
            Debug.LogWarning($"파편 아이템 데이터 CSV 파일을 찾을 수 없습니다: {fragmentItemsPath}");
            return;
        }

        string[] lines = File.ReadAllLines(fragmentItemsPath);
        string[] headers = lines[0].Split(',');

        for (int i = 1; i < lines.Length; i++)
        {
            string[] values = lines[i].Split(',');
            if (values.Length < 2) continue; // 빈 줄 무시

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
                Debug.LogWarning($"파편 아이템 ID 파싱 실패 (행 {i + 1}): {values[0]}");
            }
        }

        Debug.Log($"파편 아이템 데이터 로드 완료: {fragmentData.Count}개 항목");
    }

    private async Task LoadPotionItemData()
    {
        if (!File.Exists(potionItemsPath))
        {
            Debug.LogWarning($"포션 아이템 데이터 CSV 파일을 찾을 수 없습니다: {potionItemsPath}");
            return;
        }

        string[] lines = File.ReadAllLines(potionItemsPath);
        string[] headers = lines[0].Split(',');

        for (int i = 1; i < lines.Length; i++)
        {
            string[] values = lines[i].Split(',');
            if (values.Length < 2) continue; // 빈 줄 무시

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
                Debug.LogWarning($"포션 아이템 ID 파싱 실패 (행 {i + 1}): {values[0]}");
            }
        }

        Debug.Log($"포션 아이템 데이터 로드 완료: {potionData.Count}개 항목");
    }

    private async Task LoadMaterialItemData()
    {
        if (!File.Exists(materialItemsPath))
        {
            Debug.LogWarning($"재료 아이템 데이터 CSV 파일을 찾을 수 없습니다: {materialItemsPath}");
            return;
        }

        string[] lines = File.ReadAllLines(materialItemsPath);
        string[] headers = lines[0].Split(',');

        for (int i = 1; i < lines.Length; i++)
        {
            string[] values = lines[i].Split(',');
            if (values.Length < 2) continue; // 빈 줄 무시

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
                Debug.LogWarning($"재료 아이템 ID 파싱 실패 (행 {i + 1}): {values[0]}");
            }
        }

        Debug.Log($"재료 아이템 데이터 로드 완료: {materialData.Count}개 항목");
    }

    private async Task CreateItemInstances()
    {
        foreach (var kvp in itemBaseData)
        {
            int itemId = kvp.Key;
            var baseData = kvp.Value;

            string itemType = baseData["ItemType"];
            Item item = null;

            // 아이템 유형에 따라 적절한 인스턴스 생성
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

    // 기본 아이템 생성
    private Item CreateBasicItem(int itemId, Dictionary<string, string> baseData)
    {
        Item item = new Item();

        // 기본 속성 설정
        item.itemID = itemId.ToString();
        item.itemName = baseData["ItemName"];
        item.description = baseData["Description"];

        // 아이템 스택 속성
        item.isStackable = bool.Parse(baseData["IsStackable"]);
        if (int.TryParse(baseData["MaxStackSize"], out int maxStack))
        {
            item.maxStackSize = maxStack;
        }

        // 아이템 유형 설정
        if (Enum.TryParse<Item.ItemType>(baseData["ItemType"], out var type))
        {
            item.itemType = type;
        }

        // 아이템 희귀도 설정
        if (Enum.TryParse<Item.ItemRarity>(baseData["Rarity"], out var rarity))
        {
            item.rarity = rarity;
        }

        // 아이콘 로드
        LoadItemIcon(item, baseData["Icon"]);

        return item;
    }

    // 파편 아이템 생성
    private FragmentItem CreateFragmentItem(int itemId, Dictionary<string, string> baseData)
    {
        FragmentItem item = new FragmentItem();

        // 기본 속성 설정
        item.itemID = itemId.ToString();
        item.itemName = baseData["ItemName"];
        item.description = baseData["Description"];
        item.isStackable = bool.Parse(baseData["IsStackable"]);
        if (int.TryParse(baseData["MaxStackSize"], out int maxStack))
        {
            item.maxStackSize = maxStack;
        }

        // 아이템 유형 및 희귀도 설정
        item.itemType = Item.ItemType.Fragment;
        if (Enum.TryParse<Item.ItemRarity>(baseData["Rarity"], out var rarity))
        {
            item.rarity = rarity;
        }

        // 아이콘 로드
        LoadItemIcon(item, baseData["Icon"]);

        // 특수 속성 설정 (파편 데이터가 있는 경우)
        if (fragmentData.TryGetValue(itemId, out var fragData))
        {
            // 스탯 보너스 설정
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

            // 공명 상태 설정
            item.isResonated = bool.Parse(fragData["IsResonated"]);
            item.associatedBossID = fragData["AssociatedBossID"];

            // 파편 타입 설정
            if (Enum.TryParse<FragmentItem.FragmentType>(fragData["FragmentType"], out var fragType))
            {
                item.fragType = fragType;
            }
        }

        return item;
    }

    // 포션 아이템 생성
    private PotionItem CreatePotionItem(int itemId, Dictionary<string, string> baseData)
    {
        PotionItem item = new PotionItem();

        // 기본 속성 설정
        item.itemID = itemId.ToString();
        item.itemName = baseData["ItemName"];
        item.description = baseData["Description"];
        item.isStackable = bool.Parse(baseData["IsStackable"]);
        if (int.TryParse(baseData["MaxStackSize"], out int maxStack))
        {
            item.maxStackSize = maxStack;
        }

        // 아이템 유형 및 희귀도 설정
        item.itemType = Item.ItemType.Potion;
        if (Enum.TryParse<Item.ItemRarity>(baseData["Rarity"], out var rarity))
        {
            item.rarity = rarity;
        }

        // 아이콘 로드
        LoadItemIcon(item, baseData["Icon"]);

        // 특수 속성 설정 (포션 데이터가 있는 경우)
        if (potionData.TryGetValue(itemId, out var potData))
        {
            // 포션 효과 값 설정
            if (float.TryParse(potData["PotionValue"], out float value))
            {
                item.potionValue = value;
            }

            // 포션 지속 시간 설정
            if (float.TryParse(potData["Duration"], out float duration))
            {
                item.duration = duration;
            }

            // 포션 타입 설정
            if (Enum.TryParse<PotionItem.PotionType>(potData["PotionType"], out var potType))
            {
                item.potionType = potType;
            }
        }

        return item;
    }

    // 재료 아이템 생성
    private MaterialItem CreateMaterialItem(int itemId, Dictionary<string, string> baseData)
    {
        MaterialItem item = new MaterialItem();

        // 기본 속성 설정
        item.itemID = itemId.ToString();
        item.itemName = baseData["ItemName"];
        item.description = baseData["Description"];
        item.isStackable = bool.Parse(baseData["IsStackable"]);
        if (int.TryParse(baseData["MaxStackSize"], out int maxStack))
        {
            item.maxStackSize = maxStack;
        }

        // 아이템 유형 및 희귀도 설정
        item.itemType = Item.ItemType.Material;
        if (Enum.TryParse<Item.ItemRarity>(baseData["Rarity"], out var rarity))
        {
            item.rarity = rarity;
        }

        // 아이콘 로드
        LoadItemIcon(item, baseData["Icon"]);

        // 특수 속성 설정 (재료 데이터가 있는 경우)
        if (materialData.TryGetValue(itemId, out var matData))
        {
            // 재료 카테고리 설정
            item.materialCategory = matData["MaterialCategory"];
        }

        return item;
    }

    // 아이템 아이콘 로드 (Addressables 사용)
    private void LoadItemIcon(Item item, string iconPath)
    {
        if (!string.IsNullOrEmpty(iconPath))
        {
            Addressables.LoadAssetAsync<Sprite>(iconPath).Completed += (op) =>
            {
                if (op.Status == AsyncOperationStatus.Succeeded)
                {
                    item.icon = op.Result;
                    // 아이콘 로드 완료 이벤트 발생
                    OnAnyItemIconLoaded?.Invoke();
                    Debug.Log($"아이템 아이콘 로드 성공: {iconPath}");
                }
                else
                {
                    Debug.LogWarning($"아이템 아이콘 로드 실패: {iconPath}");
                }
            };
        }
    }

    // 아이템 ID로 아이템 가져오기
    public Item GetItem(int itemId)
    {
        if (itemDatabase.TryGetValue(itemId, out Item item))
        {
            return item;
        }

        Debug.LogWarning($"아이템을 찾을 수 없습니다: ID {itemId}");
        return null;
    }

    // 보스 ID로 관련 파편 찾기
    public FragmentItem GetFragmentByBossID(string bossId)
    {
        foreach (var item in itemDatabase.Values)
        {
            if (item is FragmentItem fragment && fragment.associatedBossID == bossId)
            {
                return fragment;
            }
        }

        Debug.LogWarning($"보스 ID {bossId}에 해당하는 파편을 찾을 수 없습니다.");
        return null;
    }

    // 아이템 유형별로 아이템 목록 가져오기
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

    // 게임 종료 시 리소스 해제
    private void OnDestroy()
    {
        // 필요한 경우 리소스 해제 로직 추가
        itemDatabase.Clear();
        itemBaseData.Clear();
        fragmentData.Clear();
        potionData.Clear();
        materialData.Clear();
    }
}