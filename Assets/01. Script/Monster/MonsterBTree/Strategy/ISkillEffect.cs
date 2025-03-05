using System;
using UnityEngine;

public interface ISkillEffect
{
    void Initialize(ICreatureStatus status, Transform target);
    void Execute();
    void OnComplete();

    // 새로 추가할 이벤트
    event Action OnEffectCompleted;


}