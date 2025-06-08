using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Rendering.Universal;

public class HealthSystem : MonoBehaviour
{
    [Header("Health System")]
    public int maxHealth = 100;
    public int currentHealth;

    [Header("Dead")]
    MovementPlayerImproved movementPlayerImproved = new MovementPlayerImproved();
    public bool isDead = false;
    public Animator animator;

    [Header("Light Detection")]
    public float checkInterval = 0.2f;
    public float lightDetectionRadius = 5f;


    [Header("Visuals")]
    public SpriteRenderer spriteRenderer;
    public float flashDuration = 0.1f;
    public int flashCount = 2;

    private Coroutine flashCoroutine;


    private void Start()
    {
        currentHealth = maxHealth;
        StartCoroutine(CheckForLightDamage());
    }


    IEnumerator CheckForLightDamage()
    {
        while (!isDead)
        {
            CheckLights();

            yield return new WaitForSeconds(checkInterval);
        }
    }

    public void CheckLights()
    {
        Light2D[] allLights = Object.FindObjectsByType<Light2D>(FindObjectsSortMode.None);

        foreach (Light2D light in allLights)
        {
            DamageLight dmg = light.GetComponent<DamageLight>();

            if (dmg == null || !light.enabled)
            {
                continue;
            }
            if (Vector2.Distance(transform.position, light.transform.position) > lightDetectionRadius)
            {
                continue;
            }

            Vector2 dir = (transform.position - light.transform.position).normalized;
            RaycastHit2D hit = Physics2D.Raycast(light.transform.position, dir, lightDetectionRadius);

            if (hit.collider != null && hit.collider.gameObject == gameObject)
            {
                TakeDamage(dmg.damagePerSecond * checkInterval);
            }
        }

    }


    public void TakeDamage(float damage)
    {
        currentHealth -= Mathf.RoundToInt(damage);
        currentHealth = Mathf.Clamp(currentHealth, 0, maxHealth);   // Hace que no baje de 0 ni suba de 100 de vida

        if (flashCoroutine != null)
        {
            StopCoroutine(flashCoroutine);
        }
        flashCoroutine = StartCoroutine(FlashEffect());

        if (currentHealth <= 0)
        {
            Die();
        }
    }


    IEnumerator FlashEffect()
    {
        if (!isDead)
        {
            for (int i = 0; i < flashCount; i++)
            {
                spriteRenderer.color = Color.red;
                yield return new WaitForSeconds(flashDuration);
                spriteRenderer.color = Color.white;
                yield return new WaitForSeconds(flashDuration);
            }
        }
    }

    public void Die()
    {
        isDead = true;
        animator.SetBool("IsDead", true);
        GetComponent<MovementPlayerImproved>().isDead = true;
        FindAnyObjectByType<GameOver>().ShowGameOver();
    }

    public void ResetHealth()
    {
        isDead = false;
        currentHealth = maxHealth;
        animator.SetBool("IsDead", false);

    }
}
