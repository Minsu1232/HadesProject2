// DungeonAbilityManager.cs - 단순화된 버전
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class DungeonAbilityManager : MonoBehaviour
{
    // Singleton 패턴
    public static DungeonAbilityManager Instance { get; private set; }

    // 현재 보유 중인 능력들
    private List<DungeonAbility> currentAbilities = new List<DungeonAbility>();

    public event System.Action OnAbilityAcquired; // 능력 선택 호출 이벤트
    public event System.Action OnAbilityLevelUp; // 능력 중복 선택 레벨업 호출 이벤트
    private float[] rarityWeights = { 0.5f, 0.3f, 0.15f, 0.04f, 0.01f }; // 희귀도별 가중치

    [SerializeField]
    private int abilitiesPerSelection = 3; // 한 번에 제공할 선택지 수

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
            Debug.LogError("PlayerClass를 찾을 수 없습니다.");
        }

        // 로더 초기화 확인
        InitializeLoaders();
    }

    // 필요한 로더 초기화
    private void InitializeLoaders()
    {
        // 패시브 능력 로더 초기화
        if (PassiveAbilityLoader.Instance == null)
        {
            GameObject loaderObj = new GameObject("PassiveAbilityLoader");
            loaderObj.AddComponent<PassiveAbilityLoader>();
            Debug.Log("PassiveAbilityLoader 생성됨");
        }

        // 공격 능력 로더 초기화
        if (AttackAbilityLoader.Instance == null)
        {
            GameObject loaderObj = new GameObject("AttackAbilityLoader");
            loaderObj.AddComponent<AttackAbilityLoader>();
            Debug.Log("AttackAbilityLoader 생성됨");
        }

        // 필요한 다른 로더 초기화...
    }

    // 던전 입장 시 초기화
    public void InitializeDungeon()
    {
        ResetAllAbilities();
        currentAbilities.Clear();
    }

    // 던전 나갈 때 모든 능력 초기화
    public void ResetAllAbilities()
    {
        foreach (DungeonAbility ability in currentAbilities)
        {
            ability.OnReset(playerClass);
        }
    }

    // 스테이지 클리어 시 선택지 생성
    public List<DungeonAbility> GetAbilitySelection()
    {
        List<DungeonAbility> availableAbilities = FilterAvailableAbilities();
        List<DungeonAbility> selection = new List<DungeonAbility>();

        // 필요한 개수만큼 랜덤하게 선택
        for (int i = 0; i < abilitiesPerSelection; i++)
        {
            if (availableAbilities.Count == 0) break;

            // 가중치 기반 랜덤 선택
            DungeonAbility selectedAbility = SelectWeightedRandomAbility(availableAbilities);
            selection.Add(selectedAbility);

            // 중복 방지를 위해 이미 선택된 능력과 같은 ID를 가진 능력 제거
            availableAbilities.RemoveAll(a => a.id == selectedAbility.id);
        }

        return selection;
    }

    // 현재 상황에 맞는 능력 필터링
    private List<DungeonAbility> FilterAvailableAbilities()
    {
        List<DungeonAbility> filtered = new List<DungeonAbility>();
        List<DungeonAbility> allAbilities = GetAllAbilities();

        foreach (DungeonAbility ability in allAbilities)
        {
            // 이미 보유 중인 능력은 복제본을 만들어 레벨업 옵션으로 제공
            DungeonAbility existingAbility = currentAbilities.Find(a => a.id == ability.id);
            if (existingAbility != null)
            {
                // 최대 레벨이 아닌 경우에만 레벨업 옵션 제공
                if (existingAbility.level < 5) // 최대 레벨 예시
                {
                    // 타입에 따라 복제본 생성
                    DungeonAbility upgradedAbility = CreateUpgradedAbilityCopy(ability, existingAbility);
                    if (upgradedAbility != null)
                    {
                        filtered.Add(upgradedAbility);
                    }
                }
            }
            else
            {
                // 보유하지 않은 새 능력 추가
                filtered.Add(ability);
            }
        }

        return filtered;
    }

    // 모든 가능한 능력 목록 가져오기
    private List<DungeonAbility> GetAllAbilities()
    {
        List<DungeonAbility> allAbilities = new List<DungeonAbility>();

        // 패시브 능력 추가
        var passiveAbilities = PassiveAbilityFactory.CreateAllPassiveAbilities();
        allAbilities.AddRange(passiveAbilities.Cast<DungeonAbility>());

        // 공격 능력 추가
        var attackAbilities = AttackAbilityFactory.CreateAllAttackAbilities();
        allAbilities.AddRange(attackAbilities.Cast<DungeonAbility>());

        // 다른 타입의 능력 추가...

        return allAbilities;
    }

    // 레벨업을 위한 능력 복제본 생성
    private DungeonAbility CreateUpgradedAbilityCopy(DungeonAbility templateAbility, DungeonAbility existingAbility)
    {
        // 패시브 능력인 경우
        if (templateAbility is PassiveAbility passiveTemplate)
        {
            PassiveAbility upgraded = new PassiveAbility();
            upgraded.Initialize(
                ((PassiveAbility)passiveTemplate).passiveType,
                ((PassiveAbility)passiveTemplate).effectValue,
                templateAbility.name,
                $"{templateAbility.description} (현재 Lv.{existingAbility.level})",
                templateAbility.rarity
            );
            upgraded.id = templateAbility.id;
            upgraded.level = existingAbility.level;
            return upgraded;
        }
        // 공격 능력인 경우
        else if (templateAbility is AttackAbility attackTemplate)
        {
            AttackAbility upgraded = new AttackAbility();
            upgraded.Initialize(
                ((AttackAbility)attackTemplate).attackType,
                ((AttackAbility)attackTemplate).effectValue,
                templateAbility.name,
                $"{templateAbility.description} (현재 Lv.{existingAbility.level})",
                templateAbility.rarity
            );
            upgraded.id = templateAbility.id;
            upgraded.level = existingAbility.level;
            return upgraded;
        }
        // 다른 타입의 능력도 여기에 추가...

        Debug.LogWarning($"능력 {templateAbility.id}의 업그레이드 복사본을 생성할 수 없습니다. 지원되지 않는 타입: {templateAbility.GetType().Name}");
        return null;
    }

    // 가중치 기반 랜덤 선택
    private DungeonAbility SelectWeightedRandomAbility(List<DungeonAbility> abilities)
    {
        // 희귀도별로 그룹화
        Dictionary<Rarity, List<DungeonAbility>> groupedByRarity = new Dictionary<Rarity, List<DungeonAbility>>();
        foreach (Rarity rarity in System.Enum.GetValues(typeof(Rarity)))
        {
            groupedByRarity[rarity] = abilities.Where(a => a.rarity == rarity).ToList();
        }

        // 가중치 기반 희귀도 선택
        float totalWeight = rarityWeights.Sum();
        float randomValue = UnityEngine.Random.Range(0f, totalWeight);
        float weightSum = 0;
        Rarity selectedRarity = Rarity.Common; // 기본값

        for (int i = 0; i < rarityWeights.Length; i++)
        {
            weightSum += rarityWeights[i];
            if (randomValue <= weightSum)
            {
                selectedRarity = (Rarity)i;
                break;
            }
        }

        // 선택된 희귀도에 능력이 없다면, 더 흔한 희귀도에서 선택
        while (groupedByRarity[selectedRarity].Count == 0 && (int)selectedRarity > 0)
        {
            selectedRarity = (Rarity)((int)selectedRarity - 1);
        }

        // 그래도 없다면 전체에서 랜덤 선택
        if (groupedByRarity[selectedRarity].Count == 0)
        {
            return abilities[Random.Range(0, abilities.Count)];
        }

        // 선택된 희귀도에서 랜덤 선택
        return groupedByRarity[selectedRarity][Random.Range(0, groupedByRarity[selectedRarity].Count)];
    }

    // 능력 선택 처리
    public void AcquireAbility(DungeonAbility selectedAbility)
    {
        // 이미 있는 능력인지 확인
        DungeonAbility existingAbility = currentAbilities.Find(a => a.id == selectedAbility.id);
        Debug.Log($"능력치 획득 시작: ID={selectedAbility.id}, 이름={selectedAbility.name}, 레벨={selectedAbility.level}");
        Debug.Log($"현재 보유 능력 수: {currentAbilities.Count}");

        if (existingAbility != null)
        {
            // 레벨업
            existingAbility.OnLevelUp(playerClass);

            // 이벤트 발생
            OnAbilityLevelUp?.Invoke();
        }
        else
        {
            // 새 능력 추가
            selectedAbility.OnAcquire(playerClass);
            currentAbilities.Add(selectedAbility);

            // 이벤트 발생
            OnAbilityAcquired?.Invoke();
        }
    }

    // 현재 보유 중인 능력 목록 가져오기
    public List<DungeonAbility> GetCurrentAbilities()
    {
        return new List<DungeonAbility>(currentAbilities);
    }

    // 특정 ID의 능력 가져오기
    public DungeonAbility GetAbilityById(string id)
    {
        return currentAbilities.Find(a => a.id == id);
    }
}