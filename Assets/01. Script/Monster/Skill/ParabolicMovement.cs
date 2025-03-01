using UnityEngine;

public class ParabolicMovement : IProjectileMovement
{
    private float gravity = 9.8f;

    public void Move(Transform projectile, Transform target, float speed)
    {
        BaseProjectile baseProjectile = projectile.GetComponent<BaseProjectile>();
        if (!baseProjectile.isInitialized)
        {
            // �ʱ�ȭ �ܰ迡�� Ÿ�� ��ġ�� �����ϰ� �ʱ� �ӵ� ����
            InitializeProjectile(projectile, target.position, speed);
            baseProjectile.isInitialized = true;
        }

        // �ð� ������Ʈ
        baseProjectile.elapsedTime += Time.deltaTime;

        // ������ � �����Ŀ� ���� ��ġ ���
        Vector3 newPosition = baseProjectile.startPos +
                              baseProjectile.initialVelocity * baseProjectile.elapsedTime +
                              0.5f * new Vector3(0, -gravity, 0) * baseProjectile.elapsedTime * baseProjectile.elapsedTime;

        // ���� ���� ���
        Vector3 moveDirection = (newPosition - projectile.position).normalized;
        projectile.position = newPosition;

        // ���� ����
        if (moveDirection != Vector3.zero)
        {
            projectile.forward = moveDirection;
        }
        Debug.Log("������");
    }

    private void InitializeProjectile(Transform projectile, Vector3 targetPos, float speed)
    {
        BaseProjectile baseProjectile = projectile.GetComponent<BaseProjectile>();

        // �Ÿ� ���
        Vector3 toTarget = targetPos - baseProjectile.startPos;
        float distanceXZ = new Vector3(toTarget.x, 0, toTarget.z).magnitude;

        // �ʱ� �ӵ� ��� (���� �Ÿ��� 2�� ���� ���̱��� �ö󰡴� ������)
        float timeToTarget = distanceXZ / speed;
        float heightY = Mathf.Max(3f, distanceXZ * 0.4f); // �Ÿ��� ����� ����

        // ���� ���� �ӵ�
        Vector3 horizontalDir = new Vector3(toTarget.x, 0, toTarget.z).normalized;
        Vector3 horizontalVelocity = horizontalDir * speed;

        // ���� ���� �ӵ� (Ÿ�� ���� ���� + ������ ���� ���)
        float verticalVelocity = (toTarget.y / timeToTarget) + (gravity * timeToTarget / 2);

        // Ÿ�ٿ� ��Ȯ�� �����ϱ� ���� �ʱ� �ӵ� ����
        baseProjectile.initialVelocity = horizontalVelocity;
        baseProjectile.initialVelocity.y = verticalVelocity + heightY; // �� ���� �ھƿ����� ��

        Debug.Log($"������ �ʱ�ȭ: �Ÿ�={distanceXZ}, �ӵ�={baseProjectile.initialVelocity}");
    }
}