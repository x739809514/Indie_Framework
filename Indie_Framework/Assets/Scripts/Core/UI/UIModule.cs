using System;
using System.Collections.Generic;
using Core.UI;
using Unity.VisualScripting;
using UnityEngine;
using Object = UnityEngine.Object;


public class UIModule
{
#region Filed

    public Dictionary<string, UIBase> PanelDic = new Dictionary<string, UIBase>();
    private Stack<UIBase> UIStack;
    private UIBase[] uiArr;
    private static UIBase curPanel;
    private float timer = 0;
    private static UIModule instance;
    private static readonly object _lock = new object();

    public static UIModule Instance
    {
        get
        {
            if (instance == null)
            {
                lock (_lock)
                {
                    if (instance == null) { instance = new UIModule(); }
                }
            }

            return instance;
        }
    }

    /// <summary>
    /// 销毁列表，每五秒会执行一次面板销毁，将不用的面板销毁
    /// </summary>
    private List<UIBase> DestroyList = new List<UIBase>();

#endregion


#region Public

    /// <summary>
    /// 加载一个新页面
    /// </summary>
    /// <param name="panelName"></param>
    /// <param name="action"></param>
    /// <typeparam name="T"></typeparam>
    public void OpenPanel<T>(PanelName panelName,System.Object arg = null) where T : UIBase
    {
        if (CanvasControl.instance == null)
        {
            Debug.LogError("CanvasControl instance is null! Make sure CanvasControl is in the scene.");
            return;
        }
        if ( PanelPath.path.TryGetValue(panelName,out var path)==false)
        {
            Debug.LogError($"Path {panelName} doesn't exist");
            return;
        }
        var panel = IsPanelExit<T>();
        if (panel != null)
        {
            OpenUI(path, panel, arg);
            return;
        }

        //正常应该是从AssetBundle中接入，这里先暂且从resource中读取
        
        var obj = Resources.Load<GameObject>(path);
        if (obj == null)
        {
            Debug.LogError($"Failed to load UI prefab at path: {path}");
            return;
        }

        var go = Object.Instantiate(obj, CanvasControl.instance.canvasTransform);
        var uiComponent = go.GetComponent<T>();
        if (uiComponent == null)
        {
            Debug.LogError($"Prefab at {path} does not have component {typeof(T).Name}");
            Object.Destroy(go);
            return;
        }
        OpenUI(path, uiComponent, arg);
    }

    /// <summary>
    /// 弹出一个界面
    /// </summary>
    /// <param name="panelName"></param>
    /// <param name="action"></param>
    /// <typeparam name="T"></typeparam>
    public void PopPanel<T>(PanelName panelName,System.Object arg = null) where T : UIBase
    {
        if (CanvasControl.instance == null)
        {
            Debug.LogError("CanvasControl instance is null! Make sure CanvasControl is in the scene.");
            return;
        }
        if ( PanelPath.path.TryGetValue(panelName,out var path)==false)
        {
            Debug.LogError($"Path {panelName} doesn't exist");
            return;
        }
        var panel = IsPanelExit<T>();
        if (panel != null)
        {
            PopUI(path, panel, arg);
            return;
        }

        // 根据路径创建prefab
       
        var obj = Resources.Load<GameObject>(path);
        if (obj == null)
        {
            Debug.LogError($"Failed to load UI prefab at path: {path}");
            return;
        }

        var go = Object.Instantiate(obj, CanvasControl.instance.canvasTransform);
        var uiComponent = go.GetComponent<T>();
        if (uiComponent == null)
        {
            Debug.LogError($"Prefab at {path} does not have component {typeof(T).Name}");
            Object.Destroy(go);
            return;
        }

        PopUI(path, uiComponent, arg);
    }

    /// <summary>
    /// 直接关掉最上层的UI
    /// </summary>
    public void CloseUI()
    {
        if (UIStack == null || UIStack.Count == 0)
        {
            Debug.LogWarning("UIStack is empty, cannot close UI!");
            return;
        }

        var ui = UIStack.Pop();
        ui.GameObj.SetActive(false);
        // 加入待销毁列表
        DestroyList.Add(ui);

        if (UIStack.Count > 0)
        {
            var top = UIStack.Peek();
            top.GameObj.SetActive(true);
            curPanel = top;
        }
        else { curPanel = null; }

        uiArr = UIStack.ToArray();
    }

    public void CloseUI<T>()
    {
        if (UIStack == null || UIStack.Count == 0)
        {
            Debug.LogWarning("UIStack is empty, cannot close UI!");
            return;
        }

        var panel = IsPanelExit<T>();
        if (panel == null) return;

        var tempStack = new Stack<UIBase>();
        while (UIStack.Count > 0)
        {
            var p = UIStack.Pop();
            if (p is not T t) { tempStack.Push(p); }
            else
            {
                p.GameObj.SetActive(false);
                DestroyList.Add(p);
                break;
            }
        }

        while (tempStack.Count > 0)
        {
            var tp = tempStack.Pop();
            UIStack.Push(tp);
        }

        if (UIStack.Count > 0)
        {
            var top = UIStack.Peek();
            top.GameObj.SetActive(true);
            curPanel = top;
        }
        else { curPanel = null; }

        uiArr = UIStack.ToArray();
    }

    public void Update(float deltaTime)
    {
        if (timer < 5f) { timer += Time.deltaTime; }
        else
        {
            for (int i = DestroyList.Count - 1; i >= 0; i--)
            {
                // 只销毁未激活的UI，且不再从字典中移除（字典管理统一由销毁方法处理）
                if (DestroyList[i] != null && DestroyList[i].GameObj != null &&
                    DestroyList[i].GameObj.activeSelf == false)
                {
                    DoDestroySingle(DestroyList[i]);
                }
            }

            // 清空销毁列表
            DestroyList.Clear();
            timer = 0;
        }

        //UpdateUI(deltaTime);
    }

    public void DestroyUI() { DoDestroy(); }

#endregion


#region Private

    private void PopUI(string path, UIBase ui, System.Object arg)
    {
        if (UIStack == null) { UIStack = new Stack<UIBase>(); }

        if (UIStack.Count != 0)
        {
            var topUI = UIStack.Peek();
            //如果页面已经打开，则无视

            if (PanelDic.ContainsKey(path) && topUI == PanelDic[path]) return;
        }

        if (PanelDic.ContainsKey(path))
        {
            //把界面压栈
            var existingUI = PanelDic[path];

            // 确保UI不在DestroyList中（可能之前被标记为待销毁）
            if (DestroyList.Contains(existingUI))
            {
                DestroyList.Remove(existingUI);
            }

            UIStack.Push(existingUI);
            existingUI.GameObj.SetActive(true);
        }
        else
        {
            PanelDic.Add(path, ui);
            UIStack.Push(ui);
            ui.GameObj.SetActive(true);
            ui.StartUI(arg);
        }

        curPanel = ui;
        uiArr = UIStack.ToArray();
    }
    private void OpenUI(string path, UIBase ui, System.Object arg)
    {
        if (UIStack == null) { UIStack = new Stack<UIBase>(); }

        // 先检查是否已经在栈顶
        if (UIStack.Count > 0)
        {
            var topUI = UIStack.Peek();
            if (PanelDic.ContainsKey(path) && topUI == PanelDic[path])
            {
                // 已经在顶部，直接返回
                return;
            }

            // 不在顶部，Pop并隐藏
            UIStack.Pop();
            topUI.GameObj.SetActive(false);
            // 加入待销毁列表
            DestroyList.Add(topUI);
        }

        if (PanelDic.ContainsKey(path))
        {
            //把界面压栈
            var existingUI = PanelDic[path];

            // 确保UI不在DestroyList中（可能之前被标记为待销毁）
            if (DestroyList.Contains(existingUI))
            {
                DestroyList.Remove(existingUI);
            }

            UIStack.Push(existingUI);
            existingUI.GameObj.SetActive(true);
        }
        else
        {
            PanelDic.Add(path, ui);
            UIStack.Push(ui);
            ui.GameObj.SetActive(true);
            ui.StartUI(arg);
        }

        curPanel = ui;
        uiArr = UIStack.ToArray();
    }
    private void DoDestroy()
    {
        foreach (var panel in PanelDic.Values)
        {
            if (panel != null && panel.GameObj != null)
            {
                Object.Destroy(panel.GameObj);
            }
        }

        UIStack?.Clear();
        PanelDic?.Clear();
    }

    private void DoDestroySingle(UIBase ui)
    {
        if (ui == null || ui.GameObj == null) return;
        if (PanelPath.path.TryGetValue(ui.panelName,out var path)==false)
        {
            Debug.LogError($"Path {ui.panelName} doesn't exist");
            return;
        }
        if (PanelDic.ContainsKey(path))
        {
            PanelDic.Remove(path);
        }
        Object.Destroy(ui.GameObj);
    }

    private T IsPanelExit<T>()
    {
        string panelRemove = String.Empty;
        foreach (var panel in PanelDic)
        {
            if (panel.Value is T t)
            {
                // 可能存在竞态问题，检查一下
                if (panel.Value.GameObj==null)
                {
                    panelRemove = panel.Key;
                    break;
                }
                return t;
            }
        }

        if (panelRemove!=String.Empty)
        {
            PanelDic.Remove(panelRemove);
        }

        return default;
    }

#endregion
}