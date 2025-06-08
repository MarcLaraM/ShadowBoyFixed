using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Rendering.Universal;
using UnityEngine.UI;

public class Buttons : MonoBehaviour
{
    public bool isPlayerInside = false;
    public KeyCode interactionKey = KeyCode.E;
    public Light2D[] lights;
    public LightHazard[] hazards;
    public PolygonCollider2D[] polygons;
    public Lazer[] lazers;
    public PlatformMoving[] platforms;
    public StopZone[] stopzones;
    private Button button;

    private void Start()
    {
        button = GetComponent<Button>();
        button.onClick.AddListener(ToggleLights);
    }
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            isPlayerInside = true;
        }
    }
    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            isPlayerInside = false;
        }
    }

    private void Update()
    {
        if (isPlayerInside && Input.GetKeyDown(interactionKey))
        {
            ToggleLights();
            ShutDownLazers();
            ActivateElevators();
            DeleteStopZones();
        }
    }
    void ToggleLights()
    {
        foreach (Light2D light in lights)
        {
            light.enabled = !light.enabled;
        }
        foreach (LightHazard light in hazards)
        {
            light.enabled = !light.enabled;

            PolygonCollider2D collider = light.GetComponent<PolygonCollider2D>();
            if (collider != null)
            {
                collider.enabled = !collider.enabled;
            }
        }
        
    }
    void ShutDownLazers()
    {
        foreach (Lazer lazer in lazers)
        {
            lazer.isActivated = false;
        }
    }
    void ActivateElevators()
    {
        foreach (PlatformMoving platformMoving in platforms)
        {
            platformMoving.isActivated = true;
        }
    }
    void DeleteStopZones()
    {
        foreach (StopZone stopZone in stopzones)
        {
            stopZone.isActivated = false;
        }
    }
}
