using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class EnemyAI : MonoBehaviour
{
    public NavMeshAgent agent;

    public Transform player;

    public LayerMask whatIsGround, whatIsPlayer;

    public float health;

    // Patroling
    public Vector3 walkPoint;
    bool walkPointSet;
    public float walkPointRange;

    // Attacking
    public float timeBetweenAttacks;
    bool alreadyAttacked;
    public GameObject projectile;

    // States
    public float sightRange, attackRange;
    public bool playerInSightRange, playerInAttackRange;
    
    // Distraction States
    private bool investigating = false;
    private Vector3 investigationPoint;
    private bool playerPositionKnown = false;
    private Vector3 lastKnownPlayerPosition;
    public float investigationTime = 10f;

    private void Awake()
    {
        player = GameObject.Find("PlayerObject").transform;
        agent = GetComponent<NavMeshAgent>();
    }

    private void Update()
    {
        // Check for sight and attack range
        playerInSightRange = Physics.CheckSphere(transform.position, sightRange, whatIsPlayer);
        playerInAttackRange = Physics.CheckSphere(transform.position, attackRange, whatIsPlayer);

        // Determine current state and action
        if (playerInAttackRange && playerInSightRange)
        {
            // Direct sight is highest priority
            investigating = false;
            playerPositionKnown = false;
            AttackPlayer();
        }
        else if (playerInSightRange)
        {
            // Chase if we can see player
            investigating = false;
            playerPositionKnown = false;
            ChasePlayer();
        }
        else if (playerPositionKnown)
        {
            // Go to last known player position from rock inner circle
            agent.SetDestination(lastKnownPlayerPosition);
            
            // If we reached the position and still don't see player
            if (Vector3.Distance(transform.position, lastKnownPlayerPosition) < 2f)
            {
                playerPositionKnown = false;
                // Start patrolling the area
                walkPointSet = false;
            }
        }
        else if (investigating)
        {
            // Go investigate noise from rock outer circle
            agent.SetDestination(investigationPoint);
            
            // If we reached the investigation point
            if (Vector3.Distance(transform.position, investigationPoint) < 2f)
            {
                investigating = false;
                // Start patrolling the area
                walkPointSet = false;
            }
        }
        else
        {
            // Default patrol behavior
            Patroling();
        }
    }

    private void Patroling()
    {
        if (!walkPointSet) SearchWalkPoint();

        if (walkPointSet)
            agent.SetDestination(walkPoint);

        Vector3 distanceToWalkPoint = transform.position - walkPoint;

        // Walkpoint reached
        if (distanceToWalkPoint.magnitude < 1f)
            walkPointSet = false;
    }
    
    private void SearchWalkPoint()
    {
        // Calculate random point in range
        float randomZ = Random.Range(-walkPointRange, walkPointRange);
        float randomX = Random.Range(-walkPointRange, walkPointRange);

        walkPoint = new Vector3(transform.position.x + randomX, transform.position.y, transform.position.z + randomZ);

        if (Physics.Raycast(walkPoint, -transform.up, 2f, whatIsGround))
            walkPointSet = true;
    }

    private void ChasePlayer()
    {
        agent.SetDestination(player.position);
    }

    private void AttackPlayer()
    {
        // Make sure enemy doesn't move
        agent.SetDestination(transform.position);

        transform.LookAt(player);

        if (!alreadyAttacked)
        {
            // Attack code here
            Rigidbody rb = Instantiate(projectile, transform.position, Quaternion.identity).GetComponent<Rigidbody>();
            rb.AddForce(transform.forward * 32f, ForceMode.Impulse);
            rb.AddForce(transform.up * 8f, ForceMode.Impulse);

            alreadyAttacked = true;
            Invoke(nameof(ResetAttack), timeBetweenAttacks);
        }
    }
    
    private void ResetAttack()
    {
        alreadyAttacked = false;
    }

    public void TakeDamage(int damage)
    {
        health -= damage;

        if (health <= 0) Invoke(nameof(DestroyEnemy), 0.5f);
    }
    
    private void DestroyEnemy()
    {
        Destroy(gameObject);
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, attackRange);
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position, sightRange);
        
        // Add visualization for investigation state
        if (investigating)
        {
            Gizmos.color = Color.blue;
            Gizmos.DrawLine(transform.position, investigationPoint);
            Gizmos.DrawWireSphere(investigationPoint, 1f);
        }
        
        // Add visualization for known player position
        if (playerPositionKnown)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawLine(transform.position, lastKnownPlayerPosition);
            Gizmos.DrawWireSphere(lastKnownPlayerPosition, 1f);
        }
    }
    
    // IMPLEMENTATION OF THE ROCK DISTRACTION METHODS
    
    // Called when the enemy is within the inner circle of a thrown rock
    // This makes the enemy aware of the player's position
    public void AlertToPlayerPosition(Vector3 playerPosition)
    {
        // Store the player's position
        lastKnownPlayerPosition = playerPosition;
        playerPositionKnown = true;
        investigating = false;
        
        // Optional - alert nearby enemies
        AlertNearbyEnemies(playerPosition);
        
        // Visual/audio feedback
        StartCoroutine(AlertedFeedback());
    }
    
    // Called when the enemy is within the outer circle of a thrown rock
    // This makes the enemy investigate the rock's position
    public void InvestigatePosition(Vector3 position)
    {
        // Only investigate if not already chasing or attacking player
        if (!playerInSightRange && !playerInAttackRange && !playerPositionKnown)
        {
            investigationPoint = position;
            investigating = true;
            walkPointSet = false;
            
            // Set a timeout for investigation
            StartCoroutine(InvestigationTimeout());
            
            // Visual/audio feedback
            StartCoroutine(CuriousFeedback());
        }
    }
    
    private IEnumerator InvestigationTimeout()
    {
        yield return new WaitForSeconds(investigationTime);
        if (investigating)
        {
            investigating = false;
        }
    }
    
    private void AlertNearbyEnemies(Vector3 playerPos)
    {
        // Find other enemies within a certain radius and alert them too
        Collider[] nearbyEnemies = Physics.OverlapSphere(transform.position, 15f, LayerMask.GetMask("Enemy"));
        foreach (var enemyCollider in nearbyEnemies)
        {
            if (enemyCollider.gameObject != gameObject)
            {
                EnemyAI enemy = enemyCollider.GetComponent<EnemyAI>();
                if (enemy != null)
                {
                    enemy.AlertToPlayerPosition(playerPos);
                }
            }
        }
    }
    
    private IEnumerator AlertedFeedback()
    {
        // Example: Change color to blue temporarily to show alertness
        Renderer renderer = GetComponent<Renderer>();
        if (renderer != null)
        {
            Color originalColor = renderer.material.color;
            renderer.material.color = Color.blue;
            yield return new WaitForSeconds(1.0f);
            renderer.material.color = originalColor;
        }
    }
    
    private IEnumerator CuriousFeedback()
    {
        // Example: Change color to yellow temporarily to show curiosity
        Renderer renderer = GetComponent<Renderer>();
        if (renderer != null)
        {
            Color originalColor = renderer.material.color;
            renderer.material.color = Color.yellow;
            yield return new WaitForSeconds(1.0f);
            renderer.material.color = originalColor;
        }
    }
}