using UnityEngine;

public class QuickSound : MonoBehaviour
{
    [Header("Audio Source")]
    public AudioSource audioSource;

    [Header("Sound Settings")]
    public AudioClip sound;
    [Range(0f, 1f)]
    public float volume = 1f;
    public float randomPitchRange = 0f;

    [Header("Start Time (seconds)")]
    public float startTime = 0f; // Temps où le son commence

    public void Play()
    {
        if (audioSource == null || sound == null) return;

        audioSource.clip = sound;
        audioSource.volume = volume;

        float pitch = 1f;
        if (randomPitchRange > 0f)
        {
            pitch += Random.Range(-randomPitchRange, randomPitchRange);
        }
        audioSource.pitch = pitch;

        // On commence le son à startTime
        audioSource.time = Mathf.Clamp(startTime, 0f, sound.length);

        audioSource.Play();
    }
}
