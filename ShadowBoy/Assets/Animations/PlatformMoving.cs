using UnityEngine;

public class PlatformMoving : MonoBehaviour
{
    public float speed = 0.5f;
    private float waitTime;
    public Transform[] moveSpots;
    public float startWaitTime = 2;
    private int currentWaypointIndex = 0;
    private bool playerOnPlatform = false;
    public bool isActivated;
    private Vector2 previousPosition;
    public Vector2 CurrentVelocity { get; private set; }

    private void Start()
    {
        previousPosition = transform.position;
        waitTime = startWaitTime;

        if (moveSpots == null || moveSpots.Length == 0)
        {
            isActivated = false;
            return;
        }
    }

    private void Update()
    {
        CurrentVelocity = (Vector2)transform.position - previousPosition;
        previousPosition = transform.position;

        if (isActivated)
        {
            if (!playerOnPlatform) return;

            if (waitTime > 0)
            {
                waitTime -= Time.deltaTime;
                return;
            }

            if (moveSpots == null || moveSpots.Length == 0 || currentWaypointIndex < 0 || currentWaypointIndex >= moveSpots.Length)
            {
                return;
            }
            transform.position = Vector2.MoveTowards(transform.position, moveSpots[currentWaypointIndex].position, speed * Time.deltaTime);
            if (Vector2.Distance(transform.position, moveSpots[currentWaypointIndex].position) < 0.1f)
            {
                if (currentWaypointIndex < moveSpots.Length - 1)
                {
                    currentWaypointIndex++;
                }

            }
        }
    }


    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.collider.CompareTag("Player"))
        {
            if (isActivated)
            {
                collision.collider.transform.SetParent(transform);
                MovementPlayerImproved player = collision.collider.GetComponent<MovementPlayerImproved>();
                playerOnPlatform = true;
                waitTime = startWaitTime;
                if (player != null)
                {
                    player.SetPlayerOnMovingPlatform(true);
                    player.SetCurrentMovingPlatform(this);
                }
            }
        }
    }
    private void OnCollisionExit2D(Collision2D collision)
    {
        if (collision.collider.CompareTag("Player"))
        {
            collision.collider.transform.SetParent(null);
            MovementPlayerImproved player = collision.collider.GetComponent<MovementPlayerImproved>();
            playerOnPlatform = false;

            if (player != null)
            {
                player.SetPlayerOnMovingPlatform(false);
                player.SetCurrentMovingPlatform(null);
            }
        }

    }
}
