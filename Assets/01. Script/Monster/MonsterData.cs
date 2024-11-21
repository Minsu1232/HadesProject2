using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;

[CreateAssetMenu(menuName = "Monster")]

public class MonsterData : ScriptableObject
{
    public string monsterName;
    public int initialHp;   
    public int initialAttackPower;
    public float initialAttackSpeed;
    public int initialSpeed;
    public float attackRange;
    public float dropChance;
    public int dropItem;
    public int moveRange;
    public int chaseRange;
       

    public AssetReferenceGameObject monsterPrefab; // Addressable 프리팹 레퍼런스

    
}

