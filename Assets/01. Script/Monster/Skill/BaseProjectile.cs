// 기본 프로젝타일 클래스
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
    public float heightFactor;
    public float damage;
    public bool isLaunched = false;

    public IProjectileMovement moveStrategy;
    public IProjectileImpact impactEffect;

    public virtual void Initialize(Vector3 startPos, Transform target, float speed, float damage,
        IProjectileMovement moveStrategy, IProjectileImpact impactEffect, GameObject hitEffect,float heightFactor)
    {
        this.startPos = startPos;
        this.target = target;
        this.targetPosition = target.position; // 목표 위치 저장
        this.speed = speed;
        this.damage = damage;
        this.moveStrategy = moveStrategy;
        this.impactEffect = impactEffect;
        this.hitEffect = hitEffect;
        this.heightFactor = heightFactor;
        transform.position = startPos;
        SetInitialDirection(startPos);

        isInitialized = false;
        elapsedTime = 0;

        Destroy(gameObject,10f);
        
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
       
        if (!isLaunched || moveStrategy == null) 
        {           
            return;
        }
       
        moveStrategy.Move(transform, target, speed);
       
    }

    protected abstract void OnImpact(Collider other);
}
