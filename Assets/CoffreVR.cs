using UnityEngine;
using UnityEngine.Events;

public class CoffreVR : MonoBehaviour
{
    [Header("Références")]
    public Transform objetCache;    // L'objet à sortir

    [Header("Paramètres du coffre")]
    public int coupsPourSortir = 3;
    public float distanceSortie = 1f;

    [Header("Événements")]
    public UnityEvent evenementApproche;       // À déclencher quand le joueur approche
    public UnityEvent evenementCreusageFini;   // À déclencher quand l'objet est complètement sorti

    private int compteurCoups = 0;
    private Vector3 positionInitiale;

    void Start()
    {
        if (objetCache != null)
            positionInitiale = objetCache.localPosition;
    }

    // À appeler depuis TriggerApproche
    public void PremierEvent()
    {
        Debug.Log("Premier événement déclenché !");
        evenementApproche?.Invoke();
    }

    // À appeler depuis TriggerCreuse
    public void CoupDePelle()
    {
        compteurCoups++;
        float fraction = (float)compteurCoups / coupsPourSortir;

        if (objetCache != null)
        {
            objetCache.localPosition = positionInitiale + new Vector3(0, fraction * distanceSortie, 0);
        }

        Debug.Log("Nombre de coups : " + compteurCoups);

        if (compteurCoups >= coupsPourSortir)
        {
            Debug.Log("L'objet est complètement sorti !");
            evenementCreusageFini?.Invoke();
        }
    }
}
