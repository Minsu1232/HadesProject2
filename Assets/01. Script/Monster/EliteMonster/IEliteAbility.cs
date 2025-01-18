using UnityEngine;
using static AttackData;

public interface IEliteAbility
{
    string AbilityName { get; }
    string Description { get; }
    
    void ApplyAbility(MonsterStatus monsterStatus);  // Ư�� ����� �ߵ�
    void OnAttack(MonsterStatus monsterStatus);      // ���ݽ� �ߵ�
    void OnHit(MonsterStatus monsterStatus, int damage, AttackType attackType);  // �ǰݽ� �ߵ�
    void OnUpdate(MonsterStatus monsterStatus);      // �� ������ üũ�� �͵�
}