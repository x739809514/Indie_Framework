using System;
using Core.UI;
using UnityEngine;

namespace Test
{
    public class UITest: MonoBehaviour
    {
        public InputReader inputReader;

        private void Start()
        {
            inputReader.EnablePlayerInput();
            inputReader.InteractEvent += OpenTestPanel;
        }

        private void OpenTestPanel()
        {
            UIModule.Instance.OpenPanel<TestPanel>(PanelName.TestPanel);
        }

        private void OnDestroy()
        {
            inputReader.InteractEvent -= OpenTestPanel;
        }
    }
}