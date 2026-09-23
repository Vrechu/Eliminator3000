using UnityEngine;

public class InGameMenuFunctions : MonoBehaviour
{
    [SerializeField] private GameObject inGameMenu;

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

    public void OnMainMenuButtonPressed()
    {
        EventBus<LoadSceneEvent>.Publish(new LoadSceneEvent("MenuScene"));
        EventBus<LevelExitEvent>.Publish(new LevelExitEvent());
    }

    private void EnableMenu()
    {
        inGameMenu.SetActive(true);
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
