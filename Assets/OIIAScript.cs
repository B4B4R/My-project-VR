using System.Collections;
using UnityEngine;

public class OIIAScript : MonoBehaviour
{
    [Header("Audio Players")]
    public LoopPlaylistPlayer loopPlayer;
    public QuickSound quickSound;

    [Header("Animator")]
    public Animator animator;
    public string defaultAnim = "Bounce";
    public string quickSoundAnim = "Spin";

    public void PlayEvent()
    {
        StartCoroutine(PlayQuickSoundEvent());
    }

    private IEnumerator PlayQuickSoundEvent()
    {
        // 1. Arrêter la playlist
        loopPlayer.enabled = false;
        if (loopPlayer.GetComponent<AudioSource>().isPlaying)
        {
            loopPlayer.GetComponent<AudioSource>().Stop();
        }

        // 2. Changer l'animation
        if (animator != null)
        {
            animator.Play(quickSoundAnim);
        }

        // 3. Jouer le QuickSound
        quickSound.Play();

        // 4. Attendre la fin du son
        float duration;
        if (quickSound.endTime > 0f)
        {
            duration = Mathf.Clamp(quickSound.endTime - quickSound.startTime, 0f, quickSound.sound.length - quickSound.startTime);
        }
        else
        {
            duration = quickSound.sound.length - quickSound.startTime;
        }

        yield return new WaitForSeconds(duration);

        // 5. Revenir à l'animation par défaut
        if (animator != null)
        {
            animator.Play(defaultAnim);
        }

        // 6. Relancer la playlist
        loopPlayer.enabled = true;
    }
}