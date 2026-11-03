using System.Globalization;
using System.Runtime.CompilerServices;
using UnityEngine;
using TMPro;

public class MainMenuManager : MonoBehaviour
{
    [SerializeField] private string firstSceneName = "SampleScene";
    [SerializeField] private GameObject[] pages;
    [SerializeField] private GameObject p1JoinedUI, p2JoinedUI;

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
        OpenPage(0);
    }

    public void OnStartButtonPressed()
    {
        EventBus<LoadSceneEvent>.Publish(new LoadSceneEvent(firstSceneName));
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
    }

    private void ClosePages()
    {
        for (int i = 0; i < pages.Length; i++)
        {
            pages[i].SetActive(false);
        }
    }
}
