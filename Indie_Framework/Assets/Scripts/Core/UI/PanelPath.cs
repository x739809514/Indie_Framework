using System.Collections.Generic;

namespace Core.UI
{
    public static class PanelPath
    {
        public static readonly Dictionary<PanelName, string> path = new Dictionary<PanelName, string>()
        {
            { PanelName.TestPanel, "uiPrefab/TestPanel" },
            { PanelName.PopPanel ,"uiPrefab/PopPanel"}
        };
    }

    public enum PanelName
    {
        TestPanel,
        PopPanel,
    }
}