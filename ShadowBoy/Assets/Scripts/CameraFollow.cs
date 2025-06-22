using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    public float FollowSpeed = 2.0f;
    public Transform target;

    
    void Update()
    {
        Vector3 newPos = new Vector3(target.position.x, target.position.y+2.5f, -10f);
        transform.position = Vector3.Slerp(transform.position, newPos, FollowSpeed * Time.deltaTime);
    }
}
