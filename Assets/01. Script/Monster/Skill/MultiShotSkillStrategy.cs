using UnityEngine;

public class MultiShotSkillStrategy : ISkillStrategy
{
    private ISkillEffect skillEffect;
    private ICreatureStatus monsterStatus;
    private bool isUsingSkill = false;
    private float lastSkillTime;
    private float skillCoolTime;

    // �߰�: �߻� Ƚ�� ���� ����
    private int shotsFired = 0;
    private int maxShots;

    public float SkillRange { get; set; }
    public bool IsSkillComplete => !isUsingSkill;  // ��ų ���� ���̸� false
    public bool IsUsingSkill => isUsingSkill;
    public float GetLastSkillTime => lastSkillTime;

    public MultiShotSkillStrategy(CreatureAI owner)
    {
        monsterStatus = owner.GetStatus();
    }

    public void Initialize(ISkillEffect effect)
    {
        this.skillEffect = effect;
    }

    public void StartSkill(Transform transform, Transform target, IMonsterClass monsterData)
    {
        maxShots = monsterData.CurrentMultiShotCount;
        skillCoolTime = monsterData.CurrentSkillCooldown;
        skillEffect.Initialize(monsterStatus, target);
        isUsingSkill = true;
        shotsFired = 0;  // �ʱ�ȭ
        lastSkillTime = Time.time;
    }

    public void UpdateSkill(Transform transform, Transform target, IMonsterClass monsterData)
    {
        if (!isUsingSkill) return;

        // ��ų ȿ�� ���� (��: �Ѿ� ���� ��)
        skillEffect.Execute();
        shotsFired++;

        // �ִ� �߻� Ƚ���� �����ϸ� ��ų ����
        if (shotsFired >= maxShots)
        {
            Debug.Log("�Ϸ�Ǿ����");
            CompleteSkill();
        }
    }

    private void CompleteSkill()
    {
        isUsingSkill = false;
    }

    public bool CanUseSkill(float distanceToTarget, IMonsterClass monsterData)
    {
        return !isUsingSkill &&
               Time.time > lastSkillTime + skillCoolTime &&
               distanceToTarget <= SkillRange;
    }
}
