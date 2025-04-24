using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Unity.VisualScripting;
using UnityEngine;
using static AttackData;
using static IMonsterState;

public class MonsterStatus : MonoBehaviour, IDamageable, ICreatureStatus
{
    [SerializeField] CreatureAI creatureAI;
    [SerializeField] private Transform skillSpawnPoint;
    private MonsterUIManager uiManager;
    protected SimpleDissolveController simpleDissolveContorller;
    protected bool isDie = false;
    [FoldoutGroup("Monster Stats")]
    [ReadOnly]
    [ShowInInspector]    
    protected IMonsterClass monsterClass;  // MonsterClass 대신 인터페이스 사용
    [FoldoutGroup("Current Stats"), ReadOnly]
    [ShowInInspector]
    public int CurrentHealth => monsterClass?.CurrentHealth ?? 0;

    [FoldoutGroup("Current Stats"), ReadOnly]
    [ShowInInspector]
    public int CurrentDefense => monsterClass?.CurrentDeffense ?? 0;

    [FoldoutGroup("Current Stats"), ReadOnly]
    [ShowInInspector]
    public int CurrentAttackPower => monsterClass?.CurrentAttackPower ?? 0;

    [FoldoutGroup("Current Stats"), ReadOnly]
    [ShowInInspector]
    public float CurrentAttackSpeed => monsterClass?.CurrentAttackSpeed ?? 0;

    [FoldoutGroup("Current Stats"), ReadOnly]
    [ShowInInspector]
    public int CurrentSpeed => monsterClass?.CurrentSpeed ?? 0;

    [FoldoutGroup("Current Stats"), ReadOnly]
    [ShowInInspector]
    public float CurrentAttackRange => monsterClass?.CurrentAttackRange ?? 0;

    [FoldoutGroup("Skill Stats"), ReadOnly]
    [ShowInInspector]
    public float CurrentSkillCooldown => monsterClass?.CurrentSkillCooldown ?? 0;

    [FoldoutGroup("Skill Stats"), ReadOnly]
    [ShowInInspector]
    public float CurrentSkillRange => monsterClass?.CurrentSkillRange ?? 0;

    [FoldoutGroup("Skill Stats"), ReadOnly]
    [ShowInInspector]
    public float CurrentSkillDuration => monsterClass?.CurrentSKillDuration ?? 0;

    [FoldoutGroup("Armor"), ReadOnly]
    [ShowInInspector]
    public float CurrentArmor => monsterClass?.CurrentArmor ?? 0;

    [FoldoutGroup("Status")]
    [ReadOnly]
    [ShowInInspector]
    public bool IsDead => isDie;

    [FoldoutGroup("Elite Stats"), ReadOnly]
    [ShowInInspector]
    public List<string> EliteAbilities
    {
        get
        {
            var abilities = new List<string>();
            if (monsterClass is EliteMonster eliteMonster)
            {
                foreach (var ability in eliteMonster.GetEliteAbilities())
                {
                    abilities.Add($"{ability.AbilityName} - {ability.Description}");
                }
            }
            return abilities;
        }
    }
    private void Awake()
    {
        uiManager = GetComponent<MonsterUIManager>(); // 시작 시 한 번만 가져오기
        simpleDissolveContorller = GetComponent<SimpleDissolveController>();
        skillSpawnPoint = GetComponentsInChildren<Transform>()
          .FirstOrDefault(t => t.name == "SkillSpawnPoint");

        if (skillSpawnPoint == null)
        {
            Debug.LogWarning($"{gameObject.name}에서 SkillSpawnPoint를 찾지 못했습니다. 몬스터 위치를 사용합니다.");
            skillSpawnPoint = transform;
           
        }
    }

    public virtual void Initialize(IMonsterClass monster)
    {
        monsterClass = monster;
        
        // AI 타입에 따라 적절한 AI 컴포넌트 추가
        creatureAI = /*monster is BossMonster ?*/
            //gameObject.AddComponent<BossAI>() :
            gameObject.AddComponent<BasicCreatureAI>();
    }
    //public AttackType GetAttackType()
    //{
    //    //return monsterClass.IsBasicAttack() ? AttackType.Normal : AttackType.Charge;
    //}
   
    public virtual void TakeDamage(int damage)
    {
        if (isDie) return;

        // 원래 데미지 저장
        int originalDamage = damage;

        // TakeDamage 호출
        monsterClass.TakeDamage(damage);

        // 최종 적용된 데미지 가져오기
        int finalDamage = monsterClass.LastAppliedDamage;

        // UI 업데이트
        if (uiManager != null)
        {
            uiManager.UpdateHealthUI(monsterClass.CurrentHealth);
            uiManager.UpdateArmorUI(monsterClass.CurrentArmor);
            uiManager.SpawnDamageText(finalDamage); // 실제 적용된 데미지
        }

        // 나머지 코드는 그대로
        var CreatureAI = GetComponent<CreatureAI>();
        if (CreatureAI != null)
        {
            CreatureAI.OnDamaged(originalDamage); // AI에는 원래 데미지 전달
        }

        Rigidbody rb = GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.velocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
        }

        CameraShakeManager.TriggerShake(0.5f, 0.05f);

        if (monsterClass.CurrentHealth <= 0 && gameObject != null)
        {
            Die();
        }
    }
    public void TakeDotDamage(int dotDamage)
    {
        //if (!isDie)
        //{
        //    monsterClass.TakeDotDamage(dotDamage);

        //    if (monsterClass.CurrentHealth <= 0)
        //    {    
        //        Die();
        //    }
        //}

    }
    public Transform GetSkillSpawnPoint()
    {
        return skillSpawnPoint;
    }
    // 지속 피해에는 애니메이션 트리거 없음
    public virtual void Die()
    {
        if (!isDie)
        {
           Debug.Log(monsterClass.MONSTERNAME);
            Debug.Log("쭉었다");
            monsterClass.Die();
            isDie = true;
            // 몬스터 타입에 따라 업적 진행도 증가
            if (monsterClass is BossMonster)
            {
                AchievementManager.Instance.IncrementAchievement(1004); // 보스 업적 ID
               
            }
            else if (monsterClass is EliteMonster)
            {
                AchievementManager.Instance.IncrementAchievement(1003); // 엘리트 업적 ID
            }
            else
            {
                AchievementManager.Instance.IncrementAchievement(1001); // 일반 몬스터 업적 ID
                AchievementManager.Instance.IncrementAchievement(1002); // 일반 업적 ID
            }
            if (SaveManager.Instance != null)
            {
                var playerData = SaveManager.Instance.GetPlayerData();

                if (monsterClass is BossMonster)
                {
                    playerData.bossKillCount++; 
                }
                else if (monsterClass is EliteMonster)
                {
                    playerData.eliteMonsterKillCount++;
                }
                else
                {
                    playerData.normalMonsterKillCount++;
                }

                SaveManager.Instance.SavePlayerData();
            }
            Debug.Log(monsterClass.IsAlive);
            if (ItemDropSystem.Instance != null)
            {
                // 몬스터 데이터 확인
                ICreatureData monData = monsterClass.GetMonsterData();
                Debug.Log($"[Monster] Die - 몬스터 ID: {monData.MonsterID}, 드롭 아이템: {monData.dropItem}, 드롭 확률: {monData.dropChance}");

                // 드롭 테이블 확인
                if (DropTableManager.Instance != null)
                {
                    var dropTable = DropTableManager.Instance.GetMonsterDropTable(monData.MonsterID);
                    Debug.Log($"[Monster] 드롭 테이블 검색 결과: {monsterClass.MONSTERNAME}{(dropTable != null ? dropTable.Count + "개 항목" : "없음")}");
                }
                else
                {
                    Debug.LogError("[Monster] DropTableManager 인스턴스가 null입니다!");
                }

                // 아이템 드롭 실행
                ItemDropSystem.Instance.DropItemFromMonster(monData, transform.position);
                DungeonManager.Instance.OnMonsterDefeated(this);
            }
            if (simpleDissolveContorller != null)
            {
                simpleDissolveContorller.OnMonsterDeath();
            }
            /*Destroy(gameObject);*/ // 몬스터 오브젝트 삭제 => 디졸브에서 관리
        }

    }
    public IMonsterClass GetMonsterClass()
    {
        return monsterClass;
    }

    public void ModifyHealth(int modifier)
    {
        monsterClass.ModifyStats(healthAmount: modifier);
        if (uiManager != null)
        {
            uiManager.UpdateHealthUI(monsterClass.CurrentHealth);
        }
    }
    public void ModifyMaxHealth(int modifier)
    {
        monsterClass.ModifyStats(maxHealthAmount: modifier);
        if (uiManager != null)
        {
            uiManager.UpdateHealthUI(monsterClass.MaxHealth);
        }
    }
    public void ModifyDefense(int modifier)
    {
        monsterClass.ModifyStats(defenseAmount: modifier);
      
    }

    public void ModifyAttackPower(int modifier)
    {
        monsterClass.ModifyStats(attackAmount: modifier);
       
    }

    public void ModifyAttackSpeed(float modifier)
    {
        monsterClass.ModifyStats(attackSpeedAmount: modifier);
        // 애니메이터가 있다면 애니메이션 속도도 조정
        //Animator animator = GetComponent<Animator>();
        //if (animator != null)
        //{
        //    animator.speed = monsterClass.CurrentAttackSpeed;
        //}
    }

    public void ModifySpeed(int modifier)
    {
        monsterClass.ModifyStats(speedAmount: modifier);
       
    }

    public void ModifySkillCooldown(float modifier)
    {
        monsterClass.ModifyStats(skillCooldownAmount: modifier);
    }

    public void ModifyAreaRadius(float modifier)
    {
        monsterClass.ModifyStats(areaRadiusAmount: modifier);
    }

    public void ModifyBuffValue(float modifier)
    {
        monsterClass.ModifyStats(buffValueAmount: modifier);
    }

    public void ModifySkillRange(float modifier)
    {
        monsterClass.ModifyStats(skillRangeAmount: modifier);
    }

    public void ModifyAttackRange(float modifier)
    {
        monsterClass.ModifyStats(attackRangeAmount: modifier);
    }
    public void ModifyArmor(int modifier)
    {
        monsterClass.ModifyStats(armorAmount: modifier);
        if (uiManager != null)
        {
            uiManager.UpdateArmorUI(monsterClass.CurrentArmor);
        }
    }

    // 일시적인 버프/디버프 적용을 위한 메서드
    public void ApplyTemporaryModifier(string statType, float modifier, float duration)
    {
        StartCoroutine(TemporaryModifierCoroutine(statType, modifier, duration));
    }

    private IEnumerator TemporaryModifierCoroutine(string statType, float modifier, float duration)
    {
        // 스탯 증가
        ApplyModifier(statType, modifier);

        // 지정된 시간만큼 대기
        yield return new WaitForSeconds(duration);

        // 스탯 복구
        ApplyModifier(statType, -modifier);
    }

    private void ApplyModifier(string statType, float modifier)
    {
        switch (statType.ToLower())
        {
            case "health":
                ModifyHealth((int)modifier);
                break;
            case "defense":
                ModifyDefense((int)modifier);
                break;
            case "attackpower":
                ModifyAttackPower((int)modifier);
                break;
            case "attackspeed":
                ModifyAttackSpeed(-modifier);
                break;
            case "speed":
                ModifySpeed((int)modifier);
                break;
            case "skillrange":
                ModifySkillRange(modifier);
                break;
            case "attackrange":
                ModifyAttackRange(modifier);
                break;
            default:
                Debug.LogWarning($"Unknown stat type: {statType}");
                break;
        }
    }

    // 버프 효과 적용을 위한 편의 메서드
    public void ApplyBuff(BuffType buffType, float value, float duration)
    {
        switch (buffType)
        {
            case BuffType.AttackUp:
                ApplyTemporaryModifier("attackpower", value, duration);
                break;
            case BuffType.DefenseUp:
                ApplyTemporaryModifier("defense", value, duration);
                break;
            case BuffType.SpeedUp:
                ApplyTemporaryModifier("speed", value, duration);
                break;
            case BuffType.AttackSpeedUp:
                ApplyTemporaryModifier("attackspeed", value, duration);
                break;
                // 다른 버프 타입들에 대한 처리 추가
        }

        // 버프 이펙트 표시
        //if (uiManager != null)
        //{
        //    uiManager.ShowBuffEffect(buffType);
        //}
    }

    protected virtual void OnDestroy()
    {

    }

  

    IMonsterClass ICreatureStatus.GetMonsterClass()
    {
        return monsterClass;
    }

    public virtual DamageType GetDamageType()
    {
        Debug.Log("몬스터데미지타입호출");
        return DamageType.Monster;
    }

    public Transform GetMonsterTransform()
    {
        Debug.Log("호출!@!@!@");
        return gameObject.transform;
    }
}
