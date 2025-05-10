using UnityEngine;
using UnityEngine.InputSystem;

public class InputController : MonoBehaviour
{
    [field: Header("Debug")]
    [field: SerializeField] public bool LeftClick { get; private set; } = false;
    [field: SerializeField] public bool RightClick { get; private set; } = false;
    [field: SerializeField] public bool MiddleClick { get; private set; } = false;
    [field: SerializeField] public bool ShiftClick { get; private set; } = false;
    [field: SerializeField] public Vector2 RightClickStart { get; private set; } = Vector2.zero;
    [field: SerializeField] public Vector2 MiddleClickStart { get; private set; } = Vector2.zero;
    [field: SerializeField] public Vector2 MoveDelta { get; private set; } = Vector2.zero;
    [field: SerializeField] public Vector2 MousePosition { get; private set; } = Vector2.zero;
    [field: SerializeField] public float MouseScroll { get; private set; } = 0;

    public void OnLeftClick(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            LeftClick = true;
        }
        else if (context.canceled)
        {
            LeftClick = false;
        }
    }

    public void ConsumeLeftClick()
    {
        LeftClick = false;
    }

    public void OnRightClick(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            RightClick = true;
            RightClickStart = MousePosition;
        }
        else if (context.canceled)
        {
            RightClick = false;
        }
    }

    public void OnMiddleClick(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            MiddleClick = true;
            MiddleClickStart = MousePosition;
        }
        else if (context.canceled)
        {
            MiddleClick = false;
        }
    }

    public void OnShiftClick(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            ShiftClick = true;
        }
        else if (context.canceled)
        {
            ShiftClick = false;
        }
    }

    public void OnMouseMove(InputAction.CallbackContext context)
    {
        MoveDelta = context.ReadValue<Vector2>();
    }

    public void OnMousePosition(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            MousePosition = context.ReadValue<Vector2>();
        }
    }

    public void OnMouseScroll(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            MouseScroll = context.ReadValue<Vector2>().y;
        }
    }
}
