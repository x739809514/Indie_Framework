using Core.UI;
using UnityEngine.UI;

public class PopPanel : UIBase
{
    public override PanelName panelName => PanelName.PopPanel;

    public void CloseSelf() { UIModule.Instance.CloseUI(); }
}