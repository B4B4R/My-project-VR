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
    public GameObject photoPlane;  // Le plane utilisé comme modèle pour chaque photo

    [Header("Photo Exit")]
    public Transform photoExitPoint;
    public float ejectForce = 2f;

    [Header("Sound / Quick Action")]
    public UnityEvent quickSoundFunction;

    [Header("Render Texture")]
    public int textureWidth = 512;
    public int textureHeight = 512;

    [Header("Réglages du Raycast")]
    public Transform rayOrigin; // Transform d'où part le rayon (ex: un empty)
    public float rayDistance = 100f;
    public bool debugRay = true; // Affiche le rayon dans la scène pour debug


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
        quickSoundFunction?.Invoke();
        StartCoroutine(CaptureRoutine());
    }

    IEnumerator CaptureRoutine()
    {
        yield return new WaitForEndOfFrame();

        RenderTexture currentRT = RenderTexture.active;
        RenderTexture.active = renderTexture;

        Texture2D photoTexture = new Texture2D(textureWidth, textureHeight, TextureFormat.RGB24, false);
        photoTexture.ReadPixels(new Rect(0, 0, textureWidth, textureHeight), 0, 0);
        photoTexture.Apply();

        RenderTexture.active = currentRT;
        ShootRay();
        SpawnPhoto(photoTexture);
    }

    void SpawnPhoto(Texture2D texture)
    {
        // Crée une nouvelle photo à partir du template
        GameObject newPhoto = Instantiate(photoPlane, photoExitPoint.position, photoExitPoint.rotation);

        // Applique la texture
        MeshRenderer renderer = newPhoto.GetComponent<MeshRenderer>();
        renderer.material = new Material(renderer.material);
        renderer.material.mainTexture = texture;

        // Éjecte la photo avec Rigidbody
        Rigidbody rb = newPhoto.GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.velocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
            rb.AddForce(photoExitPoint.forward * ejectForce, ForceMode.Impulse);
        }

        // Active la photo si le template était désactivé
        newPhoto.SetActive(true);
    }

    void ShootRay()
    {
        if (rayOrigin == null)
        {
            Debug.LogWarning("RayOrigin non défini !");
            return;
        }

        Ray ray = new Ray(rayOrigin.position, rayOrigin.forward);

        if (debugRay)
            Debug.DrawRay(rayOrigin.position, rayOrigin.forward * rayDistance, Color.red, 2f);

        if (Physics.Raycast(ray, out RaycastHit hit, rayDistance))
        {
            LancerEventFromPhoto eventScript = hit.collider.GetComponent<LancerEventFromPhoto>();
            if (eventScript != null)
            {
                eventScript.TriggerAllEvents();
            }
        }
    }
}
