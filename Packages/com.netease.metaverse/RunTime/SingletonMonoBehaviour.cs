using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MetaVerse.FrameWork
{
    public class SingletonMonoBehaviour<T> : MonoBehaviour where T : MonoBehaviour
    {
        private static object _singletonLock = new object();
        private static T _instance;

        public static T Instance
        {
            get
            {
                if (_instance == null)
                {
                    lock (_singletonLock)
                    {
                        T[] singletonInstances = FindObjectsOfType(typeof(T)) as T[];
                        if (singletonInstances.Length > 1)
                        {
                            if (Application.isEditor)
                                Debug.LogError("MonoSingleton<T>.Instance: Only 1 singleton instance can exist in the scene. Null will be returned.");
                            return null;
                        }

                        if (singletonInstances.Length == 0)
                        {
                            GameObject singletonInstance = new GameObject();
                            _instance = singletonInstance.AddComponent<T>();
                            singletonInstance.name = "(singleton) " + typeof(T).ToString();
                        }
                        else
                            _instance = singletonInstances[0];
                    }

                    _instance = FindObjectOfType<T>();
                }

                return _instance;
            }
        }

        protected virtual void OnEnable()
        {
            if (Instance != this)
            {
                Destroy(this);
            }
        }
    }
}