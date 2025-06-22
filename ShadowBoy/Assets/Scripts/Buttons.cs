using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Rendering.Universal;
using UnityEngine.UI;
using UnityEngine.UIElements;

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
    private Buttons button;
    public GameObject[] invisibleWalls;
    public Sprite spriteOriginal;
    public Sprite sprite2;
    private SpriteRenderer spriteRenderer;
    private bool using2 = false;
    private void Start()
    {
        button = GetComponent<Buttons>();
        //button.onClick.AddListener(ToggleLights);
        spriteRenderer = GetComponent<SpriteRenderer>();
        spriteRenderer.sprite = spriteOriginal;

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
            DesactiveWalls();
            ChangeImage();
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

    void DesactiveWalls()
    {
        foreach(GameObject wall in invisibleWalls)
        {
            wall.SetActive(false);
        }
    }
    public void ChangeImage()
    {
        if (using2)
        {
            spriteRenderer.sprite = spriteOriginal;
            spriteRenderer.sortingLayerName = "Background";
            spriteRenderer.sortingOrder = 0;
            spriteRenderer.
            Debug.Log("Using Sprite 1");
            using2 = false;
        }
        else
        {
            spriteRenderer.sprite = sprite2;
            spriteRenderer.sortingLayerName = "Background";
            spriteRenderer.sortingOrder = 0;
            Debug.Log("Using Sprite 2");
            using2 = true;
        }
    }
}
