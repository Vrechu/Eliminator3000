using UnityEngine;

/// <summary>
/// Profile for a gun, containing its information, unlock status, projectile prefab, and fire rate.
/// </summary>
[CreateAssetMenu(fileName = "GunProfileScriptableObject", menuName = "ScriptableObjects/GunProfile")]
public class GunProfile : ScriptableObject
{
    [TextArea(15, 20), SerializeField] 
    private string Info;

    public bool Unlocked;
    public GameObject ProjectilePrefab;
    public float fireRate;
}
