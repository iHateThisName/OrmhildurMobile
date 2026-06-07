using UnityEngine;
namespace Assets._Scripts.Utilities.Singleton {
    /// <summary>
    /// Defines a persistent singleton in Unity. That will not be destroyed when changing scenes
    /// Ensures that only one instance of this class exists in the application.
    /// </summary>
    /// <typeparam name="T">The generic type parameter. Must be a subclass of Component.</typeparam>
    /// Author: https://github.com/adammyhre
    public class PersistenSingleton<T> : MonoBehaviour where T : Component {
        public bool AutoUnparentOnAwake = true;
        protected static T instance;

        public static bool HasInstance => instance != null;
        public static T TryGetInstance() => HasInstance ? instance : null;

        public static T Instance {
            get {
                if (instance == null) {
                    instance = FindAnyObjectByType<T>();
                    if (instance == null) {
                        GameObject go = new GameObject(typeof(T).Name + " Auto-Generated");
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

            if (instance == null) {
                instance = this as T;
                DontDestroyOnLoad(gameObject);
            } else if (instance != this) {
                Destroy(gameObject);
            }
        }


    }
}
