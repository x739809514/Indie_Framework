using Core.UI;
using UnityEngine;
using UnityEngine.UI;

namespace Test
{
    public class TestPanel: UIBase
    {
        public override PanelName panelName => PanelName.TestPanel;

        public GameObject txt;
        public Button btn;

        protected override void DoStart(object arg)
        {
            base.DoStart(arg);
            btn.onClick.AddListener(DoTrigger);
        }

        private void DoTrigger()
        {
            txt.SetActive(true);
        }

        protected override void DoDestroy()
        {
            base.DoDestroy();
            btn.onClick.RemoveAllListeners();
        }

        public void OpenPopPanel()
        {
            UIModule.Instance.PopPanel<PopPanel>(PanelName.PopPanel);
        }
    }
}