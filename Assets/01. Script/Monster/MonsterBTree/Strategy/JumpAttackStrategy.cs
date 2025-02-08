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
        lastAttackTime = Time.time - 100f;  // ù ������ �ٷ� �����ϵ���
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

    public override void Attack(Transform transform, Transform target, IMonsterClass monsterData)
    {
        // �̹� ���� ���̰ų� ���� ������ �������� ���ϸ� �������� ����
        if (isJumping || !CanAttack(Vector3.Distance(transform.position, target.position), monsterData))
        {   
            Debug.Log("�ȴ��" + isJumping + CanAttack(Vector3.Distance(transform.position, target.position), monsterData));
            return;
        }
        Debug.Log("Executing Jump Attack Strategy...");
        // ���� �غ�
        StartAttack();
        FaceTarget(transform, target);
        isAttackAnimation = true;
        isJumping = true;
        jumpStartPosition = transform.position;

        // ���� ���� ���
        Vector3 startPos = transform.position;
        Vector3 endPos = target.position;  // ���� ������ Ÿ���� ���� ��ġ�� ���� ���� �ְ�, Y��ǥ�� startPos.y�� ���� �� ��Ȳ�� ���� ����
        

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
            lastAttackTime = Time.time;
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