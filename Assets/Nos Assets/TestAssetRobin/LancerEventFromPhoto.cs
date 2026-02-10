using UnityEngine;
using UnityEngine.Events;
using System.Collections.Generic;

public class LancerEventFromPhoto : MonoBehaviour
{
    [Header("Liste d'événements à lancer")]
    public List<UnityEvent> eventsToTrigger = new List<UnityEvent>();

    public void TriggerAllEvents()
    {
        foreach (UnityEvent unityEvent in eventsToTrigger)
        {
            if (unityEvent != null)
                unityEvent.Invoke();
        }
    }
}
