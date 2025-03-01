using UnityEngine;

public class ParabolicMovement : IProjectileMovement
{
    private float gravity = 9.8f;

    public void Move(Transform projectile, Transform target, float speed)
    {
        BaseProjectile baseProjectile = projectile.GetComponent<BaseProjectile>();
        if (!baseProjectile.isInitialized)
        {
            // 초기화 단계에서 타겟 위치를 저장하고 초기 속도 설정
            InitializeProjectile(projectile, target.position, speed);
            baseProjectile.isInitialized = true;
        }

        // 시간 업데이트
        baseProjectile.elapsedTime += Time.deltaTime;

        // 포물선 운동 방정식에 따른 위치 계산
        Vector3 newPosition = baseProjectile.startPos +
                              baseProjectile.initialVelocity * baseProjectile.elapsedTime +
                              0.5f * new Vector3(0, -gravity, 0) * baseProjectile.elapsedTime * baseProjectile.elapsedTime;

        // 현재 방향 계산
        Vector3 moveDirection = (newPosition - projectile.position).normalized;
        projectile.position = newPosition;

        // 방향 설정
        if (moveDirection != Vector3.zero)
        {
            projectile.forward = moveDirection;
        }
        Debug.Log("포물선");
    }

    private void InitializeProjectile(Transform projectile, Vector3 targetPos, float speed)
    {
        BaseProjectile baseProjectile = projectile.GetComponent<BaseProjectile>();

        // 거리 계산
        Vector3 toTarget = targetPos - baseProjectile.startPos;
        float distanceXZ = new Vector3(toTarget.x, 0, toTarget.z).magnitude;

        // 초기 속도 계산 (직선 거리의 2배 정도 높이까지 올라가는 포물선)
        float timeToTarget = distanceXZ / speed;
        float heightY = Mathf.Max(3f, distanceXZ * 0.4f); // 거리에 비례한 높이

        // 수평 방향 속도
        Vector3 horizontalDir = new Vector3(toTarget.x, 0, toTarget.z).normalized;
        Vector3 horizontalVelocity = horizontalDir * speed;

        // 수직 방향 속도 (타겟 높이 차이 + 포물선 높이 고려)
        float verticalVelocity = (toTarget.y / timeToTarget) + (gravity * timeToTarget / 2);

        // 타겟에 정확히 도달하기 위해 초기 속도 보정
        baseProjectile.initialVelocity = horizontalVelocity;
        baseProjectile.initialVelocity.y = verticalVelocity + heightY; // 더 높이 솟아오르게 함

        Debug.Log($"포물선 초기화: 거리={distanceXZ}, 속도={baseProjectile.initialVelocity}");
    }
}