using UnityEngine;

public interface ISkillEffect
{
    void Initialize(ICreatureStatus status, Transform target);
    void Execute();
    void OnComplete();

  
  
}