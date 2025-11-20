using System;
using System.Collections.Generic;
using Core.UI;
using UnityEngine;


public abstract class UIBase : MonoBehaviour
{
#region Field

    public abstract PanelName panelName { get; }

    public GameObject GameObj => gameObject;
    //因为可能要多次调用，所以现在这里获取一次
    private Dictionary<string, UIBase> panelDic = UIModule.Instance.PanelDic;

#endregion


#region Init

    public void StartUI(System.Object arg) { DoStart(arg); }
    private void OnEnable() { DoEnable(); }
    private void Update() { DoUpdate(); }
    private void FixedUpdate() { DoFixUpdate(); }
    private void OnDisable() { DoDisable(); }
    private void OnDestroy() { DoDestroy(); }

#endregion


#region override

    protected virtual void DoStart(System.Object arg) { }
    protected virtual void DoEnable() { }
    protected virtual void DoUpdate() { }
    protected virtual void DoFixUpdate() { }
    protected virtual void DoDisable() { }
    protected virtual void DoDestroy() { }

#endregion
}