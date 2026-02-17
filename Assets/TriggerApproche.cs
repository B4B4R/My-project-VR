using UnityEngine;

public class TriggerApproche : MonoBehaviour
{
    public CoffreVR coffre; // référence au script principal sur le parent

    private void OnTriggerEnter(Collider other)
    {
        Debug.Log("Trigger Approche détecté : " + other.name);

        if (other.CompareTag("Player"))
        {
            Debug.Log("Joueur détecté dans la zone d'approche !");
            coffre.PremierEvent();
        }
    }
}
