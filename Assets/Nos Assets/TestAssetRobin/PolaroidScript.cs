using UnityEngine;
using System.Collections;
using UnityEngine.Events;

public class PolaroidCamera : MonoBehaviour
{
    [Header("Camera")]
    public Camera polaroidCamera;

    [Header("Screen")]
    public MeshRenderer screenRenderer;

    [Header("Photo Plane Template")]
    public GameObject photoPlane;

    [Header("Photo Exit")]
    public Transform photoExitPoint;

    [Header("Sound / Quick Action")]
    public UnityEvent quickSoundFunction;

    [Header("Render Texture")]
    public int textureWidth = 512;
    public int textureHeight = 512;

    [Header("Raycast")]
    public Transform rayOrigin;
    public float rayDistance = 100f;
    public bool debugRay = true;

    [Header("Photo State")]
    public bool photoInCamera = false;
    private GameObject currentPhoto;

    [Header("Photo Exit Animation")]
    public float startZ = 0.1f;
    public float endZ = 1f;
    public float exitMoveDuration = 0.5f;

    private RenderTexture renderTexture;

    void Start()
    {
        SetupRenderTexture();
    }

    void SetupRenderTexture()
    {
        renderTexture = new RenderTexture(textureWidth, textureHeight, 16);
        polaroidCamera.targetTexture = renderTexture;
        screenRenderer.material.mainTexture = renderTexture;
    }

    public void CapturePhoto()
    {
        if (photoInCamera)
            return;

        photoInCamera = true;
        quickSoundFunction?.Invoke();
        StartCoroutine(CaptureRoutine());
    }

    IEnumerator CaptureRoutine()
    {
        yield return new WaitForEndOfFrame();

        RenderTexture currentRT = RenderTexture.active;
        RenderTexture.active = renderTexture;

        Texture2D photoTexture = new Texture2D(
            textureWidth,
            textureHeight,
            TextureFormat.RGB24,
            false
        );

        photoTexture.ReadPixels(
            new Rect(0, 0, textureWidth, textureHeight),
            0,
            0
        );
        photoTexture.Apply();

        RenderTexture.active = currentRT;

        ShootRay();

        yield return StartCoroutine(SpawnAndAnimatePhoto(photoTexture));
    }

    IEnumerator SpawnAndAnimatePhoto(Texture2D texture)
    {
        // 1. Spawn immédiat de la photo EN ENFANT
        GameObject newPhoto = Instantiate(
            photoPlane,
            photoExitPoint.position,
            photoExitPoint.rotation,
            photoExitPoint
        );

        currentPhoto = newPhoto;

        // 2. Désactivation du BoxCollider uniquement
        BoxCollider box = newPhoto.GetComponent<BoxCollider>();
        if (box != null)
            box.enabled = false;

        // 3. Application de la texture
        MeshRenderer renderer = newPhoto.GetComponent<MeshRenderer>();
        Material[] materials = renderer.materials;

        if (materials.Length > 1)
        {
            materials[1] = new Material(materials[1]);
            materials[1].mainTexture = texture;
            materials[1].color = Color.white;
            renderer.materials = materials;
        }

        // 4. Init du script photo
        PolaroidPhoto photoScript = newPhoto.GetComponent<PolaroidPhoto>();
        if (photoScript != null)
            photoScript.Init(this);

        newPhoto.SetActive(true);

        // 5. Animation du spawn point
        yield return StartCoroutine(AnimatePhotoExit());

        // 6. Réactivation de la collision à la fin du mouvement
        if (box != null)
            box.enabled = true;
    }

    IEnumerator AnimatePhotoExit()
    {
        Vector3 localPos = photoExitPoint.localPosition;
        localPos.z = startZ;
        photoExitPoint.localPosition = localPos;

        float timer = 0f;

        while (timer < exitMoveDuration)
        {
            timer += Time.deltaTime;
            float t = timer / exitMoveDuration;

            localPos.z = Mathf.Lerp(startZ, endZ, t);
            photoExitPoint.localPosition = localPos;

            yield return null;
        }

        localPos.z = endZ;
        photoExitPoint.localPosition = localPos;
    }

    public void OnPhotoRemoved()
    {
        photoInCamera = false;
        currentPhoto = null;
    }

    void ShootRay()
    {
        if (rayOrigin == null)
            return;

        Ray ray = new Ray(rayOrigin.position, rayOrigin.forward);

        if (debugRay)
            Debug.DrawRay(rayOrigin.position, rayOrigin.forward * rayDistance, Color.red, 2f);

        // Raycast simple pour tester le premier objet
        if (Physics.Raycast(ray, out RaycastHit firstHit, rayDistance))
        {
            // Si le premier objet a le tag "Wall", on fait un RaycastAll
            if (firstHit.collider.CompareTag("Wall"))
            {
                RaycastHit[] hits = Physics.RaycastAll(ray, rayDistance);
                // Tri par distance pour traiter dans l'ordre d'apparition
                System.Array.Sort(hits, (a, b) => a.distance.CompareTo(b.distance));

                foreach (RaycastHit hit in hits)
                {
                    LancerEventFromPhoto eventScript = hit.collider.GetComponent<LancerEventFromPhoto>();
                    if (eventScript != null)
                        eventScript.TriggerAllEvents();
                }
            }
            else
            {
                // Sinon, on déclenche l'événement sur le premier objet uniquement
                LancerEventFromPhoto eventScript = firstHit.collider.GetComponent<LancerEventFromPhoto>();
                if (eventScript != null)
                    eventScript.TriggerAllEvents();
            }
        }
    }
}
