using UnityEngine;

public class ParabolicMovement : IProjectileMovement
{
    private float gravity = 9.8f;

    public void Move(Transform projectile, Transform target, float speed)
    {
        BaseProjectile baseProjectile = projectile.GetComponent<BaseProjectile>();
        if (!baseProjectile.isInitialized)
        {
            InitializeProjectile(projectile, target.position, speed);
            baseProjectile.isInitialized = true;
        }

        baseProjectile.elapsedTime += Time.deltaTime;

        Vector3 newPosition = CalculatePosition(
            baseProjectile.startPos,
            baseProjectile.targetPosition,
            baseProjectile.initialVelocity,
            baseProjectile.elapsedTime
        );

        Vector3 moveDirection = (newPosition - projectile.position).normalized;
        projectile.position = newPosition;

        if (moveDirection != Vector3.zero)
        {
            projectile.forward = moveDirection;
        }
    }

    private void InitializeProjectile(Transform projectile, Vector3 targetPos, float speed)
    {
        BaseProjectile baseProjectile = projectile.GetComponent<BaseProjectile>();
        Vector3 direction = targetPos - baseProjectile.startPos;
        float distance = direction.magnitude;
        float height = Mathf.Max(5f, distance * 0.5f);

        Vector3 horizontalDir = direction.normalized;
        horizontalDir.y = 0;

        baseProjectile.initialVelocity = horizontalDir * speed;
        baseProjectile.initialVelocity.y = Mathf.Sqrt(2 * gravity * height);
    }

    private Vector3 CalculatePosition(Vector3 startPos, Vector3 targetPos, Vector3 initialVelocity, float time)
    {
        return startPos + new Vector3(
            initialVelocity.x * time,
            initialVelocity.y * time - 0.5f * gravity * time * time,
            initialVelocity.z * time
        );
    }
}
