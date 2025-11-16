using System;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;

[CreateAssetMenu(fileName = "InputReader", menuName = "Game/Input Reader")]
public class InputReader: ScriptableObject, InputSystem_Actions.IPlayerActions,InputSystem_Actions.IUIActions
{
    public event UnityAction<Vector2> MoveEvent; 
    public event UnityAction AttackEvent;
    public event UnityAction AttackCancleEvent;
    public event UnityAction InteractEvent;
    
    // UI
    public event UnityAction<Vector2> NavigateEvent;
    public event UnityAction<Vector2> ScrollWheelEvent;
    public event UnityAction SubmitEvent;
    public event UnityAction CancleEvent;
    public event UnityAction ClickEvent;

    private InputSystem_Actions inputSystemActions;
    
    private void OnEnable()
    {
        if (inputSystemActions==null)
        {
            inputSystemActions = new InputSystem_Actions();
            inputSystemActions.Player.SetCallbacks(this);
            inputSystemActions.UI.SetCallbacks(this);
        }
    }

    private void OnDisable()
    {
        inputSystemActions.Player.Disable();
        inputSystemActions.UI.Disable();
    }
    
    // 切换到玩家控制
    public void EnablePlayerInput()
    {
        inputSystemActions.Player.Enable();
        inputSystemActions.UI.Disable();
    }
    
    // 切换到UI控制
    public void EnableUIInput()
    {
        inputSystemActions.Player.Disable();
        inputSystemActions.UI.Enable();
    }


    public void OnMove(InputAction.CallbackContext context)
    {
        MoveEvent?.Invoke(context.ReadValue<Vector2>());
    }

    public void OnAttack(InputAction.CallbackContext context)
    {
        if (context.phase == InputActionPhase.Performed)
        {
            AttackEvent?.Invoke();
        }
    }

    public void OnInteract(InputAction.CallbackContext context)
    {
        if (context.phase == InputActionPhase.Performed)
        {
            InteractEvent?.Invoke();
        }
    }

    public void OnNavigate(InputAction.CallbackContext context)
    {
        NavigateEvent?.Invoke(context.ReadValue<Vector2>());
    }

    public void OnSubmit(InputAction.CallbackContext context)
    {
        if (context.phase == InputActionPhase.Performed)
        {
            SubmitEvent?.Invoke();
        }
    }

    public void OnCancel(InputAction.CallbackContext context)
    {
        if (context.phase == InputActionPhase.Performed)
        {
            CancleEvent?.Invoke();
        }
    }

    public void OnClick(InputAction.CallbackContext context)
    {
        if (context.phase == InputActionPhase.Performed)
        {
            ClickEvent?.Invoke();
        }
    }

    public void OnScrollWheel(InputAction.CallbackContext context)
    {
        ScrollWheelEvent?.Invoke(context.ReadValue<Vector2>());
    }
}