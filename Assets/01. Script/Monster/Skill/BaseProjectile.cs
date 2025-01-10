// �⺻ ������Ÿ�� Ŭ����
using UnityEngine;

public abstract class BaseProjectile : MonoBehaviour
{
    public Transform target;
    public Vector3 targetPosition;
    public Vector3 startPos;
    public GameObject hitEffect;
    public Vector3 initialVelocity;
    public float elapsedTime = 0;
    public bool isInitialized = false;

    public float speed;
    public float damage;
    public bool isLaunched = false;

    public IProjectileMovement moveStrategy;
    public IProjectileImpact impactEffect;

    public virtual void Initialize(Vector3 startPos, Transform target, float speed, float damage,
        IProjectileMovement moveStrategy, IProjectileImpact impactEffect, GameObject hitEffect)
    {
        this.startPos = startPos;
        this.target = target;
        this.targetPosition = target.position; // ��ǥ ��ġ ����
        this.speed = speed;
        this.damage = damage;
        this.moveStrategy = moveStrategy;
        this.impactEffect = impactEffect;
        this.hitEffect = hitEffect;

        transform.position = startPos;
        SetInitialDirection(startPos);

        isInitialized = false;
        elapsedTime = 0;
    }

    protected virtual void SetInitialDirection(Vector3 startPos)
    {
        Vector3 targetPoint = targetPosition + Vector3.up * 1.5f;
        transform.forward = (targetPoint - startPos).normalized;
    }

    public virtual void Launch()
    {
        isLaunched = true;
    }

    protected virtual void Update()
    {
        if (!isLaunched || moveStrategy == null) return;
        moveStrategy.Move(transform, target, speed);
    }

    protected abstract void OnImpact(Collider other);
}
