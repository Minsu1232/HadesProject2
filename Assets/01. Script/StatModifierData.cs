using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu (fileName = "modifeData")]
public class StatModifierData : ScriptableObject
{
    public int buffDuration;
    public int healthBoost;
    public int attackBoost;
    public int speedBoost;  
    public float criticalChanceBoost;
    public GameObject buffParticle;


}
