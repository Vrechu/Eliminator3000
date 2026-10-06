using System.Collections.Generic;
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
    public float FireRate;
    public bool Cluster;
    public float Angle;
    public int Rows;
    public int Columns;
}
