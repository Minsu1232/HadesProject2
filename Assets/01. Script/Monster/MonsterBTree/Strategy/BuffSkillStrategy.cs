using UnityEngine;

public class BuffSkillStrategy : ISkillStrategy
{
    private ISkillEffect skillEffect;
    private MonsterStatus monsterStatus;
    private bool isUsingSkill = false;
    private bool skillComplete;
    private float lastSkillTime;
    private float skillCoolTime;
    private float skillDuration;    // ��ų ���/���� �ð�
    private float skillTimer;
    private bool hasExecutedSkill = false;
    
    public float SkillRange { get; set; }
    public bool IsSkillComplete => skillComplete;
    public bool IsUsingSkill => isUsingSkill;
    public float GetLastSkillTime => lastSkillTime;

    public BuffSkillStrategy(CreatureAI owner)
    {
        monsterStatus = owner.GetStatus();
       
    }

    public void Initialize(ISkillEffect effect)
    {
        this.skillEffect = effect;
    }

    public void StartSkill(Transform transform, Transform target, MonsterClass monsterData)
    {
        skillCoolTime = monsterData.CurrentSkillCooldown;
        skillDuration = monsterData.CurrentSKillDuration;
        skillEffect.Initialize(monsterStatus, target);

        isUsingSkill = true;
        skillComplete = false;
        hasExecutedSkill = false;
        skillTimer = 0f;
        lastSkillTime = Time.time;

        //// ���� ��ų ���� ����Ʈ�� �ִϸ��̼��� �ʿ��� ���
        //if (monsterStatus.TryGetComponent<Animator>(out var animator))
        //{
        //    animator.SetTrigger("BuffSkill");
        //}
    }

    public void UpdateSkill(Transform transform, Transform target, MonsterClass monsterData)
    {
        if (!isUsingSkill) return;

        skillTimer += Time.deltaTime;

        // ������ ���� �߿� ���� ��ȯ�� �ʿ��� �� ���� (Ÿ�� ��������)
        if (!hasExecutedSkill)
        {
            Vector3 directionToTarget = (target.position - transform.position).normalized;
            if (directionToTarget != Vector3.zero)
            {
                transform.rotation = Quaternion.Lerp(
                    transform.rotation,
                    Quaternion.LookRotation(directionToTarget),
                    Time.deltaTime * 5f
                );
            }
        }

        // ��ų ���� �ð��� �Ǿ���, ���� ������� �ʾҴٸ� ����
        if (skillTimer >= skillDuration && !hasExecutedSkill)
        {
            skillEffect.Execute();  // ���� ȿ�� ����
            hasExecutedSkill = true;

           
        }

        // ��ų�� ����Ǿ��ٸ� �Ϸ� ó��
        if (hasExecutedSkill)
        {
            CompleteSkill();
        }
    }

    private void CompleteSkill()
    {
        isUsingSkill = false;
        skillComplete = true;
        hasExecutedSkill = false;
        skillTimer = 0;
    }

    public bool CanUseSkill(float distanceToTarget, MonsterClass monsterData)
    {
        // �⺻ ����
        bool basicConditions = !isUsingSkill &&
                             Time.time > lastSkillTime + skillCoolTime &&
                             distanceToTarget <= SkillRange;

        // ���� ��ų Ưȭ ���� (��: HP�� Ư�� ���� ������ ���� ���)
        bool buffConditions = true;
        if (monsterStatus != null)
        {
            var currentHP = monsterStatus.GetMonsterClass().CurrentHealth;
            var maxHP = monsterStatus.GetMonsterClass().GetMonsterData().initialHp;
            buffConditions = (currentHP / (float)maxHP) <= 0.7f; // ü�� 70% ������ ���� ���
        }

        return basicConditions || buffConditions;
    }
}