using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

public class VerifInObject : MonoBehaviour
{
    [Header("Companion Settings")]
    public FloatingCompanion companion;
    public bool useTagFilter = false;
    public string importantTag = "Important";

    private void OnTriggerEnter(Collider other)
    {
        if (companion == null || other == companion.GetComponent<Collider>()) return;
        if (useTagFilter && other.gameObject.tag != importantTag) return;

        XRGrabInteractable grabInteractable = other.GetComponent<XRGrabInteractable>();

        // Log pour savoir quel objet entre dans la zone
        Debug.Log($"Objet détecté dans la zone : {other.name}");

        // Si l'objet est tenu, on le dégrab avant de l'envoyer
        if (grabInteractable != null && grabInteractable.isSelected)
        {
            XRBaseInteractor interactor = grabInteractable.selectingInteractor;
            if (interactor != null && grabInteractable.interactionManager != null)
            {
                grabInteractable.interactionManager.SelectExit(interactor, grabInteractable);
                Debug.Log($"Dégrab forcé de l'objet {other.name} pour le companion.");
            }
        }

        companion.FetchObject(other.transform);
    }
}
