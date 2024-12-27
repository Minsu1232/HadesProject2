using UnityEngine;

public class HomingMovement : IProjectileMovement
{
    private float rotationSpeed = 2f;

    public void Move(Transform projectileTransform, Transform target, float speed)
    {
        Vector3 direction = (target.position - projectileTransform.position).normalized;
        Quaternion targetRotation = Quaternion.LookRotation(direction);
        projectileTransform.rotation = Quaternion.Lerp(projectileTransform.rotation, targetRotation, Time.deltaTime * rotationSpeed);
        projectileTransform.Translate(Vector3.forward * speed * Time.deltaTime);
    }
}