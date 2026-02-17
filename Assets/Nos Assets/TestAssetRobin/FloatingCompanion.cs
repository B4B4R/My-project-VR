using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(AudioSource))]
[RequireComponent(typeof(Collider))]
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

    [Header("Fetch System")]
    public float fetchTimeout = 10f;
    public float fetchSpeedMultiplier = 2f;
    public Vector3 handOffset = new Vector3(0, 0, 0.5f);

    private Rigidbody rb;
    private AudioSource audioSource;
    private Collider companionCollider;

    private bool isFollowing = true;
    private bool isFetching = false;

    private float stuckTimer = 0f;
    private Vector3 lastPosition;

    private Transform originalTarget;
    private Queue<Transform> fetchQueue = new Queue<Transform>();
    private HashSet<Transform> queuedObjects = new HashSet<Transform>();

    private float originalMaxSpeed;
    private bool teleportationEnabled = true;

    // ===== AUDIO CONTROL =====
    private AudioClip pendingClip;
    private float pendingVolume;
    private bool waitingToPlay = false;
    private Coroutine soundCoroutine;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        audioSource = GetComponent<AudioSource>();
        companionCollider = GetComponent<Collider>();

        audioSource.playOnAwake = false;
        audioSource.loop = false;
        audioSource.spatialBlend = 1f;

        originalMaxSpeed = maxSpeed;
        lastPosition = transform.position;

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
        CheckPendingSound();
    }

    void Update()
    {
        if (!isFollowing) return;
        HandleRotation();
    }

    // ===============================
    // MOVEMENT
    // ===============================
    private bool wasClose = false; // Ajoute cette variable en haut de la classe

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

            // Si avant on était proche, mais qu'on s'éloigne maintenant
            if (wasClose)
            {
                //Debug.Log("Companion is moving away from target");
                wasClose = false;
            }
        }
        else
        {
            rb.velocity *= 0.9f;

            // Si on est proche, on marque qu'on est attaché
            if (!wasClose)
            {
                //Debug.Log("Companion reached target");
                wasClose = true;
            }
        }

        lastPosition = transform.position;
    }


    void HandleRotation()
    {
        Transform lookTarget = null;

        if (isFetching && fetchQueue.Count > 0)
            lookTarget = fetchQueue.Peek();
        else if (lookAtPlayer && player != null)
            lookTarget = player;
        else if (currentTarget != null)
            lookTarget = currentTarget;

        if (lookTarget == null) return;

        Vector3 direction = lookTarget.position - transform.position;
        if (direction.sqrMagnitude < 0.001f) return;

        Quaternion targetRotation = Quaternion.LookRotation(direction.normalized);
        rb.MoveRotation(Quaternion.Slerp(rb.rotation, targetRotation, rotationSpeed * Time.deltaTime));
    }

    // ===============================
    // STUCK + TELEPORT
    // ===============================
    void DetectStuck()
    {
        if (isFetching && !teleportationEnabled) return;
        if (currentTarget == null) return;

        float distanceToTarget = Vector3.Distance(transform.position, currentTarget.position);

        // Si proche de la cible  pas besoin de téléport, reset stuckTimer
        if (distanceToTarget <= stoppingDistance)
        {
            stuckTimer = 0f;
            return;
        }

        // Sinon on vérifie si on est bloqué
        float movedDistance = Vector3.Distance(transform.position, lastPosition);
        if (movedDistance < minMoveThreshold)
            stuckTimer += Time.fixedDeltaTime;
        else
            stuckTimer = 0f;

        // Téléport uniquement si bloqué loin de la cible
        if (stuckTimer >= maxStuckTime && teleportationEnabled)
        {
            TeleportToTarget();
            stuckTimer = 0f;
        }
    }

    void TeleportToTarget()
    {
        if (currentTarget == null) return;

        rb.velocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;
        transform.position = currentTarget.position;
    }

    // ===============================
    // AUDIO SYSTEM
    // ===============================
    public void PlaySoundOnce(AudioClip clip, float volume = 1f)
    {
        if (clip == null) return;

        pendingClip = clip;
        pendingVolume = volume;
        waitingToPlay = true;

        TryPlaySound();
    }

    void TryPlaySound()
    {
        if (!waitingToPlay) return;
        if (isFetching) return;
        if (currentTarget == null) return;

        float distance = Vector3.Distance(transform.position, currentTarget.position);
        if (distance > stoppingDistance) return;

        if (soundCoroutine != null)
            StopCoroutine(soundCoroutine);

        soundCoroutine = StartCoroutine(PlayAndMonitor());
    }

    IEnumerator PlayAndMonitor()
    {
        waitingToPlay = false;
        audioSource.PlayOneShot(pendingClip, pendingVolume);

        float timer = 0f;
        while (timer < pendingClip.length)
        {
            if (isFetching)
            {
                audioSource.Stop();
                waitingToPlay = true;
                yield break;
            }

            timer += Time.deltaTime;
            yield return null;
        }

        soundCoroutine = null;
    }

    void CheckPendingSound()
    {
        if (waitingToPlay)
            TryPlaySound();
    }

    // ===============================
    // FETCH SYSTEM
    // ===============================
    public void FetchObject(Transform targetObject)
    {
        if (queuedObjects.Contains(targetObject)) return;

        fetchQueue.Enqueue(targetObject);
        queuedObjects.Add(targetObject);

        if (!isFetching)
            StartCoroutine(ProcessFetchQueue());
    }

    private IEnumerator ProcessFetchQueue()
    {
        isFetching = true;

        if (audioSource.isPlaying)
        {
            audioSource.Stop();
            waitingToPlay = true;
        }

        while (fetchQueue.Count > 0)
        {
            Transform obj = fetchQueue.Dequeue();
            queuedObjects.Remove(obj);

            originalTarget = currentTarget;

            maxSpeed *= fetchSpeedMultiplier;
            teleportationEnabled = false;

            // Désactive collision
            if (companionCollider != null)
                companionCollider.enabled = false;

            currentTarget = obj;

            float timer = 0f;
            while (Vector3.Distance(transform.position, obj.position) > 1f && timer < fetchTimeout)
            {
                timer += Time.deltaTime;
                yield return null;
            }

            if (obj != null)
            {
                Rigidbody objRb = obj.GetComponent<Rigidbody>();
                Collider objCol = obj.GetComponent<Collider>();

                if (objRb != null)
                {
                    objRb.isKinematic = true;
                    objRb.velocity = Vector3.zero;
                }

                if (objCol != null)
                    objCol.enabled = false;

                obj.position = transform.position + handOffset;
            }

            currentTarget = originalTarget;

            timer = 0f;
            while (Vector3.Distance(transform.position, currentTarget.position) > stoppingDistance && timer < fetchTimeout)
            {
                timer += Time.deltaTime;
                if (obj != null)
                    obj.position = transform.position + handOffset;
                yield return null;
            }

            if (obj != null)
            {
                Rigidbody objRb = obj.GetComponent<Rigidbody>();
                Collider objCol = obj.GetComponent<Collider>();

                if (objRb != null)
                    objRb.isKinematic = false;

                if (objCol != null)
                    objCol.enabled = true;
            }

            // Réactive collision
            if (companionCollider != null)
                companionCollider.enabled = true;

            maxSpeed = originalMaxSpeed;
            teleportationEnabled = true;

            yield return null;
        }

        isFetching = false;
    }
}
