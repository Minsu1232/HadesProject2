// �߻�ü �̵� ���� �������̽�
using UnityEngine;

public interface IProjectileMovement
{
    void Move(Transform projectileTransform, Transform target, float speed);
}