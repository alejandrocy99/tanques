using UnityEngine;

public class EnemigoController : MonoBehaviour
{
    public Transform playerTank; // Posición del tanque del jugador
    public Transform[] waypoints; // Puntos de referencia para patrullar
    public Transform turret; // Objeto de la torreta
    public float patrolSpeed = 5f;
    public float chaseSpeed = 7f;
    public float attackRange = 100f;
    public float detectionRange = 300f;
    public float fireRate = 1f;
    public GameObject bulletPrefab;
    public Transform bulletSpawnPoint;
    public float health = 100f;

    private int currentWaypointIndex = 0;
    private float nextFireTime = 0f;
    private enum State { Patrol, Chase, Attack, Dead }
    private State currentState = State.Patrol;

    private bool isAlive = true;
    private Vector3 lastMoveDirection = Vector3.zero;

    void Update()
    {
        if (!isAlive) return;

        switch (currentState)
        {
            case State.Patrol:
                UpdatePatrolState();
                break;
            case State.Chase:
                UpdateChaseState();
                break;
            case State.Attack:
                UpdateAttackState();
                break;
            case State.Dead:
                UpdateDeadState();
                break;
        }
    }

    private void UpdatePatrolState()
    {
        if (waypoints.Length == 0) return;

        Vector3 targetPosition = waypoints[currentWaypointIndex].position;

        // Mover hacia el siguiente punto de referencia
        lastMoveDirection = MoveTowards(targetPosition, patrolSpeed);

        // Rotar la torreta hacia la dirección del movimiento
        RotateTurretTowardsMovement();

        // Cambiar al siguiente punto de referencia si llegamos al actual
        if (Vector3.Distance(transform.position, targetPosition) <= 100f)
        {
            FindNextPoint();
        }

        // Cambiar al estado de persecución si el jugador está cerca
        if (Vector3.Distance(transform.position, playerTank.position) <= detectionRange)
        {
            currentState = State.Chase;
        }
    }

    private void UpdateChaseState()
    {
        Vector3 playerPosition = playerTank.position;

        // Mover hacia el tanque del jugador
        lastMoveDirection = MoveTowards(playerPosition, chaseSpeed);

        // Rotar la torreta hacia el jugador
        RotateTurretTowardsPlayer();

        // Cambiar al estado de ataque si estamos lo suficientemente cerca
        if (Vector3.Distance(transform.position, playerTank.position) <= attackRange)
        {
            currentState = State.Attack;
        }
        // Volver al estado de patrulla si el jugador se escapa
        else if (Vector3.Distance(transform.position, playerTank.position) > detectionRange)
        {
            currentState = State.Patrol;
        }
    }

    private void UpdateAttackState()
    {
        Vector3 playerPosition = playerTank.position;

        // Mover hacia el tanque del jugador
        lastMoveDirection = MoveTowards(playerPosition, chaseSpeed);

        // Girar la torreta hacia el jugador
        RotateTurretTowardsPlayer();

        // Disparar si es el momento adecuado
        if (Time.time >= nextFireTime)
        {
            Fire();
            nextFireTime = Time.time + 1f / fireRate; // Control del tiempo entre disparos
        }

        // Cambiar al estado de persecución si el jugador se aleja
        if (Vector3.Distance(transform.position, playerTank.position) > attackRange)
        {
            currentState = State.Chase;
        }
    }

    private void UpdateDeadState()
    {
        // Lógica de explosión (opcional)
        Destroy(gameObject, 2f);
    }

    private Vector3 MoveTowards(Vector3 target, float speed)
    {
        Vector3 direction = (target - transform.position).normalized;
        transform.Translate(direction * speed * Time.deltaTime, Space.World);
        return direction;
    }

    private void RotateTurretTowardsPlayer()
    {
        Vector3 directionToPlayer = (playerTank.position - turret.position).normalized;
        directionToPlayer.y = 0; // Ignorar la altura
        Quaternion lookRotation = Quaternion.LookRotation(directionToPlayer);
        turret.rotation = Quaternion.Slerp(turret.rotation, lookRotation, Time.deltaTime * 5f);
    }

    private void RotateTurretTowardsMovement()
    {
        if (lastMoveDirection != Vector3.zero)
        {
            Quaternion lookRotation = Quaternion.LookRotation(lastMoveDirection);
            turret.rotation = Quaternion.Slerp(turret.rotation, lookRotation, Time.deltaTime * 5f);
        }
    }

    private void FindNextPoint()
    {
        currentWaypointIndex = Random.Range(0, waypoints.Length);
    }

    private void Fire()
    {
        if (bulletPrefab != null && bulletSpawnPoint != null)
        {
            // Instanciar la bala
            GameObject bullet = Instantiate(bulletPrefab, bulletSpawnPoint.position, bulletSpawnPoint.rotation);
            Rigidbody rb = bullet.GetComponent<Rigidbody>();
            if (rb != null)
            {
                rb.velocity = bulletSpawnPoint.forward * 20f; // Velocidad de la bala
            }
        }
    }

    public void TakeDamage(float damage)
    {
        health -= damage;
        if (health <= 0 && isAlive)
        {
            isAlive = false;
            currentState = State.Dead;
        }
    }

    private void OnDrawGizmosSelected()
    {
        // Visualizar el rango de detección
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, detectionRange);

        // Visualizar el rango de ataque
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRange);
    }
}
