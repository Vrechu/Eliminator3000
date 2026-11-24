using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class MainMenuManager : MonoBehaviour
{
    [SerializeField] private MainMenuPage startingPage = MainMenuPage.AttractScreen;
    [SerializeField] private GameObject[] pages;
    [SerializeField] private Button defaultButtonMainPage, defaultButtonLeaderboardPage, defaultButtonLevelSelectPage;
    [SerializeField] private GameObject p1JoinedUI, p2JoinedUI;
    [SerializeField] private GameObject Player2JoinCheck;


    public enum MainMenuPage
    {
        AttractScreen = 0,
        PlayerSelectPage = 1,
        MainPage = 2,
        LeaderboardPage = 3,
        LevelSelectPage = 4,
    }

    private MainMenuPage currentMenuPage;

    private void Awake()
    {
        ResetPlayerJoinedUI();
    }

    private void OnEnable()
    {
        EventBus<PlayerJoinedEvent>.Subscribe(OnPlayerJoined);
    }

    private void OnDestroy()
    {
        EventBus<PlayerJoinedEvent>.UnSubscribe(OnPlayerJoined);
    }

    private void Start()
    {
        OpenPage((int)startingPage);
    }

    private void Update()
    {
        switch (currentMenuPage)
        {
            case MainMenuPage.AttractScreen:
                AttractScreenInputListener();
                break;

            case MainMenuPage.PlayerSelectPage:
                PlayerSelectInputs();
                break;

            case MainMenuPage.MainPage:
                InMenuPlayerJoinInputs();
                LevelSelectCodeInputs();
                break;

            case MainMenuPage.LeaderboardPage:
            case MainMenuPage.LevelSelectPage:
                InMenuPlayerJoinInputs();
                BackInputs();
                break;
        }
    }

    public void StartLevel(string _levelName)
    {
        EventBus<LoadSceneEvent>.Publish(new LoadSceneEvent(_levelName));
    }

    private void OnPlayerJoined(PlayerJoinedEvent _playerJoinedEvent)
    {
        if (_playerJoinedEvent.PlayerIndex == 0)
        {
            p1JoinedUI.SetActive(true);
        }
        else if (_playerJoinedEvent.PlayerIndex == 1)
        {
            p2JoinedUI.SetActive(true);
        }
    }

    private void ResetPlayerJoinedUI()
    {
        p1JoinedUI.SetActive(false);
        p2JoinedUI.SetActive(false);
    }

    public void OpenPage(int _pageIndex)
    {
        ClosePages();
        pages[_pageIndex].SetActive(true);
        currentMenuPage = (MainMenuPage)_pageIndex;

        switch (_pageIndex)
        {
            case (int)MainMenuPage.MainPage:
                defaultButtonMainPage.Select();
                break;
            case (int)MainMenuPage.LeaderboardPage:
                defaultButtonLeaderboardPage.Select();
                break;
            case (int)MainMenuPage.LevelSelectPage:
                defaultButtonLevelSelectPage.Select();
                break;
        }
    }

    private void ClosePages()
    {
        for (int i = 0; i < pages.Length; i++)
        {
            pages[i].SetActive(false);
        }
    }

    private void AttractScreenInputListener()
    {
        if (Keyboard.current == null) return;
        if (Keyboard.current.anyKey.wasPressedThisFrame)
        {
            OpenPage((int)MainMenuPage.PlayerSelectPage);
        }
    }

    private void SendJoinSignal(int _playerIdex)
    {
        EventBus<PlayerMenuJoinInputEvent>.Publish(new PlayerMenuJoinInputEvent(_playerIdex));
    }

    private void PlayerSelectInputs()
    {
        if (Keyboard.current == null) return;
        else if (Keyboard.current.digit1Key.wasPressedThisFrame)
        {
            SendJoinSignal(0);
            OpenPage((int)MainMenuPage.MainPage);
        }
        else if (Keyboard.current.digit2Key.wasPressedThisFrame)
        {
            SendJoinSignal(0);
            SendJoinSignal(1);
            OpenPage((int)MainMenuPage.MainPage);
        }
    }

    private void InMenuPlayerJoinInputs()
    {

        if (Keyboard.current == null) return;
        else if (Keyboard.current.digit2Key.wasPressedThisFrame)
        {
            SendJoinSignal(1);
        }
    }

    private void LevelSelectCodeInputs()
    {
        if (Keyboard.current == null) return;
        if (Keyboard.current.gKey.isPressed && Keyboard.current.hKey.wasPressedThisFrame)
        {
            OpenPage((int)MainMenuPage.LevelSelectPage);
        }
    }

    private void BackInputs()
    {
        if (Keyboard.current == null) return;
        if (Keyboard.current.escapeKey.wasPressedThisFrame
            || Keyboard.current.gKey.wasPressedThisFrame)
        {
            OpenPage((int)MainMenuPage.MainPage);
        }
    }
}