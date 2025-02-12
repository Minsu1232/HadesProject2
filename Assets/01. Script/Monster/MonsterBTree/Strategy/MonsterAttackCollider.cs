using UnityEngine;

public class MonsterAttackCollider : MonoBehaviour
{
    private IMonsterClass monsterClass;
    private CreatureAI CreatureAI;
    private bool canDealDamage = false;
    PlayerClass player;

    private void Start()
    {
        CreatureAI = GetComponent<CreatureAI>();
        monsterClass = GetComponentInParent<MonsterStatus>().GetMonsterClass();
        player = GameInitializer.Instance.GetPlayerClass();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!canDealDamage) return;
        if (other.gameObject.CompareTag("Player"))
        {
            if (player != null)
            {
                canDealDamage = false;  // 한 번만 데미지
                IAttackStrategy strategy = CreatureAI.GetAttackStrategy();
                strategy.ApplyDamage(player, monsterClass);
            }
        }
    }

    // 애니메이션 시작 시점에서 호출
    public void EnableDamage()
    {
        canDealDamage = true;
        var strategy = CreatureAI.GetAttackStrategy();
        
    }

    // 애니메이션 종료 시점에서 호출
    public void DisableDamage()
    {
        canDealDamage = false;
        var strategy = CreatureAI.GetAttackStrategy();
        if (strategy is BossMultiAttackStrategy physicalStrategy)
        {
            physicalStrategy.StopAttack();
            physicalStrategy.UpdateLastAttackTime();
            Debug.Log("쳐다보세요" + physicalStrategy.IsAttacking);
        }
    }
}