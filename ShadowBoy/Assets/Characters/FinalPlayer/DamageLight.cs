using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Rendering.Universal;

public class DamageLight : MonoBehaviour
{
    public Light2D Light;
    public bool lightOn = true;
    public float damagePerSecond = 25f;

    private void Update()
    {
        if (lightOn)
        {
            damagePerSecond = 25;
        }
        else
        {
            damagePerSecond = 0;
            Light.intensity = 0;

        }
    }
}
