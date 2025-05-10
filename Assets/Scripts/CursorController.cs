using UnityEngine;
using UnityEngine.EventSystems;

public class CursorController : MonoBehaviour
{
    [field: Header("References")]
    [field: SerializeField] public Texture2D CursorBase { get; private set; } = null;
    [field: SerializeField] public Texture2D CursorClick { get; private set; } = null;
    [field: SerializeField] public Vector2 ClickPosition { get; private set; } = Vector2.zero;

    void Start()
    {
        Cursor.SetCursor(CursorBase, ClickPosition, CursorMode.Auto);
    }

    void Update()
    {
        if (Game.Instance.InputController.LeftClick ||
            Game.Instance.InputController.RightClick ||
            Game.Instance.InputController.MiddleClick)
        {
            Cursor.SetCursor(CursorClick, ClickPosition, CursorMode.Auto);
        }
        else
        {
            Cursor.SetCursor(CursorBase, ClickPosition, CursorMode.Auto);
        }
    }
}
