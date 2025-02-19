using UnityEngine;

public class StraightRotationMovement : IProjectileMovement
{
    private float rotationSpeed;
    private Vector3 rotationAxis;

    public StraightRotationMovement(Vector3 rotationAxis, float rotationSpeed = 360f)
    {
        this.rotationSpeed = rotationSpeed;
        this.rotationAxis = rotationAxis.normalized;
    }

    public void Move(Transform projectileTransform, Transform target, float speed)
    {
        // ���� �̵�
        projectileTransform.Translate(Vector3.forward * speed * Time.deltaTime);
        // ������ ������ ȸ��
        projectileTransform.Rotate(rotationAxis, rotationSpeed * Time.deltaTime);
    }
}