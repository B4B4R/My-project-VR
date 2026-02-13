using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

public class AnimalFlee : MonoBehaviour
{
    [Header("Prefab & Player")]
    public GameObject animalPrefab;
    public Transform player;

    [Header("Movement")]
    public float speed = 5f;

    [Header("Scale")]
    public float scaleRatio = 1f;
    public float sphereScale = 1f;

    [Header("Spawn")]
    public Transform spawnParent;

    [Header("Grab")]
    public bool enableGrab = true;

    private GameObject visualInstance;
    private Rigidbody rb;
    private SphereCollider sphereCollider;
    private XRGrabInteractable grab;
    private Animator animator;

    private bool isGrabbed;
    private bool isScared;

    public static GameObject SpawnAnimal(GameObject prefab, Transform parent, Vector3 position)
    {
        GameObject go = Instantiate(prefab, position, Quaternion.identity, parent);
        return go;
    }

    void Awake()
    {
        if (spawnParent != null)
            transform.parent = spawnParent;

        rb = GetComponent<Rigidbody>();
        if (rb == null) rb = gameObject.AddComponent<Rigidbody>();

        sphereCollider = GetComponent<SphereCollider>();
        if (sphereCollider == null) sphereCollider = gameObject.AddComponent<SphereCollider>();

        transform.localScale = Vector3.one * sphereScale;

        if (enableGrab)
        {
            grab = GetComponent<XRGrabInteractable>();
            if (grab != null)
            {
                grab.selectEntered.AddListener(OnGrab);
                grab.selectExited.AddListener(OnRelease);
            }
        }

        if (animalPrefab != null)
        {
            visualInstance = Instantiate(animalPrefab);
            visualInstance.transform.localScale = animalPrefab.transform.localScale * scaleRatio;

            animator = visualInstance.GetComponent<Animator>();
            if (animator == null)
                Debug.LogWarning("Pas d'Animator trouvé sur le prefab !");
        }
        else
        {
            Debug.LogWarning("Animal prefab non assigné !");
        }
    }

    void FixedUpdate()
    {
        if (isGrabbed || visualInstance == null) return;

        Vector3 direction = transform.position - player.position;
        direction.y = 0f;
        direction.Normalize();

        Vector3 velocity = rb.velocity;

        // l'animal recule du joueur si effrayé
        if (isScared)
        {
            velocity.x = direction.x * speed;
            velocity.z = direction.z * speed;
        }
        else
        {
            // mouvement normal (ou courir dans une direction quelconque)
            velocity.x = direction.x * speed;
            velocity.z = direction.z * speed;
        }

        rb.velocity = velocity;
    }

    public void SwitchToScare()
    {
        isScared = true;
        if (animator != null)
            animator.SetBool("IsFeared", true);
    }

    public void SwitchToNotScare()
    {
        isScared = false;
        if (animator != null)
            animator.SetBool("IsFeared", false);
    }

    void LateUpdate()
    {
        if (visualInstance == null) return;

        float radius = sphereCollider.radius * transform.lossyScale.y;
        visualInstance.transform.position = transform.position - Vector3.up * radius;


        Vector3 direction = player.position - transform.position; // direction vers le joueur
        direction.y = 0f;

        if (!isGrabbed)
        {
            // rotation normale selon la direction du mouvement
            Vector3 moveDir = rb.velocity;
            moveDir.y = 0f;
            if (moveDir.sqrMagnitude > 0.001f)
            {
                Quaternion targetRotation = Quaternion.LookRotation(moveDir);
                visualInstance.transform.rotation = Quaternion.Slerp(
                    visualInstance.transform.rotation,
                    targetRotation,
                    Time.deltaTime * 5f
                );
            }
        }
        else
        {
            // rotation fluide vers le joueur quand effrayé
            Quaternion targetRotation = Quaternion.LookRotation(direction);
            visualInstance.transform.rotation = Quaternion.Slerp(
                visualInstance.transform.rotation,
                targetRotation,
                Time.deltaTime * 5f // vitesse de rotation
            );
        }

            
    }


    void OnGrab(SelectEnterEventArgs args)
    {
        isGrabbed = true;
        rb.velocity = Vector3.zero;
    }

    void OnRelease(SelectExitEventArgs args)
    {
        isGrabbed = false;
    }
}
