using System.Collections.Generic;
using UnityEditor.SearchService;
using UnityEngine;
using UnityEngine.SceneManagement;

public enum GameState
{
    MainMenu,
    Playing,
    Paused,
}

[System.Serializable]
public class ActiveCombination
{
    [SerializeField] public List<Plant> Plants = new List<Plant>();
    [SerializeField] public List<Butterfly> Butterflies = new List<Butterfly>();

    public void AddPlant(Plant plant)
    {
        if (!Plants.Contains(plant))
            Plants.Add(plant);
    }
}

public class Game : MonoBehaviour
{
    public static Game Instance { get; private set; }

    [field: Header("Game State")]
    [field: SerializeField] public GameState CurrentState { get; private set; } = GameState.Playing;

    [field: Header("References")]
    [field: SerializeField] public Camera MainCamera { get; private set; } = null;

    [field: Header("Controllers")]
    [field: SerializeField] public InputController InputController { get; private set; } = null;
    [field: SerializeField] public CameraController CameraController { get; private set; } = null;
    [field: SerializeField] public UIController UiController { get; private set; } = null;
    [field: SerializeField] public PlacementController PlacementController { get; private set; } = null;
    [field: SerializeField] public ScreenshotController ScreenshotController { get; private set; } = null;

    private void Awake()
    {
        if (Instance)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }

    void Start()
    {
    }

    public void StartGame()
    {
        CurrentState = GameState.Playing;

        UiController.CloseMainMenu();
        UiController.OpenGameUI();
    }

    public void PauseGame()
    {
        CurrentState = GameState.Paused;

        UiController.CloseGameUI();
        UiController.OpenPauseMenu();
    }

    public void ResumeGame()
    {
        CurrentState = GameState.Playing;

        UiController.ClosePauseMenu();
        UiController.OpenGameUI();
    }

    public void ReturnToMainMenu()
    {
        SceneManager.LoadScene(0);
    }

    public void QuitGame()
    {
        Application.Quit();
    }
}
