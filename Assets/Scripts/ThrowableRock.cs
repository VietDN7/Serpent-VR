using System.Collections;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(UnityEngine.XR.Interaction.Toolkit.Interactables.XRGrabInteractable))]
public class ThrowableRock : MonoBehaviour
{
    [Header("Throw Settings")]
    [Tooltip("Extra multiplier on top of XRGrabInteractable's Throw Velocity Scale.")]
    public float throwForceMultiplier = 1f;

    [Tooltip("Minimum collision speed required to trigger enemy alerts on landing.")]
    public float minimumAlertSpeed = 0.5f;

    [Header("State (read-only)")]
    [SerializeField] private RockState state = RockState.OnGround;

    public enum RockState { OnGround, Held, InFlight, Landed }

    private Rigidbody rb;
    private UnityEngine.XR.Interaction.Toolkit.Interactables.XRGrabInteractable grabInteractable;
    private bool hasAlerted = false;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        grabInteractable = GetComponent<UnityEngine.XR.Interaction.Toolkit.Interactables.XRGrabInteractable>();

        // Hook into XRGrabInteractable events
        grabInteractable.selectEntered.AddListener(OnGrabbed);
        grabInteractable.selectExited.AddListener(OnReleased);

        // Start frozen on the ground
        FreezeRigidbody(true);
    }

    private void OnDestroy()
    {
        grabInteractable.selectEntered.RemoveListener(OnGrabbed);
        grabInteractable.selectExited.RemoveListener(OnReleased);
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (state != RockState.InFlight) return;
        if (collision.relativeVelocity.magnitude < minimumAlertSpeed) return;

        Land();
    }

    // XR GRAB INTERACTIONS

    private void OnGrabbed(SelectEnterEventArgs args)
    {
        state = RockState.Held;
        hasAlerted = false;

        // Unfreeze immediately so XRGrabInteractable can take full control of the Rigidbody
        FreezeRigidbody(false);
    }

    private void OnReleased(SelectExitEventArgs args)
    {
        state = RockState.InFlight;
        StartCoroutine(ApplyThrowForce());
    }

    private IEnumerator ApplyThrowForce()
    {
        yield return null;  // wait one frame

        if (state != RockState.InFlight) yield break;

        // Scale the velocity XRI already applied
        if (!Mathf.Approximately(throwForceMultiplier, 1f))
        {
            rb.velocity *= throwForceMultiplier;
            rb.angularVelocity *= throwForceMultiplier;
        }
    }

    // LANDING

    private void Land()
    {
        state = RockState.Landed;

        // Alert enemies immediately at the impact point
        if (!hasAlerted)
        {
            hasAlerted = true;
            AlertNearbyEnemies();
        }

        // Let physics roll/bounce naturally, then freeze once the rock has settled
        StartCoroutine(FreezeWhenSettled());
    }

    private IEnumerator FreezeWhenSettled()
    {
        // Wait a minimum time before even checking - avoids freezing on first bounce
        yield return new WaitForSeconds(0.3f);

        // Then wait until velocity is negligible
        while (rb.velocity.magnitude > 0.05f || rb.angularVelocity.magnitude > 0.05f)
        {
            yield return new WaitForSeconds(0.1f);
        }

        rb.velocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;
        FreezeRigidbody(true);
    }

    private void AlertNearbyEnemies()
    {
        EnemyAI[] enemies = FindObjectsOfType<EnemyAI>();

        foreach (EnemyAI enemy in enemies)
        {
            float dist = Vector3.Distance(transform.position, enemy.transform.position);

            if (dist <= enemy.innerRadius)
                enemy.AlertInner(transform.position);
            else if (dist <= enemy.outerRadius)
                enemy.AlertOuter(transform.position);
        }
    }

    public void ResetToGround(Vector3 worldPosition)
    {
        StopAllCoroutines();

        transform.SetParent(null);
        transform.position = worldPosition;

        rb.velocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;
        FreezeRigidbody(true);

        hasAlerted = false;
        state = RockState.OnGround;
    }

    // HELPERS

    private void FreezeRigidbody(bool freeze)
    {
        rb.constraints = freeze ? RigidbodyConstraints.FreezeAll : RigidbodyConstraints.None;
    }

#if UNITY_EDITOR
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, 0.25f);
    }
#endif
}
