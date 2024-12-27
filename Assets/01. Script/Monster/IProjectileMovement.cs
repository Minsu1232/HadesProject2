// 발사체 이동 전략 인터페이스
using UnityEngine;

public interface IProjectileMovement
{
    void Move(Transform projectileTransform, Transform target, float speed);
}