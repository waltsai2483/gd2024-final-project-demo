using UnityEngine;

public abstract class MonoSingleton<T> : MonoBehaviour where T : MonoSingleton<T>
{
    private static T _instance = null;

    public static T instance
    {
        get
        {
            // Instance requiered for the first time, we look for it
            if (!_instance)
            {
                _instance = GameObject.FindObjectOfType(typeof(T)) as T;

                // Object not found, we create a temporary one
                if (!_instance)
                {
                    Debug.LogWarning("No instance of " + typeof(T) + ", a temporary one is created.");

                    isTemporaryInstance = true;
                    _instance = new GameObject("Temp Instance of " + typeof(T), typeof(T)).GetComponent<T>();

                    // Problem during the creation, this should not happen
                    if (!_instance)
                    {
                        Debug.LogError("Problem during the creation of " + typeof(T));
                    }
                }

                if (!_isInitialized)
                {
                    _isInitialized = true;
                    _instance.Init();
                }
            }

            return _instance;
        }
    }

    public static bool isTemporaryInstance { private set; get; }

    private static bool _isInitialized;

    // If no other monobehaviour request the instance in an awake function
    // executing before this one, no need to search the object.
    private void Awake()
    {
        if (_instance == null)
        {
            _instance = this as T;
        }
        else if (_instance != this)
        {
            Debug.LogError("Another instance of " + GetType() + " is already exist! Destroying self...");
            DestroyImmediate(this);
            return;
        }

        if (!_isInitialized)
        {
            DontDestroyOnLoad(gameObject);
            _isInitialized = true;
            _instance.Init();
        }
    }


    /// <summary>
    /// This function is called when the instance is used the first time
    /// Put all the initializations you need here, as you would do in Awake
    /// </summary>
    public virtual void Init()
    {
    }

    /// Make sure the instance isn't referenced anymore when the user quit, just in case.
    private void OnApplicationQuit()
    {
        _instance = null;
    }
}