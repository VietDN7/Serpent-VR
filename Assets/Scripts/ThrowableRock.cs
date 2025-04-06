using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

public class ThrowableRock : MonoBehaviour
{
    [Header("Physics Settings")]
    [SerializeField] private float throwForceMultiplier = 1.5f;
    [SerializeField] private float maxThrowVelocity = 20f;
    
    [Header("Area of Effect Settings")]
    [SerializeField] private float innerRadius = 5f;
    [SerializeField] private float outerRadius = 15f;
    [SerializeField] private float aoeLifetime = 10f;
    [SerializeField] private GameObject aoePrefab; // Optional visual representation
    [SerializeField] private LayerMask enemyLayer;
    [SerializeField] private Color innerCircleColor = new Color(1f, 0f, 0f, 0.3f);
    [SerializeField] private Color outerCircleColor = new Color(0.5f, 0.5f, 1f, 0.2f);
    [SerializeField] private AudioClip rockHitSound;
    
    private Rigidbody rb;
    private UnityEngine.XR.Interaction.Toolkit.Interactables.XRGrabInteractable grabInteractable;
    private Vector3 previousPosition;
    private Quaternion previousRotation;
    private List<Vector3> velocityList = new List<Vector3>();
    private List<Vector3> angularVelocityList = new List<Vector3>();
    private int velocityHistorySize = 10;
    private bool hasLanded = false;
    private bool aoeActive = false;
    
    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        grabInteractable = GetComponent<UnityEngine.XR.Interaction.Toolkit.Interactables.XRGrabInteractable>();
        
        if (rb == null)
        {
            rb = gameObject.AddComponent<Rigidbody>();
            Debug.Log("Rigidbody component added to rock");
        }
        
        if (grabInteractable == null)
        {
            grabInteractable = gameObject.AddComponent<UnityEngine.XR.Interaction.Toolkit.Interactables.XRGrabInteractable>();
            Debug.Log("XRGrabInteractable component added to rock");
        }
        
        // Realistic rock physics...?
        rb.mass = 5f;
        rb.drag = 0.5f;
        rb.angularDrag = 0.5f;
        rb.collisionDetectionMode = CollisionDetectionMode.Continuous;
        
        // Grab interactable configuration
        grabInteractable.throwOnDetach = true;
        grabInteractable.trackPosition = true;
        grabInteractable.trackRotation = true;
        grabInteractable.throwSmoothingDuration = 0.1f;
        grabInteractable.throwVelocityScale = throwForceMultiplier;
        grabInteractable.throwAngularVelocityScale = 0.5f;
        
        // Event handlers
        grabInteractable.selectEntered.AddListener(OnGrab);
        grabInteractable.selectExited.AddListener(OnRelease);
        
        // If no AoE prefab is assigned, we'll create one at runtime
        if (aoePrefab == null)
        {
            CreateDefaultAoePrefab();
        }
    }
    
    private void CreateDefaultAoePrefab()
    {
        aoePrefab = new GameObject("DefaultAoE");
        aoePrefab.SetActive(false);
    }
    
    private void OnGrab(SelectEnterEventArgs args)
    {
        // Reset landing state when grabbed
        hasLanded = false;
        aoeActive = false;
        
        // Disable gravity while held to make it feel more responsive
        rb.useGravity = false;
        rb.isKinematic = true;
        
        // Clear velocity history
        velocityList.Clear();
        angularVelocityList.Clear();
        
        previousPosition = transform.position;
        previousRotation = transform.rotation;
    }
    
    private void OnRelease(SelectExitEventArgs args)
    {
        rb.useGravity = true;
        rb.isKinematic = false;
        
        // Calculate throw velocity based on motion history
        Vector3 finalVelocity = CalculateAverageVelocity();
        Vector3 finalAngularVelocity = CalculateAverageAngularVelocity();
        
        // Apply velocity
        rb.velocity = Vector3.ClampMagnitude(finalVelocity * throwForceMultiplier, maxThrowVelocity);
        rb.angularVelocity = finalAngularVelocity;
        
        // Add some noise to make throws feel more natural
        rb.AddTorque(Random.insideUnitSphere * 0.5f, ForceMode.Impulse);
    }
    
    void FixedUpdate()
    {
        if (grabInteractable.isSelected)
        {
            // Track velocity while held
            TrackVelocity();
        }
        else if (!hasLanded && !rb.isKinematic && rb.velocity.magnitude < 0.1f)
        {
            // Rock has come to rest after being thrown
            if (!aoeActive)
            {
                hasLanded = true;
                StartCoroutine(ActivateAreaOfEffect());
            }
        }
    }
    
    private void TrackVelocity()
    {
        Vector3 currentVelocity = (transform.position - previousPosition) / Time.fixedDeltaTime;
        velocityList.Add(currentVelocity);
        
        // Calculate angular velocity
        Quaternion deltaRotation = transform.rotation * Quaternion.Inverse(previousRotation);
        deltaRotation.ToAngleAxis(out float angle, out Vector3 axis);
        if (angle > 180) angle -= 360;
        Vector3 angularVelocity = axis * (angle * Mathf.Deg2Rad / Time.fixedDeltaTime);
        angularVelocityList.Add(angularVelocity);
        
        // Maintain fixed history size
        if (velocityList.Count > velocityHistorySize)
        {
            velocityList.RemoveAt(0);
            angularVelocityList.RemoveAt(0);
        }
        
        previousPosition = transform.position;
        previousRotation = transform.rotation;
    }
    
    private Vector3 CalculateAverageVelocity()
    {
        Vector3 sum = Vector3.zero;
        int count = Mathf.Min(velocityList.Count, 5); // Use only the most recent entries
        
        if (count == 0) return Vector3.zero;
        
        for (int i = velocityList.Count - 1; i >= velocityList.Count - count; i--)
        {
            // Weight recent velocities more heavily
            float weight = (float)(i - (velocityList.Count - count)) / count;
            sum += velocityList[i] * (0.5f + weight * 0.5f);
        }
        
        return sum / count;
    }
    
    private Vector3 CalculateAverageAngularVelocity()
    {
        Vector3 sum = Vector3.zero;
        int count = Mathf.Min(angularVelocityList.Count, 5);
        
        if (count == 0) return Vector3.zero;
        
        for (int i = angularVelocityList.Count - 1; i >= angularVelocityList.Count - count; i--)
        {
            sum += angularVelocityList[i];
        }
        
        return sum / count;
    }
    
    private void OnCollisionEnter(Collision collision)
    {
        // Play sound effect when rock hits something
        if (collision.relativeVelocity.magnitude > 1.0f)
        {
            // Calculate volume based on impact force (between 0.1 and 1.0)
            float volume = Mathf.Clamp(collision.relativeVelocity.magnitude / 10f, 0.1f, 1.0f);
    
            // Play the sound at the collision point
            AudioSource.PlayClipAtPoint(rockHitSound, transform.position, volume);

            // If the collision is hard enough, activate AoE immediately
            if (collision.relativeVelocity.magnitude > 5.0f && !aoeActive)
            {
                hasLanded = true;
                StartCoroutine(ActivateAreaOfEffect());
            }
        }
    }
    
    private IEnumerator ActivateAreaOfEffect()
    {
        aoeActive = true;
        
        // Create visual representation of AoE
        GameObject aoeInstance = Instantiate(aoePrefab, transform.position, Quaternion.identity);
        aoeInstance.SetActive(true);
        
        // Create inner and outer circle visualizations
        CreateCircleVisual(aoeInstance, innerRadius, innerCircleColor);
        CreateCircleVisual(aoeInstance, outerRadius, outerCircleColor);
        
        // Find all enemies in range
        NotifyEnemiesInRange();
        
        // Wait for AoE duration
        float remainingTime = aoeLifetime;
        while (remainingTime > 0)
        {
            yield return new WaitForSeconds(1.0f);
            remainingTime -= 1.0f;
            
            // Update enemy awareness every second
            UpdateEnemyAwareness();
        }
        
        // Clean up
        Destroy(aoeInstance);
        aoeActive = false;
    }
    
    private void CreateCircleVisual(GameObject parent, float radius, Color color)
    {
        GameObject circle = new GameObject("Circle");
        circle.transform.SetParent(parent.transform);
        circle.transform.localPosition = Vector3.zero;
        
        // Create a line renderer for the circle
        LineRenderer lineRenderer = circle.AddComponent<LineRenderer>();
        lineRenderer.useWorldSpace = false;
        lineRenderer.startWidth = 0.1f;
        lineRenderer.endWidth = 0.1f;
        lineRenderer.positionCount = 51;
        lineRenderer.material = new Material(Shader.Find("Sprites/Default"));
        lineRenderer.startColor = color;
        lineRenderer.endColor = color;
        
        // Create points around circle
        float angle = 0f;
        for (int i = 0; i < 51; i++)
        {
            float x = Mathf.Sin(Mathf.Deg2Rad * angle) * radius;
            float z = Mathf.Cos(Mathf.Deg2Rad * angle) * radius;
            lineRenderer.SetPosition(i, new Vector3(x, 0.05f, z)); // Slightly above ground
            angle += (360f / 50f);
        }
    }
    
    private void NotifyEnemiesInRange()
    {
        // Find all enemies in outer radius
        Collider[] outerColliders = Physics.OverlapSphere(transform.position, outerRadius, enemyLayer);
        foreach (Collider enemyCollider in outerColliders)
        {
            EnemyAI enemy = enemyCollider.GetComponent<EnemyAI>();
            if (enemy != null)
            {
                float distance = Vector3.Distance(transform.position, enemyCollider.transform.position);
                
                if (distance <= innerRadius)
                {
                    // Inner circle - alert enemy to player's position
                    enemy.AlertToPlayerPosition(GameObject.FindGameObjectWithTag("Player").transform.position);
                }
                else
                {
                    // Outer circle - investigate rock location
                    enemy.InvestigatePosition(transform.position);
                }
            }
        }
    }
    
    private void UpdateEnemyAwareness()
    {
        // Keep updating enemies that might enter the area during the AoE lifetime
        NotifyEnemiesInRange();
    }
}