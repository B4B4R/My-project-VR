using UnityEngine;
using System.Collections;

[RequireComponent(typeof(Rigidbody))]
public class FloatingCompanion : MonoBehaviour
{
    [Header("Targets")]
    public Transform currentTarget;
    public Transform player;

    [Header("Movement")]
    public float moveForce = 15f;
    public float maxSpeed = 5f;
    public float stoppingDistance = 0.5f;

    [Header("Rotation")]
    public bool lookAtPlayer = true;
    public float rotationSpeed = 5f;

    [Header("Stuck System")]
    public float maxStuckTime = 30f;
    public float minMoveThreshold = 0.05f;

    private Rigidbody rb;
    private bool isFollowing = true;

    private float stuckTimer = 0f;
    private Vector3 lastPosition;

    private Coroutine activateCoroutine;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        lastPosition = transform.position;

        ActivateFollow();
            rb.constraints =
        RigidbodyConstraints.FreezeRotationX |
        RigidbodyConstraints.FreezeRotationY |
        RigidbodyConstraints.FreezeRotationZ;
    }

    void FixedUpdate()
    {
        if (!isFollowing) return;

        MoveToTarget();
        DetectStuck();
    }

    void Update()
    {
        if (!isFollowing) return;

        HandleRotation();
    }

    // =============================
    // MOVEMENT
    // =============================

    void MoveToTarget()
    {
        if (currentTarget == null) return;

        Vector3 direction = currentTarget.position - transform.position;
        float distance = direction.magnitude;

        if (distance > stoppingDistance)
        {
            Vector3 force = direction.normalized * moveForce;
            rb.AddForce(force, ForceMode.Acceleration);

            if (rb.velocity.magnitude > maxSpeed)
                rb.velocity = rb.velocity.normalized * maxSpeed;
        }
        else
        {
            rb.velocity *= 0.9f;
        }
    }

    // =============================
    // ROTATION
    // =============================

    void HandleRotation()
    {
        Transform lookTarget = null;

        if (lookAtPlayer && player != null)
            lookTarget = player;
        else if (currentTarget != null)
            lookTarget = currentTarget;

        if (lookTarget == null) return;

        Vector3 direction = lookTarget.position - transform.position;

        if (direction.sqrMagnitude < 0.001f) return;

        Quaternion targetRotation = Quaternion.LookRotation(direction.normalized);

        rb.MoveRotation(
            Quaternion.Slerp(
                rb.rotation,
                targetRotation,
                rotationSpeed * Time.deltaTime
            )
        );
    }

    // =============================
    // STUCK DETECTION
    // =============================

    void DetectStuck()
    {
        float movedDistance = Vector3.Distance(transform.position, lastPosition);

        if (movedDistance < minMoveThreshold)
            stuckTimer += Time.fixedDeltaTime;
        else
            stuckTimer = 0f;

        if (stuckTimer >= maxStuckTime)
        {
            TeleportToTarget();
            stuckTimer = 0f;
        }

        lastPosition = transform.position;
    }

    void TeleportToTarget()
    {
        if (currentTarget == null) return;

        rb.velocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;
        transform.position = currentTarget.position;
    }

    // =============================
    // FOLLOW CONTROL
    // =============================

    public void ActivateFollow()
    {
        if (isFollowing) return;

        isFollowing = true;

        // Annule une éventuelle attente
        if (activateCoroutine != null)
        {
            StopCoroutine(activateCoroutine);
            activateCoroutine = null;
        }

        rb.useGravity = false;
        rb.velocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;

        rb.constraints =
            RigidbodyConstraints.FreezeRotationX |
            RigidbodyConstraints.FreezeRotationY |
            RigidbodyConstraints.FreezeRotationZ;

        // Réinitialise proprement la rotation vers la cible
        if (currentTarget != null)
        {
            Vector3 dir = currentTarget.position - transform.position;

            if (dir.sqrMagnitude > 0.001f)
                rb.rotation = Quaternion.LookRotation(dir.normalized);
        }

        stuckTimer = 0f;
        lastPosition = transform.position;
    }

    public void DeactivateFollow()
    {
        isFollowing = false;

        // Annule la coroutine si elle tourne
        if (activateCoroutine != null)
        {
            StopCoroutine(activateCoroutine);
            activateCoroutine = null;
        }

        rb.useGravity = true;
        rb.constraints = RigidbodyConstraints.None;
    }

    public void ActivateWithDelay(float delay = 10f)
    {
        DeactivateFollow();
        if (activateCoroutine != null)
            StopCoroutine(activateCoroutine);

        activateCoroutine = StartCoroutine(ActivateAfterDelay(delay));
    }

    private IEnumerator ActivateAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);

        activateCoroutine = null;
        ActivateFollow();
    }

    // =============================
    // PUBLIC TARGET SWITCH
    // =============================

    public void SetTarget(Transform newTarget)
    {
        currentTarget = newTarget;
        stuckTimer = 0f;
    }
}
