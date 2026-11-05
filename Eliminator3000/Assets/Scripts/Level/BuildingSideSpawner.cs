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

    [Header("Horizontal Variety")]
    [Tooltip("Max sideways shift per building (units). Every building gets a random shift between -value and +value, so some sit closer to the ship and some further away. Keep it small (3 to 6) so it does not look chaotic. 0 = off (old layout).")]
    public float horizontalVariation = 4f;
    [Tooltip("Safety: a building never gets closer to the centre line than this, so a big variation cannot push it onto the road.")]
    public float minDistanceFromCenter = 8f;

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

        // Separate random stream for the sideways shift, so the prefab / rotation choices
        // above stay exactly the same as before for a given seed.
        var sideRng = new System.Random(randomSeed + 12345);

        // Loop down the track using strict, non-overlapping spacing
        for (float z = startZ; z <= endZ; z += spacing)
        {
            SpawnBuildingSide(z, leftOffset, sideRng);
            SpawnBuildingSide(z, rightOffset, sideRng);
        }

        Debug.Log("Buildings generated cleanly without overlap!");
    }

    private void SpawnBuildingSide(float zPos, float xPos, System.Random sideRng)
    {
        int randomIndex = Random.Range(0, buildingPrefabs.Length);
        GameObject prefab = buildingPrefabs[randomIndex];

        // Always consume the random numbers, even if the prefab slot is empty, to keep the layout stable.
        float shift = ((float)sideRng.NextDouble() * 2f - 1f) * horizontalVariation;

        if (prefab == null) return;

#if UNITY_EDITOR
        GameObject newBuilding = UnityEditor.PrefabUtility.InstantiatePrefab(prefab, transform) as GameObject;
#else
        GameObject newBuilding = Instantiate(prefab, transform);
#endif

        if (newBuilding != null)
        {
            float x = xPos + shift;

            // Keep the building on its own side and not closer than the safety distance.
            if (xPos < 0f) x = Mathf.Min(x, -minDistanceFromCenter);
            else x = Mathf.Max(x, minDistanceFromCenter);

            // Apply exact grid placement with the height offset to fix sinking models
            newBuilding.transform.localPosition = new Vector3(x, heightOffset, zPos);

            // Clean 90-degree rotations so they face the track correctly
            float randomYRot = randomizeRotation ? Random.Range(0, 4) * 90f : 0f;
            newBuilding.transform.localRotation = Quaternion.Euler(0f, randomYRot, 0f);
        }
    }
}