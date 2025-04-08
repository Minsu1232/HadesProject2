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
    // InventoryUI Ŭ���� ����
    [SerializeField] private List<FragmentSlotUI> fragmentSlots = new List<FragmentSlotUI>();
    [SerializeField] private int maxInventorySize = 20;
    [SerializeField] private bool hasFoundFirstFragment = false; // ù ���� ȹ�� ����


    public event Action OnInventoryChanged;

    public event Action OnFirstFragmentFound;
    
    public event Action<int, int> OnItemAdded;
    // �÷��̾� ���� (������ ��� ȿ�� �����)
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
        // ���� �ڵ�...
        if (ItemDataManager.Instance != null)
        {
            ItemDataManager.Instance.OnAnyItemIconLoaded += OnIconLoaded;
        }

        playerClass = GameInitializer.Instance.GetPlayerClass();


    }
    private void Update()
    {
        // I Ű�� ���� �κ��丮 ���
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

            //// �κ��丮 ���� �� ������ ��� ���ΰ�ħ
            //if (isActive)
            //{
            //    RefreshAllUI();
            //}

            // ���� �Ͻ����� �� �߰� ��� ���� (�ʿ��)
            // Time.timeScale = isActive ? 0 : 1;
        }
    }
    // ������ �ε� �Ϸ� �� ȣ��
    private void OnIconLoaded()
    {
        // �κ��丮�� ����Ǿ��ٴ� �̺�Ʈ �߻� - �� �̺�Ʈ�� InventoryUI�� �����ϰ� ����
        OnInventoryChanged?.Invoke();
    }

    
    // ������ ID�� ������ �߰�
    public bool AddItem(int itemId, int quantity = 1)
    {
        Item itemToAdd = ItemDataManager.Instance.GetItem(itemId);

        if (itemToAdd == null)
        {
            Debug.LogError($"������ �߰� ����. ID {itemId}�� �������� ã�� �� �����ϴ�.");
            return false;
        }
        Debug.Log("������ȹ�� ����");
        bool result = AddItem(itemToAdd, quantity);
        SaveManager.Instance.SaveAllData();
        //// ������ �߰� ���� �� �̺�Ʈ �߻�
        //if (result)
        //{
        //    OnItemAdded?.Invoke(itemId, quantity);
        //}

        return result;
    }

    // ������ ��ü�� ������ �߰�
    public bool AddItem(Item itemToAdd, int quantity = 1)
    {
        if (itemToAdd == null || quantity <= 0)
        {
            return false;
        }

        // ������ ó�� ȹ���� ��� �̺�Ʈ �߻�
        if (itemToAdd.itemType == Item.ItemType.Fragment && !hasFoundFirstFragment)
        {
            hasFoundFirstFragment = true;
            OnFirstFragmentFound?.Invoke();
            Debug.Log("ù ������ ȹ���߽��ϴ�! ���� UI�� Ȱ��ȭ�˴ϴ�.");
        }
    
        // ���� ������ �������� ��� ���� ���ÿ� �߰� �õ�
        if (itemToAdd.isStackable)
        {
            for (int i = 0; i < itemSlots.Count; i++)
            {
                if (itemSlots[i].item.itemID == itemToAdd.itemID)
                {
                    // �ִ� ���� ũ�� üũ
                    if (itemSlots[i].quantity + quantity <= itemToAdd.maxStackSize)
                    {
                        itemSlots[i].quantity += quantity;
                        OnInventoryChanged?.Invoke();
                        Debug.Log($"{itemToAdd.itemName} x{quantity}���� �κ��丮�� �߰��߽��ϴ�. (����)");
                        return true;
                    }
                    else
                    {
                        // �Ϻθ� ���ÿ� �߰��ϰ� �������� �� ���Կ� �߰�
                        int remainingSpace = itemToAdd.maxStackSize - itemSlots[i].quantity;
                        itemSlots[i].quantity = itemToAdd.maxStackSize;

                        //Debug.Log($"{itemToAdd.itemName} x{remainingSpace}���� ���� ���Կ� �߰��ϰ�, �������� �� ���Կ� �߰��մϴ�.");

                        // ������ �������� ��� ȣ��
                        return AddItem(itemToAdd, quantity - remainingSpace);
                    }
                }
            }
        }

        // �� ���� �ʿ�
        if (itemSlots.Count >= maxInventorySize)
        {
            Debug.LogWarning("�κ��丮�� ���� á���ϴ�!");
            return false;
        }
        if (itemToAdd is FragmentItem)
        {
            OnItemAdded?.Invoke(itemToAdd.itemID, quantity);
            FragmentItem fragmentItem = (FragmentItem) itemToAdd;
            FragmentManager.Instance.EquipFragment(fragmentItem);
        }
        // �� ���Կ� ������ �߰�
        itemSlots.Add(new ItemSlot(itemToAdd, quantity));
        OnInventoryChanged?.Invoke();
        //SaveManager.Instance.SavePlayerData();
        SaveManager.Instance.UpdateInventory(this);
        Debug.Log($"{itemToAdd.itemName} x{quantity}���� �κ��丮�� �߰��߽��ϴ�. (�� ����)");
        return true;
    }

    // ������ ����
    public bool RemoveItem(int itemID, int quantity = 1)
    {
        for (int i = 0; i < itemSlots.Count; i++)
        {
            if (itemSlots[i].item.itemID == itemID)
            {
                if (itemSlots[i].quantity > quantity)
                {
                    // ���� ����
                    itemSlots[i].quantity -= quantity;
                    Debug.Log($"{itemSlots[i].item.itemName} x{quantity}���� �κ��丮���� �����߽��ϴ�.");
                }
                else
                {
                    // ���� ����
                    Debug.Log($"{itemSlots[i].item.itemName} x{itemSlots[i].quantity}���� �κ��丮���� ��� �����߽��ϴ�.");
                    itemSlots.RemoveAt(i);
                }

                OnInventoryChanged?.Invoke();
                return true;
            }
        }

        Debug.LogWarning($"������ �������� ã�� �� �����ϴ�: {itemID}");
        return false;
    }

    // ������ ���
    public bool UseItem(int itemID)
    {
        for (int i = 0; i < itemSlots.Count; i++)
        {
            if (itemSlots[i].item.itemID == itemID)
            {
                Item item = itemSlots[i].item;
                bool used = false;

                // ������ ������ ���� ó��
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
                            // ���� ȿ�� ����
                            ApplyPotionEffect(potion);
                            used = true;
                        }
                        break;

                    default:
                        // �⺻ Use �޼��� ȣ��
                        used = item.Use();
                        break;
                }

                if (used)
                {
                    Debug.Log($"{item.itemName}��(��) ����߽��ϴ�.");

                    // ������ �ƴ� �������� ��� �� ���� ����
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

        Debug.LogWarning($"����� �������� ã�� �� �����ϴ�: {itemID}");
        return false;
    }

    // ���� ȿ�� ���� (PlayerClass�� ȣȯ)
    private void ApplyPotionEffect(PotionItem potion)
    {
        if (playerClass == null) return;

        switch (potion.potionType)
        {
            case PotionItem.PotionType.Health:
                // ü�� ȸ��
                playerClass.ModifyPower(healthAmount: (int)potion.potionValue);
                Debug.Log($"ü�� ���� ���: {potion.potionValue} ȸ��");
                break;

            case PotionItem.PotionType.Buff:
                // ���� ȿ�� (���ݷ� ����)
                playerClass.ModifyPower(attackAmount: (int)potion.potionValue);
                Debug.Log($"���ݷ� ���� ���� ���: {potion.potionValue} ����");

                // ���ӽð��� �ִ� ���, �ð� �� ȿ�� ����
                if (potion.duration > 0)
                {
                    StartCoroutine(RemoveBuffAfterDuration(potion));
                }
                break;

            default:
                Debug.LogWarning($"ó������ ���� ���� ����: {potion.potionType}");
                break;
        }
    }

    // ���� ȿ�� �ð� �� ����
    private System.Collections.IEnumerator RemoveBuffAfterDuration(PotionItem potion)
    {
        yield return new WaitForSeconds(potion.duration);

        if (playerClass != null)
        {
            // ȿ�� ���� (������ ����)
            switch (potion.potionType)
            {
                case PotionItem.PotionType.Buff:
                    playerClass.ModifyPower(attackAmount: -(int)potion.potionValue);
                    Debug.Log($"���ݷ� ���� ȿ�� ����: {potion.potionValue} ����");
                    break;
            }
        }
    }

    // ������ ���� ���� Ȯ��
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

    // Ÿ�Ժ� ������ ��� ��������
    public List<ItemSlot> GetItemsByType(Item.ItemType type)
    {
        return itemSlots.FindAll(slot => slot.item.itemType == type);
    }

    // �κ��丮 ��ü ������ ��� ��������
    public List<ItemSlot> GetAllItems()
    {
        return new List<ItemSlot>(itemSlots);
    }

    // ù ���� ȹ�� ���� Ȯ��
    public bool HasFoundFirstFragment()
    {
        return hasFoundFirstFragment;
    }

    // �κ��丮 ���� (�����/�׽�Ʈ��)
    public void ClearInventory()
    {
        itemSlots.Clear();
        OnInventoryChanged?.Invoke();
        Debug.Log("�κ��丮�� ������ϴ�.");
    }

    // �κ��丮���� ������ ���� ã��
    public ItemSlot FindItemSlot(int itemID)
    {
        return itemSlots.Find(slot => slot.item.itemID == itemID);
    }
    public int GetItemQuantity(int itemID)
    {
        ItemSlot slot = FindItemSlot(itemID);
        
        return slot != null ? slot.quantity : 0;
    }
    // ������: �κ��丮 ���� ���
    public void DebugPrintInventory()
    {
        Debug.Log("===== �κ��丮 ���� =====");
        if (itemSlots.Count == 0)
        {
            Debug.Log("�κ��丮�� ��� �ֽ��ϴ�.");
            return;
        }

        foreach (var slot in itemSlots)
        {
            Debug.Log($"������: {slot.item.itemName}, ����: {slot.quantity}, Ÿ��: {slot.item.itemType}");
        }
        Debug.Log("========================");
    }
}