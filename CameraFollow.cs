using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    [Header("Target")]
    public Transform target; 

    [Header("Offset & Dead Zone")]
    public Vector3 offset = new Vector3(0, 2, -10); 
    public Vector2 deadZoneSize = new Vector2(1.5f, 1f); 

    [Header("Look Ahead")]
    public float lookAheadDistance = 3f;
    public float lookAheadSpeed = 2f;
    
    [Header("Smoothness")]
    public float smoothTime = 0.15f; 
    private Vector3 currentVelocity = Vector3.zero;
    private float currentLookAheadX;

    private void LateUpdate()
    {
        if (target == null) return;

        // 1. Determine direction for Look Ahead
        // We look at the player's Rigidbody to see which way they are moving
        Rigidbody2D targetRb = target.GetComponent<Rigidbody2D>();
        float moveDir = 0;

        if (targetRb != null && Mathf.Abs(targetRb.velocity.x) > 0.1f)
        {
            moveDir = Mathf.Sign(targetRb.velocity.x);
        }

        // 2. Smoothly slide the look-ahead offset
        float targetLookAhead = moveDir * lookAheadDistance;
        currentLookAheadX = Mathf.Lerp(currentLookAheadX, targetLookAhead, Time.deltaTime * lookAheadSpeed);

        // 3. Calculate Target Position
        Vector3 targetPos = target.position + offset + new Vector3(currentLookAheadX, 0, 0);

        // 4. Dead Zone Logic
        Vector3 currentPos = transform.position;

        if (Mathf.Abs(currentPos.x - targetPos.x) < deadZoneSize.x)
            targetPos.x = currentPos.x;

        if (Mathf.Abs(currentPos.y - targetPos.y) < deadZoneSize.y)
            targetPos.y = currentPos.y;

        // 5. Final Movement (SmoothDamp)
        transform.position = Vector3.SmoothDamp(transform.position, targetPos, ref currentVelocity, smoothTime);
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireCube(transform.position, new Vector3(deadZoneSize.x * 2, deadZoneSize.y * 2, 0.1f));
    }
}
