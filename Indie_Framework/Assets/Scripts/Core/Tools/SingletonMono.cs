using UnityEngine;

namespace Core.Tools
{
    public class SingletonMono<T> : MonoBehaviour where T : MonoBehaviour
    {
        private static T instance = default(T);

        public static T Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = FindFirstObjectByType<T>();
                }

                return instance;
            }
        }

        private void Awake()
        {
            if (instance != null)
            {
                Destroy(this.gameObject);
                return;
            }

            DontDestroyOnLoad(this.gameObject);
            instance = this.gameObject.GetComponent<T>();
            OnAwake();
        }

        protected virtual void OnAwake()
        {
        }
    }
}