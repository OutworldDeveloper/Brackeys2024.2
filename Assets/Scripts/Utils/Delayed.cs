using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Delayed : MonoBehaviour
{

    private static Delayed _instance;

    public static void Do(Action action, float delay)
    {
        if (_instance == null)
        {
            _instance = new GameObject().AddComponent<Delayed>();
        }

        _instance._calls.Add(new Call(action, Time.time + delay));
    }

    private readonly List<Call> _calls = new List<Call>(10);

    private void Update()
    {
        for (int i = _calls.Count - 1; i >= 0; i--)
        {
            var call = _calls[i];

            if (Time.time > call.CallTime)
            {
                call.Action();
                _calls.RemoveAt(i);
            }
        }
    }

    private readonly struct Call
    {

        public readonly Action Action;
        public readonly float CallTime;

        public Call(Action action, float callTime)
        {
            Action = action;
            CallTime = callTime;
        }

    }

}
