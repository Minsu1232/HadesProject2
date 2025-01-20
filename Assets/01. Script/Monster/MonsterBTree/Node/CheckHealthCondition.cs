public class CheckHealthCondition : BTNode
{
    public CheckHealthCondition(CreatureAI owner) : base(owner)
    {
    }

    public override NodeStatus Execute()
    {
        IMonsterClass monster = owner.GetStatus().GetMonsterClass();
        ICreatureData data = monster.GetMonsterData();

        // 이 몬스터가 체력 체크를 사용하지 않으면 바로 실패
        if (!data.useHealthRetreat)
            return NodeStatus.Failure;

        float healthPercentage = (float)monster.CurrentHealth / data.initialHp;

        if (healthPercentage <= data.healthRetreatThreshold)
        {
            // 페이즈 전환용이면 다른 상태로 전환하는 등의 처리 가능
            if (data.isPhaseChange)
            {
                // 페이즈 전환 관련 처리
            }
            return NodeStatus.Success;
        }

        return NodeStatus.Failure;
    }
}