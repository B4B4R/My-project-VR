using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class GeyserManager : MonoBehaviour
{
    [System.Serializable]
    public class Geyser
    {
        public ParticleSystem[] particleSystems;
        public QuickSound quickSound;
        public GameObject linkedObject;
    }

    public List<Geyser> geysers = new List<Geyser>();

    public bool geyserActive = true;

    public float intervalBetweenGeysers = 15f;
    public float activeDurationMin = 2f;
    public float activeDurationMax = 3f;

    public GameObject objectToActivateAfterEvent;

    private Coroutine geyserLoopCoroutine;

    // Pour empêcher de relancer l'event
    private bool isEventOccured = false;

    private void Start()
    {
        DisableAllGeysers();

        if (objectToActivateAfterEvent != null)
            objectToActivateAfterEvent.SetActive(false);

        // Ajoute dynamiquement le composant LancerEventFromPhoto à tous les linkedObject
        foreach (Geyser geyser in geysers)
        {
            if (geyser.linkedObject != null)
            {
                LancerEventFromPhoto lancer = geyser.linkedObject.GetComponent<LancerEventFromPhoto>();
                if (lancer == null)
                    lancer = geyser.linkedObject.AddComponent<LancerEventFromPhoto>();

                // On ajoute l'EventSequence du manager comme action
                lancer.eventsToTrigger.Clear();
                UnityEvent unityEvent = new UnityEvent();
                unityEvent.AddListener(() =>
                {
                    StartEventSequence();
                });
                lancer.eventsToTrigger.Add(unityEvent);
            }
        }

        geyserLoopCoroutine = StartCoroutine(GeyserLoop());
    }

    void DisableAllGeysers()
    {
        foreach (Geyser geyser in geysers)
        {
            foreach (ParticleSystem ps in geyser.particleSystems)
            {
                if (ps != null)
                    ps.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
            }

            if (geyser.quickSound != null && geyser.quickSound.IsPlaying)
                geyser.quickSound.audioSource.Stop();

            if (geyser.linkedObject != null)
                geyser.linkedObject.SetActive(false);
        }
    }

    IEnumerator GeyserLoop()
    {
        while (true)
        {
            if (geyserActive && geysers.Count > 0)
            {
                yield return new WaitForSeconds(intervalBetweenGeysers);

                int randomIndex = Random.Range(0, geysers.Count);
                yield return StartCoroutine(ActivateGeyser(geysers[randomIndex]));
            }
            else
            {
                yield return null;
            }
        }
    }

    IEnumerator ActivateGeyser(Geyser geyser)
    {
        float duration = Random.Range(activeDurationMin, activeDurationMax);

        ActivateSingleGeyser(geyser);
        yield return new WaitForSeconds(duration);
        DeactivateSingleGeyser(geyser);
    }

    void ActivateSingleGeyser(Geyser geyser)
    {
        if (geyser.linkedObject != null)
            geyser.linkedObject.SetActive(true);

        if (geyser.quickSound != null)
            geyser.quickSound.Play();

        foreach (ParticleSystem ps in geyser.particleSystems)
        {
            if (ps != null)
                ps.Play();
        }
    }
    public float linkedObjectExtraActiveTime = 5f; // Nouveau champ à régler dans l'inspecteur

    void DeactivateSingleGeyser(Geyser geyser)
    {
        // Stop particules
        foreach (ParticleSystem ps in geyser.particleSystems)
        {
            if (ps != null)
                ps.Stop();
        }

        // On désactive l'objet lié avec délai si ce n'est pas un event
        if (geyser.linkedObject != null)
        {
            if (!isEventOccured) // seulement si c'est la boucle normale
                StartCoroutine(DeactivateWithDelay(geyser.linkedObject, linkedObjectExtraActiveTime));
            else
                geyser.linkedObject.SetActive(false); // event: désactivation immédiate
        }
    }

    IEnumerator DeactivateWithDelay(GameObject obj, float delay)
    {
        yield return new WaitForSeconds(delay);
        obj.SetActive(false);
    }


    // EVENT SEQUENCE
    public void StartEventSequence()
    {
        Debug.Log("Coucou");
        if (isEventOccured) return; // Empêche plusieurs triggers
        isEventOccured = true;

        StartCoroutine(EventSequence());
    }

    IEnumerator EventSequence()
    {
        geyserActive = false;

        yield return new WaitForSeconds(15f);
        // Désactive complètement le système

        if (geyserLoopCoroutine != null)
            StopCoroutine(geyserLoopCoroutine);

        DisableAllGeysers();

        // Active le premier geyser pendant 10 secondes
        if (geysers.Count > 0)
        {
            Geyser firstGeyser = geysers[0];

            ActivateSingleGeyser(firstGeyser);
            yield return new WaitForSeconds(10f);
            DeactivateSingleGeyser(firstGeyser);
        }

        // Active l'objet final
        if (objectToActivateAfterEvent != null)
        {
            objectToActivateAfterEvent.SetActive(true);

            // Ajout d'une vitesse de rotation aléatoire
            Rigidbody rb = objectToActivateAfterEvent.GetComponent<Rigidbody>();
            if (rb != null)
            {
                // Expulsion verticale
                float verticalForce = Random.Range(5f, 10f); // Force verticale aléatoire
                rb.velocity = Vector3.up * verticalForce;

                // Vitesse de rotation aléatoire
                float randomRotationX = Random.Range(-360f, 360f);
                float randomRotationY = Random.Range(-360f, 360f);
                float randomRotationZ = Random.Range(-360f, 360f);
                rb.angularVelocity = new Vector3(randomRotationX, randomRotationY, randomRotationZ) * Mathf.Deg2Rad; // Converti en radians
            }
        }

        // Réactive le système normal
        geyserActive = true;
        geyserLoopCoroutine = StartCoroutine(GeyserLoop());
    }
}
