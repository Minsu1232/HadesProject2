using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MonsterHitBoxController : MonoBehaviour
{
    // �ִϸ��̼� �̺�Ʈ���� ȣ���� �޼���
    public void EnableHitBoxes()
    {
        MonsterHitBox.EnableAllHitBoxes(gameObject);
    }

    public void DisableHitBoxes()
    {
        MonsterHitBox.DisableAllHitBoxes(gameObject);
    }
}
