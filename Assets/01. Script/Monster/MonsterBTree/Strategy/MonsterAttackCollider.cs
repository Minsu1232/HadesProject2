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
            
            canDealDamage = false;  // �� ���� ������
            IAttackStrategy strategy = CreatureAI.GetAttackStrategy();
           
            // ������ ����(MonoBehaviour)�� �Բ� ����
            // ���� �ڽ�(this.gameObject�� MonoBehaviour ������Ʈ)�� ����
           

            // IAttackStrategy �������̽� Ȯ���� �ʿ� (������ ���� �Ű����� �߰�)
            // strategy.ApplyDamage(player, monsterClass, attackerMonster);

            // �Ǵ� IDamageable �������̽��� ����ü�� PlayerClass�� ĳ�����Ͽ� ���� ȣ��
            if (player is PlayerClass playerClass)
            {
                ICreatureStatus monsterStatus = CreatureAI.creatureStatus;
                int damage = (int)strategy.GetAttackPowerMultiplier() * monsterClass.CurrentAttackPower;
                strategy.ApplyDamage(player, monsterClass);
                playerClass.PlayerGetAttacker(damage, monsterStatus);
            }
            
        }
    }

    // �ִϸ��̼� ���� �������� ȣ��
    public void EnableDamage()
    {
        canDealDamage = true;
        var strategy = CreatureAI.GetAttackStrategy();
        
    }

    // �ִϸ��̼� ���� �������� ȣ��
    public void DisableDamage()
    {
        canDealDamage = false;
        var strategy = CreatureAI.GetAttackStrategy();
        if (strategy is BossMultiAttackStrategy physicalStrategy)
        {
            physicalStrategy.StopAttack();
            physicalStrategy.UpdateLastAttackTime();
            Debug.Log("�Ĵٺ�����" + physicalStrategy.IsAttacking);
        }
        else
        {
            strategy.StopAttack();
            strategy.ResetAttackTime();
           
        }
    }
}