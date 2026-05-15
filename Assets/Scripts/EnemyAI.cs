using System.Collections;
using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
public class EnemyAI : MonoBehaviour
{
    [Header("Detection Radii")]
    [Tooltip("If player/rock enters this radius the enemy immediately charges.")]
    public float innerRadius = 5f;

    [Tooltip("If player/rock enters this radius the enemy starts walking toward it.")]
    public float outerRadius = 10f;

    [Header("References")]
    [Tooltip("The player's Transform. Drag the player GameObject here.")]
    public Transform player;

    [Header("Movement Speeds")]
    public float roamSpeed = 1f;
    public float investigateSpeed = 2f;
    public float chaseSpeed = 3f;

    [Header("Roaming")]
    [Tooltip("How far from the enemy's starting position random roam points are chosen.")]
    public float roamRadius = 20f;

    [Tooltip("How long the enemy pauses at a roam waypoint before picking a new one.")]
    public float roamWaitTime = 2f;

    [Header("Rock Investigation")]
    [Tooltip("How long the enemy spends inspecting a rock before returning to roam.")]
    public float rockInspectDuration = 10f;

    public enum State { Roaming, Investigating, Chasing, InspectRock }

    [Header("Debug (read-only)")]
    [SerializeField] private State currentState = State.Roaming;

    [Header("Debug Visuals")]
    [Tooltip("Show the inner and outer radii as circles in the Game view at runtime.")]
    public bool showDebugRadii = false;

    [Tooltip("How many segments make up each debug circle. Higher = smoother.")]
    public int radiusSegments = 64;

    public Color innerRadiusColor = Color.red;
    public Color outerRadiusColor = Color.yellow;

    private LineRenderer innerLineRenderer;
    private LineRenderer outerLineRenderer;

    private NavMeshAgent agent;
    private Vector3 startPosition;
    private Vector3 roamTarget;
    private Vector3 investigateTarget;  // last known position of distraction
    private bool isWaitingAtWaypoint = false;
    private Coroutine roamWaitCoroutine;
    private Coroutine rockInspectCoroutine;

    private void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
        startPosition = transform.position;
        SetupDebugRenderers();
    }

    private void Start()
    {
        EnterRoaming();
    }

    private void Update()
    {
        switch (currentState)
        {
            case State.Roaming: UpdateRoaming(); break;
            case State.Investigating: UpdateInvestigating(); break;
            case State.Chasing: UpdateChasing(); break;
            case State.InspectRock: UpdateInspectRock(); break;
        }

        UpdateDebugRenderers();
    }

    // ROAMING

    private void EnterRoaming()
    {
        currentState = State.Roaming;
        agent.speed = roamSpeed;
        isWaitingAtWaypoint = false;
        PickNewRoamTarget();
    }

    private void UpdateRoaming()
    {
        // Check if player has entered either radius
        float distToPlayer = Vector3.Distance(transform.position, player.position);

        if (distToPlayer <= innerRadius)
        {
            EnterChasing();
            return;
        }
        if (distToPlayer <= outerRadius)
        {
            EnterInvestigating(player.position);
            return;
        }

        // Continue roaming
        if (!isWaitingAtWaypoint && HasReachedDestination())
        {
            roamWaitCoroutine = StartCoroutine(WaitThenRoam());
        }
    }

    private void PickNewRoamTarget()
    {
        // Try up to 10 times to find a valid NavMesh point
        for (int i = 0; i < 10; i++)
        {
            Vector3 randomDir = Random.insideUnitSphere * roamRadius;
            randomDir += startPosition;
            randomDir.y = transform.position.y;

            if (NavMesh.SamplePosition(randomDir, out NavMeshHit hit, roamRadius, NavMesh.AllAreas))
            {
                roamTarget = hit.position;
                agent.SetDestination(roamTarget);
                return;
            }
        }
    }

    private IEnumerator WaitThenRoam()
    {
        isWaitingAtWaypoint = true;
        yield return new WaitForSeconds(roamWaitTime);
        isWaitingAtWaypoint = false;
        PickNewRoamTarget();
    }

    // INVESTIGATING

    public void AlertOuter(Vector3 targetPosition)
    {
        // Don't interrupt a chase for an outer-radius alert
        if (currentState == State.Chasing) return;

        EnterInvestigating(targetPosition);
    }

    private void EnterInvestigating(Vector3 targetPosition)
    {
        StopRoamCoroutine();
        StopRockInspectCoroutine();

        currentState = State.Investigating;
        agent.speed = investigateSpeed;
        investigateTarget = targetPosition;
        agent.SetDestination(investigateTarget);
    }

    private void UpdateInvestigating()
    {
        // Re-evaluate player proximity every frame
        float distToPlayer = Vector3.Distance(transform.position, player.position);

        if (distToPlayer <= innerRadius)
        {
            EnterChasing();
            return;
        }

        // Keep tracking player if they are the source
        if (distToPlayer <= outerRadius)
        {
            investigateTarget = player.position;
            agent.SetDestination(investigateTarget);
        }

        // Reached the investigate position with no player nearby -> roam
        if (HasReachedDestination() && distToPlayer > outerRadius)
        {
            EnterRoaming();
        }
    }

    // CHASING

    private void EnterChasing()
    {
        StopRoamCoroutine();
        StopRockInspectCoroutine();

        currentState = State.Chasing;
        agent.speed = chaseSpeed;
    }

    private void UpdateChasing()
    {
        float distToPlayer = Vector3.Distance(transform.position, player.position);

        // Still within inner radius - keep charging
        if (distToPlayer <= innerRadius)
        {
            agent.SetDestination(player.position);
            return;
        }

        // Left inner but still in outer - downgrade to investigating
        if (distToPlayer <= outerRadius)
        {
            EnterInvestigating(player.position);
            return;
        }

        // Player fully escaped - go back to roaming
        EnterRoaming();
    }

    // INSPECTING ROCK

    public void AlertInner(Vector3 targetPosition)
    {
        StopRoamCoroutine();
        StopRockInspectCoroutine();

        currentState = State.InspectRock;
        agent.speed = chaseSpeed;  // sprint to the rock
        investigateTarget = targetPosition;
        agent.SetDestination(investigateTarget);
    }

    private void UpdateInspectRock()
    {
        // Check for player intrusion even while inspecting rock
        float distToPlayer = Vector3.Distance(transform.position, player.position);
        if (distToPlayer <= innerRadius)
        {
            EnterChasing();
            return;
        }
        if (distToPlayer <= outerRadius)
        {
            EnterInvestigating(player.position);
            return;
        }

        // Walk to rock position; once arrived, begin timed inspection
        if (HasReachedDestination())
        {
            if (rockInspectCoroutine == null)
                rockInspectCoroutine = StartCoroutine(InspectRockRoutine());
        }
    }

    private IEnumerator InspectRockRoutine()
    {
        // Enemy "sniffs around" the rock position for rockInspectDuration seconds
        yield return new WaitForSeconds(rockInspectDuration);
        rockInspectCoroutine = null;
        EnterRoaming();
    }

    // HELPERS

    private bool HasReachedDestination()
    {
        if (agent.pathPending) return false;
        if (agent.remainingDistance > agent.stoppingDistance) return false;
        if (agent.hasPath && agent.velocity.sqrMagnitude > 0.01f) return false;
        return true;
    }

    private void StopRoamCoroutine()
    {
        if (roamWaitCoroutine != null)
        {
            StopCoroutine(roamWaitCoroutine);
            roamWaitCoroutine = null;
            isWaitingAtWaypoint = false;
        }
    }

    private void StopRockInspectCoroutine()
    {
        if (rockInspectCoroutine != null)
        {
            StopCoroutine(rockInspectCoroutine);
            rockInspectCoroutine = null;
        }
    }

    // RUNTIME DEBUG VISUALS

    private void SetupDebugRenderers()
    {
        innerLineRenderer = CreateCircleRenderer("InnerRadiusDebug", innerRadiusColor);
        outerLineRenderer = CreateCircleRenderer("OuterRadiusDebug", outerRadiusColor);
    }

    private LineRenderer CreateCircleRenderer(string objName, Color color)
    {
        GameObject obj = new GameObject(objName);
        obj.transform.SetParent(transform);
        obj.transform.localPosition = Vector3.zero;

        LineRenderer lr = obj.AddComponent<LineRenderer>();
        lr.useWorldSpace = true;
        lr.loop = true;
        lr.positionCount = radiusSegments;
        lr.startWidth = 0.05f;
        lr.endWidth = 0.05f;
        lr.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
        lr.receiveShadows = false;

        lr.material = new Material(Shader.Find("Unlit/Color"));
        lr.material.color = color;

        lr.enabled = false;
        return lr;
    }

    private void UpdateDebugRenderers()
    {
        if (innerLineRenderer == null || outerLineRenderer == null) return;

        innerLineRenderer.enabled = showDebugRadii;
        outerLineRenderer.enabled = showDebugRadii;

        if (!showDebugRadii) return;

        DrawCircle(innerLineRenderer, innerRadius);
        DrawCircle(outerLineRenderer, outerRadius);
    }

    private void DrawCircle(LineRenderer lr, float radius)
    {
        float angleStep = 360f / radiusSegments;
        float y = transform.position.y + 0.05f; // slightly above ground

        for (int i = 0; i < radiusSegments; i++)
        {
            float angle = i * angleStep * Mathf.Deg2Rad;
            float x = transform.position.x + Mathf.Cos(angle) * radius;
            float z = transform.position.z + Mathf.Sin(angle) * radius;
            lr.SetPosition(i, new Vector3(x, y, z));
        }
    }

#if UNITY_EDITOR
    private void OnDrawGizmosSelected()
    {
        // Inner radius - red
        Gizmos.color = new Color(1f, 0.1f, 0.1f, 0.35f);
        Gizmos.DrawSphere(transform.position, innerRadius);

        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, innerRadius);

        // Outer radius - yellow
        Gizmos.color = new Color(1f, 0.9f, 0.1f, 0.10f);
        Gizmos.DrawSphere(transform.position, outerRadius);

        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, outerRadius);
    }
#endif
}
