using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
public class EnemyController : MonoBehaviour
{
    [SerializeField] private float viewRange = 35f;
    [SerializeField] private float attackRange = 18f;
    [SerializeField] private float damage = 10f;
    [SerializeField] private float fireInterval = 1.2f;
    [SerializeField] private Transform firePoint;
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip shootClip;

    private NavMeshAgent agent;
    private Transform player;
    private PlayerHealth playerHealth;
    private float nextFireTime;
    private bool dead;

    private void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
    }

    private void Start()
    {
        GameObject playerObject = GameObject.FindGameObjectWithTag("Player");
        if (playerObject != null)
        {
            player = playerObject.transform;
            playerHealth = playerObject.GetComponent<PlayerHealth>();
        }

        GameManager.Instance.RegisterEnemy(this);
    }

    private void Update()
    {
        if (dead || player == null || Time.timeScale <= 0f) return;

        Vector3 target = player.position;
        float distance = Vector3.Distance(transform.position, target);
        if (distance > viewRange) return;

        if (agent.isOnNavMesh)
        {
            agent.SetDestination(target);
            agent.isStopped = distance <= attackRange * 0.75f;
        }
        else
        {
            Vector3 flatTarget = new Vector3(target.x, transform.position.y, target.z);
            transform.position = Vector3.MoveTowards(transform.position, flatTarget, agent.speed * Time.deltaTime);
        }

        Vector3 lookTarget = new Vector3(target.x, transform.position.y, target.z);
        transform.LookAt(lookTarget);

        if (distance <= attackRange && Time.time >= nextFireTime)
        {
            FireAtPlayer();
        }
    }

    public void Die()
    {
        if (dead) return;

        dead = true;
        GameManager.Instance.EnemyDefeated(this);
        Destroy(gameObject);
    }

    private void FireAtPlayer()
    {
        nextFireTime = Time.time + fireInterval;
        if (audioSource != null && shootClip != null) audioSource.PlayOneShot(shootClip);

        Vector3 origin = firePoint != null ? firePoint.position : transform.position + Vector3.up * 1.3f;
        Vector3 target = player.position + Vector3.up * 1.1f;
        if (Physics.Raycast(origin, (target - origin).normalized, out RaycastHit hit, attackRange))
        {
            if (hit.collider.TryGetComponent(out PlayerHealth health))
            {
                health.TakeDamage(damage);
            }
        }
        else if (playerHealth != null)
        {
            playerHealth.TakeDamage(damage);
        }
    }
}
