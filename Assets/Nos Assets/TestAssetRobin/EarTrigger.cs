using UnityEngine;

public class EarTrigger : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
{
    if (other.CompareTag("Coquillage"))
    {
        QuickSound qs = other.GetComponent<QuickSound>();
        if (qs != null && !qs.IsPlaying) // si QuickSound a une propriété pour savoir s'il joue
        {
            qs.Play();
            Debug.Log("QuickSound lancé pour " + other.name);
        }
    }
}

}
