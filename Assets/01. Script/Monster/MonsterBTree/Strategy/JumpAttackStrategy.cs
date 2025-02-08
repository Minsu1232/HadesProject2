// 새로운 점프 공격 구현
using DG.Tweening;
using Unity.VisualScripting;
using UnityEngine;

public class JumpAttackStrategy : BasePhysicalAttackStrategy
{
    public override PhysicalAttackType AttackType => PhysicalAttackType.Jump;
    public override float GetAttackPowerMultiplier() => 1.5f; // 점프 공격은 50% 더 강력

    private Vector3 jumpStartPosition;
    private bool isJumping;
    private float jumpHeight = 3f;
    private float jumpDuration = 1f;
    private DG.Tweening.Sequence jumpSequence;
    private GameObject shockwaveEffect; // 충격파 이펙트 프리팹
    private float shockwaveRadius;
    public JumpAttackStrategy(GameObject shockwaveEffectPrefab, float shockwaveRadius)
    {
        this.shockwaveEffect = shockwaveEffectPrefab;
        this.shockwaveRadius = shockwaveRadius;
        lastAttackTime = Time.time - 100f;  // 첫 공격이 바로 가능하도록
    }

    public override bool CanAttack(float distanceToTarget, IMonsterClass monsterData)
    {
        float timeCheck = Time.time - lastAttackTime;
        bool isInRange = distanceToTarget <= monsterData.CurrentAttackRange * 10f;
        bool isCooldownReady = Time.time >= lastAttackTime + monsterData.CurrentAttackSpeed;

        //Debug.Log($"[JumpAttack] Distance: {distanceToTarget}, Required: {monsterData.CurrentAttackRange * 10f}, IsInRange: {isInRange}");
        //Debug.Log($"[JumpAttack] TimeCheck: {timeCheck}, Required Cooldown: {monsterData.CurrentAttackSpeed}, IsCooldownReady: {isCooldownReady}");
        //Debug.Log($"[JumpAttack] CanAttack Result: {isInRange && isCooldownReady}");
        //Debug.Log(lastAttackTime)
        return isInRange && isCooldownReady;
    }


    private void CreateShockwave(Vector3 position, IMonsterClass monsterData)
    {
        if (shockwaveEffect != null)
        {
            GameObject effect = GameObject.Instantiate(shockwaveEffect, position, Quaternion.identity);

            // 주변 플레이어 찾아서 데미지 적용
            
            Collider[] hitColliders = Physics.OverlapSphere(position, shockwaveRadius);
            foreach (var hitCollider in hitColliders)
            {
                if (hitCollider.CompareTag("Player"))
                {
                    
                    IDamageable player = GameInitializer.Instance.GetPlayerClass();

                        ApplyDamage(player, monsterData);
                        break;
                    
                }
            }

            GameObject.Destroy(effect, 2f);
        }
    }

    public override void Attack(Transform transform, Transform target, IMonsterClass monsterData)
    {
        // 이미 점프 중이거나 공격 조건을 만족하지 못하면 공격하지 않음
        if (isJumping || !CanAttack(Vector3.Distance(transform.position, target.position), monsterData))
        {   
            Debug.Log("안대용" + isJumping + CanAttack(Vector3.Distance(transform.position, target.position), monsterData));
            return;
        }
        Debug.Log("Executing Jump Attack Strategy...");
        // 공격 준비
        StartAttack();
        FaceTarget(transform, target);
        isAttackAnimation = true;
        isJumping = true;
        jumpStartPosition = transform.position;

        // 착지 지점 계산
        Vector3 startPos = transform.position;
        Vector3 endPos = target.position;  // 착지 지점을 타겟의 현재 위치로 삼을 수도 있고, Y좌표만 startPos.y를 쓰는 등 상황에 맞춰 조정
        

        // 시퀀스 생성
        jumpSequence = DOTween.Sequence();

        // 0 ~ 1까지 t를 올리면서 X, Z는 선형 이동, Y는 포물선으로 계산
        jumpSequence.Append(
            DOTween.To(
                () => 0f,   // 시작값
                t =>
                {
                    // t: 0 ~ 1
                    float newX = Mathf.Lerp(startPos.x, endPos.x, t);
                    float newZ = Mathf.Lerp(startPos.z, endPos.z, t);

                    // Y는 기본 선형 보간 + jumpHeight를 활용한 포물선 형태
                    float baseY = Mathf.Lerp(startPos.y, endPos.y, t);

                    // 간단한 포물선 식: 4t(1 - t) = 최대치가 t=0.5일 때 발생
                    float parabola = 4f * t * (1f - t) * jumpHeight;

                    // 최종 좌표 세팅
                    transform.position = new Vector3(newX, baseY + parabola, newZ);
                },
                1f,           // 목표치
                jumpDuration  // 걸리는 시간
            )
            .SetEase(Ease.Linear)
        );

        // 착지 시 충격파
        jumpSequence.AppendCallback(() =>
        {
            CreateShockwave(transform.position, monsterData);
        });

        // 완료 시점
        jumpSequence.OnComplete(() =>
        {
            lastAttackTime = Time.time;
            OnAttackAnimationEnd();    // isAttackAnimation, isAttacking 해제
            isJumping = false;
            jumpSequence.Kill();
            Debug.Log("킬~");
        });

        jumpSequence.Play();
    }

    public override void StopAttack()
    {
        base.StopAttack();
        
    }
}