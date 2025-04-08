using System;
using System.Collections.Generic;
using UnityEngine;

public class InventorySystem : MonoBehaviour
{
    public static InventorySystem Instance { get; private set; }
    
    [System.Serializable]
    public class ItemSlot
    {
        public Item item;
        public int quantity;

        public ItemSlot(Item item, int quantity)
        {
            this.item = item;
            this.quantity = quantity;
        }
    }

    [SerializeField] private List<ItemSlot> itemSlots = new List<ItemSlot>();
    // InventoryUI 클래스 내부
    [SerializeField] private List<FragmentSlotUI> fragmentSlots = new List<FragmentSlotUI>();
    [SerializeField] private int maxInventorySize = 20;
    [SerializeField] private bool hasFoundFirstFragment = false; // 첫 파편 획득 여부


    public event Action OnInventoryChanged;

    public event Action OnFirstFragmentFound;
    
    public event Action<int, int> OnItemAdded;
    // 플레이어 참조 (아이템 사용 효과 적용용)
    [SerializeField] private PlayerClass playerClass;
    [SerializeField] private GameObject inventoryPanel;

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

    private async void Start()
    {
   
        await ItemDataManager.Instance.InitializeItems();
        // 기존 코드...
        if (ItemDataManager.Instance != null)
        {
            ItemDataManager.Instance.OnAnyItemIconLoaded += OnIconLoaded;
        }

        playerClass = GameInitializer.Instance.GetPlayerClass();


    }
    private void Update()
    {
        // I 키를 눌러 인벤토리 토글
        if (Input.GetKeyDown(KeyCode.I))
        {
            ToggleInventory();
        }
    }
    public void ToggleInventory()
    {
        if (inventoryPanel != null)
        {
            bool isActive = !inventoryPanel.activeSelf;
            inventoryPanel.SetActive(isActive);

            //// 인벤토리 열릴 때 아이템 목록 새로고침
            //if (isActive)
            //{
            //    RefreshAllUI();
            //}

            // 게임 일시정지 등 추가 기능 구현 (필요시)
            // Time.timeScale = isActive ? 0 : 1;
        }
    }
    // 아이콘 로드 완료 시 호출
    private void OnIconLoaded()
    {
        // 인벤토리가 변경되었다는 이벤트 발생 - 이 이벤트는 InventoryUI가 구독하고 있음
        OnInventoryChanged?.Invoke();
    }

    
    // 아이템 ID로 아이템 추가
    public bool AddItem(int itemId, int quantity = 1)
    {
        Item itemToAdd = ItemDataManager.Instance.GetItem(itemId);

        if (itemToAdd == null)
        {
            Debug.LogError($"아이템 추가 실패. ID {itemId}인 아이템을 찾을 수 없습니다.");
            return false;
        }
        Debug.Log("아이템획득 성공");
        bool result = AddItem(itemToAdd, quantity);
        SaveManager.Instance.SaveAllData();
        //// 아이템 추가 성공 시 이벤트 발생
        //if (result)
        //{
        //    OnItemAdded?.Invoke(itemId, quantity);
        //}

        return result;
    }

    // 아이템 객체로 아이템 추가
    public bool AddItem(Item itemToAdd, int quantity = 1)
    {
        if (itemToAdd == null || quantity <= 0)
        {
            return false;
        }

        // 파편을 처음 획득한 경우 이벤트 발생
        if (itemToAdd.itemType == Item.ItemType.Fragment && !hasFoundFirstFragment)
        {
            hasFoundFirstFragment = true;
            OnFirstFragmentFound?.Invoke();
            Debug.Log("첫 파편을 획득했습니다! 파편 UI가 활성화됩니다.");
        }
    
        // 스택 가능한 아이템인 경우 기존 스택에 추가 시도
        if (itemToAdd.isStackable)
        {
            for (int i = 0; i < itemSlots.Count; i++)
            {
                if (itemSlots[i].item.itemID == itemToAdd.itemID)
                {
                    // 최대 스택 크기 체크
                    if (itemSlots[i].quantity + quantity <= itemToAdd.maxStackSize)
                    {
                        itemSlots[i].quantity += quantity;
                        OnInventoryChanged?.Invoke();
                        Debug.Log($"{itemToAdd.itemName} x{quantity}개를 인벤토리에 추가했습니다. (스택)");
                        return true;
                    }
                    else
                    {
                        // 일부만 스택에 추가하고 나머지는 새 슬롯에 추가
                        int remainingSpace = itemToAdd.maxStackSize - itemSlots[i].quantity;
                        itemSlots[i].quantity = itemToAdd.maxStackSize;

                        //Debug.Log($"{itemToAdd.itemName} x{remainingSpace}개를 기존 슬롯에 추가하고, 나머지를 새 슬롯에 추가합니다.");

                        // 나머지 수량으로 재귀 호출
                        return AddItem(itemToAdd, quantity - remainingSpace);
                    }
                }
            }
        }

        // 새 슬롯 필요
        if (itemSlots.Count >= maxInventorySize)
        {
            Debug.LogWarning("인벤토리가 가득 찼습니다!");
            return false;
        }
        if (itemToAdd is FragmentItem)
        {
            OnItemAdded?.Invoke(itemToAdd.itemID, quantity);
            FragmentItem fragmentItem = (FragmentItem) itemToAdd;
            FragmentManager.Instance.EquipFragment(fragmentItem);
        }
        // 새 슬롯에 아이템 추가
        itemSlots.Add(new ItemSlot(itemToAdd, quantity));
        OnInventoryChanged?.Invoke();
        //SaveManager.Instance.SavePlayerData();
        SaveManager.Instance.UpdateInventory(this);
        Debug.Log($"{itemToAdd.itemName} x{quantity}개를 인벤토리에 추가했습니다. (새 슬롯)");
        return true;
    }

    // 아이템 제거
    public bool RemoveItem(int itemID, int quantity = 1)
    {
        for (int i = 0; i < itemSlots.Count; i++)
        {
            if (itemSlots[i].item.itemID == itemID)
            {
                if (itemSlots[i].quantity > quantity)
                {
                    // 수량 감소
                    itemSlots[i].quantity -= quantity;
                    Debug.Log($"{itemSlots[i].item.itemName} x{quantity}개를 인벤토리에서 제거했습니다.");
                }
                else
                {
                    // 슬롯 제거
                    Debug.Log($"{itemSlots[i].item.itemName} x{itemSlots[i].quantity}개를 인벤토리에서 모두 제거했습니다.");
                    itemSlots.RemoveAt(i);
                }

                OnInventoryChanged?.Invoke();
                return true;
            }
        }

        Debug.LogWarning($"제거할 아이템을 찾을 수 없습니다: {itemID}");
        return false;
    }

    // 아이템 사용
    public bool UseItem(int itemID)
    {
        for (int i = 0; i < itemSlots.Count; i++)
        {
            if (itemSlots[i].item.itemID == itemID)
            {
                Item item = itemSlots[i].item;
                bool used = false;

                // 아이템 유형에 따라 처리
                switch (item.itemType)
                {
                    case Item.ItemType.Fragment:
                        FragmentItem fragment = item as FragmentItem;
                        if (fragment != null && FragmentManager.Instance != null)
                        {
                            used = FragmentManager.Instance.EquipFragment(fragment);
                        }
                        break;

                    case Item.ItemType.Potion:
                        PotionItem potion = item as PotionItem;
                        if (potion != null && playerClass != null)
                        {
                            // 포션 효과 적용
                            ApplyPotionEffect(potion);
                            used = true;
                        }
                        break;

                    default:
                        // 기본 Use 메서드 호출
                        used = item.Use();
                        break;
                }

                if (used)
                {
                    Debug.Log($"{item.itemName}을(를) 사용했습니다.");

                    // 파편이 아닌 아이템은 사용 후 수량 감소
                    if (item.itemType != Item.ItemType.Fragment)
                    {
                        itemSlots[i].quantity--;
                        if (itemSlots[i].quantity <= 0)
                        {
                            itemSlots.RemoveAt(i);
                        }
                    }

                    OnInventoryChanged?.Invoke();
                }

                return used;
            }
        }

        Debug.LogWarning($"사용할 아이템을 찾을 수 없습니다: {itemID}");
        return false;
    }

    // 포션 효과 적용 (PlayerClass와 호환)
    private void ApplyPotionEffect(PotionItem potion)
    {
        if (playerClass == null) return;

        switch (potion.potionType)
        {
            case PotionItem.PotionType.Health:
                // 체력 회복
                playerClass.ModifyPower(healthAmount: (int)potion.potionValue);
                Debug.Log($"체력 포션 사용: {potion.potionValue} 회복");
                break;

            case PotionItem.PotionType.Buff:
                // 버프 효과 (공격력 증가)
                playerClass.ModifyPower(attackAmount: (int)potion.potionValue);
                Debug.Log($"공격력 버프 포션 사용: {potion.potionValue} 증가");

                // 지속시간이 있는 경우, 시간 후 효과 제거
                if (potion.duration > 0)
                {
                    StartCoroutine(RemoveBuffAfterDuration(potion));
                }
                break;

            default:
                Debug.LogWarning($"처리되지 않은 포션 유형: {potion.potionType}");
                break;
        }
    }

    // 버프 효과 시간 후 제거
    private System.Collections.IEnumerator RemoveBuffAfterDuration(PotionItem potion)
    {
        yield return new WaitForSeconds(potion.duration);

        if (playerClass != null)
        {
            // 효과 제거 (음수로 적용)
            switch (potion.potionType)
            {
                case PotionItem.PotionType.Buff:
                    playerClass.ModifyPower(attackAmount: -(int)potion.potionValue);
                    Debug.Log($"공격력 버프 효과 종료: {potion.potionValue} 감소");
                    break;
            }
        }
    }

    // 아이템 소유 여부 확인
    public bool HasItem(int itemID, int quantity = 1)
    {
        foreach (ItemSlot slot in itemSlots)
        {
            if (slot.item.itemID == itemID && slot.quantity >= quantity)
            {
                return true;
            }
        }

        return false;
    }

    // 타입별 아이템 목록 가져오기
    public List<ItemSlot> GetItemsByType(Item.ItemType type)
    {
        return itemSlots.FindAll(slot => slot.item.itemType == type);
    }

    // 인벤토리 전체 아이템 목록 가져오기
    public List<ItemSlot> GetAllItems()
    {
        return new List<ItemSlot>(itemSlots);
    }

    // 첫 파편 획득 여부 확인
    public bool HasFoundFirstFragment()
    {
        return hasFoundFirstFragment;
    }

    // 인벤토리 비우기 (디버그/테스트용)
    public void ClearInventory()
    {
        itemSlots.Clear();
        OnInventoryChanged?.Invoke();
        Debug.Log("인벤토리를 비웠습니다.");
    }

    // 인벤토리에서 아이템 슬롯 찾기
    public ItemSlot FindItemSlot(int itemID)
    {
        return itemSlots.Find(slot => slot.item.itemID == itemID);
    }
    public int GetItemQuantity(int itemID)
    {
        ItemSlot slot = FindItemSlot(itemID);
        
        return slot != null ? slot.quantity : 0;
    }
    // 디버깅용: 인벤토리 내용 출력
    public void DebugPrintInventory()
    {
        Debug.Log("===== 인벤토리 내용 =====");
        if (itemSlots.Count == 0)
        {
            Debug.Log("인벤토리가 비어 있습니다.");
            return;
        }

        foreach (var slot in itemSlots)
        {
            Debug.Log($"아이템: {slot.item.itemName}, 수량: {slot.quantity}, 타입: {slot.item.itemType}");
        }
        Debug.Log("========================");
    }
}