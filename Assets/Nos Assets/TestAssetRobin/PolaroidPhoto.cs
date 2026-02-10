using UnityEngine;
using System.Collections;
using UnityEngine.Events;

public class PolaroidPhoto : MonoBehaviour
{
    private PolaroidCamera cameraScript;
    private bool isInCamera = true;

    public void Init(PolaroidCamera cam)
    {
        cameraScript = cam;
    }

    // À appeler quand la photo est retirée (grab, collision, trigger, etc.)
    public void RemoveFromCamera()
    {
        if (!isInCamera)
            return;

        isInCamera = false;

        transform.SetParent(null);

        if (cameraScript != null)
            cameraScript.OnPhotoRemoved();

        StartCoroutine(ApplyPhysicsNextFrame());
    }

    IEnumerator ApplyPhysicsNextFrame()
    {
        yield return null; // attendre la fin du frame XR

        Rigidbody rb = GetComponent<Rigidbody>();
        rb.isKinematic = false;
        rb.useGravity = true;
    }

}
