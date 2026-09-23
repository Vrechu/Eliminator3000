using UnityEngine;

public class LevelInfo : MonoBehaviour
{
    [SerializeField] private string levelName;
    [SerializeField] private int levelNumber;

    private void Start()
    {
        EventBus<LevelEnteredEvent>.Publish(new LevelEnteredEvent(levelNumber));
    }
}
