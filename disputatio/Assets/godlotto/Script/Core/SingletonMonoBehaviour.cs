using UnityEngine;

/// <summary>
/// Generic singleton base for MonoBehaviour-derived managers.
/// Handles duplicate detection, optional <c>DontDestroyOnLoad</c>, and a
/// virtual hook (<see cref="OnSingletonAwake"/>) for subclass-specific init.
/// </summary>
/// <typeparam name="T">Concrete MonoBehaviour subclass.</typeparam>
public abstract class SingletonMonoBehaviour<T> : MonoBehaviour where T : SingletonMonoBehaviour<T>
{
    private static T _instance;

    public static T Instance
    {
        get => _instance;
        protected set => _instance = value;
    }

    /// <summary>
    /// Override to <c>true</c> when the singleton must survive scene transitions
    /// (<c>DontDestroyOnLoad</c> is called automatically).
    /// </summary>
    protected virtual bool PersistAcrossScenes => false;

    /// <summary>
    /// Called once on the surviving instance, immediately after <see cref="Instance"/>
    /// is assigned. Place one-time initialization here instead of overriding <c>Awake</c>.
    /// </summary>
    protected virtual void OnSingletonAwake() { }

    protected virtual void Awake()
    {
        if (_instance != null && _instance != this)
        {
            Destroy(gameObject);
            return;
        }

        _instance = (T)this;

        if (PersistAcrossScenes)
            DontDestroyOnLoad(gameObject);

        OnSingletonAwake();
    }

    protected virtual void OnDestroy()
    {
        if (_instance == this)
            _instance = null;
    }
}
