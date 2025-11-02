using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class MulaController : MonoBehaviour
{
    [Header("Movimentação")]
    public Transform[] waypoints;
    public float speed = 2f;
    public float rotationSpeed = 5f;
    public float waypointThreshold = 1f;

    private Rigidbody rb;
    private int currentWaypointIndex = 0;
    private bool movingForward = true;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        rb.useGravity = true;
        rb.constraints = RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationZ;
    }

    void FixedUpdate()
    {
        if (waypoints.Length == 0) return;

        Transform target = waypoints[currentWaypointIndex];
        Vector3 direction = (target.position - transform.position);
        direction.y = 0; // não tenta subir/voar

        if (direction.magnitude > waypointThreshold)
        {
            Vector3 moveDir = direction.normalized * speed;
            Vector3 newVelocity = new Vector3(moveDir.x, rb.velocity.y, moveDir.z);
            rb.velocity = newVelocity;

            // Rotação suave
            if (moveDir != Vector3.zero)
            {
                Quaternion targetRot = Quaternion.LookRotation(moveDir);
                transform.rotation = Quaternion.Slerp(transform.rotation, targetRot, rotationSpeed * Time.fixedDeltaTime);
            }
        }
        else
        {
            // Troca de waypoint
            if (movingForward)
            {
                currentWaypointIndex++;
                if (currentWaypointIndex >= waypoints.Length - 1)
                    movingForward = false;
            }
            else
            {
                currentWaypointIndex--;
                if (currentWaypointIndex <= 0)
                    movingForward = true;
            }
        }
    }
}