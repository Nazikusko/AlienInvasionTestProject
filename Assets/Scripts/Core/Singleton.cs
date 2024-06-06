using UnityEngine;

public class Singleton<T> : MonoBehaviour where T : MonoBehaviour
{
    private static T _instance;

    public static T Instance => Init();

    public static bool InstanceExists() => _instance != null;

    public static T Init()
    {
        if (_instance != null) return _instance;

        var go = new GameObject { name = typeof(T).ToString() };
        _instance = go.AddComponent<T>();
        DontDestroyOnLoad(_instance);
#if UNITY_EDITOR
        Debug.LogWarning($"[[ ManagerSingleton: Asked to create instance {go.name}!! ]]");
#endif
        return _instance;
    }

    public static void DestroyInstance()
    {
        if (_instance == null) return;
        Destroy(_instance.gameObject);
    }

    void Awake()
    {
        if (_instance == null)
        {
            _instance = this as T;
            if (transform.parent == null) DontDestroyOnLoad(this);
        }
        else if (_instance != this)
        {
            Destroy(gameObject);
            return;
        }

        DoAwake();
    }

    void OnDestroy()
    {
        if (_instance == this)
        {
            _instance = null;
            DoDestroy();
        }
        else if (_instance != null) OnInstanceDuplicate();
    }

    protected virtual void DoAwake() { /* empty */ }
    protected virtual void DoDestroy() { /* empty */ }
    protected virtual void OnInstanceDuplicate() { /* empty */ }
}