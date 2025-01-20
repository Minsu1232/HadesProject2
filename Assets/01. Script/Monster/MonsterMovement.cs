//using System.Collections;
//using System.Collections.Generic;
//using UnityEngine;

//public class MonsterMovement : MonoBehaviour
//{
//    [SerializeField] float moveRange; // 태어난 위치로부터의 이동 범위
//    [SerializeField] float chaseRange; // 플레이어를 추격하기 시작하는 거리
//    [SerializeField] int aggroDropRange;
//    public LayerMask wallLayer; // 벽 레이어

//    private MonsterClass monsterClass;
//    private MonsterData monsterData;
//    private float moveSpeed;
//    private Vector3 originPosition;
//    private Transform player;
//    private bool isChasing = false;
//    private bool isRandomMoving = false; // 랜덤 이동 상태 체크 플래그
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
       

//        StartRandomMove(); // 랜덤 이동 시작
//    }

//    private void Update()
//    {
//        float distanceToPlayer = Vector3.Distance(transform.position, player.position);

//        // 플레이어 추격 모드 전환
//        if (distanceToPlayer <= chaseRange)
//        {
//            if (!isChasing)
//            {
//                isChasing = true;
//                StopCoroutine(RandomMoveRoutine()); // 추격 시 랜덤 이동 중지
//                isRandomMoving = false;
//            }
//            ChasePlayer();
//        }
//        else if (distanceToPlayer > aggroDropRange && isChasing)
//        {
//            // 추격 범위를 벗어났을 때 랜덤 이동 재개
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
//        while (!isChasing) // 추격 중이 아닐 때만 랜덤 이동
//        {
//            randomDirection = GetRandomDirection();
//            currentMoveTime = Random.Range(1f, 3f);

//            while (currentMoveTime > 0)
//            {
//                if (isChasing) yield break; // 추격 모드가 활성화되면 랜덤 이동 중지

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
