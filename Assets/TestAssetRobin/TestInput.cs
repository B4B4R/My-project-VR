using UnityEngine;
using System;
using System.Collections.Generic;
using UnityEngine.Events;

public class TestInput : MonoBehaviour
{
    [Serializable]
    public class KeyBinding
    {
        public KeyCode key;
        public UnityEvent action;
    }

    public KeyBinding[] keyBindings;

    void Update()
    {
        foreach (var binding in keyBindings)
        {
            if (Input.GetKeyDown(binding.key))
            {
                binding.action.Invoke();
            }
        }
    }
}