// Castle maze map - one-click Unity setup (URP).
// Menu: Tools > Castle Map > Set Up Map In Open Scene
// Creates URP materials (with emission for flames, windows, sigil, beam), remaps them onto the FBX,
// places the model, rebuilds every light from the LIGHT_ markers, adds colliders, night ambience, fog,
// and a global post-processing volume with Bloom so the glowing parts actually glow.
#if UNITY_EDITOR
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public static class CastleMapSetup
{
    const string ROOT = "Assets/CastleMap";
    const string FBX = ROOT + "/CastleMap_Unity.fbx";
    const string MAT_DIR = ROOT + "/Materials";

    struct M
    {
        public string name; public Color baseCol; public float smooth, metal; public Color emit; public float emitI; public bool beam;
        public M(string n, Color b, float s, float mt, Color e, float ei, bool bm) { name = n; baseCol = b; smooth = s; metal = mt; emit = e; emitI = ei; beam = bm; }
    }

    // values exported from the Blender file
    static readonly M[] MATS = {
        new M("Bark", new Color(0.248f,0.206f,0.172f,1f), 0.05f, 0.00f, new Color(0.000f,0.000f,0.000f), 0.00f, false),
        new M("Brazier_Fire", new Color(0.349f,0.152f,0.485f,1f), 0.60f, 0.00f, new Color(0.854f,0.584f,1.000f), 4.00f, false),
        new M("Bush", new Color(0.190f,0.272f,0.172f,1f), 0.05f, 0.00f, new Color(0.000f,0.000f,0.000f), 0.00f, false),
        new M("Grass", new Color(0.152f,0.206f,0.128f,1f), 0.00f, 0.00f, new Color(0.000f,0.000f,0.000f), 0.00f, false),
        new M("Grass_Tufts", new Color(0.221f,0.293f,0.172f,1f), 0.00f, 0.00f, new Color(0.000f,0.000f,0.000f), 0.00f, false),
        new M("Gravel_Path", new Color(0.381f,0.366f,0.341f,1f), 0.05f, 0.00f, new Color(0.000f,0.000f,0.000f), 0.00f, false),
        new M("Hedge", new Color(0.172f,0.283f,0.172f,1f), 0.10f, 0.00f, new Color(0.000f,0.000f,0.000f), 0.00f, false),
        new M("Iron", new Color(0.152f,0.152f,0.160f,1f), 0.50f, 0.60f, new Color(0.000f,0.000f,0.000f), 0.00f, false),
        new M("Lamp_Flame", new Color(0.485f,0.313f,0.000f,1f), 0.50f, 0.00f, new Color(1.000f,0.798f,0.485f), 4.00f, false),
        new M("Moon_Disc", new Color(0.906f,0.931f,1.000f,1f), 0.00f, 0.00f, new Color(0.906f,0.931f,1.000f), 1.50f, false),
        new M("Rock", new Color(0.381f,0.373f,0.366f,1f), 0.10f, 0.00f, new Color(0.000f,0.000f,0.000f), 0.00f, false),
        new M("Roof_Slate", new Color(0.190f,0.190f,0.235f,1f), 0.30f, 0.00f, new Color(0.000f,0.000f,0.000f), 0.00f, false),
        new M("Spawn_Beam", new Color(0.62f,0.45f,1f,0.35f), 0f, 0f, new Color(0.62f,0.45f,1f), 2.5f, true),
        new M("Spawn_Sigil", new Color(0.349f,0.152f,0.485f,1f), 0.60f, 0.00f, new Color(0.809f,0.506f,1.000f), 3.00f, false),
        new M("Stone", new Color(0.437f,0.424f,0.403f,1f), 0.15f, 0.00f, new Color(0.000f,0.000f,0.000f), 0.00f, false),
        new M("Stone_Dark", new Color(0.313f,0.304f,0.293f,1f), 0.10f, 0.00f, new Color(0.000f,0.000f,0.000f), 0.00f, false),
        new M("Tree_Leaves", new Color(0.152f,0.221f,0.172f,1f), 0.05f, 0.00f, new Color(0.000f,0.000f,0.000f), 0.00f, false),
        new M("Window_Glow", new Color(0.485f,0.313f,0.000f,1f), 0.50f, 0.00f, new Color(1.000f,0.767f,0.424f), 2.00f, false),
        new M("Wood", new Color(0.332f,0.260f,0.190f,1f), 0.15f, 0.00f, new Color(0.000f,0.000f,0.000f), 0.00f, false)
    };

    struct L
    {
        public UnityEngine.LightType type; public Color col; public float intensity, range; public LightShadows shadows;
        public L(UnityEngine.LightType t, Color c, float i, float r, LightShadows s) { type = t; col = c; intensity = i; range = r; shadows = s; }
    }

    // marker prefix -> light settings (tweak these freely)
    static readonly Dictionary<string, L> LIGHTS = new Dictionary<string, L> {
        { "LIGHT_Lamp_Light",       new L(UnityEngine.LightType.Point, new Color(1.00f, 0.72f, 0.45f), 3.0f, 9f,  LightShadows.None) },
        { "LIGHT_Brazier_Light",    new L(UnityEngine.LightType.Point, new Color(0.78f, 0.55f, 1.00f), 3.0f, 7f,  LightShadows.None) },
        { "LIGHT_Spawn_Light_High", new L(UnityEngine.LightType.Point, new Color(0.70f, 0.50f, 1.00f), 4.0f, 25f, LightShadows.None) },
        { "LIGHT_Spawn_Light",      new L(UnityEngine.LightType.Point, new Color(0.75f, 0.55f, 1.00f), 6.0f, 18f, LightShadows.Soft) },
    };

    // objects that get a MeshCollider (walkable / blocking)
    static readonly string[] COLLIDE = {
        "Ground", "Maze_Floor", "Gravel_Paths", "F0_Hedges", "Floor1_Hedges", "Floor2_Hedges", "Floor1_Walkways", "Floor2_Walkways", "Pillars",
        "Stair_", "Spawn_Tower", "Spawn_Dais", "Spawn_Balustrade", "Spawn_Gazebo", "Spawn_Braziers", "Spawn_Gargoyles",
        "Hedges_Garden_Border", "Castle", "Castle_Gate", "Well", "Statues_Ruins", "Graveyard_Stones", "Iron_Fences",
        "Benches", "Urns", "Walkway_Urns", "Crates_Barrels", "Lamp_Posts", "Tree_Trunks", "Rocks"
    };
    static readonly string[] NO_COLLIDE = { "Castle_Roofs", "Castle_Windows", "Urn_Plants", "Well_Water" };
    // objects that should not cast shadows (glowing / tiny / far)
    static readonly string[] NO_SHADOW = { "Grass_Tufts", "Lamp_Flames", "Spawn_Beam", "Spawn_Sigil", "Spawn_Brazier_Fire", "Moon_Disc", "Castle_Windows", "Well_Water" };

    [MenuItem("Tools/Castle Map/Set Up Map In Open Scene")]
    public static void SetUp()
    {
        if (AssetDatabase.LoadAssetAtPath<GameObject>(FBX) == null)
        {
            EditorUtility.DisplayDialog("Castle Map", "Could not find " + FBX + "\nCopy the CastleMap folder into Assets first.", "OK");
            return;
        }
        bool urp = GraphicsSettings.currentRenderPipeline is UniversalRenderPipelineAsset;
        if (!urp)
            Debug.LogWarning("[CastleMap] No URP asset is active (Project Settings > Graphics). Materials will use the Standard shader and no Bloom volume is added.");

        var mats = BuildMaterials(urp);
        RemapFbx(mats);
        var map = PlaceModel();
        BuildLights(map);
        AddCollidersAndFlags(map);
        Atmosphere(urp);
        EditorSceneManager.MarkSceneDirty(map.scene);
        Selection.activeGameObject = map;
        Debug.Log("[CastleMap] Done. Press Play or look through the Scene view (enable Post Processing in the Scene view's effects menu to see Bloom).");
    }

    // ------------------------------------------------------------------ materials
    static Dictionary<string, Material> BuildMaterials(bool urp)
    {
        if (!AssetDatabase.IsValidFolder(MAT_DIR)) AssetDatabase.CreateFolder(ROOT, "Materials");
        var lit = Shader.Find(urp ? "Universal Render Pipeline/Lit" : "Standard");
        var unlit = Shader.Find(urp ? "Universal Render Pipeline/Unlit" : "Unlit/Transparent");
        var result = new Dictionary<string, Material>();
        foreach (var d in MATS)
        {
            string path = MAT_DIR + "/" + d.name + ".mat";
            var m = AssetDatabase.LoadAssetAtPath<Material>(path);
            var sh = d.beam ? unlit : lit;
            if (m == null) { m = new Material(sh); AssetDatabase.CreateAsset(m, path); }
            m.shader = sh;
            if (urp)
            {
                m.SetColor("_BaseColor", d.baseCol);
                if (!d.beam) { m.SetFloat("_Smoothness", d.smooth); m.SetFloat("_Metallic", d.metal); }
            }
            else { m.SetColor("_Color", d.baseCol); m.SetFloat("_Glossiness", d.smooth); m.SetFloat("_Metallic", d.metal); }

            if (d.emitI > 0 && !d.beam)
            {
                m.EnableKeyword("_EMISSION");
                m.SetColor("_EmissionColor", d.emit * d.emitI);           // HDR colour > 1 -> picked up by Bloom
                m.globalIlluminationFlags = MaterialGlobalIlluminationFlags.BakedEmissive;
            }
            else { m.DisableKeyword("_EMISSION"); m.SetColor("_EmissionColor", Color.black); }

            if (d.beam && urp)                                              // soft additive light column
            {
                m.SetColor("_BaseColor", new Color(d.emit.r * d.emitI, d.emit.g * d.emitI, d.emit.b * d.emitI, d.baseCol.a));
                m.SetFloat("_Surface", 1); m.SetFloat("_Blend", 2);
                m.SetFloat("_SrcBlend", (float)UnityEngine.Rendering.BlendMode.SrcAlpha); m.SetFloat("_DstBlend", (float)UnityEngine.Rendering.BlendMode.One);
                m.SetFloat("_SrcBlendAlpha", (float)UnityEngine.Rendering.BlendMode.One); m.SetFloat("_DstBlendAlpha", (float)UnityEngine.Rendering.BlendMode.One);
                m.SetFloat("_ZWrite", 0); m.SetFloat("_Cull", 0);
                m.SetOverrideTag("RenderType", "Transparent");
                m.EnableKeyword("_SURFACE_TYPE_TRANSPARENT");
                m.renderQueue = (int)RenderQueue.Transparent;
            }
            EditorUtility.SetDirty(m);
            result[d.name] = m;
        }
        AssetDatabase.SaveAssets();
        return result;
    }

    static void RemapFbx(Dictionary<string, Material> mats)
    {
        var imp = (ModelImporter)AssetImporter.GetAtPath(FBX);
        imp.materialImportMode = ModelImporterMaterialImportMode.ImportViaMaterialDescription;
        imp.materialLocation = ModelImporterMaterialLocation.InPrefab;
        foreach (var kv in mats)
            imp.AddRemap(new AssetImporter.SourceAssetIdentifier(typeof(Material), kv.Key), kv.Value);
        imp.globalScale = 1f;
        imp.useFileScale = true;
        imp.addCollider = false;            // colliders are added selectively below
        imp.isReadable = false;
        imp.SaveAndReimport();
    }

    // ------------------------------------------------------------------ scene
    static GameObject PlaceModel()
    {
        var old = GameObject.Find("CastleMap");
        if (old != null) Undo.DestroyObjectImmediate(old);
        var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(FBX);
        var map = (GameObject)PrefabUtility.InstantiatePrefab(prefab);
        map.name = "CastleMap";
        map.transform.position = Vector3.zero;
        Undo.RegisterCreatedObjectUndo(map, "Castle Map");
        return map;
    }

    static void BuildLights(GameObject map)
    {
        int n = 0;
        Transform moonPos = null, moonTarget = null;
        foreach (var t in map.GetComponentsInChildren<Transform>(true))
        {
            if (t.name == "LIGHT_Moon") { moonPos = t; continue; }
            if (t.name == "LIGHT_MoonTarget") { moonTarget = t; continue; }
            foreach (var kv in SortedLights())
            {
                if (!t.name.StartsWith(kv.Key + "_")) continue;
                var l = t.gameObject.GetComponent<Light>();
                if (l == null) l = t.gameObject.AddComponent<Light>();
                l.type = kv.Value.type; l.color = kv.Value.col; l.intensity = kv.Value.intensity;
                l.range = kv.Value.range; l.shadows = kv.Value.shadows;
                l.lightmapBakeType = LightmapBakeType.Realtime;   // switch to Mixed/Baked later if you bake lighting
                n++;
                break;
            }
        }
        // the moon: a separate directional light (not a child, so it is easy to find)
        var oldMoon = GameObject.Find("Moon");
        if (oldMoon != null) Undo.DestroyObjectImmediate(oldMoon);
        var moon = new GameObject("Moon");
        Undo.RegisterCreatedObjectUndo(moon, "Moon");
        var ml = moon.AddComponent<Light>();
        ml.type = UnityEngine.LightType.Directional; ml.color = new Color(0.62f, 0.70f, 1f); ml.intensity = 0.35f;
        ml.shadows = LightShadows.Soft; ml.shadowStrength = 0.8f;
        if (moonPos != null && moonTarget != null)
            moon.transform.rotation = Quaternion.LookRotation(moonTarget.position - moonPos.position);
        else
            moon.transform.rotation = Quaternion.Euler(35f, 150f, 0f);
        RenderSettings.sun = ml;
        Debug.Log("[CastleMap] Created " + n + " point lights + moon.");
    }

    // longest prefix first, so "Spawn_Light_High" is not caught by "Spawn_Light"
    static List<KeyValuePair<string, L>> SortedLights()
    {
        var list = new List<KeyValuePair<string, L>>(LIGHTS);
        list.Sort((a, b) => b.Key.Length.CompareTo(a.Key.Length));
        return list;
    }

    static bool Matches(string name, string[] list)
    {
        foreach (var s in list)
            if (s.EndsWith("_") ? name.StartsWith(s) : name == s) return true;
        return false;
    }

    static void AddCollidersAndFlags(GameObject map)
    {
        var flags = StaticEditorFlags.ContributeGI | StaticEditorFlags.BatchingStatic | StaticEditorFlags.OccluderStatic |
                    StaticEditorFlags.OccludeeStatic | StaticEditorFlags.ReflectionProbeStatic;
        foreach (var mr in map.GetComponentsInChildren<MeshRenderer>(true))
        {
            var go = mr.gameObject;
            GameObjectUtility.SetStaticEditorFlags(go, flags);
            if (Matches(go.name, NO_SHADOW)) { mr.shadowCastingMode = ShadowCastingMode.Off; }
            if (go.name == "Spawn_Beam") mr.receiveShadows = false;
            if (Matches(go.name, COLLIDE) && !Matches(go.name, NO_COLLIDE) && go.GetComponent<MeshCollider>() == null)
                go.AddComponent<MeshCollider>();
        }
    }

    static void Atmosphere(bool urp)
    {
        RenderSettings.ambientMode = AmbientMode.Trilight;
        RenderSettings.ambientSkyColor = new Color(0.09f, 0.10f, 0.18f);
        RenderSettings.ambientEquatorColor = new Color(0.05f, 0.06f, 0.09f);
        RenderSettings.ambientGroundColor = new Color(0.02f, 0.025f, 0.02f);
        RenderSettings.fog = true;
        RenderSettings.fogMode = FogMode.ExponentialSquared;
        RenderSettings.fogDensity = 0.011f;
        RenderSettings.fogColor = new Color(0.035f, 0.04f, 0.07f);

        var skyPath = ROOT + "/NightSky.mat";
        var sky = AssetDatabase.LoadAssetAtPath<Material>(skyPath);
        if (sky == null) { sky = new Material(Shader.Find("Skybox/Procedural")); AssetDatabase.CreateAsset(sky, skyPath); }
        sky.SetFloat("_SunDisk", 0); sky.SetFloat("_SunSize", 0f); sky.SetFloat("_AtmosphereThickness", 0.35f);
        sky.SetColor("_SkyTint", new Color(0.25f, 0.30f, 0.55f)); sky.SetColor("_GroundColor", new Color(0.03f, 0.03f, 0.04f));
        sky.SetFloat("_Exposure", 0.22f);
        RenderSettings.skybox = sky;
        DynamicGI.UpdateEnvironment();

        if (!urp) return;
        var profPath = ROOT + "/CastleMap_PostFX.asset";
        var prof = AssetDatabase.LoadAssetAtPath<VolumeProfile>(profPath);
        if (prof == null)
        {
            prof = ScriptableObject.CreateInstance<VolumeProfile>();
            AssetDatabase.CreateAsset(prof, profPath);
            var bloom = prof.Add<Bloom>(true);
            bloom.threshold.Override(1.0f); bloom.intensity.Override(1.3f); bloom.scatter.Override(0.72f);
            var tm = prof.Add<Tonemapping>(true); tm.mode.Override(TonemappingMode.ACES);
            var vig = prof.Add<Vignette>(true); vig.intensity.Override(0.32f); vig.smoothness.Override(0.5f);
            var ca = prof.Add<ColorAdjustments>(true); ca.postExposure.Override(0.3f); ca.saturation.Override(-10f);
            foreach (var c in prof.components) AssetDatabase.AddObjectToAsset(c, prof);
            AssetDatabase.SaveAssets();
        }
        var oldVol = GameObject.Find("CastleMap_PostFX");
        if (oldVol != null) Undo.DestroyObjectImmediate(oldVol);
        var volGo = new GameObject("CastleMap_PostFX");
        Undo.RegisterCreatedObjectUndo(volGo, "PostFX");
        var vol = volGo.AddComponent<Volume>();
        vol.isGlobal = true; vol.sharedProfile = prof;

        foreach (var cam in Object.FindObjectsByType<Camera>(FindObjectsSortMode.None))
        {
            var data = cam.GetUniversalAdditionalCameraData();
            data.renderPostProcessing = true;
            cam.farClipPlane = Mathf.Max(cam.farClipPlane, 600f);
        }
    }
}
#endif
