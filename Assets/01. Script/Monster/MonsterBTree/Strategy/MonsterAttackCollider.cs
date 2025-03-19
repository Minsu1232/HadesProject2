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
            
            canDealDamage = false;  // 한 번만 데미지
            IAttackStrategy strategy = CreatureAI.GetAttackStrategy();
           
            // 공격자 정보(MonoBehaviour)를 함께 전달
            // 몬스터 자신(this.gameObject의 MonoBehaviour 컴포넌트)을 전달
           

            // IAttackStrategy 인터페이스 확장이 필요 (공격자 정보 매개변수 추가)
            // strategy.ApplyDamage(player, monsterClass, attackerMonster);

            // 또는 IDamageable 인터페이스의 구현체인 PlayerClass로 캐스팅하여 직접 호출
            if (player is PlayerClass playerClass)
            {
                ICreatureStatus monsterStatus = CreatureAI.creatureStatus;
                int damage = (int)strategy.GetAttackPowerMultiplier() * monsterClass.CurrentAttackPower;
                strategy.ApplyDamage(player, monsterClass);
                playerClass.PlayerGetAttacker(damage, monsterStatus);
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
        else
        {
            strategy.StopAttack();
            strategy.ResetAttackTime();
           
        }
    }
}