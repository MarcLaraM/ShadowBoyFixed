using System.Collections;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Rendering.Universal;
using UnityEngine.UIElements;

public class LightHazard : MonoBehaviour
{
    public int damagePerSecond;
    public float damageInterval;
    private float nextDamageTime;
    private bool playerInTrigger = false;
    private HealthSystem currentHealth;
    private Light2D light2D;
    private PolygonCollider2D polygonCollider;
    public bool blink = true;

    private float blinkTimer;
    public float blinkInterval = 1f;
    public bool isLightOn = true;

    private void Start()
    {
        light2D = GetComponent<Light2D>();
        polygonCollider = GetComponent<PolygonCollider2D>();
        blinkTimer = blinkInterval;

    }

    void Update()
    {
        if (playerInTrigger && Time.time >= nextDamageTime && currentHealth != null)
        {
            currentHealth.TakeDamage(damagePerSecond);
            nextDamageTime = Time.time + damageInterval;
        }
        if (blink)
        {
            blinkTimer -= Time.deltaTime;
            if (blinkTimer <= 0)
            {
                isLightOn = !isLightOn;
                BlinkLight(isLightOn);
                blinkTimer = blinkInterval;
            }
        }
    }

    private void BlinkLight(bool state)
    {
        if (light2D != null)
        {
            light2D.enabled = state;
        }
        if (polygonCollider != null)
        {
            polygonCollider.enabled = state;
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            currentHealth = other.GetComponent<HealthSystem>();
            playerInTrigger = true;
        }
    }
    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            playerInTrigger = false;
        }
    }
}
