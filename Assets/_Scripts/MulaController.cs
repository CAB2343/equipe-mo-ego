using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class MulaController : MonoBehaviour
{
    public enum State { Patrol, Chase }
    private State lastState;
    public MenuManager menuManager;
    public GameObject canvas;

    [Header("Waypoints")]
    public Transform[] waypoints;
    public float waypointThreshold = 1f;

    [Header("Movimentação")]
    public float patrolSpeed = 2f;
    public float chaseSpeed = 6f;
    public float rotationSpeed = 5f;

    [Header("Visão")]
    [Range(0f, 360f)] public float fieldOfViewAngle = 90f;
    public float viewDistance = 10f;
    public LayerMask obstacleMask;
    public string playerTag = "Player";
    public Transform player; 

    private Rigidbody rb;
    private int currentWaypointIndex = 0;
    private bool movingForward = true;
    public State state = State.Patrol;
    private float dist;

    private bool playerDead = false;

    [Header("Som")] 
    public SoundsManager sounds;

    public bool veio = true;
    
    void Start()
    {
        rb = GetComponent<Rigidbody>();
        rb.useGravity = true;
        rb.constraints = RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationZ;

        if (player == null)
        {
            GameObject p = GameObject.FindGameObjectWithTag(playerTag);
            if (p != null) player = p.transform;
        }
    }
    
    void FixedUpdate()
    {
        // checa visão do jogador primeiro (se existir)
        bool canSeePlayer = false;
        if (player != null)
            canSeePlayer = CheckPlayerInSight();

        // troca de estado
        if (canSeePlayer)
            state = State.Chase;
        else
            state = State.Patrol;

        // comportamento por estado
        if (state == State.Chase)
        {
            ChasePlayer();
            sounds.AjustarVolumePorDistancia(dist, viewDistance);
            veio = false;
        }
        else
        {
            if (!canvas.activeSelf)
            {
                Patrol();
                sounds.ReajustarVolume();
                veio = true;
            }
            
        }
        
        if (state != lastState)
        {
            if (state == State.Chase)
            {
                sounds.ChangeTheme(2);
                sounds.SoundPlay(5);
            }
            else if (state == State.Patrol)
            {
                sounds.ChangeTheme(1);
            }

            lastState = state; // atualiza o estado anterior
        }
    }

    bool CheckPlayerInSight()
    {
        Vector3 toPlayer = player.position - transform.position;
        Vector3 toPlayerFlat = new Vector3(toPlayer.x, 0f, toPlayer.z);
        if (toPlayerFlat.magnitude > viewDistance) return false;

        // ângulo no plano horizontal
        float angle = Vector3.Angle(transform.forward, toPlayerFlat.normalized);
        if (angle > fieldOfViewAngle * 0.5f) return false;

        // raycast para checar se há obstáculo entre inimigo e jogador (usa origem no chest/head do inimigo)
        Vector3 origin = transform.position + Vector3.up * 1.0f; // ajuste se necessário
        Vector3 targetPos = player.position + Vector3.up * 1.0f;
        Vector3 dir = (targetPos - origin).normalized;
        dist = Vector3.Distance(origin, targetPos);

        if (Physics.Raycast(origin, dir, out RaycastHit hit, dist, obstacleMask))
        {
            // atingiu um obstáculo antes de chegar no jogador -> não viu
            return false;
        }

        if (playerDead)
        {
            return false;
        }
        // nenhum obstáculo bloqueando -> viu o jogador
        return true;
    }

    void Patrol()
    {
        if (waypoints == null || waypoints.Length == 0)
        {
            // sem waypoints: para
            rb.velocity = new Vector3(0f, rb.velocity.y, 0f);
            return;
        }

        Transform target = waypoints[currentWaypointIndex];
        Vector3 direction = target.position - transform.position;
        direction.y = 0f;

        if (direction.magnitude > waypointThreshold)
        {
            Vector3 moveDir = direction.normalized * patrolSpeed;
            Vector3 newVel = new Vector3(moveDir.x, rb.velocity.y, moveDir.z);
            rb.velocity = newVel;

            // rotação suave apenas no eixo Y
            if (moveDir != Vector3.zero)
            {
                Quaternion targetRot = Quaternion.LookRotation(moveDir);
                transform.rotation = Quaternion.Slerp(transform.rotation, targetRot, rotationSpeed * Time.fixedDeltaTime);
            }
        }
        else
        {
            // avançar waypoint
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

    void ChasePlayer()
    {
        if (player == null) return;

        Vector3 direction = (player.position - transform.position);
        direction.y = 0f;
        Vector3 moveDir = direction.normalized * chaseSpeed;
        Vector3 newVel = new Vector3(moveDir.x, rb.velocity.y, moveDir.z);
        rb.velocity = newVel;

        if (moveDir != Vector3.zero)
        {
            Quaternion targetRot = Quaternion.LookRotation(moveDir);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRot, rotationSpeed * Time.fixedDeltaTime);
        }
    }

    // Gizmos para visualizar o FOV no editor
    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, viewDistance);

        Vector3 forward = transform.forward;
        Quaternion leftRot = Quaternion.Euler(0, -fieldOfViewAngle * 0.5f, 0);
        Quaternion rightRot = Quaternion.Euler(0, fieldOfViewAngle * 0.5f, 0);

        Vector3 leftDir = leftRot * forward;
        Vector3 rightDir = rightRot * forward;

        Gizmos.color = Color.red;
        Gizmos.DrawLine(transform.position, transform.position + leftDir * viewDistance);
        Gizmos.DrawLine(transform.position, transform.position + rightDir * viewDistance);

        // se tiver player e o inimigo enxergar, desenha linha
        if (Application.isPlaying && player != null)
        {
            bool v = CheckPlayerInSight();
            Gizmos.color = v ? Color.green : Color.gray;
            Gizmos.DrawLine(transform.position + Vector3.up * 1.0f, player.position + Vector3.up * 1.0f);
        }
    }
    
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            state = State.Patrol;
            playerDead = true;
            menuManager.Perdeu();
        }
    }
}
