using UnityEngine;

public class Destroyer : MonoBehaviour
{
    public float timeTilDestroy;
    private void Update()
    {
        Destroy(gameObject, timeTilDestroy);
    }
}
