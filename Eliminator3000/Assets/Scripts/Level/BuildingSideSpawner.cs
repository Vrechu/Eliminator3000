using UnityEngine;
using System.Collections.Generic;

[ExecuteAlways]
public class BuildingSideSpawner : MonoBehaviour
{
    [System.Serializable]
    public class OpenZone
    {
        public string label = "Open field";
        public float fromZ = 500f;
        public float toZ = 900f;
        [Tooltip("Left side stays empty.")] public bool emptyLeft = false;
        [Tooltip("Right side stays empty.")] public bool emptyRight = false;
        [Tooltip("0 = completely empty, 0.1 = an occasional building.")]
        [Range(0f, 1f)] public float buildingChance = 0.08f;
        [Tooltip("Buildings in this zone sit this much further from the road.")]
        public float extraOffset = 25f;
    }

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
    [Tooltip("Max sideways shift per building (units). Keep it small (3 to 6). 0 = off.")]
    public float horizontalVariation = 4f;
    [Tooltip("Safety: a building never gets closer to the centre line than this.")]
    public float minDistanceFromCenter = 8f;

    [Header("Open Zones (everything else is full city)")]
    public List<OpenZone> openZones = new List<OpenZone>();

    public void GenerateBuildings()
    {
        if (Application.isPlaying) return;

        // Clear existing generated buildings
        for (int i = transform.childCount - 1; i >= 0; i--)
            DestroyImmediate(transform.GetChild(i).gameObject);

        if (buildingPrefabs == null || buildingPrefabs.Length == 0) return;

        if (spacing < 1f)
        {
            Debug.LogWarning("Building spawner: Spacing must be at least 1.");
            return;
        }

        Random.InitState(randomSeed);
        var sideRng = new System.Random(randomSeed + 12345);
        var zoneRng = new System.Random(randomSeed + 999);

        for (float z = startZ; z <= endZ; z += spacing)
        {
            OpenZone zone = null;
            foreach (var oz in openZones)
            {
                if (z >= oz.fromZ && z <= oz.toZ) { zone = oz; break; }
            }

            // always consume the random numbers so the layout stays stable
            double rl = zoneRng.NextDouble();
            double rr = zoneRng.NextDouble();
            float extraL = (float)zoneRng.NextDouble();
            float extraR = (float)zoneRng.NextDouble();

            bool leftOn = zone == null || (!zone.emptyLeft && rl < zone.buildingChance);
            bool rightOn = zone == null || (!zone.emptyRight && rr < zone.buildingChance);
            float offL = zone != null ? extraL * zone.extraOffset : 0f;
            float offR = zone != null ? extraR * zone.extraOffset : 0f;

            SpawnBuildingSide(z, leftOffset, sideRng, leftOn, offL);
            SpawnBuildingSide(z, rightOffset, sideRng, rightOn, offR);
        }

        Debug.Log("Buildings generated cleanly without overlap!");
    }

    private void SpawnBuildingSide(float zPos, float xPos, System.Random sideRng, bool allowSpawn, float extraOut)
    {
        int randomIndex = Random.Range(0, buildingPrefabs.Length);
        GameObject prefab = buildingPrefabs[randomIndex];

        float shift = ((float)sideRng.NextDouble() * 2f - 1f) * horizontalVariation;

        if (prefab == null || !allowSpawn) return;

#if UNITY_EDITOR
        GameObject newBuilding = UnityEditor.PrefabUtility.InstantiatePrefab(prefab, transform) as GameObject;
#else
        GameObject newBuilding = Instantiate(prefab, transform);
#endif

        if (newBuilding != null)
        {
            float x = xPos + shift + (xPos < 0f ? -extraOut : extraOut);

            if (xPos < 0f) x = Mathf.Min(x, -minDistanceFromCenter);
            else x = Mathf.Max(x, minDistanceFromCenter);

            newBuilding.transform.localPosition = new Vector3(x, heightOffset, zPos);

            float randomYRot = randomizeRotation ? Random.Range(0, 4) * 90f : 0f;
            newBuilding.transform.localRotation = Quaternion.Euler(0f, randomYRot, 0f);
        }
    }
}