using UnityEngine;

public class MoveProjectile : MonoBehaviour
{
    [SerializeField]
    private float speed = 30f, lifeSeconds = 3;
    private Timer deleteTimer;



    private void Start()
    {          
        deleteTimer = new Timer(lifeSeconds);
    }

    private void Update()
    {
        if (GameStateManager.Instance.CurrentState 
            != GameStateManager.GameState.Ingame)  return;
        transform.Translate(Vector3.up * speed * Time.deltaTime);
        CountdownTillDelete();
    }

    private void CountdownTillDelete()
    {
        if (deleteTimer.IsFinished()) Destroy(gameObject);
    }
}
