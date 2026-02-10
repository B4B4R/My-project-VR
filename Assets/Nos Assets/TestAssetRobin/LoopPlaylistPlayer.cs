using System.Collections;
using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class LoopPlaylistPlayer : MonoBehaviour
{
    [Tooltip("Liste des musiques à jouer")]
    public AudioClip[] playlist;

    [Tooltip("Durée du fondu en secondes")]
    public float fadeDuration = 2f;

    private AudioSource audioSource;
    private int currentIndex = 0;

    void Start()
    {
        audioSource = GetComponent<AudioSource>();

        if (playlist == null || playlist.Length == 0)
        {
            Debug.LogWarning("La playlist est vide !");
            return;
        }

        audioSource.loop = false; // On gère la boucle nous-mêmes
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
        audioSource.volume = 0;
        audioSource.Play();

        float timer = 0f;
        while (timer < fadeDuration)
        {
            timer += Time.deltaTime;
            audioSource.volume = Mathf.Lerp(0, 1, timer / fadeDuration);
            yield return null;
        }
        audioSource.volume = 1;
    }

    IEnumerator FadeOut()
    {
        float startVolume = audioSource.volume;
        float timer = 0f;
        while (timer < fadeDuration)
        {
            timer += Time.deltaTime;
            audioSource.volume = Mathf.Lerp(startVolume, 0, timer / fadeDuration);
            yield return null;
        }
        audioSource.volume = 0;
        audioSource.Stop();
    }
}
