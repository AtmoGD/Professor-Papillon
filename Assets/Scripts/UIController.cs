using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class UIController : MonoBehaviour
{
    [field: Header("References")]
    [field: SerializeField] public Animator MainMenuAnimator { get; private set; } = null;
    [field: SerializeField] public Animator GameUIAnimator { get; private set; } = null;
    [field: SerializeField] public Animator PauseMenuAnimator { get; private set; } = null;

    [field: Header("Jars")]
    [field: SerializeField] public TMP_Text JarAText { get; private set; } = null;
    [field: SerializeField] public ButterflyData JarAData { get; private set; } = null;
    [field: SerializeField] public TMP_Text JarBText { get; private set; } = null;
    [field: SerializeField] public ButterflyData JarBData { get; private set; } = null;
    [field: SerializeField] public TMP_Text JarCText { get; private set; } = null;
    [field: SerializeField] public ButterflyData JarCData { get; private set; } = null;

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

    public void UpdateJars()
    {
        List<Butterfly> butterflies = Game.Instance.PlacementController.ActiveButterflies;

        int jarA = butterflies.FindAll(b => b.Data == JarAData).Count;
        int jarB = butterflies.FindAll(b => b.Data == JarBData).Count;
        int jarC = butterflies.FindAll(b => b.Data == JarCData).Count;

        UpdateJarA(jarA);
        UpdateJarB(jarB);
        UpdateJarC(jarC);
    }

    public void UpdateJarA(int amount)
    {
        JarAText.text = amount.ToString();
    }
    public void UpdateJarB(int amount)
    {
        JarBText.text = amount.ToString();
    }
    public void UpdateJarC(int amount)
    {
        JarCText.text = amount.ToString();
    }
}
