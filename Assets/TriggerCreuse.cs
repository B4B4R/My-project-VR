using UnityEngine;

public class TriggerCreuse : MonoBehaviour
{
    public CoffreVR coffre; // référence au script principal sur le parent
    public GameObject pelle; // référence à l'objet pelle

    private void OnTriggerEnter(Collider other)
    {
        Debug.Log("Trigger Creuse détecté : " + other.name);

        if (other.gameObject == pelle)
        {
            Debug.Log("Pelle détectée dans la zone de creusage !");
            coffre.CoupDePelle();
        }
    }
}
