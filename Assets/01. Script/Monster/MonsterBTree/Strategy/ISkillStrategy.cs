using UnityEngine;

public interface ISkillStrategy
{
    void Initialize(ISkillEffect skillEffect);
    void StartSkill(Transform transform, Transform target, MonsterClass monsterData);
    void UpdateSkill(Transform transform, Transform target, MonsterClass monsterData);
    bool IsSkillComplete { get; }
    bool CanUseSkill(float distanceToTarget, MonsterClass monsterData);
    bool IsUsingSkill { get; }
    float GetLastSkillTime { get; }

    float SkillRange { get; set; }
}