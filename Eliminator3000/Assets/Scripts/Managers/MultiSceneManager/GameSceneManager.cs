using UnityEngine;
using UnityEngine.SceneManagement;

public class GameSceneManager : MonoBehaviour
{
    public static GameSceneManager Instance { get; private set; }
    private bool spawnerReady = false, GUIReady = false;

    /// <summary>
    /// Creates a singleton instance of the GameSceneManager. 
    /// If an instance already exists, it destroys the duplicate. 
    /// The instance persists across scene loads.
    /// </summary>
    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        else
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
    }

    private void OnEnable()
    {
        EventBus<LoadSceneEvent>.Subscribe(LoadScene);
        EventBus<SpawnerReadyEvent>.Subscribe(OnSpawnerReady);
        EventBus<GUIReadyEvent>.Subscribe(OnGUIReady);
    }

    private void OnDestroy()
    {
        EventBus<LoadSceneEvent>.UnSubscribe(LoadScene);
        EventBus<SpawnerReadyEvent>.UnSubscribe(OnSpawnerReady);
        EventBus<GUIReadyEvent>.UnSubscribe(OnGUIReady);
    }


    public void LoadScene(string _sceneName)
    { 
        GUIReady = false;
        spawnerReady = false;
        SceneManager.LoadScene(_sceneName);
    }

    private void LoadScene(LoadSceneEvent _loadSceneEvent)
    {
        LoadScene(_loadSceneEvent.SceneName);
    }

    private void OnSpawnerReady(SpawnerReadyEvent _spawnerReadyEvent)
    {
        spawnerReady = true;
        CheckLevelReady();
    }

    private void OnGUIReady(GUIReadyEvent _gUIReadyEvent)
    {
        GUIReady = true;
        CheckLevelReady();
    }

    private void CheckLevelReady()
    {
        if (spawnerReady && GUIReady)
        {
            EventBus<LevelReadyEvent>.Publish(new LevelReadyEvent());
        }
    }
}
