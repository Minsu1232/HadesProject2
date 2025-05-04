using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Steamworks;
public class ResetStats : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.U)) // ���ϸ� Ű ���� ����
        {
            if (SteamManager.Initialized)
            {
                bool success = SteamUserStats.ResetAllStats(true);  // true�� ���� + ��� ��� �ʱ�ȭ
                SteamUserStats.StoreStats();
                Debug.Log("���� �ʱ�ȭ��: " + success);
            }
        }
    }
}
