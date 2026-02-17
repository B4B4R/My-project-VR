using System.Collections;
using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class LoopPlaylistPlayer : MonoBehaviour
{
    [Tooltip("Base")]
    public float Volume = 0.1f;
    [Tooltip("Liste des musiques à jouer")]
    public AudioClip[] playlist;

    [Tooltip("Durée du fondu en secondes")]
    public float fadeDuration = 2f;

    private AudioSource audioSource;
    private int currentIndex = 0;

    void Awake()
    {
        audioSource = GetComponent<AudioSource>();
        audioSource.volume = 0.1f;       // Valeur initiale souhaitée
        audioSource.playOnAwake = false; // Empêche le son de jouer tout seul
        audioSource.loop = false;        // On gère la boucle nous-mêmes
    }

    void Start()
    {
        if (playlist == null || playlist.Length == 0)
        {
            Debug.LogWarning("La playlist est vide !");
            return;
        }

        StartCoroutine(PlayPlaylist());
    }

    IEnumerator PlayPlaylist()
    {
        while (true)
        {
            AudioClip currentClip = playlist[currentIndex];
            yield return StartCoroutine(FadeIn(currentClip));

            // Attendre la fin du clip moins le fade out
            yield return new WaitForSeconds(currentClip.length - fadeDuration);

            yield return StartCoroutine(FadeOut());

            // Passer au clip suivant
            currentIndex = (currentIndex + 1) % playlist.Length;
        }
    }

    IEnumerator FadeIn(AudioClip clip)
    {
        audioSource.clip = clip;
        float targetVolume = Volume; // Volume maximal du fade
        float timer = 0f;
        audioSource.Play();

        while (timer < fadeDuration)
        {
            timer += Time.deltaTime;
            audioSource.volume = Mathf.Lerp(0.1f, targetVolume, timer / fadeDuration);
            yield return null;
        }

        audioSource.volume = targetVolume;
    }

    IEnumerator FadeOut()
    {
        float startVolume = audioSource.volume;
        float timer = 0f;

        while (timer < fadeDuration)
        {
            timer += Time.deltaTime;
            audioSource.volume = Mathf.Lerp(startVolume, 0.1f, timer / fadeDuration);
            yield return null;
        }

        audioSource.volume = 0.1f; // Volume minimum après le fade
        audioSource.Stop();
    }
}
