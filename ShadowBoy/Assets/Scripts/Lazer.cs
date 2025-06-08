using UnityEngine;

public class Lazer : MonoBehaviour
{
    private float timeTilSpawn;

    public float startTimeTilSpawn;
    public bool isActivated = true;

    public GameObject lazer;
    public Transform whereToSpawn;

    private void Update()
    {
        if (isActivated)
        {
            if (timeTilSpawn <= 0)
            {
                Instantiate(lazer, whereToSpawn.position, whereToSpawn.rotation);
                timeTilSpawn = startTimeTilSpawn;
            }
            else
            {
                timeTilSpawn -= Time.deltaTime;
            }
        }
            
    }
}
