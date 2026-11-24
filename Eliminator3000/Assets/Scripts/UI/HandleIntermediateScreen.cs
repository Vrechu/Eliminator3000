using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;

public class HandleIntermediateScreen : MonoBehaviour
{
    [SerializeField] private string nextSceneName;
    [SerializeField] private GameObject ContinueVisual;
    [SerializeField] private float continueTime = 4f;
    [SerializeField] TextMeshProUGUI continueTimerVisual;
    private Timer continueTimer;

    private void Start()
    {
        continueTimer = new Timer(continueTime);
        ContinueVisual.SetActive(false);
    }

    private void Update()
    {
        if (Keyboard.current.gKey.isPressed && Keyboard.current.hKey.isPressed)
            EventBus<LoadSceneEvent>.Publish(new LoadSceneEvent(nextSceneName));

        if ((continueTimer.IsFinished()))
        {
            ContinueVisual.SetActive(true);
            continueTimerVisual.enabled = false;
            if (Keyboard.current.fKey.wasPressedThisFrame)
            {
                EventBus<LoadSceneEvent>.Publish(new LoadSceneEvent(nextSceneName));
            }
        }
        continueTimerVisual.text = continueTimer.TimeLeft.ToString("F1");
    }
}
