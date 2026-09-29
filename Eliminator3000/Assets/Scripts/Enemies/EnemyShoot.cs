using UnityEngine;

public class EnemyShoot : MonoBehaviour
{
    [SerializeField]
    private GameObject projectilePrefab;
    [SerializeField]
    private float shootInterval = 6f;

    private Timer timer;

    [SerializeField] private bool multiShot = false;
    [SerializeField] private GameObject[] shotOrigins;


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
            if (multiShot)
            {
                ShootMultiProjectile();
            }
            else
            {
                ShootProjectile();
            }
        }
    }


    private void ShootProjectile()
    {
        Instantiate(projectilePrefab, transform.position, Quaternion.Euler(90f, 180f, 0f));
    }

    private void ShootMultiProjectile()
    {
        for (int i = 0; i < shotOrigins.Length; i++)
        {
            Instantiate(projectilePrefab, shotOrigins[i].transform.position, Quaternion.Euler(90f, 180f, 0f));
        }
    }
}
