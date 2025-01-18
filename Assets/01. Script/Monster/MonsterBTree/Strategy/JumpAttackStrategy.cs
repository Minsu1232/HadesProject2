// ���ο� ���� ���� ����
using DG.Tweening;
using Unity.VisualScripting;
using UnityEngine;

public class JumpAttackStrategy : BasePhysicalAttackStrategy
{
    public override PhysicalAttackType AttackType => PhysicalAttackType.Jump;
    public override float GetAttackPowerMultiplier() => 1.5f; // ���� ������ 50% �� ����

    private Vector3 jumpStartPosition;
    private bool isJumping;
    private float jumpHeight = 3f;
    private float jumpDuration = 1f;
    private DG.Tweening.Sequence jumpSequence;
    private GameObject shockwaveEffect; // ����� ����Ʈ ������
    private float shockwaveRadius;
    public JumpAttackStrategy(GameObject shockwaveEffectPrefab, float shockwaveRadius)
    {
        this.shockwaveEffect = shockwaveEffectPrefab;
        this.shockwaveRadius = shockwaveRadius;

    }

    public override bool CanAttack(float distanceToTarget, MonsterClass monsterData)
    {
        // ���� ������ �� �� �� �Ÿ����� ����
        return distanceToTarget <= monsterData.CurrentAttackRange * 1.5f &&
               Time.time >= lastAttackTime + monsterData.CurrentAttackSpeed * 1.2f;
    }

    private void CreateShockwave(Vector3 position, MonsterClass monsterData)
    {
        if (shockwaveEffect != null)
        {
            GameObject effect = GameObject.Instantiate(shockwaveEffect, position, Quaternion.identity);

            // �ֺ� �÷��̾� ã�Ƽ� ������ ����
            
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

    public override void Attack(Transform transform, Transform target, MonsterClass monsterData)
    {
        // �̹� ���� ���̰ų� ���� ������ �������� ���ϸ� �������� ����
        if (isJumping || !CanAttack(Vector3.Distance(transform.position, target.position), monsterData))
        {
            return;
        }

        // ���� �غ�
        StartAttack();
        FaceTarget(transform, target);
        isAttackAnimation = true;
        isJumping = true;
        jumpStartPosition = transform.position;

        // ���� ���� ���
        Vector3 startPos = transform.position;
        Vector3 endPos = target.position;  // ���� ������ Ÿ���� ���� ��ġ�� ���� ���� �ְ�, Y��ǥ�� startPos.y�� ���� �� ��Ȳ�� ���� ����
        lastAttackTime = Time.time;

        // ������ ����
        jumpSequence = DOTween.Sequence();

        // 0 ~ 1���� t�� �ø��鼭 X, Z�� ���� �̵�, Y�� ���������� ���
        jumpSequence.Append(
            DOTween.To(
                () => 0f,   // ���۰�
                t =>
                {
                    // t: 0 ~ 1
                    float newX = Mathf.Lerp(startPos.x, endPos.x, t);
                    float newZ = Mathf.Lerp(startPos.z, endPos.z, t);

                    // Y�� �⺻ ���� ���� + jumpHeight�� Ȱ���� ������ ����
                    float baseY = Mathf.Lerp(startPos.y, endPos.y, t);

                    // ������ ������ ��: 4t(1 - t) = �ִ�ġ�� t=0.5�� �� �߻�
                    float parabola = 4f * t * (1f - t) * jumpHeight;

                    // ���� ��ǥ ����
                    transform.position = new Vector3(newX, baseY + parabola, newZ);
                },
                1f,           // ��ǥġ
                jumpDuration  // �ɸ��� �ð�
            )
            .SetEase(Ease.Linear)
        );

        // ���� �� �����
        jumpSequence.AppendCallback(() =>
        {
            CreateShockwave(transform.position, monsterData);
        });

        // �Ϸ� ����
        jumpSequence.OnComplete(() =>
        {
            OnAttackAnimationEnd();    // isAttackAnimation, isAttacking ����
            isJumping = false;
            jumpSequence.Kill();
            Debug.Log("ų~");
        });

        jumpSequence.Play();
    }

    public override void StopAttack()
    {
        base.StopAttack();
        
    }
}