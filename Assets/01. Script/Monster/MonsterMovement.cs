//using System.Collections;
//using System.Collections.Generic;
//using UnityEngine;

//public class MonsterMovement : MonoBehaviour
//{
//    [SerializeField] float moveRange; // �¾ ��ġ�κ����� �̵� ����
//    [SerializeField] float chaseRange; // �÷��̾ �߰��ϱ� �����ϴ� �Ÿ�
//    [SerializeField] int aggroDropRange;
//    public LayerMask wallLayer; // �� ���̾�

//    private MonsterClass monsterClass;
//    private MonsterData monsterData;
//    private float moveSpeed;
//    private Vector3 originPosition;
//    private Transform player;
//    private bool isChasing = false;
//    private bool isRandomMoving = false; // ���� �̵� ���� üũ �÷���
//    private float currentMoveTime = 0f;
//    private Vector3 randomDirection;

//    private void Start()
//    {   
        
//        monsterClass = DungeonManager.Instance.GetMonsterClass();
        
//        moveRange = monsterClass.CurrentMoveRange;
//        moveSpeed = monsterClass.CurrentSpeed;
//        chaseRange = monsterClass.CurrentChaseRange;
//        aggroDropRange = monsterClass.CurrentAggroDropRange;
//        player = DungeonManager.Instance.GetPlayerTransform();
//        originPosition = transform.position;
       

//        StartRandomMove(); // ���� �̵� ����
//    }

//    private void Update()
//    {
//        float distanceToPlayer = Vector3.Distance(transform.position, player.position);

//        // �÷��̾� �߰� ��� ��ȯ
//        if (distanceToPlayer <= chaseRange)
//        {
//            if (!isChasing)
//            {
//                isChasing = true;
//                StopCoroutine(RandomMoveRoutine()); // �߰� �� ���� �̵� ����
//                isRandomMoving = false;
//            }
//            ChasePlayer();
//        }
//        else if (distanceToPlayer > aggroDropRange && isChasing)
//        {
//            // �߰� ������ ����� �� ���� �̵� �簳
//            isChasing = false;
//            StartRandomMove();
//        }
//    }

//    private void ChasePlayer()
//    {
//        Vector3 direction = (player.position - transform.position).normalized;
//        transform.position += direction * moveSpeed * Time.deltaTime;
//    }

//    private void StartRandomMove()
//    {
//        if (!isRandomMoving)
//        {
//            isRandomMoving = true;
//            StartCoroutine(RandomMoveRoutine());
//        }
//    }

//    private IEnumerator RandomMoveRoutine()
//    {
//        while (!isChasing) // �߰� ���� �ƴ� ���� ���� �̵�
//        {
//            randomDirection = GetRandomDirection();
//            currentMoveTime = Random.Range(1f, 3f);

//            while (currentMoveTime > 0)
//            {
//                if (isChasing) yield break; // �߰� ��尡 Ȱ��ȭ�Ǹ� ���� �̵� ����

//                if (IsWallInDirection(randomDirection))
//                {
//                    randomDirection = GetRandomDirection();
//                }

//                transform.position += randomDirection * moveSpeed * Time.deltaTime;
//                currentMoveTime -= Time.deltaTime;

//                yield return null;
//            }
//            yield return new WaitForSeconds(Random.Range(1f, 2f));
//        }
//    }

//    private Vector3 GetRandomDirection()
//    {
//        Vector2 randomDirection2D = Random.insideUnitCircle.normalized;
//        return new Vector3(randomDirection2D.x, 0, randomDirection2D.y);
//    }

//    private bool IsWallInDirection(Vector3 direction)
//    {
//        RaycastHit hit;
//        return Physics.Raycast(transform.position, direction, 0.5f, wallLayer);
//    }
//}
