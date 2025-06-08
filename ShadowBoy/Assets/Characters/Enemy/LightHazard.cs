using System.Collections;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Rendering.Universal;

public class LightHazard : MonoBehaviour
{
    public int damagePerSecond = 5;
    public float damageInterval = 0.5f; // Cada cuánto aplica daño
    private float nextDamageTime;
    private bool playerInTrigger = false;
    private HealthSystem currentHealth;
    public float blinkInterval = 1.0f;
    public bool startOn = true;
    private Light2D light2D;
    private PolygonCollider2D polygonCollider;

    private void Start()
    {
        light2D = GetComponent<Light2D>();
        polygonCollider = GetComponent<PolygonCollider2D>();

        if(light2D != null)
        {
            light2D.enabled = startOn;
        }
        if(polygonCollider != null)
        {
            polygonCollider.enabled = startOn;
        }
        StartCoroutine(BlinkLight());
    }

    private IEnumerator BlinkLight()
    {
        while (true)
        {
            if (light2D != null)
            {
                light2D.enabled = !light2D.enabled;
            }
            if (polygonCollider != null)
            {
                polygonCollider.enabled = !polygonCollider.enabled;
            }
            yield return new WaitForSeconds(blinkInterval);
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
    
    void Update()
    {
        if (playerInTrigger && Time.time >= nextDamageTime && currentHealth != null)
        {
            currentHealth.TakeDamage(damagePerSecond);
            nextDamageTime = Time.time + damageInterval;
        }
    }
}
