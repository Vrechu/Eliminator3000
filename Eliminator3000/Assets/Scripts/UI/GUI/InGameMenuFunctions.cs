using UnityEngine;
using UnityEngine.UI;

public class InGameMenuFunctions : MonoBehaviour
{
    [SerializeField] private GameObject inGameMenu;
    [SerializeField] private Button continueButton;

    private void OnEnable()
    {
        EventBus<GameWinEvent>.Subscribe(OnGameWinEvent);
        EventBus<GameLoseEvent>.Subscribe(OnGameLoseEven);
    }

    private void OnDestroy()
    {
        EventBus<GameWinEvent>.UnSubscribe(OnGameWinEvent);
        EventBus<GameLoseEvent>.UnSubscribe(OnGameLoseEven);
    }

    private void Start()
    {
        inGameMenu.SetActive(false);
    }

    public void OnSceneLoadButtonPressed(string sceneName = "MenuScene")
    {
        EventBus<LoadSceneEvent>.Publish(new LoadSceneEvent(sceneName));
    }

    private void EnableMenu()
    {
        inGameMenu.SetActive(true);
        continueButton.Select();
    }

    private void OnGameWinEvent(GameWinEvent _winEvent)
    {
        EnableMenu();
    }

    private void OnGameLoseEven(GameLoseEvent _loseEvent)
    {
        EnableMenu();
    }
}
