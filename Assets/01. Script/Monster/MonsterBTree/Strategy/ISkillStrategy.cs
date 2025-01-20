using UnityEngine;

public interface ISkillStrategy
{
    void Initialize(ISkillEffect skillEffect);
    void StartSkill(Transform transform, Transform target, IMonsterClass monsterData);
    void UpdateSkill(Transform transform, Transform target, IMonsterClass monsterData);
    bool IsSkillComplete { get; }
    bool CanUseSkill(float distanceToTarget, IMonsterClass monsterData);
    bool IsUsingSkill { get; }
    float GetLastSkillTime { get; }

    float SkillRange { get; set; }
}