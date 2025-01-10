// 프로젝타일이 가질 수 있는 효과들의 인터페이스
using UnityEngine;

public interface IProjectileImpact
{
    void OnImpact(Vector3 impactPosition, float damage);
}