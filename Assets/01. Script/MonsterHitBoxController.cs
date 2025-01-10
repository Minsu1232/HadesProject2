using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MonsterHitBoxController : MonoBehaviour
{
    // 애니메이션 이벤트에서 호출할 메서드
    public void EnableHitBoxes()
    {
        MonsterHitBox.EnableAllHitBoxes(gameObject);
    }

    public void DisableHitBoxes()
    {
        MonsterHitBox.DisableAllHitBoxes(gameObject);
    }
}
