using UnityEngine;

public class StopZone : MonoBehaviour
{
    public PlatformMoving[] platforms;
    public bool isActivated;
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (isActivated)
        {
            if (other.CompareTag("Player"))
            {
                foreach (PlatformMoving elevator in platforms)
                {
                    elevator.isActivated = false;
                }
            }
        }
        
        
    }
}
