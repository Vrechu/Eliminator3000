using UnityEngine;

public class MoveProjectile : MonoBehaviour
{
    [SerializeField]
    private float speed = 30f, lifeSeconds = 3;
    public int parentPlayerId;


    private void Start()
    {
        CountdownTillDelete();  
    }

    // Update is called once per frame
    private void Update()
    {
        if (GameStateManager.Instance.CurrentState 
            != GameStateManager.GameState.Ingame)  return;
        transform.Translate(Vector3.up * speed * Time.deltaTime);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Enemy"))
        {
            Destroy(other.gameObject);
            Destroy(gameObject);
        }
    }

    private void CountdownTillDelete()
    {
        Destroy(gameObject, lifeSeconds);
    }
}
