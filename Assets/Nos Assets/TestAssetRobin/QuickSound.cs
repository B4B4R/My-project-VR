using UnityEngine;
using UnityEngine.Events;

public class QuickSound : MonoBehaviour
{
    [Header("Audio Source")]
    public AudioSource audioSource;

    [Header("Sound Settings")]
    public AudioClip sound;
    [Range(0f, 1f)]
    public float volume = 1f;
    public float randomPitchRange = 0f;

    [Header("Start/End Time (seconds)")]
    public float startTime = 0f; // Temps où le son commence
    public float endTime = 0f;   // Temps où le son doit s'arrêter (0 = fin naturelle)

    [Header("Sound / Quick Action")]
    public UnityEvent quickSoundFunction;

    private bool isStoppingScheduled = false;

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

        // Clamp startTime
        audioSource.time = Mathf.Clamp(startTime, 0f, sound.length);

        audioSource.Play();
        quickSoundFunction?.Invoke();

        // Si endTime est défini et inférieur à la longueur du son, on planifie l'arrêt
        if (endTime > 0f && endTime > startTime)
        {
            if (!isStoppingScheduled)
            {
                isStoppingScheduled = true;
                float duration = Mathf.Clamp(endTime - startTime, 0f, sound.length - startTime);
                Invoke(nameof(StopAudio), duration);
            }
        }
    }

    private void StopAudio()
    {
        if (audioSource != null && audioSource.isPlaying)
        {
            audioSource.Stop();
        }
        isStoppingScheduled = false;
    }

    public bool IsPlaying
    {
        get { return audioSource.isPlaying; }
    }
}
