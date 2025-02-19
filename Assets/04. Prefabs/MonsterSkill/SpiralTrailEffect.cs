using UnityEngine;
public class SpiralTrailEffect : MonoBehaviour
{
    [Header("Trail Settings")]
    public float spiralRadius = 0.5f;
    public float spiralFrequency = 15f;
    public float spreadInterval = 120f;  // 트레일 간 퍼지는 각도

    [Header("Trail References")]
    public Transform[] trails;  // Inspector에서 직접 할당

    private float elapsedTime = 0f;
    private Vector3 startPoint;

    void Start()
    {
        startPoint = new Vector3(0, 0, 0.5f);
    }

    void Update()
    {
        elapsedTime += Time.deltaTime;
        float baseAngle = elapsedTime * spiralFrequency;

        for (int i = 0; i < trails.Length; i++)
        {
            if (trails[i] != null)
            {
                float spreadAngle = i * spreadInterval;
                float angle = baseAngle + (spreadAngle * Mathf.Deg2Rad);

                Vector3 offset = new Vector3(
                    Mathf.Cos(angle) * spiralRadius,
                    Mathf.Sin(angle) * spiralRadius,
                    0f
                );

                trails[i].localPosition = startPoint + offset;
            }
        }
    }
}