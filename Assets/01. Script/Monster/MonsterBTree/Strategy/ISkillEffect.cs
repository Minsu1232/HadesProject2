using UnityEngine;

public interface ISkillEffect
{
    void Initialize(MonsterStatus status, Transform target);
    void Execute();
    void OnComplete();
  
}