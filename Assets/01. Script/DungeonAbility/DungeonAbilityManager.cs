// DungeonAbilityManager �ϼ���
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class DungeonAbilityManager : MonoBehaviour
{
    // Singleton ����
    public static DungeonAbilityManager Instance { get; private set; }

    // ���� ���� ���� �ɷµ�
    private List<DungeonAbility> currentAbilities = new List<DungeonAbility>();

    // ��͵��� ����ġ
    [SerializeField]
    private float[] rarityWeights = { 0.5f, 0.3f, 0.15f, 0.04f, 0.01f }; // ��͵��� ����ġ

    [SerializeField]
    private int abilitiesPerSelection = 3; // �� ���� ������ ������ ��

    private PlayerClass playerClass;

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

    private void Start()
    {
        playerClass = GameInitializer.Instance.GetPlayerClass();
        if (playerClass == null)
        {
            Debug.LogError("PlayerClass�� ã�� �� �����ϴ�.");
        }

        // CSV �δ� �ʱ�ȭ Ȯ��
        if (PassiveAbilityLoader.Instance == null)
        {
            Debug.LogWarning("PassiveAbilityLoader�� �ʱ�ȭ���� �ʾҽ��ϴ�. �ʿ��ϴٸ� �����մϴ�.");
            GameObject loaderObj = new GameObject("PassiveAbilityLoader");
            loaderObj.AddComponent<PassiveAbilityLoader>();
        }
    }

    // ���� ���� �� �ʱ�ȭ
    public void InitializeDungeon()
    {
        ResetAllAbilities();
        currentAbilities.Clear();
    }

    // ���� ���� �� ��� �ɷ� �ʱ�ȭ
    public void ResetAllAbilities()
    {
        foreach (DungeonAbility ability in currentAbilities)
        {
            ability.OnReset(playerClass);
        }
    }

    // �������� Ŭ���� �� ������ ����
    public List<DungeonAbility> GetAbilitySelection()
    {
        List<PassiveAbility> availableAbilities = FilterAvailableAbilities();
        List<DungeonAbility> selection = new List<DungeonAbility>();

        // �ʿ��� ������ŭ �����ϰ� ����
        for (int i = 0; i < abilitiesPerSelection; i++)
        {
            if (availableAbilities.Count == 0) break;

            // ����ġ ��� ���� ����
            DungeonAbility selectedAbility = SelectWeightedRandomAbility(availableAbilities);
            selection.Add(selectedAbility);

            // �ߺ� ������ ���� �̹� ���õ� �ɷ°� ���� ID�� ���� �ɷ� ����
            availableAbilities.RemoveAll(a => a.id == selectedAbility.id);
        }

        return selection;
    }

    // ���� ��Ȳ�� �´� �ɷ� ���͸�
    private List<PassiveAbility> FilterAvailableAbilities()
    {
        List<PassiveAbility> filtered = new List<PassiveAbility>();
        List<PassiveAbility> allAbilities = PassiveAbilityFactory.CreateAllPassiveAbilities();

        foreach (PassiveAbility ability in allAbilities)
        {
            // �̹� ���� ���� �ɷ��� �������� ����� ������ �ɼ����� ����
            DungeonAbility existingAbility = currentAbilities.Find(a => a.id == ability.id);
            if (existingAbility != null)
            {
                // �ִ� ������ �ƴ� ��쿡�� ������ �ɼ� ����
                if (existingAbility.level < 5) // �ִ� ���� ����
                {
                    PassiveAbility upgraded = new PassiveAbility();
                    upgraded.Initialize(
                        ability.passiveType,
                        ability.effectValue,
                        ability.name,
                        $"{ability.description} (���� Lv.{existingAbility.level})",
                        ability.rarity
                    );
                    upgraded.id = ability.id;
                    upgraded.level = existingAbility.level;
                    filtered.Add(upgraded);
                }
            }
            else
            {
                // �������� ���� �� �ɷ� �߰�
                filtered.Add(ability);
            }
        }

        return filtered;
    }

    // ����ġ ��� ���� ����
    private DungeonAbility SelectWeightedRandomAbility(List<PassiveAbility> abilities)
    {
        // ��͵����� �׷�ȭ
        Dictionary<Rarity, List<PassiveAbility>> groupedByRarity = new Dictionary<Rarity, List<PassiveAbility>>();
        foreach (Rarity rarity in System.Enum.GetValues(typeof(Rarity)))
        {
            groupedByRarity[rarity] = abilities.Where(a => a.rarity == rarity).ToList();
        }

        // ����ġ ��� ��͵� ����
        float totalWeight = rarityWeights.Sum();
        float randomValue = UnityEngine.Random.Range(0f, totalWeight);
        float weightSum = 0;
        Rarity selectedRarity = Rarity.Common; // �⺻��

        for (int i = 0; i < rarityWeights.Length; i++)
        {
            weightSum += rarityWeights[i];
            if (randomValue <= weightSum)
            {
                selectedRarity = (Rarity)i;
                break;
            }
        }

        // ���õ� ��͵��� �ɷ��� ���ٸ�, �� ���� ��͵����� ����
        while (groupedByRarity[selectedRarity].Count == 0 && (int)selectedRarity > 0)
        {
            selectedRarity = (Rarity)((int)selectedRarity - 1);
        }

        // �׷��� ���ٸ� ��ü���� ���� ����
        if (groupedByRarity[selectedRarity].Count == 0)
        {
            return abilities[Random.Range(0, abilities.Count)];
        }

        // ���õ� ��͵����� ���� ����
        return groupedByRarity[selectedRarity][Random.Range(0, groupedByRarity[selectedRarity].Count)];
    }

    // �ɷ� ���� ó��
    public void AcquireAbility(DungeonAbility selectedAbility)
    {
        // �̹� �ִ� �ɷ����� Ȯ��
        DungeonAbility existingAbility = currentAbilities.Find(a => a.id == selectedAbility.id);

        if (existingAbility != null)
        {
            // ������
            existingAbility.OnLevelUp(playerClass);
        }
        else
        {
            // �� �ɷ� �߰�
            selectedAbility.OnAcquire(playerClass);
            currentAbilities.Add(selectedAbility);
        }
    }

    // ���� ���� ���� �ɷ� ��� ��������
    public List<DungeonAbility> GetCurrentAbilities()
    {
        return new List<DungeonAbility>(currentAbilities);
    }

    // Ư�� ID�� �ɷ� ��������
    public DungeonAbility GetAbilityById(string id)
    {
        return currentAbilities.Find(a => a.id == id);
    }
}