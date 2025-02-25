//using System.Collections.Generic;
//using UnityEngine;

///// <summary>
///// ���� ��ų �������� ��� �����̳�
///// �������� ��Ÿ�� ������ �Բ� �켱���� ��� ������ �����մϴ�
///// </summary>
//public class BossMultiSkillStrategy : ISkillStrategy
//{
//    private List<ISkillStrategy> strategies = new List<ISkillStrategy>();
//    private Dictionary<ISkillStrategy, float> weights = new Dictionary<ISkillStrategy, float>();
//    private ISkillStrategy currentStrategy;
//    private readonly BasicSkillStrategy defaultStrategy;

//    // ���� Ÿ�̸� - BossMultiSkillStrategy ��ü ��Ÿ�� ������
//    private float unifiedLastSkillTime;

//    // ��ų ���� - �⺻��
//    private float skillRange = 10f;

//    public event System.Action OnSkillStateChanged;  // ���� ���� �̺�Ʈ �߰�

//    public BossMultiSkillStrategy(CreatureAI owner)
//    {
//        defaultStrategy = new BasicSkillStrategy(owner);
//        unifiedLastSkillTime = 0f;
//    }

//    public bool IsSkillComplete => currentStrategy?.IsSkillComplete ?? true;
//    public bool IsUsingSkill => currentStrategy?.IsUsingSkill ?? false;

//    // ���� ��ų ��Ÿ�� - SkillState���� �� ���� �������� ��ų ��� ���� ���θ� ����
//    public float GetLastSkillTime => unifiedLastSkillTime;

//    public float SkillRange
//    {
//        get => skillRange;
//        set
//        {
//            skillRange = value;
//            // ��� ������ ���� ����
//            foreach (var strategy in strategies)
//            {
//                strategy.SkillRange = value;
//            }
//        }
//    }


//    // ��ų �߰� �޼��� 
//    public void AddSkillStrategy(
//        SkillStrategyType strategyType,   // ��ų ���� Ÿ�� (��Ƽ��, ���� ��)
//        SkillEffectType effectType,       // ��ų ����Ʈ Ÿ��
//        ProjectileMovementType moveType,  // �߻�ü ������ Ÿ��
//        ProjectileImpactType impactType,  // �߻�ü �浹 ȿ�� Ÿ��
//        float weight,                     // ����ġ
//        CreatureAI owner,
//        ICreatureData data,
//        BuffData buffData = null)        // ���� ������ (null�̸� �⺻�� ���)
//    {
//        // �� ���� ��� ����
//        ISkillEffect skillEffect = StrategyFactory.CreateSkillEffect(effectType, data, owner);
//        IProjectileMovement moveStrategy = StrategyFactory.CreateProjectileMovement(moveType, data);
//        IProjectileImpact impactEffect = StrategyFactory.CreateProjectileImpact(impactType, data);

//        // ���� ��ų�� ��� BuffData ����
//        if (strategyType == SkillStrategyType.Buff && buffData != null)
//        {
//            // data�� buffData ���� (�ӽ� ��ü ���� �Ǵ� ���� ������ ������Ʈ)
//            if (data is BossData bossData)
//            {
//                bossData.buffData = buffData;
//            }
//        }

//        // ��ų ���� ����
//        ISkillStrategy skillStrategy = StrategyFactory.CreateSkillStrategy(strategyType, owner);

//        // ���� ��� ����
//        if (skillStrategy is ISkillStrategyComponentInjection injectionStrategy)
//        {
//            injectionStrategy.SetSkillEffect(skillEffect);
//            injectionStrategy.SetProjectileMovement(moveStrategy);
//            injectionStrategy.SetProjectileImpact(impactEffect);
//        }

//        // ������ ����ġ ����
//        strategies.Add(skillStrategy);
//        weights[skillStrategy] = weight;
//    }

//    public void Initialize(ISkillEffect skillEffect)
//    {
//        // ��� ������ ����Ʈ �ʱ�ȭ
//        foreach (var strategy in strategies)
//        {
//            strategy.Initialize(skillEffect);
//        }

//        // �⺻ �������� �ʱ�ȭ
//        defaultStrategy.Initialize(skillEffect);
//    }

//    public void StartSkill(Transform transform, Transform target, IMonsterClass monsterData)
//    {
//        if (currentStrategy == null || !currentStrategy.IsUsingSkill)
//        {
//            currentStrategy = SelectStrategy(Vector3.Distance(transform.position, target.position), monsterData);
//            Debug.Log($"���õ� ��ų ����: {currentStrategy.GetType().Name}");
//        }

//        if (currentStrategy != null)
//        {
//            currentStrategy.StartSkill(transform, target, monsterData);
//        }
//    }

//    public void UpdateSkill(Transform transform, Transform target, IMonsterClass monsterData)
//    {
//        if (currentStrategy != null && currentStrategy.IsUsingSkill)
//        {
//            currentStrategy.UpdateSkill(transform, target, monsterData);

//            // ��ų�� �Ϸ�Ǹ� ���� Ÿ�̸� ������Ʈ �� ���� ���� �ʱ�ȭ
//            if (currentStrategy.IsSkillComplete)
//            {
//                unifiedLastSkillTime = Time.time;  // ��ų ��� �� ���� ��Ÿ�� ������Ʈ
//                currentStrategy = null;
//                OnSkillStateChanged?.Invoke();
//            }
//        }
//    }

//    public bool CanUseSkill(float distanceToTarget, IMonsterClass monsterData)
//    {
//        // ���յ� ��Ÿ�� üũ
//        if (Time.time < unifiedLastSkillTime + monsterData.CurrentSkillCooldown)
//            return false;

//        // ���� ��ų ��� ���̸� �Ұ���
//        if (currentStrategy != null && currentStrategy.IsUsingSkill)
//            return false;

//        // �켱���� ������� ��� ������ ù ��° ��ų ã��
//        foreach (var strategy in strategies)
//        {
//            if (strategy.CanUseSkill(distanceToTarget, monsterData))
//                return true;
//        }

//        return false;
//    }

//    private ISkillStrategy SelectStrategy(float distanceToTarget, IMonsterClass monsterData)
//    {
//        // �켱���� ������� ��� ������ ù ��° ��ų ����
//        foreach (var strategy in strategies)
//        {
//            if (strategy.CanUseSkill(distanceToTarget, monsterData))
//                return strategy;
//        }

//        return defaultStrategy;
//    }

//    // �ʿ��� ��� ��ų �ߴ� �޼���
//    public void StopSkill()
//    {
//        if (currentStrategy != null && currentStrategy.IsUsingSkill)
//        {
//            // �����Ǿ� �ִٸ� ȣ��
//            if (currentStrategy is SkillState skillState)
//            {
//                skillState.Exit(); // ���� - ���� ������ �°� ���� �ʿ�
//            }
//            currentStrategy = null;
//        }
//    }

//    /// <summary>
//    /// ������ ��ȯ �� �� ������ ���� ��ų�� �ٷ� ������� �ʵ��� unifiedLastSkillTime�� ���� �ð����� �缳��
//    /// </summary>
//    public void ResetTimer(float bufferTime = 1f)
//    {
//        unifiedLastSkillTime = Time.time + bufferTime;
//    }

//    /// <summary>
//    /// ���� ���� ����Ʈ�� ����, Ÿ�̸ӿ� ���۸� ��� �ʱ�ȭ�մϴ�.
//    /// </summary>
//    public void ResetAll()
//    {
//        // ���� ���� ����Ʈ�� ����ġ ������ ��� ���ϴ�.
//        strategies.Clear();
//        weights.Clear();
//        currentStrategy = null;

//        // Ÿ�̸Ӹ� ���� �ð����� �ʱ�ȭ�մϴ�.
//        unifiedLastSkillTime = Time.time;

//        Debug.Log("ResetAll: ��ų ���� ��� �ʱ�ȭ �� Ÿ�̸� �缳�� �Ϸ�");
//    }
//}