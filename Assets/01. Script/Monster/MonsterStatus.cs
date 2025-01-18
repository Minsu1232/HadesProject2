using RPGCharacterAnims.Lookups;
using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Unity.VisualScripting;
using UnityEngine;
using static AttackData;
using static IMonsterState;

public class MonsterStatus : MonoBehaviour,IDamageable
{
    [SerializeField] CreatureAI creatureAI;
    [SerializeField] private Transform skillSpawnPoint;
    private MonsterUIManager uiManager;
    private bool isDie = false;
    [FoldoutGroup("Monster Stats")]
    [ReadOnly]
    [ShowInInspector]
    protected MonsterClass monsterClass;

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
       
        skillSpawnPoint = GetComponentsInChildren<Transform>()
          .FirstOrDefault(t => t.name == "SkillSpawnPoint");

        if (skillSpawnPoint == null)
        {
            Debug.LogWarning($"{gameObject.name}에서 SkillSpawnPoint를 찾지 못했습니다. 몬스터 위치를 사용합니다.");
            skillSpawnPoint = transform;
        }
    }

    public virtual void Initialize(MonsterClass monster)
    {
        monsterClass = monster;
        
        // AI 타입에 따라 적절한 AI 컴포넌트 추가
        creatureAI = /*monster is BossMonster ?*/
            //gameObject.AddComponent<BossAI>() :
            gameObject.AddComponent<BasicCreatureAI>();
    }
    public AttackType GetAttackType()
    {
        return monsterClass.IsBasicAttack() ? AttackType.Normal : AttackType.Charge;
    }
    public virtual void TakeDamage(int damage, AttackType attackType)
    {
        if (isDie) return;
        Debug.Log($"{monsterClass.CurrentArmor}");
        monsterClass.TakeDamage(damage, GetAttackType());
        Debug.Log($"{monsterClass.CurrentArmor}");
        //Debug.Log($"맞기전 체력 아머 {monsterClass.CurrentHealth + damage},  맞은 후 체력 {monsterClass.CurrentHealth}");

        // 캐싱된 uiManager 사용
        if (uiManager != null)
        {
            uiManager.UpdateHealthUI(monsterClass.CurrentHealth);
            uiManager.UpdateArmorUI(monsterClass.CurrentArmor);
            uiManager.SpawnDamageText(damage, attackType);
        }

        var CreatureAI = GetComponent<CreatureAI>();
        if (CreatureAI != null)
        {
            CreatureAI.OnDamaged(damage, attackType);
        }

        if (monsterClass.CurrentHealth <= 0 && gameObject != null)
        {
            Die();
        }
    }
    public void TakeDotDamage(int dotDamage)
    {
        if (!isDie)
        {
            monsterClass.TakeDotDamage(dotDamage);

            if (monsterClass.CurrentHealth <= 0)
            {    
                Die();
            }
        }

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
            isDie = true;
            Destroy(gameObject); // 몬스터 오브젝트 삭제
        }

    }
    public MonsterClass GetMonsterClass()
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
                ModifyAttackSpeed(modifier);
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
}
