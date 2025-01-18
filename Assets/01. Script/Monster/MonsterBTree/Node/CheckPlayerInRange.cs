using UnityEngine;

public class CheckPlayerInRange : BTNode
{
    public CheckPlayerInRange(CreatureAI owner) : base(owner) { }

    public override NodeStatus Execute()
    {
        MonsterClass monster = owner.GetStatus().GetMonsterClass();
        float distanceToPlayer = Vector3.Distance(
            owner.transform.position,
            owner.GetStatus().GetMonsterClass().GetPlayerPosition()
        );

        // ���� ���� �ȿ� �ִ��� üũ
        if (distanceToPlayer <= monster.CurrentChaseRange)
        {
            return NodeStatus.Success;
        }

        return NodeStatus.Failure;
    }
}