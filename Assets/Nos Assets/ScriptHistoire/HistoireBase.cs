using UnityEngine;
using UnityEngine.Events;
using System.Collections;

public class HistoireBase : MonoBehaviour
{
    [Header("Reference")]
    public FloatingCompanion floatingCompanion;

    [Header("Audio Clips")]
    public AudioClip son1;
    public AudioClip son2;
    public AudioClip son3;
    public AudioClip son4;
    public AudioClip son5;
    public AudioClip son6;
    public AudioClip son7;
    public AudioClip son8;

    public AudioClip sonUnique;
    private bool uniqueAudioJoue = false;

    [Header("Volume")]
    [Range(0f, 1f)]
    public float volume = 1f;

    [Header("Start Settings")]
    public float delayAvantDebut = 10f;

    [Header("Histoire4 Settings")]
    public GameObject objetTremblement;           // Objet contenant AudioSource pour tremblement
    public AudioClip sonTremblement;              // Son tremblement
    public UnityEvent evenementApresTremblement; // Event à activer après 9s

    private int etapeActuelle = 0;
    private bool[] checkpoints = new bool[8];

    private void Start()
    {
        if (floatingCompanion == null)
        {
            Debug.LogWarning("[Histoire] FloatingCompanion non assigné.");
            return;
        }

        Debug.Log("[Histoire] Initialisation. Companion désactivé.");
        floatingCompanion.gameObject.SetActive(false);

        StartCoroutine(DebutHistoire());
    }

    private IEnumerator DebutHistoire()
    {
        Debug.Log("[Histoire] Attente de " + delayAvantDebut + " secondes avant démarrage.");
        yield return new WaitForSeconds(delayAvantDebut);

        floatingCompanion.gameObject.SetActive(true);
        Debug.Log("[Histoire] Companion activé. Lancement Histoire1.");

        Histoire1();
    }

    // Fonctions pour chaque étape
    public void Histoire1() => TenterLecture(1, son1);
    public void Histoire2() => TenterLecture(2, son2);
    public void Histoire3() => TenterLecture(3, son3);
    public void Histoire5() => TenterLecture(5, son5);
    public void Histoire6() => TenterLecture(6, son6);
    public void Histoire7() => TenterLecture(7, son7);
    public void Histoire8() => TenterLecture(8, son8);

    // Histoire4 spéciale
    public void Histoire4()
    {
        StartCoroutine(Histoire4Coroutine());
    }

    private IEnumerator Histoire4Coroutine()
    {
        int numeroEtape = 4;

        if (checkpoints[numeroEtape - 1])
        {
            Debug.LogWarning("[Histoire] Étape 4 déjà jouée.");
            yield break;
        }

        if (etapeActuelle != numeroEtape - 1)
        {
            Debug.LogWarning("[Histoire] Étape 4 impossible : ordre incorrect. Étape actuelle = " + etapeActuelle);
            yield break;
        }

        checkpoints[numeroEtape - 1] = true;
        etapeActuelle = numeroEtape;

        // On s'assure que le companion est actif
        if (!floatingCompanion.gameObject.activeInHierarchy)
            floatingCompanion.gameObject.SetActive(true);

        // Lecture du son Histoire4
        if (son4 != null)
        {
            floatingCompanion.PlaySoundOnce(son4, volume);
            Debug.Log("[Histoire] Son Histoire4 joué.");
        }
        else
        {
            Debug.LogWarning("[Histoire] Son4 non assigné.");
        }

        // Attente 15 secondes avant tremblement
        Debug.Log("[Histoire] Attente 15 secondes avant tremblement...");
        yield return new WaitForSeconds(15f);

        // Tremblement
        if (objetTremblement != null && sonTremblement != null)
        {
            AudioSource source = objetTremblement.GetComponent<AudioSource>();
            if (source == null)
                source = objetTremblement.AddComponent<AudioSource>();
            source.PlayOneShot(sonTremblement, volume);
            Debug.Log("[Histoire] Son tremblement de terre joué.");
        }
        else
        {
            Debug.LogWarning("[Histoire] Objet ou son tremblement non assigné.");
        }

        // 9 secondes  Event
        yield return new WaitForSeconds(7f);
        if (evenementApresTremblement != null)
        {
            evenementApresTremblement.Invoke();
            Debug.Log("[Histoire] Event après tremblement activé.");
        }

        // 5 secondes  Histoire5
        yield return new WaitForSeconds(5f);
        Debug.Log("[Histoire] Lancement automatique Histoire5.");
        Histoire5();
    }

    // Skip de l'étape 3
    public void skipHistoire3()
    {
        Debug.Log("[Histoire] Demande de skip de l'étape 3");

        if (etapeActuelle < 2)
        {
            Debug.LogWarning("[Histoire] Impossible de sauter Histoire3 : Histoire2 non terminée. Étape actuelle = " + etapeActuelle);
            return;
        }

        // Marque Histoire3 comme faite si non encore jouée
        if (!checkpoints[2])
        {
            checkpoints[2] = true;
            etapeActuelle = 3;
            Debug.Log("[Histoire] Étape 3 non jouée, marquée comme sautée. Étape actuelle = " + etapeActuelle);
        }
        else
        {
            etapeActuelle = 3;
            Debug.Log("[Histoire] Étape 3 déjà jouée, passage direct à Histoire4. Étape actuelle = " + etapeActuelle);
        }

        // Lance Histoire4
        Histoire4();
    }

    // Fonction centrale pour lire les sons des autres étapes
    private void TenterLecture(int numeroEtape, AudioClip clip)
    {
        Debug.Log("[Histoire] Demande de lecture étape " + numeroEtape);

        if (clip == null)
        {
            Debug.LogWarning("[Histoire] AudioClip manquant pour l'étape " + numeroEtape);
            return;
        }

        if (checkpoints[numeroEtape - 1])
        {
            Debug.LogWarning("[Histoire] Étape " + numeroEtape + " refusée : déjà validée.");
            return;
        }

        if (numeroEtape != etapeActuelle + 1)
        {
            Debug.LogWarning("[Histoire] Étape " + numeroEtape + " refusée : ordre incorrect. Étape actuelle attendue = " + (etapeActuelle + 1));
            return;
        }

        floatingCompanion.PlaySoundOnce(clip, volume);
        checkpoints[numeroEtape - 1] = true;
        etapeActuelle = numeroEtape;
        Debug.Log("[Histoire] Étape " + numeroEtape + " complétée. Nouvelle étape actuelle = " + etapeActuelle);

        // Si c'est Histoire4, utiliser la coroutine spéciale
        if (numeroEtape == 4)
            Histoire4();
    }

    public void JouerAudioUnique()
    {
        if (uniqueAudioJoue)
        {
            Debug.LogWarning("[Histoire] Audio unique déjà joué.");
            return;
        }

        if (sonUnique == null)
        {
            Debug.LogWarning("[Histoire] Audio unique non assigné.");
            return;
        }

        // S'assure que le companion est actif
        if (!floatingCompanion.gameObject.activeInHierarchy)
            floatingCompanion.gameObject.SetActive(true);

        // Lance la coroutine qui attend la fin des autres sons
        StartCoroutine(JouerAudioUniqueCoroutine());
    }

    private IEnumerator JouerAudioUniqueCoroutine()
    {
        AudioSource source = floatingCompanion.GetComponent<AudioSource>();
        if (source == null)
            source = floatingCompanion.gameObject.AddComponent<AudioSource>();

        // Attendre que le compagnon ait fini tout autre son
        while (source.isPlaying)
        {
            yield return null;
        }

        // Jouer l'audio unique
        source.PlayOneShot(sonUnique, volume);
        uniqueAudioJoue = true;
        Debug.Log("[Histoire] Audio unique joué après la fin des autres sons.");
    }

}
