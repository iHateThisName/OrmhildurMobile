using UnityEngine;
namespace Assets._Scripts.Utilities.Singleton {
    /// <summary>
    /// Persistent Regulator singleton, will destroy any other older components of the same type it finds on awake.
    /// </summary>
    /// <typeparam name="T">The generic type parameter. Must be a subclass of Component.</typeparam>
    /// Author: https://github.com/adammyhre
    public class RegulatorSingelton<T> : MonoBehaviour where T : Component {
        public bool AutoUnparentOnAwake = true;
        protected static T instance;

        public static bool HasInstance => instance != null;

        public float InitializationTime { get; private set; }

        public static T Instance {
            get {
                if (instance == null) {
                    instance = FindAnyObjectByType<T>();
                    if (instance == null) {
                        GameObject go = new GameObject(typeof(T).Name + " Auto-Generated");
                        go.hideFlags = HideFlags.HideAndDontSave;
                        instance = go.AddComponent<T>();
                    }
                }
                return instance;
            }
        }

        protected virtual void Awake() { InitializeSingletion(); }

        protected virtual void InitializeSingletion() {
            if (!Application.isPlaying) return;
            if (this.AutoUnparentOnAwake) transform.SetParent(null);
            InitializationTime = Time.time;
            DontDestroyOnLoad(gameObject);

            T[] oldInstances = FindObjectsByType<T>(FindObjectsSortMode.None);

            foreach (T old in oldInstances) {
                if (old.GetComponent<RegulatorSingelton<T>>().InitializationTime < InitializationTime) {
                    Destroy(old.gameObject);
                }
            }

            if (instance == null) {
                instance = this as T;
            }
        }
    }
}
