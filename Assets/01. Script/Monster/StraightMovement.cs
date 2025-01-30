// 직선 이동
using UnityEngine;

public class StraightMovement : IProjectileMovement
{
    public StraightMovement()
    {
        
    }
    public void Move(Transform projectileTransform, Transform target, float speed)
    {
        projectileTransform.Translate(Vector3.forward * speed * Time.deltaTime);
       
    }
}