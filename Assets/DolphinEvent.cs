using UnityEngine;
using System.Collections;

public class DolphinEvent : MonoBehaviour
{
    public Transform[] spawnPoints;           // empty pos 1, 2, 3...
    public Transform dolphinModelEmpty;       // Empty Model Dauphin, pivot
    public Transform player;                  // Référence du joueur
    public int jumpsPerSpawn = 5;             // Nombre de rotations/sauts par spawn
    public float rotationSpeed = 180f;        // Vitesse de rotation en degrés par seconde
    public float intervalBetweenSpawns = 2f;  // Pause avant le prochain spawn
    public float initialRotationOffset = 180f; // Offset initial en degrés

    void Start()
    {
        if (spawnPoints.Length == 0 || dolphinModelEmpty == null || player == null)
        {
            Debug.LogError("Spawn points, dolphin model ou player non assigné !");
            return;
        }

        StartCoroutine(DolphinRoutine());
    }

    IEnumerator DolphinRoutine()
    {
        while (true)
        {
            // Choisir un spawn aléatoire et téléporter le pivot
            Transform currentSpawn = spawnPoints[Random.Range(0, spawnPoints.Length)];
            transform.position = currentSpawn.position;

            // Tourner le pivot pour être de profil par rapport au joueur
            Vector3 toPlayer = player.position - transform.position;
            toPlayer.y = 0; // ignorer la hauteur pour profil strict
            if (toPlayer != Vector3.zero)
            {
                // Orienter le dauphin de profil : axe latéral vers le joueur
                dolphinModelEmpty.rotation = Quaternion.LookRotation(Vector3.Cross(Vector3.up, toPlayer), Vector3.up);
                // Ajouter l'offset de 180 degrés
                dolphinModelEmpty.Rotate(Vector3.right, initialRotationOffset, Space.Self);
            }

            // Effectuer les 5 rotations/sauts autour de l’axe horizontal local
            for (int i = 0; i < jumpsPerSpawn; i++)
            {
                yield return StartCoroutine(RotatePivotHorizontalOneJump());
            }

            // Pause avant le prochain spawn
            yield return new WaitForSeconds(intervalBetweenSpawns);
        }
    }

    IEnumerator RotatePivotHorizontalOneJump()
    {
        float rotated = 0f;

        while (rotated < 360f) // une rotation complète = un saut
        {
            float delta = rotationSpeed * Time.deltaTime;
            dolphinModelEmpty.Rotate(Vector3.right, delta, Space.Self); // rotation autour de l'axe horizontal local
            rotated += delta;
            yield return null;
        }
    }
}