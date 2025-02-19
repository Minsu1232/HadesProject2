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
        // 직선 이동
        projectileTransform.Translate(Vector3.forward * speed * Time.deltaTime);
        // 지정된 축으로 회전
        projectileTransform.Rotate(rotationAxis, rotationSpeed * Time.deltaTime);
    }
}