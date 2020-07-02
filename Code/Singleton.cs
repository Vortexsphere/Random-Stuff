using UnityEngine;

/// <summary>
/// The base class for singletons. Classes should inherit from this if they wish to be singletons.
/// Note: Singleton inherits from <see cref="MonoBehaviour"/>.
/// </summary>
/// <typeparam name="T">The type of class you are making into a singleton.</typeparam>
public class Singleton<T> : MonoBehaviour where T : Singleton<T>
{
    private static T _instance;
    public static T I
    {
        get { return _instance; }
    }

    protected virtual void Awake()
    {
        if (_instance != null)
        {
            Debug.LogError($"[Singleton] Trying to instantiate a second instance of a singleton class: {this}.");
        }
        else
        {
            _instance = (T)this;
        }
    }

    protected virtual void OnDestroy()
    {
        if (_instance == this)
        {
            _instance = null;
        }
    }

    // This shouldn't be needed if you follow good practices and ensure that you
    // always create your manager singletons before you first need them, but it
    // doesn't hurt to have it just in case.
    public static bool IsInitialized()
    {
        return _instance != null;
    }
}

////////////////////////////////////////////////////////////////////////////////
// EXAMPLE USAGE
////////////////////////////////////////////////////////////////////////////////
public class GameManager : Singleton<GameManager>
{
    protected override void Awake()
    {
        base.Awake();
    }

    protected override void OnDestroy()
    {
        base.OnDestroy();
    }

    private void Start()
    {
        // Setup all your other managers and stuff.
    }

    public void DoSomething()
    {
        // Do something!
    }
}

public class SomeOtherClass : MonoBehaviour
{
    private void Start()
    {
        GameManager.I.DoSomething();
    }
}