using UnityEngine;

[CreateAssetMenu(fileName = "GunProfileScriptableObject", menuName = "ScriptableObjects/GunProfile")]
public class GunProfile : ScriptableObject
{
    [TextArea(15, 20), SerializeField] 
    private string Info;

    public bool Unlocked;
    public GameObject ProjectilePrefab;
    public float fireRate;
}
