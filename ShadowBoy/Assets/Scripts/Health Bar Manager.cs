using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class HealthManager : MonoBehaviour
{
    public Image healthBar;
    private HealthSystem healthSystem;
    private float healthMaxAmount;

    private void Start()
    {
        healthSystem = GameObject.FindWithTag("Player").GetComponent<HealthSystem>();
        healthMaxAmount = healthSystem.maxHealth;
    }
    void Update()
    {
        healthBar.fillAmount = healthSystem.currentHealth / healthMaxAmount;
    }
}   
