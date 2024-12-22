using UnityEngine;

public interface IDetectionStrategy
{
    bool DetectTarget(Transform transform, Transform target, MonsterClass monsterData);
    Vector3 GetTargetPosition(Transform transform, Transform target, MonsterClass monsterData);
}