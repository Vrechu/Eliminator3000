using UnityEngine;

public class LevelInfo : MonoBehaviour
{
    [SerializeField] private string levelName;
    [TextArea(15, 20), SerializeField]
    private string Info;
    [SerializeField] private int levelIndex;

    private void Start()
    {
        if (levelIndex < 1) EventBus<MainMenuEnterEvent>.Publish(new MainMenuEnterEvent()); 
        else EventBus<LevelEnteredEvent>.Publish(new LevelEnteredEvent(levelIndex));
    }
}
