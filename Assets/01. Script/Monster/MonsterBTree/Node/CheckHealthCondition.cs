public class CheckHealthCondition : BTNode
{
    public CheckHealthCondition(CreatureAI owner) : base(owner)
    {
    }

    public override NodeStatus Execute()
    {
        IMonsterClass monster = owner.GetStatus().GetMonsterClass();
        ICreatureData data = monster.GetMonsterData();

        // �� ���Ͱ� ü�� üũ�� ������� ������ �ٷ� ����
        if (!data.useHealthRetreat)
            return NodeStatus.Failure;

        float healthPercentage = (float)monster.CurrentHealth / data.initialHp;

        if (healthPercentage <= data.healthRetreatThreshold)
        {
            // ������ ��ȯ���̸� �ٸ� ���·� ��ȯ�ϴ� ���� ó�� ����
            if (data.isPhaseChange)
            {
                // ������ ��ȯ ���� ó��
            }
            return NodeStatus.Success;
        }

        return NodeStatus.Failure;
    }
}