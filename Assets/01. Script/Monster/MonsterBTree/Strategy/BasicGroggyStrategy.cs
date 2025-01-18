using UnityEngine;

public class BasicGroggyStrategy : IGroggyStrategy
{
    private float groggyDuration;
    private float groggyTimer;
    private bool isGroggyComplete;
    private bool isInGroggy;

    public BasicGroggyStrategy(float groggyTime)
    {
        this.groggyDuration = groggyTime;
    }

    public bool IsGroggyComplete => isGroggyComplete;

    public void OnGroggy(Transform transform, MonsterClass monsterData)
    {
        groggyTimer = 0f;
        isGroggyComplete = false;
        isInGroggy = true;
    }

    public void UpdateGroggy()
    {
        if (isInGroggy)
        {
            groggyTimer += Time.deltaTime;
            if (groggyTimer >= groggyDuration)
            {
                isGroggyComplete = true;
                isInGroggy = false;
            }
        }
    }
}