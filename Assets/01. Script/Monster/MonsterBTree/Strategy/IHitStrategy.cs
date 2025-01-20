using UnityEngine;

public interface IHitStrategy
{
    void OnHit(Transform transform, IMonsterClass monsterData, int damage);
    bool IsHitComplete { get; }
    void UpdateHit();
}