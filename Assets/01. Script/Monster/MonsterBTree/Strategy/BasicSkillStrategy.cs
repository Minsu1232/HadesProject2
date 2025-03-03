using UnityEngine;

public class BasicSkillStrategy : ISkillStrategy
{
    private ISkillEffect skillEffect;
    private ICreatureStatus monsterStatus;
    private bool isUsingSkill = false;
    private bool skillComplete;
    private float lastSkillTime;
    private float skillCoolTime;
    private float skillDuration;    // ��ų ���/���� �ð�
    private float skillTimer;
    private bool hasExecutedSkill = false;  // ��ų�� ������ ����Ǿ����� üũ
    public float SkillRange { get; set; }
    public bool IsSkillComplete => skillComplete;
    public bool IsUsingSkill => isUsingSkill;
    public float GetLastSkillTime => lastSkillTime;
    public Transform Target { get; set; } // Ÿ�� �Ӽ� ����
    public BasicSkillStrategy(CreatureAI owner)
    {
        monsterStatus = owner.GetStatus();
        Target = null;
    }

    public void Initialize(ISkillEffect effect)
    {
        this.skillEffect = effect;
        Debug.Log("Initialize ȣ���: " + new System.Diagnostics.StackTrace());
    }


    public void StartSkill(Transform transform, Transform target, IMonsterClass monsterData)
    {// StartSkill �޼��� ���� �κп�
        Debug.Log($"StartSkill: ����={monsterData.MONSTERNAME}, ��ų��={monsterData.CurrentSkillCooldown}, ��ų���ӽð�={monsterData.CurrentSKillDuration}");
        Debug.Log("!!!!!!!!!!!" + "�ä�������");
        skillCoolTime = monsterData.CurrentSkillCooldown;
        skillDuration = monsterData.CurrentSKillDuration;
        skillEffect.Initialize(monsterStatus, target);

        // ��ų ���� ���¸� ����
        isUsingSkill = true;
        skillComplete = false;
        hasExecutedSkill = false;
        skillTimer = 0f;
        lastSkillTime = Time.time;
       
    }

    public void UpdateSkill(Transform transform, Transform target, IMonsterClass monsterData)
    {
        Debug.Log("@@@@@#@#!" + "������?");
        if (!isUsingSkill) return;
        Debug.Log("@@@@@#@#!" + "������!");
        skillTimer += Time.deltaTime;
        // UpdateSkill �޼��� �ȿ��� ���� ������
        Debug.Log($"���� �˻�: skillTimer={skillTimer}, skillDuration={skillDuration}, hasExecutedSkill={hasExecutedSkill}");
        // ��ų ���� �ð��� �Ǿ���, ���� ������� �ʾҴٸ� ����
        if (skillTimer >= skillDuration && !hasExecutedSkill)
        {
            Debug.Log("�������");
            skillEffect.Execute();  // ���� ��ų ����
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
        Debug.Log("�������22222222");
        isUsingSkill = false;
        skillComplete = true;
        hasExecutedSkill = false;
        skillTimer = 0;
    }

    public bool CanUseSkill(float distanceToTarget, IMonsterClass monsterData)
    {
        return !isUsingSkill &&
               Time.time > lastSkillTime + skillCoolTime &&
               distanceToTarget <= SkillRange;
    }

}