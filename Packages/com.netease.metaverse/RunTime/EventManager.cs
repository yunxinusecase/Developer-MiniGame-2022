using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

namespace MetaVerse.FrameWork
{
    // 
    // Event System
    //
    public static class EventManager
    {
        public static void TriggerEvent(Action input)
        {
            if (input != null)
            {
                input.Invoke();
            }
        }

        public static void TriggerEvent<T>(Action<T> input, T value)
        {
            if (input != null)
            {
                input.Invoke(value);
            }
        }

        public static void TriggerEvent<T, U>(Action<T, U> input, T p1, U p2)
        {
            if (input != null)
            {
                input.Invoke(p1, p2);
            }
        }

        public static void TriggerEvent<T, U, V>(Action<T, U, V> input, T p1, U p2, V p3)
        {
            if (input != null)
            {
                input.Invoke(p1, p2, p3);
            }
        }

        public static void TriggerEvent<T, U, V, S>(Action<T, U, V, S> input, T p1, U p2, V p3, S p4)
        {
            if (input != null)
            {
                input.Invoke(p1, p2, p3, p4);
            }
        }

        public static void TriggerEvent<T, U, V, S, Y>(Action<T, U, V, S, Y> input, T p1, U p2, V p3, S p4, Y p5)
        {
            if (input != null)
            {
                input.Invoke(p1, p2, p3, p4, p5);
            }
        }
    }
}