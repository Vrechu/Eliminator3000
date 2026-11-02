using UnityEngine;

[ExecuteAlways]
public class BuildingSideSpawner : MonoBehaviour
{
    [Header("Building Prefabs")]
    [Tooltip("Drag your building prefabs here.")]
    public GameObject[] buildingPrefabs;

    [Header("Track Layout")]
    public float startZ = -150f;
    public float endZ = 2000f;
    [Tooltip("Base distance between building spawn points along the track.")]
    public float spacing = 35f;

    [Header("Road Offsets & Height")]
    public float leftOffset = -25f;
    public float rightOffset = 25f;
    [Tooltip("Use this to lift buildings up or down if their pivot point is in the floor.")]
    public float heightOffset = 0f;

    [Header("Variation")]
    [Tooltip("Allow random 90-degree rotations for variety.")]
    public bool randomizeRotation = true;
    public int randomSeed = 0;

    public void GenerateBuildings()
    {
        if (Application.isPlaying) return;

        // Clear existing generated buildings
        int childCount = transform.childCount;
        for (int i = childCount - 1; i >= 0; i--)
        {
            DestroyImmediate(transform.GetChild(i).gameObject);
        }

        if (buildingPrefabs == null || buildingPrefabs.Length == 0) return;

        Random.InitState(randomSeed);

        // Loop down the track using strict, non-overlapping spacing
        for (float z = startZ; z <= endZ; z += spacing)
        {
            SpawnBuildingSide(z, leftOffset);
            SpawnBuildingSide(z, rightOffset);
        }

        Debug.Log("Buildings generated cleanly without overlap!");
    }

    private void SpawnBuildingSide(float zPos, float xPos)
    {
        int randomIndex = Random.Range(0, buildingPrefabs.Length);
        GameObject prefab = buildingPrefabs[randomIndex];

        if (prefab == null) return;

#if UNITY_EDITOR
        GameObject newBuilding = UnityEditor.PrefabUtility.InstantiatePrefab(prefab, transform) as GameObject;
#else
        GameObject newBuilding = Instantiate(prefab, transform);
#endif

        if (newBuilding != null)
        {
            // Apply exact grid placement with the height offset to fix sinking models
            newBuilding.transform.localPosition = new Vector3(xPos, heightOffset, zPos);

            // Clean 90-degree rotations so they face the track correctly
            float randomYRot = randomizeRotation ? Random.Range(0, 4) * 90f : 0f;
            newBuilding.transform.localRotation = Quaternion.Euler(0f, randomYRot, 0f);
        }
    }
}