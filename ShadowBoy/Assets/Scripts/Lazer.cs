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
            if (startTimeTilSpawn <= 0)
            {
                Instantiate(lazer, whereToSpawn.position, whereToSpawn.rotation);
                timeTilSpawn = startTimeTilSpawn;
            }
            else
            {
                startTimeTilSpawn -= Time.deltaTime;
            }
        }
            
    }
}
