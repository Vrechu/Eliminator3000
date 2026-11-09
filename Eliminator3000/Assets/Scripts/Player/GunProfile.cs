using UnityEngine;

/// <summary>
/// Profile for a gun, containing its information, unlock status, projectile prefab, and fire rate.
/// </summary>
[CreateAssetMenu(fileName = "GunProfileScriptableObject", menuName = "ScriptableObjects/GunProfile")]
public class GunProfile : ScriptableObject
{
    [TextArea(15, 20), SerializeField] 
    private string Info;

    public bool UnlockedFromStart;
    public GameObject ProjectilePrefab;
    public bool InfiniteAmmo;
    public int MaxAmmo;
    public int AmmoCostPerShot;
    public float FireRate;
    public bool Cluster;
    public float Angle;
    public int Rows;
    public int Columns;
}
