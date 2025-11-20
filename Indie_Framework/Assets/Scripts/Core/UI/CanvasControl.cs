using System;
using UnityEngine;

namespace Core.UI
{
    public class CanvasControl: MonoBehaviour
    {
        public Transform canvasTransform;
        public static CanvasControl instance;

        private void Awake()
        {
            if (instance != null && instance != this)
            {
                Debug.LogWarning("Multiple CanvasControl instances detected! Destroying duplicate.");
                Destroy(gameObject);
                return;
            }

            instance = this;
            DontDestroyOnLoad(gameObject);
        }

        private void OnDestroy()
        {
            if (instance == this)
            {
                instance = null;
            }
        }
    }
}