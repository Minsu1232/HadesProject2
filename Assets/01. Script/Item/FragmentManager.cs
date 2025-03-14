using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class FragmentManager : MonoBehaviour
{
    public static FragmentManager Instance { get; private set; }

    [SerializeField] private List<FragmentItem> equippedFragments = new List<FragmentItem>();
    [SerializeField] private int maxEquippedFragments = 3; // �ִ� ���� ���� ���� ��

    [SerializeField] private PlayerClass playerClass; // �÷��̾� Ŭ���� ����

    // �̺�Ʈ
    public delegate void FragmentsChangedHandler();
    public event FragmentsChangedHandler OnFragmentsChanged;

    // UI ���� ����
    [SerializeField] private GameObject fragmentTabButton;

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

        // �ʱ⿡�� ���� �� ��Ȱ��ȭ
        if (fragmentTabButton != null)
        {
            fragmentTabButton.SetActive(false);
        }
    }

    private void Start()
    {
        // �κ��丮 �ý����� ù ���� �̺�Ʈ ����
        if (InventorySystem.Instance != null)
        {
            InventorySystem.Instance.OnFirstFragmentFound += OnFirstFragmentFound;
        }

        // �÷��̾� Ŭ���� ã��
        if (playerClass == null)
        {
            // ������ PlayerClass�� ���� ������Ʈ ã��
            // ���� ������ PlayerClass�� ��� �����Ǵ����� ���� �޶���
            // ��: PlayerManager.Instance.GetPlayerClass() ��
            Debug.LogWarning("playerClass�� FragmentManager�� �Ҵ���� �ʾҽ��ϴ�.");
        }
    }

    // ù ���� ȹ�� �� ȣ��� �޼���
    private void OnFirstFragmentFound()
    {
        Debug.Log("ù ������ �߰��߽��ϴ�! ���� �ý����� Ȱ��ȭ�Ǿ����ϴ�.");

        // ���� UI Ȱ��ȭ
        if (fragmentTabButton != null)
        {
            fragmentTabButton.SetActive(true);
        }
    }

    // ���� ����
    public bool EquipFragment(FragmentItem fragment)
    {
        // �̹� ���� ������ Ȯ��
        if (equippedFragments.Any(f => f.itemID == fragment.itemID))
        {
            Debug.LogWarning("�� ������ �̹� ���� ���Դϴ�!");
            return false;
        }

        // ���� ���� Ȯ��
        if (equippedFragments.Count >= maxEquippedFragments)
        {
            Debug.LogWarning("�ִ� ���� ���� �̹� �����Ǿ����ϴ�!");
            return false;
        }

        // ���� ����
        equippedFragments.Add(fragment);

        // ���� ȿ�� ����
        ApplyFragmentEffects(fragment);

        Debug.Log($"{fragment.itemName} ������ �����߽��ϴ�.");
        OnFragmentsChanged?.Invoke();
        return true;
    }

    // ���� ����
    public bool UnequipFragment(int fragmentID)
    {
        FragmentItem fragmentToRemove = null;

        foreach (var fragment in equippedFragments)
        {
            if (fragment.itemID == fragmentID)
            {
                fragmentToRemove = fragment;
                break;
            }
        }

        if (fragmentToRemove != null)
        {
            // ȿ�� ����
            RemoveFragmentEffects(fragmentToRemove);

            // ��Ͽ��� ����
            equippedFragments.Remove(fragmentToRemove);

            Debug.Log($"{fragmentToRemove.itemName} ������ �����߽��ϴ�.");
            OnFragmentsChanged?.Invoke();
            return true;
        }

        Debug.LogWarning($"������ ������ ã�� �� �����ϴ�: {fragmentID}");
        return false;
    }

    // ���� ȿ�� ���� (PlayerClass Ŭ���� ȣȯ ����)
    private void ApplyFragmentEffects(FragmentItem fragment)
    {
        if (playerClass != null)
        {
            // �÷��̾� ���ȿ� ���� ȿ�� ����
            int attackBonus = Mathf.RoundToInt(fragment.attackBonus);
            int healthBonus = Mathf.RoundToInt(fragment.healthBonus);
            float speedBonus = fragment.speedBonus;

            // PlayerClass�� ModifyPower �޼��� Ȱ��
            playerClass.ModifyPower(
                maxHealth: healthBonus,  // �ִ� ü�� ����
                attackAmount: attackBonus,  // ���ݷ� ����
                speedAmount: speedBonus,  // �̵��ӵ� ����
                damageReceive: -fragment.defenseBonus / 100f  // ������ ������ �������� ��ȯ
            );

            // ���� ���� Ư�� ȿ��
            if (fragment.isResonated)
            {
                Debug.Log($"����� ���� {fragment.itemName}�� Ư�� ȿ���� �ߵ��մϴ�!");

                // ���⿡ Ư�� ȿ�� ���� �߰�
                // ��: ���丮 �ر�, Ư�� �ɷ� Ȱ��ȭ ��
            }
        }
        else
        {
            Debug.LogWarning("playerClass�� ã�� �� �����ϴ�. ���� ȿ���� ������� �ʾҽ��ϴ�.");
        }
    }

    // ���� ȿ�� ���� (PlayerClass Ŭ���� ȣȯ ����)
    private void RemoveFragmentEffects(FragmentItem fragment)
    {
        if (playerClass != null)
        {
            // �÷��̾� ���ȿ��� ���� ȿ�� ���� (������ ����)
            int attackBonus = -Mathf.RoundToInt(fragment.attackBonus);
            int healthBonus = -Mathf.RoundToInt(fragment.healthBonus);
            float speedBonus = -fragment.speedBonus;

            // PlayerClass�� ModifyPower �޼��� Ȱ��
            playerClass.ModifyPower(
                maxHealth: healthBonus,  // �ִ� ü�� ����
                attackAmount: attackBonus,  // ���ݷ� ����
                speedAmount: speedBonus,  // �̵��ӵ� ����
                damageReceive: fragment.defenseBonus / 100f  // ���� ���� (������ ������ ����)
            );
        }
    }

    // ������ ���� ��� ��������
    public List<FragmentItem> GetEquippedFragments()
    {
        return new List<FragmentItem>(equippedFragments);
    }

    // ������ ��� ���� ����
    public void UnequipAllFragments()
    {
        List<FragmentItem> fragmentsToRemove = new List<FragmentItem>(equippedFragments);

        foreach (var fragment in fragmentsToRemove)
        {
            UnequipFragment(fragment.itemID);
        }

        Debug.Log("��� ������ �����߽��ϴ�.");
    }

    // ���� ȿ�� ��� (����)
    public void CalculateTotalEffects(out float attackBonus, out float defenseBonus, out float healthBonus, out float speedBonus)
    {
        attackBonus = 0f;
        defenseBonus = 0f;
        healthBonus = 0f;
        speedBonus = 0f;

        foreach (var fragment in equippedFragments)
        {
            attackBonus += fragment.attackBonus;
            defenseBonus += fragment.defenseBonus;
            healthBonus += fragment.healthBonus;
            speedBonus += fragment.speedBonus;
        }
    }

    // ���� ���� üũ (���� �����)
    public bool HasResonatedFragmentForBoss(string bossID)
    {
        return equippedFragments.Any(f => f.isResonated && f.associatedBossID == bossID);
    }

    // ���� ���� �� üũ (���� ���� ȿ�� ����)
    public void CheckBossResonance(string bossID)
    {
        if (HasResonatedFragmentForBoss(bossID))
        {
            Debug.Log($"���� ID {bossID}�� ���� ���� ���� ȿ���� Ȱ��ȭ�Ǿ����ϴ�!");
            // ���⿡ ���� ���� Ư�� ȿ�� �߰�
        }
    }
}