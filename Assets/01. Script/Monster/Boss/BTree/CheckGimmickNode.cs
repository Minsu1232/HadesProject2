// 기믹 체크 노드
using UnityEngine;

public class CheckGimmickNode : BTNode
{
    private BossMonster boss;

    public CheckGimmickNode(CreatureAI owner) : base(owner)
    {
        boss = owner.GetStatus().GetMonsterClass() as BossMonster;
    }

    public override NodeStatus Execute()
    {
        if (boss == null)
        {
       
            return NodeStatus.Failure;
        }

        GimmickData currentGimmick = boss.CurrentPhaseGimmickData;

        if (currentGimmick == null)
        {
          
            return NodeStatus.Failure;
        }

      

        if (!currentGimmick.isEnabled)
        {
          
            return NodeStatus.Failure;
        }

        float healthRatio = (float)boss.CurrentHealth / boss.MaxHealth;
    

        if (healthRatio <= currentGimmick.triggerHealthThreshold)
        {
           
            return NodeStatus.Success;
        }

        
        return NodeStatus.Failure;
    }
}