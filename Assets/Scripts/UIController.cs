using UnityEngine;

public class UIController : MonoBehaviour
{
    [field: Header("References")]
    [field: SerializeField] public Animator MainMenuAnimator { get; private set; } = null;
    [field: SerializeField] public Animator GameUIAnimator { get; private set; } = null;
    [field: SerializeField] public Animator PauseMenuAnimator { get; private set; } = null;

    public void CloseMainMenu()
    {
        MainMenuAnimator.SetTrigger("Close");
    }

    public void OpenGameUI()
    {
        GameUIAnimator.SetTrigger("Open");
    }

    public void CloseGameUI()
    {
        GameUIAnimator.SetTrigger("Close");
    }

    public void OpenPauseMenu()
    {
        PauseMenuAnimator.SetTrigger("Open");
    }

    public void ClosePauseMenu()
    {
        PauseMenuAnimator.SetTrigger("Close");
    }
}
