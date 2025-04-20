using System;
using System.Collections.Generic;
using UnityEngine;

// 시간 폭발 클래스 (원본과 복제체 잔상이 교차할 때 발생)
public class TimeExplosion : MonoBehaviour
{
    private float damage;
    private float radius;
    private float duration = 0.5f; // 폭발 지속 시간
    private float timer;
    private bool hasDealtDamage = false;
    private List<IDamageable> hitTargets = new List<IDamageable>();

    public void Initialize(float damage, float radius)
    {
        this.damage = damage;
        this.radius = radius;

        // 시각 효과 초기화 (파티클 시스템)
        CreateVisualEffect();

        // 일정 시간 후 자동 파괴
        Destroy(gameObject, duration);
    }

    private void CreateVisualEffect()
    {
        // 폭발 파티클 시스템 추가
        ParticleSystem ps = gameObject.AddComponent<ParticleSystem>();
        var main = ps.main;
        main.duration = duration;
        main.startLifetime = duration;
        main.startSize = radius / 2f;
        main.startColor = new Color(1f, 0.6f, 0.2f, 0.8f); // 주황색/노란색 계열

        // 폭발 형태 설정
        var shape = ps.shape;
        shape.shapeType = ParticleSystemShapeType.Sphere;
        shape.radius = radius / 4f;

        // 발광 효과 추가
        var emission = ps.emission;
        emission.rateOverTime = 30f;

        // 충돌 탐지용 SphereCollider 추가
        SphereCollider col = gameObject.AddComponent<SphereCollider>();
        col.radius = radius;
        col.isTrigger = true;
    }

    private void OnTriggerEnter(Collider other)
    {
        // 이미 데미지를 입힌 대상은 무시
        IDamageable target = other.GetComponent<IDamageable>();
        if (target != null && !hitTargets.Contains(target) && target.GetDamageType() != DamageType.Player)
        {
            // 데미지 적용
            target.TakeDamage(Mathf.RoundToInt(damage));
            hitTargets.Add(target);

            Debug.Log($"시간 폭발로 {other.name}에게 {damage} 데미지");
        }
    }
}

// 플레이어 행동 기록 클래스
[Serializable]
public class PlayerAction
{
    public Vector3 position;
    public Quaternion rotation;
    public float timestamp;
    public ActionType actionType;

    public enum ActionType
    {
        Move,
        Attack,
        Dodge,
        Skill
    }

    public PlayerAction(Vector3 position, Quaternion rotation, float timestamp, ActionType actionType)
    {
        this.position = position;
        this.rotation = rotation;
        this.timestamp = timestamp;
        this.actionType = actionType;
    }
}

// 플레이어 행동 기록 컴포넌트
public class ActionRecorder : MonoBehaviour
{
    private PlayerClass playerClass;
    private TimeCloneComponent cloneComponent;
    private List<PlayerAction> actions = new List<PlayerAction>();
    private Vector3 lastPosition;
    private Quaternion lastRotation;
    private float recordInterval = 0.1f; // 기록 간격
    private float recordTimer = 0f;

    public void Initialize(PlayerClass playerClass, TimeCloneComponent cloneComponent)
    {
        this.playerClass = playerClass;
        this.cloneComponent = cloneComponent;
        this.lastPosition = playerClass.playerTransform.position;
        this.lastRotation = playerClass.playerTransform.rotation;
    }

    private void Update()
    {
        recordTimer += Time.deltaTime;

        // 일정 간격으로 플레이어 행동 기록
        if (recordTimer >= recordInterval)
        {
            RecordAction();
            recordTimer = 0f;
        }

        // 복제체에게 행동 전달
        if (cloneComponent != null)
        {
            cloneComponent.UpdateActions(actions);
        }
    }

    private void RecordAction()
    {
        Vector3 currentPos = playerClass.playerTransform.position;
        Quaternion currentRot = playerClass.playerTransform.rotation;

        // 이동 감지
        if (Vector3.Distance(lastPosition, currentPos) > 0.1f)
        {
            PlayerAction moveAction = new PlayerAction(
                currentPos,
                currentRot,
                Time.time,
                PlayerAction.ActionType.Move
            );
            actions.Add(moveAction);
        }

        // 회전 감지
        if (Quaternion.Angle(lastRotation, currentRot) > 5f)
        {
            PlayerAction rotateAction = new PlayerAction(
                currentPos,
                currentRot,
                Time.time,
                PlayerAction.ActionType.Move
            );
            actions.Add(rotateAction);
        }

        // 공격 감지 등 추가 가능
        // (실제 구현에서는 플레이어의 입력 이벤트를 기록하는 것이 더 효과적)

        // 현재 상태 저장
        lastPosition = currentPos;
        lastRotation = currentRot;
    }
}

// 시간 복제체 컴포넌트
public class TimeCloneComponent : MonoBehaviour
{
    private PlayerClass originalPlayer;
    private float actionDelay; // 원본 행동 따라하기까지의 지연 시간
    private List<PlayerAction> pendingActions = new List<PlayerAction>();
    private float damageRatio; // 원본 대비 데미지 비율 (50% = 0.5f)

    public void Initialize(PlayerClass originalPlayer, float actionDelay, float damageRatio)
    {
        this.originalPlayer = originalPlayer;
        this.actionDelay = actionDelay;
        this.damageRatio = damageRatio;
    }

    public void UpdateActions(List<PlayerAction> newActions)
    {
        // 새로운 행동 목록 업데이트
        pendingActions = new List<PlayerAction>(newActions);
    }

    private void Update()
    {
        if (pendingActions.Count == 0 || originalPlayer == null) return;

        float currentTime = Time.time;
        List<PlayerAction> actionsToExecute = new List<PlayerAction>();

        // 실행할 행동 필터링 (현재 시간 - 지연 시간보다 이전 타임스탬프를 가진 행동)
        foreach (var action in pendingActions)
        {
            if (action.timestamp + actionDelay <= currentTime)
            {
                actionsToExecute.Add(action);
            }
        }

        // 행동 실행
        foreach (var action in actionsToExecute)
        {
            ExecuteAction(action);
            pendingActions.Remove(action);
        }

        // 원본과 복제체 사이의 교차점 확인
        CheckIntersectionWithOriginal();
    }

    private void ExecuteAction(PlayerAction action)
    {
        // 행동 유형에 따라 다른 처리
        switch (action.actionType)
        {
            case PlayerAction.ActionType.Move:
                // 이동 및 회전
                transform.position = action.position;
                transform.rotation = action.rotation;

                // 이동 시각 효과 (잔상)
                CreateMoveTrail(action.position);
                break;

            case PlayerAction.ActionType.Attack:
                // 공격 모션 및 효과
                PerformAttack();
                break;

            case PlayerAction.ActionType.Dodge:
                // 회피 모션 및 효과
                break;

            case PlayerAction.ActionType.Skill:
                // 스킬 모션 및 효과
                break;
        }
    }

    private void CreateMoveTrail(Vector3 position)
    {
        // 이동 잔상 파티클 생성
        GameObject trail = new GameObject("CloneTrail");
        trail.transform.position = position;

        // 파티클 시스템 추가
        ParticleSystem ps = trail.AddComponent<ParticleSystem>();
        var main = ps.main;
        main.startColor = new Color(0.2f, 0.6f, 1.0f, 0.3f); // 파란색 반투명
        main.startSize = 0.5f;
        main.startLifetime = 0.3f;

        // 자동 파괴
        Destroy(trail, 0.5f);
    }

    private void PerformAttack()
    {
        // 공격 효과 생성
        GameObject attackEffect = new GameObject("CloneAttackEffect");
        attackEffect.transform.position = transform.position + transform.forward * 1.5f;
        attackEffect.transform.rotation = transform.rotation;

        // 파티클 시스템 추가
        ParticleSystem ps = attackEffect.AddComponent<ParticleSystem>();
        var main = ps.main;
        main.startColor = new Color(0.2f, 0.6f, 1.0f, 0.6f); // 파란색
        main.startSize = 1.0f;
        main.startLifetime = 0.5f;

        // 공격 충돌 영역 추가
        SphereCollider attackCollider = attackEffect.AddComponent<SphereCollider>();
        attackCollider.radius = 1.5f;
        attackCollider.isTrigger = true;

        // 공격 컴포넌트 추가
        CloneAttack attackComponent = attackEffect.AddComponent<CloneAttack>();
        if (originalPlayer != null && originalPlayer.GetCurrentWeapon() != null)
        {
            int originalDamage = originalPlayer.PlayerStats.AttackPower;
            attackComponent.Initialize(Mathf.RoundToInt(originalDamage * damageRatio));
        }
        else
        {
            attackComponent.Initialize(10); // 기본 데미지
        }

        // 자동 파괴
        Destroy(attackEffect, 0.5f);
    }

    private void CheckIntersectionWithOriginal()
    {
        if (originalPlayer == null) return;

        // 원본과 복제체 사이의 거리 확인
        float distance = Vector3.Distance(transform.position, originalPlayer.playerTransform.position);

        // 일정 거리 이하면 교차 처리
        if (distance < 1.0f)
        {
            // 시간 폭발 효과 생성
            Vector3 intersectionPoint = (transform.position + originalPlayer.playerTransform.position) / 2f;

            // 시간 폭발 효과 직접 생성
            GameObject explosionObj = new GameObject("TimeExplosion");
            explosionObj.transform.position = intersectionPoint;

            // 폭발 컴포넌트 추가
            TimeExplosion explosion = explosionObj.AddComponent<TimeExplosion>();

            // 데미지 및 반경 설정 (적절한 값으로 조정)
            float damage = 10f; // 기본 데미지
            if (originalPlayer.GetCurrentWeapon() != null)
            {
                damage = originalPlayer.PlayerStats.AttackPower;
            }
            explosion.Initialize(damage, 2f); // 데미지, 반경

            Debug.Log("복제체와 원본 교차 지점에 시간 폭발 생성");
        }
    }
}

// 복제체 공격 컴포넌트
public class CloneAttack : MonoBehaviour
{
    private int damage;
    private List<IDamageable> hitTargets = new List<IDamageable>();

    public void Initialize(int damage)
    {
        this.damage = damage;
    }

    private void OnTriggerEnter(Collider other)
    {
        // 이미 데미지를 입힌 대상은 무시
        IDamageable target = other.GetComponent<IDamageable>();
        if (target != null && !hitTargets.Contains(target) && target.GetDamageType() != DamageType.Player)
        {
            // 데미지 적용
            target.TakeDamage(damage);
            hitTargets.Add(target);

            Debug.Log($"복제체 공격으로 {other.name}에게 {damage} 데미지");
        }
    }
}