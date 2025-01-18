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
        uiManager = GetComponent<MonsterUIManager>(); // ���� �� �� ���� ��������
       
        skillSpawnPoint = GetComponentsInChildren<Transform>()
          .FirstOrDefault(t => t.name == "SkillSpawnPoint");

        if (skillSpawnPoint == null)
        {
            Debug.LogWarning($"{gameObject.name}���� SkillSpawnPoint�� ã�� ���߽��ϴ�. ���� ��ġ�� ����մϴ�.");
            skillSpawnPoint = transform;
        }
    }

    public virtual void Initialize(MonsterClass monster)
    {
        monsterClass = monster;
        
        // AI Ÿ�Կ� ���� ������ AI ������Ʈ �߰�
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
        //Debug.Log($"�±��� ü�� �Ƹ� {monsterClass.CurrentHealth + damage},  ���� �� ü�� {monsterClass.CurrentHealth}");

        // ĳ�̵� uiManager ���
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
    // ���� ���ؿ��� �ִϸ��̼� Ʈ���� ����
    public virtual void Die()
    {
        if (!isDie)
        {
            isDie = true;
            Destroy(gameObject); // ���� ������Ʈ ����
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
        // �ִϸ����Ͱ� �ִٸ� �ִϸ��̼� �ӵ��� ����
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

    // �Ͻ����� ����/����� ������ ���� �޼���
    public void ApplyTemporaryModifier(string statType, float modifier, float duration)
    {
        StartCoroutine(TemporaryModifierCoroutine(statType, modifier, duration));
    }

    private IEnumerator TemporaryModifierCoroutine(string statType, float modifier, float duration)
    {
        // ���� ����
        ApplyModifier(statType, modifier);

        // ������ �ð���ŭ ���
        yield return new WaitForSeconds(duration);

        // ���� ����
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

    // ���� ȿ�� ������ ���� ���� �޼���
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
                // �ٸ� ���� Ÿ�Ե鿡 ���� ó�� �߰�
        }

        // ���� ����Ʈ ǥ��
        //if (uiManager != null)
        //{
        //    uiManager.ShowBuffEffect(buffType);
        //}
    }

    protected virtual void OnDestroy()
    {

    }
}
