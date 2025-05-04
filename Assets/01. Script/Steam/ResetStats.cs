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
        if (Input.GetKeyDown(KeyCode.U)) // 원하면 키 변경 가능
        {
            if (SteamManager.Initialized)
            {
                bool success = SteamUserStats.ResetAllStats(true);  // true면 업적 + 통계 모두 초기화
                SteamUserStats.StoreStats();
                Debug.Log("업적 초기화됨: " + success);
            }
        }
    }
}
