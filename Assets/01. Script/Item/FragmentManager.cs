using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class FragmentManager : MonoBehaviour
{
    public static FragmentManager Instance { get; private set; }

    [SerializeField] private List<FragmentItem> equippedFragments = new List<FragmentItem>();
    [SerializeField] private int maxEquippedFragments = 3; // 최대 장착 가능 파편 수

    [SerializeField] private PlayerClass playerClass; // 플레이어 클래스 참조

    // 이벤트
    public delegate void FragmentsChangedHandler();
    public event FragmentsChangedHandler OnFragmentsChanged;

    // UI 관련 참조
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

        // 초기에는 파편 탭 비활성화
        if (fragmentTabButton != null)
        {
            fragmentTabButton.SetActive(false);
        }
    }

    private void Start()
    {
        // 인벤토리 시스템의 첫 파편 이벤트 구독
        if (InventorySystem.Instance != null)
        {
            InventorySystem.Instance.OnFirstFragmentFound += OnFirstFragmentFound;
        }

        // 플레이어 클래스 찾기
        if (playerClass == null)
        {
            // 씬에서 PlayerClass를 가진 오브젝트 찾기
            // 실제 구현은 PlayerClass가 어떻게 관리되는지에 따라 달라짐
            // 예: PlayerManager.Instance.GetPlayerClass() 등
            Debug.LogWarning("playerClass가 FragmentManager에 할당되지 않았습니다.");
        }
    }

    // 첫 파편 획득 시 호출될 메서드
    private void OnFirstFragmentFound()
    {
        Debug.Log("첫 파편을 발견했습니다! 파편 시스템이 활성화되었습니다.");

        // 파편 UI 활성화
        if (fragmentTabButton != null)
        {
            fragmentTabButton.SetActive(true);
        }
    }

    // 파편 장착
    public bool EquipFragment(FragmentItem fragment)
    {
        // 이미 장착 중인지 확인
        if (equippedFragments.Any(f => f.itemID == fragment.itemID))
        {
            Debug.LogWarning("이 파편은 이미 장착 중입니다!");
            return false;
        }

        // 장착 슬롯 확인
        if (equippedFragments.Count >= maxEquippedFragments)
        {
            Debug.LogWarning("최대 파편 수가 이미 장착되었습니다!");
            return false;
        }

        // 파편 장착
        equippedFragments.Add(fragment);

        // 파편 효과 적용
        ApplyFragmentEffects(fragment);

        Debug.Log($"{fragment.itemName} 파편을 장착했습니다.");
        OnFragmentsChanged?.Invoke();
        return true;
    }

    // 파편 해제
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
            // 효과 제거
            RemoveFragmentEffects(fragmentToRemove);

            // 목록에서 제거
            equippedFragments.Remove(fragmentToRemove);

            Debug.Log($"{fragmentToRemove.itemName} 파편을 해제했습니다.");
            OnFragmentsChanged?.Invoke();
            return true;
        }

        Debug.LogWarning($"해제할 파편을 찾을 수 없습니다: {fragmentID}");
        return false;
    }

    // 파편 효과 적용 (PlayerClass 클래스 호환 버전)
    private void ApplyFragmentEffects(FragmentItem fragment)
    {
        if (playerClass != null)
        {
            // 플레이어 스탯에 파편 효과 적용
            int attackBonus = Mathf.RoundToInt(fragment.attackBonus);
            int healthBonus = Mathf.RoundToInt(fragment.healthBonus);
            float speedBonus = fragment.speedBonus;

            // PlayerClass의 ModifyPower 메서드 활용
            playerClass.ModifyPower(
                maxHealth: healthBonus,  // 최대 체력 증가
                attackAmount: attackBonus,  // 공격력 증가
                speedAmount: speedBonus,  // 이동속도 증가
                damageReceive: -fragment.defenseBonus / 100f  // 방어력은 데미지 감소율로 변환
            );

            // 공명 파편 특수 효과
            if (fragment.isResonated)
            {
                Debug.Log($"공명된 파편 {fragment.itemName}의 특수 효과가 발동합니다!");

                // 여기에 특수 효과 로직 추가
                // 예: 스토리 해금, 특수 능력 활성화 등
            }
        }
        else
        {
            Debug.LogWarning("playerClass를 찾을 수 없습니다. 파편 효과가 적용되지 않았습니다.");
        }
    }

    // 파편 효과 제거 (PlayerClass 클래스 호환 버전)
    private void RemoveFragmentEffects(FragmentItem fragment)
    {
        if (playerClass != null)
        {
            // 플레이어 스탯에서 파편 효과 제거 (음수로 적용)
            int attackBonus = -Mathf.RoundToInt(fragment.attackBonus);
            int healthBonus = -Mathf.RoundToInt(fragment.healthBonus);
            float speedBonus = -fragment.speedBonus;

            // PlayerClass의 ModifyPower 메서드 활용
            playerClass.ModifyPower(
                maxHealth: healthBonus,  // 최대 체력 감소
                attackAmount: attackBonus,  // 공격력 감소
                speedAmount: speedBonus,  // 이동속도 감소
                damageReceive: fragment.defenseBonus / 100f  // 방어력 제거 (데미지 감소율 증가)
            );
        }
    }

    // 장착된 파편 목록 가져오기
    public List<FragmentItem> GetEquippedFragments()
    {
        return new List<FragmentItem>(equippedFragments);
    }

    // 장착된 모든 파편 해제
    public void UnequipAllFragments()
    {
        List<FragmentItem> fragmentsToRemove = new List<FragmentItem>(equippedFragments);

        foreach (var fragment in fragmentsToRemove)
        {
            UnequipFragment(fragment.itemID);
        }

        Debug.Log("모든 파편을 해제했습니다.");
    }

    // 파편 효과 계산 (총합)
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

    // 공명 파편 체크 (게임 진행용)
    public bool HasResonatedFragmentForBoss(string bossID)
    {
        return equippedFragments.Any(f => f.isResonated && f.associatedBossID == bossID);
    }

    // 전투 시작 시 체크 (보스 공명 효과 적용)
    public void CheckBossResonance(string bossID)
    {
        if (HasResonatedFragmentForBoss(bossID))
        {
            Debug.Log($"보스 ID {bossID}에 대한 공명 파편 효과가 활성화되었습니다!");
            // 여기에 보스 전투 특수 효과 추가
        }
    }
}