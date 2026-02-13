using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GeyserManager : MonoBehaviour
{
    [System.Serializable]
    public class Geyser
    {
        public ParticleSystem[] particleSystems;
        public QuickSound quickSound;
        public GameObject linkedObject; // Objet à activer/désactiver
    }

    public List<Geyser> geysers = new List<Geyser>();

    public bool geyserActive = true;

    public float intervalBetweenGeysers = 15f;
    public float activeDurationMin = 2f;
    public float activeDurationMax = 3f;

    private void Start()
    {
        DisableAllGeysers();
        StartCoroutine(GeyserLoop());
    }

    void DisableAllGeysers()
    {
        foreach (Geyser geyser in geysers)
        {
            // Stop particules
            foreach (ParticleSystem ps in geyser.particleSystems)
            {
                if (ps != null)
                {
                    ps.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
                }
            }

            // Stop son
            if (geyser.quickSound != null && geyser.quickSound.IsPlaying)
            {
                geyser.quickSound.audioSource.Stop();
            }

            // Désactive l’objet lié
            if (geyser.linkedObject != null)
            {
                geyser.linkedObject.SetActive(false);
            }
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

        // Active objet lié
        if (geyser.linkedObject != null)
        {
            geyser.linkedObject.SetActive(true);
        }

        // Lance le son
        if (geyser.quickSound != null)
        {
            geyser.quickSound.Play();
        }

        // Active particules
        foreach (ParticleSystem ps in geyser.particleSystems)
        {
            if (ps != null)
                ps.Play();
        }

        yield return new WaitForSeconds(duration);

        // Stop particules
        foreach (ParticleSystem ps in geyser.particleSystems)
        {
            if (ps != null)
                ps.Stop();
        }

        // Désactive objet lié
        if (geyser.linkedObject != null)
        {
            geyser.linkedObject.SetActive(false);
        }
    }
}
