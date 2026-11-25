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

    [System.Serializable]
    public class ColorPreset
    {
        public string name = "Pink";
        [ColorUsage(false, true)] public Color color = new Color(4f, 0.3f, 2.5f, 1f);
        [Tooltip("Higher = picked more often.")]
        [Min(0f)] public float weight = 1f;
        [Tooltip("ON = every building can use this colour (the global pool). OFF = unique colour, only used by buildings that list it in their rule.")]
        public bool inGlobalPool = true;
    }

    [System.Serializable]
    public class MaterialTarget
    {
        [Tooltip("Materials whose name contains this text get recoloured. Empty = every material.")]
        public string nameContains = "Emission";
        [Tooltip("Colour property to set. _EmissionColor for neon / window lights, _BaseColor to tint the surface.")]
        public string colorProperty = "_EmissionColor";
        [Tooltip("Multiplies the preset colour. Use below 1 for window lights if they are too bright.")]
        [Min(0f)] public float intensity = 1f;
        [Tooltip("ON = uses the same colour the building got. OFF = picks its own colour from Window Presets.")]
        public bool sameAsBuilding = false;
    }

    [System.Serializable]
    public class BuildingColorRule
    {
        [Tooltip("The building prefab this rule is for.")]
        public GameObject prefab;
        [Tooltip("ON = this building may use the global pool. OFF = only the colours listed below.")]
        public bool useGlobalColors = true;
        [Tooltip("Preset NAMES (exactly as written in Colour Presets). Always allowed for this building, even if the preset is not in the global pool.")]
        public List<string> extraPresets = new List<string>();
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

    [Header("Colour Presets (random per building)")]
    public bool useColorPresets = true;

    [Tooltip("Which materials get recoloured, and how. Add one entry for the neon parts and one for the window lights. First matching entry wins.")]
    public List<MaterialTarget> materialTargets = new List<MaterialTarget>
    {
        new MaterialTarget { nameContains = "Emission", colorProperty = "_EmissionColor", intensity = 1f },
        new MaterialTarget { nameContains = "Windows",  colorProperty = "_EmissionColor", intensity = 0.6f },
    };

    public List<ColorPreset> colorPresets = new List<ColorPreset>
    {
        new ColorPreset { name = "Pink",   color = new Color(4f, 0.3f, 2.5f, 1f) },
        new ColorPreset { name = "Blue",   color = new Color(0.3f, 1.5f, 5f, 1f) },
        new ColorPreset { name = "Orange", color = new Color(5f, 1.5f, 0.2f, 1f) },
    };

    [Header("Window Presets (used by targets with 'Same As Building' OFF)")]
    public List<ColorPreset> windowPresets = new List<ColorPreset>
    {
        new ColorPreset { name = "Warm",  color = new Color(5f, 2.5f, 0.8f, 1f) },
        new ColorPreset { name = "Cool",  color = new Color(0.8f, 2f, 5f, 1f) },
        new ColorPreset { name = "White", color = new Color(3f, 3f, 3f, 1f) },
    };

    [Header("Per-Building Colour Rules (optional)")]
    [Tooltip("Buildings NOT listed here simply use the global pool.")]
    public List<BuildingColorRule> buildingRules = new List<BuildingColorRule>();

    [Header("Open Zones (everything else is full city)")]
    public List<OpenZone> openZones = new List<OpenZone>();

    public void GenerateBuildings()
    {
        if (Application.isPlaying) return;

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
        var colorRng = new System.Random(randomSeed + 777);
        windowRng = new System.Random(randomSeed + 555);

        for (float z = startZ; z <= endZ; z += spacing)
        {
            OpenZone zone = null;
            foreach (var oz in openZones)
            {
                if (z >= oz.fromZ && z <= oz.toZ) { zone = oz; break; }
            }

            double rl = zoneRng.NextDouble();
            double rr = zoneRng.NextDouble();
            float extraL = (float)zoneRng.NextDouble();
            float extraR = (float)zoneRng.NextDouble();

            bool leftOn = zone == null || (!zone.emptyLeft && rl < zone.buildingChance);
            bool rightOn = zone == null || (!zone.emptyRight && rr < zone.buildingChance);
            float offL = zone != null ? extraL * zone.extraOffset : 0f;
            float offR = zone != null ? extraR * zone.extraOffset : 0f;

            SpawnBuildingSide(z, leftOffset, sideRng, colorRng, leftOn, offL);
            SpawnBuildingSide(z, rightOffset, sideRng, colorRng, rightOn, offR);
        }

        Debug.Log("Buildings generated cleanly without overlap!");
    }

    private System.Random windowRng;

    private int PickWindowPreset()
    {
        double r = windowRng.NextDouble();   // always consumed
        if (windowPresets == null || windowPresets.Count == 0) return -1;
        float total = 0f;
        foreach (var p in windowPresets) total += p.weight;
        if (total <= 0f) return -1;
        float t = (float)r * total;
        for (int i = 0; i < windowPresets.Count; i++)
        {
            t -= windowPresets[i].weight;
            if (t <= 0f) return i;
        }
        return windowPresets.Count - 1;
    }

    private BuildingColorRule FindRule(GameObject prefab)
    {
        if (buildingRules == null) return null;
        foreach (var r in buildingRules)
            if (r != null && r.prefab == prefab) return r;
        return null;
    }

    // Builds the list of preset indices this prefab is allowed to use.
    private List<int> AllowedPresets(GameObject prefab)
    {
        var result = new List<int>();
        if (colorPresets == null) return result;

        var rule = FindRule(prefab);
        bool useGlobal = rule == null || rule.useGlobalColors;

        for (int i = 0; i < colorPresets.Count; i++)
        {
            var p = colorPresets[i];
            bool allowed = useGlobal && p.inGlobalPool;
            if (!allowed && rule != null && rule.extraPresets != null)
            {
                foreach (var n in rule.extraPresets)
                    if (!string.IsNullOrEmpty(n) && n.Trim() == p.name) { allowed = true; break; }
            }
            if (allowed) result.Add(i);
        }
        return result;
    }

    private int PickPreset(System.Random rng, GameObject prefab)
    {
        double r = rng.NextDouble();   // always consumed, keeps the layout stable
        if (colorPresets == null || colorPresets.Count == 0) return -1;

        var allowed = AllowedPresets(prefab);
        if (allowed.Count == 0) return -1;

        float total = 0f;
        foreach (int i in allowed) total += colorPresets[i].weight;
        if (total <= 0f) return -1;

        float t = (float)r * total;
        foreach (int i in allowed)
        {
            t -= colorPresets[i].weight;
            if (t <= 0f) return i;
        }
        return allowed[allowed.Count - 1];
    }

    private void SpawnBuildingSide(float zPos, float xPos, System.Random sideRng, System.Random colorRng, bool allowSpawn, float extraOut)
    {
        int randomIndex = Random.Range(0, buildingPrefabs.Length);
        GameObject prefab = buildingPrefabs[randomIndex];

        float shift = ((float)sideRng.NextDouble() * 2f - 1f) * horizontalVariation;
        int preset = PickPreset(colorRng, prefab);
        int winPreset = PickWindowPreset();

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

            if (useColorPresets) ApplyColor(newBuilding, preset, winPreset);
        }
    }

    private MaterialTarget FindTarget(Material m)
    {
        if (materialTargets == null) return null;
        foreach (var t in materialTargets)
        {
            if (t == null) continue;
            if (string.IsNullOrEmpty(t.nameContains) || m.name.Contains(t.nameContains)) return t;
        }
        return null;
    }

    private void ApplyColor(GameObject building, int presetIndex, int windowIndex)
    {
#if UNITY_EDITOR
        foreach (var r in building.GetComponentsInChildren<Renderer>(true))
        {
            var mats = r.sharedMaterials;
            bool changed = false;
            for (int i = 0; i < mats.Length; i++)
            {
                if (mats[i] == null) continue;
                var target = FindTarget(mats[i]);
                if (target == null) continue;
                bool useWin = !target.sameAsBuilding;
                int idx = useWin ? windowIndex : presetIndex;
                var list = useWin ? windowPresets : colorPresets;
                if (idx < 0 || list == null || idx >= list.Count) continue;
                Material tinted = GetTinted(mats[i], idx, list[idx], target, useWin);
                if (tinted != null) { mats[i] = tinted; changed = true; }
            }
            if (changed) r.sharedMaterials = mats;
        }
#endif
    }

#if UNITY_EDITOR
    private const string MatFolder = "Assets/GeneratedBuildingMaterials";

    private Material GetTinted(Material src, int index, ColorPreset preset, MaterialTarget target, bool isWindow)
    {
        string srcPath = UnityEditor.AssetDatabase.GetAssetPath(src);
        // already one of our generated materials: nothing to do
        if (srcPath.StartsWith(MatFolder)) return src;

        if (!UnityEditor.AssetDatabase.IsValidFolder(MatFolder))
            UnityEditor.AssetDatabase.CreateFolder("Assets", "GeneratedBuildingMaterials");

        // one sub-folder per scene so scenes never overwrite each other's materials
        string sceneName = string.IsNullOrEmpty(gameObject.scene.name) ? "Unsaved" : gameObject.scene.name;
        foreach (char ch in System.IO.Path.GetInvalidFileNameChars()) sceneName = sceneName.Replace(ch, '_');
        string sceneFolder = MatFolder + "/" + sceneName;
        if (!UnityEditor.AssetDatabase.IsValidFolder(sceneFolder))
            UnityEditor.AssetDatabase.CreateFolder(MatFolder, sceneName);

        string safe = src.name + (isWindow ? "_W" : "_B") + index + "_" + preset.name;
        foreach (char c in System.IO.Path.GetInvalidFileNameChars()) safe = safe.Replace(c, '_');
        string path = sceneFolder + "/" + safe + ".mat";

        Material m = UnityEditor.AssetDatabase.LoadAssetAtPath<Material>(path);
        if (m == null)
        {
            m = new Material(src);
            UnityEditor.AssetDatabase.CreateAsset(m, path);
        }

        if (m.HasProperty(target.colorProperty))
        {
            // only touch the asset when something really changed (keeps git diffs small)
            bool dirty = false;
            if (target.colorProperty == "_EmissionColor")
            {
                if (!m.IsKeywordEnabled("_EMISSION")) { m.EnableKeyword("_EMISSION"); dirty = true; }
                if (m.globalIlluminationFlags != MaterialGlobalIlluminationFlags.BakedEmissive)
                {
                    m.globalIlluminationFlags = MaterialGlobalIlluminationFlags.BakedEmissive;
                    dirty = true;
                }
            }
            Color c = preset.color * target.intensity;
            c.a = 1f;
            if (m.GetColor(target.colorProperty) != c) { m.SetColor(target.colorProperty, c); dirty = true; }
            if (dirty) UnityEditor.EditorUtility.SetDirty(m);
        }
        return m;
    }
#endif
}