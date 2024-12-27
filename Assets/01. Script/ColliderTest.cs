using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ColliderTest : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    private void OnTriggerEnter(Collider other)
    {
        Debug.Log("트리거엔터 닿음");
    }
    private void OnCollisionEnter(Collision collision)
    {
        Debug.Log("콜리전이 닿음");
    }
}
