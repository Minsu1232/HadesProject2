using UnityEngine;

public interface IHitStrategy
{
    void OnHit(Transform transform, MonsterClass monsterData, int damage);
    bool IsHitComplete { get; }
    void UpdateHit();
}