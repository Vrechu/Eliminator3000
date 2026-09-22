using UnityEngine;

public class EnemyShoot : MonoBehaviour
{
    [SerializeField]
    private GameObject projectilePrefab;
    [SerializeField]
    private float shootInterval = 6f;

    private Timer timer;


    private void Start()
    {
        timer = new Timer(shootInterval, true);
    }


    private void Update()
    {
        if (GameStateManager.Instance.CurrentState != GameStateManager.GameState.Ingame)
            return;
        if (timer.IsFinished())
        {
            ShootProjectile();
        }
    }


    private void ShootProjectile()
    {
        Instantiate(projectilePrefab, transform.position, Quaternion.Euler(90f, 180f, 0f));
    }
}
