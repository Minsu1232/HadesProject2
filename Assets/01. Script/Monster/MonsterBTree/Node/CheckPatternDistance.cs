using UnityEngine;
using static IMonsterState;

public class CheckPatternDistance : BTNode
{
    public CheckPatternDistance(CreatureAI owner) : base(owner) { }

    public override NodeStatus Execute()
    {
        var monster = owner.GetStatus().GetMonsterClass();
        float distanceToPlayer = Vector3.Distance(
            owner.transform.position,
            monster.GetPlayerPosition()
        );

        // �Ÿ��� �ʹ� �־����ٸ�
        if (distanceToPlayer > monster.CurrentAttackRange * 6f) 
        {
            Debug.Log("���п�");
            // ���� �ߴ��ϰ� �̵� ���·�
            owner.ChangeState(MonsterStateType.Move);
            return NodeStatus.Failure;
        }

        return NodeStatus.Success;
    }
}