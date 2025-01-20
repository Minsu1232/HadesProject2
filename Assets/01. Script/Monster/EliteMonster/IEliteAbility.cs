using UnityEngine;
using static AttackData;

public interface IEliteAbility
{
    string AbilityName { get; }
    string Description { get; }

    void ApplyAbility(ICreatureStatus creatureStatus);  // Ư�� ����� �ߵ�
    void OnAttack(ICreatureStatus creatureStatus);      // ���ݽ� �ߵ�
    void OnHit(ICreatureStatus creatureStatus, int damage, AttackType attackType);  // �ǰݽ� �ߵ�
    void OnUpdate(ICreatureStatus creatureStatus);      // �� ������ üũ�� �͵�
}