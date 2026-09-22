using UnityEngine;

public class MainMenuManager : MonoBehaviour
{
    public void OnStartButtonPressed()
    {
        EventBus<LoadSceneEvent>.Publish(new LoadSceneEvent("SampleScene"));
    }
}
