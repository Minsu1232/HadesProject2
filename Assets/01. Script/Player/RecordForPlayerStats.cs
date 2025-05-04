using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class RecordForPlayerStats : MonoBehaviour
{
    [SerializeField] Button healthUpButton;
    [SerializeField] Button healthDownButton;
    [SerializeField] Button attackPowerUpButton;
    [SerializeField] Button attackPowerDownButton;

    PlayerClass playerClass;
    // Start is called before the first frame update
    void Start()
    {
        playerClass = GameInitializer.Instance.GetPlayerClass();
        healthUpButton.onClick.AddListener(PlayerHealthUP);
        healthDownButton.onClick.AddListener(PlayerHealthDown);
        attackPowerUpButton.onClick.AddListener(PlayerAttackPowerUp);
        attackPowerDownButton.onClick.AddListener(PlayerAttackPowerDown);
    }

    private void PlayerHealthUP()
    {
        playerClass.ModifyPower(healthAmount:10000);
    }
    private void PlayerHealthDown()
    {
        playerClass.ModifyPower(healthAmount: -2000);
    }
    private void PlayerAttackPowerUp()
    {
        playerClass.ModifyPower(attackAmount: 10000);
    }
    private void PlayerAttackPowerDown()
    {
        playerClass.ModifyPower(attackAmount: -10000);
    }
  
}
