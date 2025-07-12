using UnityEngine;

public abstract class Singleton<T> : MonoBehaviour where T : MonoBehaviour
{
    private static T _instance;
    private static bool _shuttingDown = false;
    private static readonly object _lock = new object();

    public static T Instance
    {
        get
        {
            if (_shuttingDown)
            {
                return null;
            }

            lock(_lock)
            {
                if (_instance == null)
                {
                    _instance = FindAnyObjectByType<T>();
                }

                // Create new GameObject if not found
                if (_instance == null)
                {
                    Debug.LogWarningFormat("Unable to find singleton of type {0}. Creating a new one.", new object[] { typeof(T).Name });

                    GameObject singletonObject = new GameObject(typeof(T).Name);
                    _instance = singletonObject.AddComponent<T>();
                }

                return _instance;
            }
        }
    }

    protected virtual void OnApplicationQuit()
    {
        _shuttingDown = true;
    }

    protected virtual void OnDestroy()
    {
        if(_instance == this)
        {
            _shuttingDown = true;
        }
    }
}