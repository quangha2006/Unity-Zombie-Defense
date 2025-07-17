using UnityEngine;

public class MonoBehaviourSingleton<T> : MonoBehaviour where T : MonoBehaviourSingleton<T>
{
    public static T Instance { get; private set; }
    public static bool IsInstantiated => Instance != null;

    protected virtual void Awake()
    {
        if (IsInstantiated && Instance != (T)this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = (T)this;
    }
    protected virtual void OnDestroy()
    {
        if (Instance == (T)this)
        {
            Instance = null;
        }
    }
}
