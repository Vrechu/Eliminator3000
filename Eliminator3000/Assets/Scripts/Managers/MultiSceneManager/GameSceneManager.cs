using System.Runtime.CompilerServices;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameSceneManager : MonoBehaviour
{
    public static GameSceneManager Instance { get; private set; }

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Debug.LogWarning("Multiple instances of GameSceneManager detected. Destroying duplicate.");
            Destroy(gameObject);
        }
    }

    private void OnEnable()
    {
        EventBus<LoadSceneEvent>.Subscribe(LoadScene);
    }

    private void OnDestroy()
    {
        EventBus<LoadSceneEvent>.UnSubscribe(LoadScene);
    }


    public void LoadScene(string _sceneName)
    { 
        SceneManager.LoadScene(_sceneName);
    }

    private void LoadScene(LoadSceneEvent _loadSceneEvent)
    {
        LoadScene(_loadSceneEvent.SceneName);
    }
}
