using System;
using UnityEngine;

namespace Core.Manager
{
    public class GameManager: MonoBehaviour
    {
        private void Start()
        {
            
        }

        private void Update()
        {
            UIModule.Instance.Update(Time.deltaTime);
        }

        private void OnDestroy()
        {
            UIModule.Instance.DestroyUI();
        }
    }
}