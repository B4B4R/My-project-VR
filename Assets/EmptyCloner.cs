using UnityEngine;

public class EmptyCloner : MonoBehaviour
{
    [Header("Source Empty (désactivé possible)")]
    public GameObject emptyToClone;

    [Header("Spawn Settings")]
    public float heightOffset = 0f;

    [Header("Optional Visual / Prefab Override")]
    public GameObject overrideAnimalPrefab;

    [Header("Optional Sound Override")]
    public AudioClip overrideClip;
    [Range(0f, 1f)]
    public float overrideVolume = 1f;
    public float overrideStartTime = 0f;
    public float overrideEndTime = 0f;
    public bool playSoundOnSpawn = false;

    private bool hasSpawned = false;
    private GameObject spawnedInstance;

    public void SpawnOnce()
    {
        if (hasSpawned || emptyToClone == null)
            return;

        // Position avec offset vertical
        Vector3 spawnPosition = transform.position + Vector3.up * heightOffset;

        // Instanciation du clone
        spawnedInstance = Instantiate(emptyToClone, spawnPosition, transform.rotation);

        // Si le clone était désactivé, on le garde désactivé temporairement
        spawnedInstance.SetActive(false);

        // Vérifie si le clone a un AnimalFlee
        AnimalFlee animalFlee = spawnedInstance.GetComponent<AnimalFlee>();

        if (animalFlee != null && overrideAnimalPrefab != null)
        {
            // Change le prefab à celui désiré
            animalFlee.animalPrefab = overrideAnimalPrefab;
        }

        // Configuration du QuickSound si présent
        QuickSound quickSound = spawnedInstance.GetComponentInChildren<QuickSound>();

        if (quickSound != null)
        {
            if (overrideClip != null)
                quickSound.sound = overrideClip;

            quickSound.volume = overrideVolume;
            quickSound.startTime = overrideStartTime;
            quickSound.endTime = overrideEndTime;

            if (playSoundOnSpawn)
                quickSound.Play();
        }

        // Active le clone après toutes les modifications
        spawnedInstance.SetActive(true);

        hasSpawned = true;
    }
}