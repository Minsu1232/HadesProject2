

using UnityEngine;

public interface ICreatureStatus
{
    IMonsterClass GetMonsterClass();
    void Initialize(IMonsterClass monster);

    // Stats Modification 메서드들
    void ModifyHealth(int modifier);
    void ModifyMaxHealth(int modifier);
    void ModifyDefense(int modifier);
    void ModifyAttackPower(int modifier);
    void ModifyAttackSpeed(float modifier);
    void ModifySpeed(int modifier);
    void ModifySkillCooldown(float modifier);
    void ModifyAreaRadius(float modifier);
    void ModifyBuffValue(float modifier);
    void ModifySkillRange(float modifier);
    void ModifyAttackRange(float modifier);
    void ModifyArmor(int modifier);

    // Buff 관련
    void ApplyBuff(BuffType buffType, float value, float duration);
    void ApplyTemporaryModifier(string statType, float modifier, float duration);

    // Utility
    Transform GetSkillSpawnPoint();
}
