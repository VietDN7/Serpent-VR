using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class EnemyAI : MonoBehaviour
{
    public NavMeshAgent agent;

    public Transform player;

    public LayerMask whatIsGround, whatIsPlayer, whatIsObstacle;

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
    
    // Vision settings
    public float visionConeAngle = 60f;
    public float visionConeHeightOffset = 1.5f;
    public bool drawVisionGizmos = true;
    
    // Distraction States
    private bool investigating = false;
    private Vector3 investigationPoint;
    private bool playerPositionKnown = false;
    private Vector3 lastKnownPlayerPosition;
    public float investigationTime = 10f;
    
    // Alert indicators
    public GameObject alertIndicator; // For the "!" symbol
    public GameObject suspiciousIndicator; // For the "?" symbol

    private void Awake()
    {
        player = GameObject.Find("XR Origin (XR Rig)").transform;
        agent = GetComponent<NavMeshAgent>();
        
        // Hide indicators at start
        if (alertIndicator != null) alertIndicator.SetActive(false);
        if (suspiciousIndicator != null) suspiciousIndicator.SetActive(false);
    }

    private void Update()
    {
        // Check if player is within attack range
        playerInAttackRange = Physics.CheckSphere(transform.position, attackRange, whatIsPlayer);
        
        // Use raycast vision cone for sight detection
        playerInSightRange = CanSeePlayer();

        // Determine current state and action
        if (playerInAttackRange && playerInSightRange)
        {
            // Direct sight is highest priority
            investigating = false;
            playerPositionKnown = false;
            
            ShowAlertIndicator();
            
            AttackPlayer();
        }
        else if (playerInSightRange)
        {
            // Chase if we can see player
            investigating = false;
            playerPositionKnown = false;
            
            ShowAlertIndicator();
            
            ChasePlayer();
        }
        else if (playerPositionKnown)
        {
            // Go to last known player position from rock inner circle
            ShowAlertIndicator();
            
            agent.SetDestination(lastKnownPlayerPosition);
            
            // If we reached the position and still don't see player
            if (Vector3.Distance(transform.position, lastKnownPlayerPosition) < 2f)
            {
                playerPositionKnown = false;
                // Start patrolling the area
                walkPointSet = false;
                
                HideAllIndicators();
            }
        }
        else if (investigating)
        {
            // Go investigate noise from rock outer circle
            ShowSuspiciousIndicator();
            
            agent.SetDestination(investigationPoint);
            
            // If we reached the investigation point
            if (Vector3.Distance(transform.position, investigationPoint) < 2f)
            {
                investigating = false;
                // Start patrolling the area around the investigation point
                SearchWalkPointNear(investigationPoint);
                
                StartCoroutine(HideIndicatorsAfterDelay(2f));
            }
        }
        else
        {
            HideAllIndicators();
            Patroling();
        }
    }

    private bool CanSeePlayer()
    {
        if (Vector3.Distance(transform.position, player.position) < sightRange)
        {
            // Calculate direction to player
            Vector3 directionToPlayer = (player.position - transform.position).normalized;
            
            // Check if player is within vision cone
            float angleToPlayer = Vector3.Angle(transform.forward, directionToPlayer);
            if (angleToPlayer < visionConeAngle / 2f)
            {
                // Cast ray to check for obstacles between enemy and player
                RaycastHit hit;
                Vector3 rayStart = transform.position + Vector3.up * visionConeHeightOffset;
                
                Debug.DrawRay(rayStart, directionToPlayer * sightRange, Color.red);
                
                if (Physics.Raycast(rayStart, directionToPlayer, out hit, sightRange, whatIsObstacle | whatIsPlayer))
                {
                    // Check if we hit the player or an obstacle first
                    if (hit.transform.gameObject.layer == Mathf.Log(whatIsPlayer.value, 2))
                    {
                        return true;
                    }
                }
            }
        }
        return false;
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
    
    private void SearchWalkPointNear(Vector3 center)
    {
        // Search around the given point rather than current position
        float randomZ = Random.Range(-walkPointRange/2, walkPointRange/2);
        float randomX = Random.Range(-walkPointRange/2, walkPointRange/2);

        walkPoint = new Vector3(center.x + randomX, center.y, center.z + randomZ);

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
            /// Attack code here
            Rigidbody rb = Instantiate(projectile, transform.position + transform.forward + Vector3.up * 1.5f, 
                    Quaternion.identity).GetComponent<Rigidbody>();
            rb.AddForce(transform.forward * 32f, ForceMode.Impulse);
            rb.AddForce(transform.up * 5f, ForceMode.Impulse);
            /// End of attack code

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
    
    private void ShowAlertIndicator()
    {
        if (alertIndicator != null)
        {
            alertIndicator.SetActive(true);
            if (suspiciousIndicator != null) suspiciousIndicator.SetActive(false);
        }
    }
    
    private void ShowSuspiciousIndicator()
    {
        if (suspiciousIndicator != null)
        {
            suspiciousIndicator.SetActive(true);
            if (alertIndicator != null) alertIndicator.SetActive(false);
        }
    }
    
    private void HideAllIndicators()
    {
        if (alertIndicator != null) alertIndicator.SetActive(false);
        if (suspiciousIndicator != null) suspiciousIndicator.SetActive(false);
    }
    
    private IEnumerator HideIndicatorsAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        HideAllIndicators();
    }

    private void OnDrawGizmosSelected()
    {
        if (!drawVisionGizmos) return;
        
        // Attack range
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, attackRange);
        
        // Max sight range
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position, sightRange);
        
        // Vision cone
        Gizmos.color = new Color(1f, 0.92f, 0.016f, 0.5f);
        Vector3 origin = transform.position + Vector3.up * visionConeHeightOffset;
        
        // Draw the vision cone using lines
        float halfAngle = visionConeAngle / 2f;
        int segments = 10;
        
        // Calculate the vision cone
        Vector3 forward = transform.forward * sightRange;
        Quaternion leftRayRotation = Quaternion.AngleAxis(-halfAngle, Vector3.up);
        Quaternion rightRayRotation = Quaternion.AngleAxis(halfAngle, Vector3.up);
        Vector3 leftRayDirection = leftRayRotation * forward;
        Vector3 rightRayDirection = rightRayRotation * forward;
        
        // Draw cone lines
        Gizmos.DrawRay(origin, forward);
        Gizmos.DrawRay(origin, leftRayDirection);
        Gizmos.DrawRay(origin, rightRayDirection);
        
        // Draw arc between the two rays
        Vector3 previousPoint = origin + leftRayDirection;
        for (int i = 0; i < segments; i++)
        {
            float angle = -halfAngle + ((float)(i + 1) / segments) * visionConeAngle;
            Quaternion rotation = Quaternion.AngleAxis(angle, Vector3.up);
            Vector3 direction = rotation * forward;
            Vector3 currentPoint = origin + direction;
            
            Gizmos.DrawLine(previousPoint, currentPoint);
            previousPoint = currentPoint;
        }
        
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
    
    /// Called when the enemy is within the outer circle of a thrown rock
    /// This makes the enemy investigate the rock's position
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
        // Change color to blue temporarily to show alertness
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
        // Change color to yellow temporarily to show curiosity
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