using System;
using System.Collections.Generic;
using UnityEngine;

// �ð� ���� Ŭ���� (������ ����ü �ܻ��� ������ �� �߻�)
public class TimeExplosion : MonoBehaviour
{
    private float damage;
    private float radius;
    private float duration = 0.5f; // ���� ���� �ð�
    private float timer;
    private bool hasDealtDamage = false;
    private List<IDamageable> hitTargets = new List<IDamageable>();

    public void Initialize(float damage, float radius)
    {
        this.damage = damage;
        this.radius = radius;

        // �ð� ȿ�� �ʱ�ȭ (��ƼŬ �ý���)
        CreateVisualEffect();

        // ���� �ð� �� �ڵ� �ı�
        Destroy(gameObject, duration);
    }

    private void CreateVisualEffect()
    {
        // ���� ��ƼŬ �ý��� �߰�
        ParticleSystem ps = gameObject.AddComponent<ParticleSystem>();
        var main = ps.main;
        main.duration = duration;
        main.startLifetime = duration;
        main.startSize = radius / 2f;
        main.startColor = new Color(1f, 0.6f, 0.2f, 0.8f); // ��Ȳ��/����� �迭

        // ���� ���� ����
        var shape = ps.shape;
        shape.shapeType = ParticleSystemShapeType.Sphere;
        shape.radius = radius / 4f;

        // �߱� ȿ�� �߰�
        var emission = ps.emission;
        emission.rateOverTime = 30f;

        // �浹 Ž���� SphereCollider �߰�
        SphereCollider col = gameObject.AddComponent<SphereCollider>();
        col.radius = radius;
        col.isTrigger = true;
    }

    private void OnTriggerEnter(Collider other)
    {
        // �̹� �������� ���� ����� ����
        IDamageable target = other.GetComponent<IDamageable>();
        if (target != null && !hitTargets.Contains(target) && target.GetDamageType() != DamageType.Player)
        {
            // ������ ����
            target.TakeDamage(Mathf.RoundToInt(damage));
            hitTargets.Add(target);

            Debug.Log($"�ð� ���߷� {other.name}���� {damage} ������");
        }
    }
}

// �÷��̾� �ൿ ��� Ŭ����
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

// �÷��̾� �ൿ ��� ������Ʈ
public class ActionRecorder : MonoBehaviour
{
    private PlayerClass playerClass;
    private TimeCloneComponent cloneComponent;
    private List<PlayerAction> actions = new List<PlayerAction>();
    private Vector3 lastPosition;
    private Quaternion lastRotation;
    private float recordInterval = 0.1f; // ��� ����
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

        // ���� �������� �÷��̾� �ൿ ���
        if (recordTimer >= recordInterval)
        {
            RecordAction();
            recordTimer = 0f;
        }

        // ����ü���� �ൿ ����
        if (cloneComponent != null)
        {
            cloneComponent.UpdateActions(actions);
        }
    }

    private void RecordAction()
    {
        Vector3 currentPos = playerClass.playerTransform.position;
        Quaternion currentRot = playerClass.playerTransform.rotation;

        // �̵� ����
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

        // ȸ�� ����
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

        // ���� ���� �� �߰� ����
        // (���� ���������� �÷��̾��� �Է� �̺�Ʈ�� ����ϴ� ���� �� ȿ����)

        // ���� ���� ����
        lastPosition = currentPos;
        lastRotation = currentRot;
    }
}

// �ð� ����ü ������Ʈ
public class TimeCloneComponent : MonoBehaviour
{
    private PlayerClass originalPlayer;
    private float actionDelay; // ���� �ൿ �����ϱ������ ���� �ð�
    private List<PlayerAction> pendingActions = new List<PlayerAction>();
    private float damageRatio; // ���� ��� ������ ���� (50% = 0.5f)

    public void Initialize(PlayerClass originalPlayer, float actionDelay, float damageRatio)
    {
        this.originalPlayer = originalPlayer;
        this.actionDelay = actionDelay;
        this.damageRatio = damageRatio;
    }

    public void UpdateActions(List<PlayerAction> newActions)
    {
        // ���ο� �ൿ ��� ������Ʈ
        pendingActions = new List<PlayerAction>(newActions);
    }

    private void Update()
    {
        if (pendingActions.Count == 0 || originalPlayer == null) return;

        float currentTime = Time.time;
        List<PlayerAction> actionsToExecute = new List<PlayerAction>();

        // ������ �ൿ ���͸� (���� �ð� - ���� �ð����� ���� Ÿ�ӽ������� ���� �ൿ)
        foreach (var action in pendingActions)
        {
            if (action.timestamp + actionDelay <= currentTime)
            {
                actionsToExecute.Add(action);
            }
        }

        // �ൿ ����
        foreach (var action in actionsToExecute)
        {
            ExecuteAction(action);
            pendingActions.Remove(action);
        }

        // ������ ����ü ������ ������ Ȯ��
        CheckIntersectionWithOriginal();
    }

    private void ExecuteAction(PlayerAction action)
    {
        // �ൿ ������ ���� �ٸ� ó��
        switch (action.actionType)
        {
            case PlayerAction.ActionType.Move:
                // �̵� �� ȸ��
                transform.position = action.position;
                transform.rotation = action.rotation;

                // �̵� �ð� ȿ�� (�ܻ�)
                CreateMoveTrail(action.position);
                break;

            case PlayerAction.ActionType.Attack:
                // ���� ��� �� ȿ��
                PerformAttack();
                break;

            case PlayerAction.ActionType.Dodge:
                // ȸ�� ��� �� ȿ��
                break;

            case PlayerAction.ActionType.Skill:
                // ��ų ��� �� ȿ��
                break;
        }
    }

    private void CreateMoveTrail(Vector3 position)
    {
        // �̵� �ܻ� ��ƼŬ ����
        GameObject trail = new GameObject("CloneTrail");
        trail.transform.position = position;

        // ��ƼŬ �ý��� �߰�
        ParticleSystem ps = trail.AddComponent<ParticleSystem>();
        var main = ps.main;
        main.startColor = new Color(0.2f, 0.6f, 1.0f, 0.3f); // �Ķ��� ������
        main.startSize = 0.5f;
        main.startLifetime = 0.3f;

        // �ڵ� �ı�
        Destroy(trail, 0.5f);
    }

    private void PerformAttack()
    {
        // ���� ȿ�� ����
        GameObject attackEffect = new GameObject("CloneAttackEffect");
        attackEffect.transform.position = transform.position + transform.forward * 1.5f;
        attackEffect.transform.rotation = transform.rotation;

        // ��ƼŬ �ý��� �߰�
        ParticleSystem ps = attackEffect.AddComponent<ParticleSystem>();
        var main = ps.main;
        main.startColor = new Color(0.2f, 0.6f, 1.0f, 0.6f); // �Ķ���
        main.startSize = 1.0f;
        main.startLifetime = 0.5f;

        // ���� �浹 ���� �߰�
        SphereCollider attackCollider = attackEffect.AddComponent<SphereCollider>();
        attackCollider.radius = 1.5f;
        attackCollider.isTrigger = true;

        // ���� ������Ʈ �߰�
        CloneAttack attackComponent = attackEffect.AddComponent<CloneAttack>();
        if (originalPlayer != null && originalPlayer.GetCurrentWeapon() != null)
        {
            int originalDamage = originalPlayer.PlayerStats.AttackPower;
            attackComponent.Initialize(Mathf.RoundToInt(originalDamage * damageRatio));
        }
        else
        {
            attackComponent.Initialize(10); // �⺻ ������
        }

        // �ڵ� �ı�
        Destroy(attackEffect, 0.5f);
    }

    private void CheckIntersectionWithOriginal()
    {
        if (originalPlayer == null) return;

        // ������ ����ü ������ �Ÿ� Ȯ��
        float distance = Vector3.Distance(transform.position, originalPlayer.playerTransform.position);

        // ���� �Ÿ� ���ϸ� ���� ó��
        if (distance < 1.0f)
        {
            // �ð� ���� ȿ�� ����
            Vector3 intersectionPoint = (transform.position + originalPlayer.playerTransform.position) / 2f;

            // �ð� ���� ȿ�� ���� ����
            GameObject explosionObj = new GameObject("TimeExplosion");
            explosionObj.transform.position = intersectionPoint;

            // ���� ������Ʈ �߰�
            TimeExplosion explosion = explosionObj.AddComponent<TimeExplosion>();

            // ������ �� �ݰ� ���� (������ ������ ����)
            float damage = 10f; // �⺻ ������
            if (originalPlayer.GetCurrentWeapon() != null)
            {
                damage = originalPlayer.PlayerStats.AttackPower;
            }
            explosion.Initialize(damage, 2f); // ������, �ݰ�

            Debug.Log("����ü�� ���� ���� ������ �ð� ���� ����");
        }
    }
}

// ����ü ���� ������Ʈ
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
        // �̹� �������� ���� ����� ����
        IDamageable target = other.GetComponent<IDamageable>();
        if (target != null && !hitTargets.Contains(target) && target.GetDamageType() != DamageType.Player)
        {
            // ������ ����
            target.TakeDamage(damage);
            hitTargets.Add(target);

            Debug.Log($"����ü �������� {other.name}���� {damage} ������");
        }
    }
}