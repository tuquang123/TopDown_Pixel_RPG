using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.Animations;
using UnityEngine;
using Object = UnityEngine.Object;

public static class IdleBattlerPrefabBuilder
{
    private const string BattlerRoot = "Assets/Art/Gif/Super_Retro_Collection/Resources/Battlers";
    private const string BossFrameRoot = "Assets/Art/BossArt/IdleWebBosses";
    private const string AnimationRoot = "Assets/Animations/IdleBattlers";
    private const string PrefabRoot = "Assets/Prefab/IdleBattlers";
    private const string SmallPrefabRoot = PrefabRoot + "/Small Enemies";
    private const string BossPrefabRoot = PrefabRoot + "/Bosses";
    private const string WaveConfigPath = "Assets/Data_Game/Level/WaveManagerConfig.asset";
    private const float FrameRate = 12f;
    private const string AutoBuildSessionKey = "IdleBattlerPrefabBuilder.AutoBuildQueued.v3";

    private sealed class Spec
    {
        public string Name;
        public string Sprite;
        public string FrameSet;
        public bool Boss;
        public int Health;
        public int Damage;
        public float Scale;
        public Vector2 ColliderSize;
        public Vector2 ColliderOffset;
    }

    [MenuItem("Tools/Idle Battlers/Build Real Idle Enemies")]
    public static void BuildFromMenu() => Build();

    [InitializeOnLoadMethod]
    private static void AutoBuildWhenImported()
    {
        if (SessionState.GetBool(AutoBuildSessionKey, false) || GeneratedLooksCurrent())
            return;

        SessionState.SetBool(AutoBuildSessionKey, true);
        EditorApplication.delayCall += () =>
        {
            try
            {
                Debug.Log("IdleBattlerPrefabBuilder: auto build started.");
                Build();
                Debug.Log("IdleBattlerPrefabBuilder: auto build finished.");
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"IdleBattlerPrefabBuilder auto build failed: {ex}");
            }
        };
    }

    public static void Build()
    {
        EnsureDirectory(SmallPrefabRoot);
        EnsureDirectory(BossPrefabRoot);
        EnsureDirectory(AnimationRoot);
        DeletePrefabs(BossPrefabRoot);

        foreach (Spec spec in Specs())
        {
            if (spec.Boss)
                ConfigureBossFrames(spec.FrameSet);
            else
                ConfigureSprite($"{BattlerRoot}/{spec.Sprite}.png");

            AnimatorController controller = spec.Boss ? CreateBossController(spec) : CreateSmallController(spec);
            CreatePrefab(spec, controller);
        }

        AddToWaveConfig();
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
    }

    private static IEnumerable<Spec> Specs()
    {
        string[] small =
        {
            "SlimeA", "SlimeB", "SlimeC", "SlimeD", "SlimeE", "SlimeF", "SlimeG", "SlimeH",
            "MushroomA", "MushroomB", "MushroomC", "GhostA", "GhostB", "GhostC", "GhostD",
            "WaspA", "WaspB", "WaspC", "WormA", "WormB"
        };

        for (int i = 0; i < small.Length; i++)
        {
            yield return new Spec
            {
                Name = $"Idle {SplitName(small[i])}",
                Sprite = small[i],
                Boss = false,
                Health = 90 + i * 22,
                Damage = 9 + i * 2,
                Scale = small[i].StartsWith("Slime") ? 2.35f : 2.05f,
                ColliderSize = new Vector2(0.8f, 1f),
                ColliderOffset = new Vector2(0f, -0.12f)
            };
        }

        yield return Boss("Idle Boss Forest Ent Brute", "ForestEntBrute", 3600, 75, 2.35f);
        yield return Boss("Idle Boss Forest Witch Doctor", "ForestWitchDoctor", 4300, 88, 2.35f);
        yield return Boss("Idle Boss Forest Goblin Shaman", "ForestGoblinShaman", 5000, 102, 2.35f);
        yield return Boss("Idle Boss Swamp Centipede", "SwampCentipede", 5700, 116, 2.8f);
        yield return Boss("Idle Boss Battle Turtle", "BattleTurtle", 6400, 130, 2.85f);
        yield return Boss("Idle Boss Bloated Swamp Ogre", "BloatedSwampOgre", 7100, 144, 2.85f);
        yield return Boss("Idle Boss Storm Cloud Beast", "StormCloudBeast", 7800, 158, 2.25f);
        yield return Boss("Idle Boss Cursed Spirit Lord", "CursedSpiritLord", 8500, 172, 2.25f);
        yield return Boss("Idle Boss Pumpkin Tyrant", "PumpkinTyrant", 9200, 186, 2.25f);
        yield return Boss("Idle Boss Abyss Demon", "AbyssDemon", 9900, 200, 1.55f);
    }

    private static Spec Boss(string name, string frameSet, int health, int damage, float scale)
    {
        return new Spec
        {
            Name = name,
            FrameSet = frameSet,
            Boss = true,
            Health = health,
            Damage = damage,
            Scale = scale,
            ColliderSize = new Vector2(1.15f, 1.45f),
            ColliderOffset = new Vector2(0f, -0.28f)
        };
    }

    private static AnimatorController CreateSmallController(Spec spec)
    {
        string dir = $"{AnimationRoot}/Small Enemies/{ToId(spec.Name)}";
        EnsureDirectory(dir);

        AnimationClip idle = CreatePoseClip($"{dir}/0_Idle.anim", spec.Scale, true, false, new[]
        {
            Key(0f, 0f, 0f, 1f, 1f, Color.white),
            Key(0.35f, 0f, 0.035f, 1.015f, 1.035f, Color.white),
            Key(0.7f, 0f, 0f, 1f, 1f, Color.white)
        });
        AnimationClip move = CreatePoseClip($"{dir}/1_Move.anim", spec.Scale, true, false, new[]
        {
            Key(0f, -0.03f, 0f, 1f, 1f, Color.white),
            Key(0.2f, 0.03f, 0.04f, 1.02f, 0.98f, Color.white),
            Key(0.4f, -0.03f, 0f, 1f, 1f, Color.white)
        });
        AnimationClip attack = CreatePoseClip($"{dir}/2_Attack.anim", spec.Scale, false, true, new[]
        {
            Key(0f, 0f, 0f, 1f, 1f, Color.white),
            Key(0.12f, 0.12f, 0f, 0.96f, 1.04f, Color.white),
            Key(0.24f, -0.22f, 0.03f, 1.12f, 0.92f, new Color(1f, 0.88f, 0.55f, 1f)),
            Key(0.42f, 0f, 0f, 1f, 1f, Color.white)
        });
        AnimationClip hit = CreatePoseClip($"{dir}/3_Damaged.anim", spec.Scale, false, false, new[]
        {
            Key(0f, 0f, 0f, 1f, 1f, Color.white),
            Key(0.08f, 0.08f, 0f, 1f, 1f, new Color(1f, 0.35f, 0.35f, 1f)),
            Key(0.18f, 0f, 0f, 1f, 1f, Color.white)
        });
        AnimationClip death = CreatePoseClip($"{dir}/4_Death.anim", spec.Scale, false, false, new[]
        {
            Key(0f, 0f, 0f, 1f, 1f, Color.white),
            Key(0.25f, 0f, -0.08f, 1.1f, 0.75f, new Color(0.75f, 0.75f, 0.75f, 0.75f)),
            Key(0.5f, 0f, -0.2f, 1.25f, 0.45f, new Color(0.35f, 0.35f, 0.35f, 0f))
        });

        return CreateController($"{dir}/{ToId(spec.Name)}.controller", idle, move, attack, hit, death);
    }

    private static AnimatorController CreateBossController(Spec spec)
    {
        string dir = $"{AnimationRoot}/Bosses/{ToId(spec.Name)}";
        EnsureDirectory(dir);

        AnimationClip idle = CreateFrameClip(spec, $"{dir}/0_Idle.anim", "Idle", true);
        AnimationClip move = CreateFrameClip(spec, $"{dir}/1_Move.anim", "Move", true);
        AnimationClip attack = CreateFrameClip(spec, $"{dir}/2_Attack.anim", "Attack", false, true);
        AnimationClip hit = CreateFrameClip(spec, $"{dir}/3_Damaged.anim", "Hit", false);
        AnimationClip death = CreateFrameClip(spec, $"{dir}/4_Death.anim", "Death", false);

        return CreateController($"{dir}/{ToId(spec.Name)}.controller", idle, move, attack, hit, death);
    }

    private static AnimatorController CreateController(string path, AnimationClip idle, AnimationClip move, AnimationClip attack, AnimationClip hit, AnimationClip death)
    {
        AssetDatabase.DeleteAsset(path);
        AnimatorController controller = AnimatorController.CreateAnimatorControllerAtPath(path);
        controller.AddParameter("1_Move", AnimatorControllerParameterType.Bool);
        controller.AddParameter("2_Attack", AnimatorControllerParameterType.Trigger);
        controller.AddParameter("3_Damaged", AnimatorControllerParameterType.Trigger);
        controller.AddParameter("4_Death", AnimatorControllerParameterType.Trigger);
        controller.AddParameter("8_Attack", AnimatorControllerParameterType.Trigger);

        AnimatorStateMachine sm = controller.layers[0].stateMachine;
        AnimatorState idleState = sm.AddState("0_Idle", new Vector3(240, 80, 0));
        AnimatorState moveState = sm.AddState("1_Move", new Vector3(240, 190, 0));
        AnimatorState attackState = sm.AddState("2_Attack", new Vector3(500, 80, 0));
        AnimatorState hitState = sm.AddState("3_Damaged", new Vector3(500, 190, 0));
        AnimatorState deathState = sm.AddState("4_Death", new Vector3(500, 300, 0));

        idleState.motion = idle;
        moveState.motion = move;
        attackState.motion = attack;
        hitState.motion = hit;
        deathState.motion = death;
        attackState.tag = "Attack";
        deathState.tag = "Death";
        sm.defaultState = idleState;

        AnimatorStateTransition toMove = idleState.AddTransition(moveState);
        toMove.hasExitTime = false;
        toMove.duration = 0f;
        toMove.AddCondition(AnimatorConditionMode.If, 0f, "1_Move");

        AnimatorStateTransition toIdle = moveState.AddTransition(idleState);
        toIdle.hasExitTime = false;
        toIdle.duration = 0f;
        toIdle.AddCondition(AnimatorConditionMode.IfNot, 0f, "1_Move");

        AddAny(sm, attackState, "2_Attack");
        AddAny(sm, attackState, "8_Attack");
        AddAny(sm, hitState, "3_Damaged");
        AddAny(sm, deathState, "4_Death", false);
        ReturnToIdle(attackState, idleState);
        ReturnToIdle(hitState, idleState);
        return controller;
    }

    private static AnimationClip CreatePoseClip(string path, float baseScale, bool loop, bool attackEvent, PoseKey[] keys)
    {
        AssetDatabase.DeleteAsset(path);
        AnimationClip clip = new AnimationClip { frameRate = FrameRate };
        SetCurve(clip, "m_LocalPosition", keys, key => new Vector3(key.X, key.Y, 0f));
        SetCurve(clip, "m_LocalScale", keys, key => new Vector3(key.ScaleX * baseScale, key.ScaleY * baseScale, 1f));
        SetColor(clip, keys);
        SetLoop(clip, loop);

        if (attackEvent)
            AnimationUtility.SetAnimationEvents(clip, new[] { new AnimationEvent { functionName = "OnAttackHit", time = 0.24f } });

        AssetDatabase.CreateAsset(clip, path);
        return clip;
    }

    private static AnimationClip CreateFrameClip(Spec spec, string path, string state, bool loop, bool attackEvent = false)
    {
        AssetDatabase.DeleteAsset(path);
        List<Sprite> sprites = LoadSprites($"{BossFrameRoot}/{spec.FrameSet}/{state}");
        if (sprites.Count == 0)
            sprites = LoadSprites($"{BossFrameRoot}/{spec.FrameSet}/Idle");

        AnimationClip clip = new AnimationClip { frameRate = FrameRate };
        ObjectReferenceKeyframe[] frames = new ObjectReferenceKeyframe[sprites.Count];
        for (int i = 0; i < sprites.Count; i++)
            frames[i] = new ObjectReferenceKeyframe { time = i / FrameRate, value = sprites[i] };

        AnimationUtility.SetObjectReferenceCurve(clip, EditorCurveBinding.PPtrCurve("", typeof(SpriteRenderer), "m_Sprite"), frames);
        SetLoop(clip, loop);

        if (attackEvent)
            AnimationUtility.SetAnimationEvents(clip, new[] { new AnimationEvent { functionName = "OnAttackHit", time = Mathf.Max(0.1f, sprites.Count * 0.55f / FrameRate) } });

        AssetDatabase.CreateAsset(clip, path);
        return clip;
    }

    private static void CreatePrefab(Spec spec, AnimatorController controller)
    {
        GameObject root = new GameObject(spec.Name);
        root.layer = LayerMask.NameToLayer("Enemy");
        TrySetTag(root, "Enemy");

        Rigidbody2D rb = root.AddComponent<Rigidbody2D>();
        rb.gravityScale = 0f;
        rb.freezeRotation = true;
        rb.interpolation = RigidbodyInterpolation2D.Interpolate;

        BoxCollider2D collider = root.AddComponent<BoxCollider2D>();
        collider.offset = spec.ColliderOffset;
        collider.size = spec.ColliderSize;

        GameObject visual = new GameObject("UnitRoot");
        visual.transform.SetParent(root.transform, false);
        visual.transform.localScale = Vector3.one * spec.Scale;

        SpriteRenderer renderer = visual.AddComponent<SpriteRenderer>();
        renderer.sprite = spec.Boss
            ? LoadSprites($"{BossFrameRoot}/{spec.FrameSet}/Idle").FirstOrDefault()
            : AssetDatabase.LoadAssetAtPath<Sprite>($"{BattlerRoot}/{spec.Sprite}.png");
        renderer.sortingOrder = spec.Boss ? 60 : 45;

        Animator animator = visual.AddComponent<Animator>();
        animator.runtimeAnimatorController = controller;
        visual.AddComponent<CommonAnimationEvents>();

        EnemyAI ai = spec.Boss ? root.AddComponent<BossAI>() : root.AddComponent<EnemyAI>();
        SerializedObject so = new SerializedObject(ai);
        SetString(so, "enemyName", spec.Name);
        SetInt(so, "maxHealth", spec.Health);
        SetInt(so, "attackDamage", spec.Damage);
        SetFloat(so, "attackRange", spec.Boss ? 1.6f : 1.15f);
        SetFloat(so, "detectionRange", spec.Boss ? 18f : 10f);
        SetFloat(so, "attackCooldown", spec.Boss ? 1.25f : 1f);
        SetFloat(so, "moveSpeed", spec.Boss ? 1.05f : 1.25f);
        SetFloat(so, "criticalChance", spec.Boss ? 8f : 3f);
        SetBool(so, "isBoss", spec.Boss);
        SetBool(so, "requireHitToAggro", false);
        SetBool(so, "enablePatrol", false);
        SetInt(so, "exp", spec.Boss ? 120 : 12);
        so.ApplyModifiedPropertiesWithoutUndo();

        PrefabUtility.SaveAsPrefabAsset(root, $"{(spec.Boss ? BossPrefabRoot : SmallPrefabRoot)}/{spec.Name}.prefab");
        Object.DestroyImmediate(root);
    }

    private readonly struct PoseKey
    {
        public readonly float Time;
        public readonly float X;
        public readonly float Y;
        public readonly float ScaleX;
        public readonly float ScaleY;
        public readonly Color Color;

        public PoseKey(float time, float x, float y, float scaleX, float scaleY, Color color)
        {
            Time = time;
            X = x;
            Y = y;
            ScaleX = scaleX;
            ScaleY = scaleY;
            Color = color;
        }
    }

    private static PoseKey Key(float time, float x, float y, float scaleX, float scaleY, Color color) => new PoseKey(time, x, y, scaleX, scaleY, color);

    private static void AddToWaveConfig()
    {
        WaveManagerConfigSO config = AssetDatabase.LoadAssetAtPath<WaveManagerConfigSO>(WaveConfigPath);
        if (config == null)
            return;

        SerializedObject so = new SerializedObject(config);
        SerializedProperty enemyEntries = so.FindProperty("enemySpawnEntries");
        SerializedProperty bossEntries = so.FindProperty("bossStageOverrides");
        RemovePrefabEntriesUnder(enemyEntries, PrefabRoot);
        RemoveBossEntriesUnder(bossEntries, PrefabRoot);

        string[] smallPrefabs = Directory.GetFiles(SmallPrefabRoot, "*.prefab").OrderBy(path => path).ToArray();
        for (int i = 0; i < smallPrefabs.Length; i++)
        {
            SerializedProperty entry = AddArrayElement(enemyEntries);
            entry.FindPropertyRelative("prefab").objectReferenceValue = AssetDatabase.LoadAssetAtPath<GameObject>(Normalize(smallPrefabs[i]));
            entry.FindPropertyRelative("minStage").intValue = 1 + i / 4;
            entry.FindPropertyRelative("maxStage").intValue = 0;
            entry.FindPropertyRelative("weight").floatValue = 3f + i % 3;
        }

        string[] bossPrefabs = Directory.GetFiles(BossPrefabRoot, "*.prefab").OrderBy(path => path).ToArray();
        for (int i = 0; i < bossPrefabs.Length; i++)
        {
            SerializedProperty entry = AddArrayElement(bossEntries);
            entry.FindPropertyRelative("stage").intValue = 9 + i;
            entry.FindPropertyRelative("bossPrefab").objectReferenceValue = AssetDatabase.LoadAssetAtPath<GameObject>(Normalize(bossPrefabs[i]));
        }

        so.ApplyModifiedPropertiesWithoutUndo();
        EditorUtility.SetDirty(config);
        Debug.Log("IdleBattlerPrefabBuilder: idle battlers refreshed in WaveManagerConfig.");
    }

    private static SerializedProperty AddArrayElement(SerializedProperty array)
    {
        int index = array.arraySize;
        array.InsertArrayElementAtIndex(index);
        return array.GetArrayElementAtIndex(index);
    }

    private static void RemovePrefabEntriesUnder(SerializedProperty entries, string folder)
    {
        for (int i = entries.arraySize - 1; i >= 0; i--)
        {
            Object prefab = entries.GetArrayElementAtIndex(i).FindPropertyRelative("prefab").objectReferenceValue;
            if (prefab != null && AssetDatabase.GetAssetPath(prefab).StartsWith(folder))
                entries.DeleteArrayElementAtIndex(i);
        }
    }

    private static void RemoveBossEntriesUnder(SerializedProperty entries, string folder)
    {
        for (int i = entries.arraySize - 1; i >= 0; i--)
        {
            Object prefab = entries.GetArrayElementAtIndex(i).FindPropertyRelative("bossPrefab").objectReferenceValue;
            if (prefab != null && AssetDatabase.GetAssetPath(prefab).StartsWith(folder))
                entries.DeleteArrayElementAtIndex(i);
        }
    }

    private static void SetCurve(AnimationClip clip, string property, PoseKey[] keys, System.Func<PoseKey, Vector3> selector)
    {
        AnimationCurve x = new AnimationCurve();
        AnimationCurve y = new AnimationCurve();
        AnimationCurve z = new AnimationCurve();
        foreach (PoseKey key in keys)
        {
            Vector3 value = selector(key);
            x.AddKey(key.Time, value.x);
            y.AddKey(key.Time, value.y);
            z.AddKey(key.Time, value.z);
        }
        clip.SetCurve("", typeof(Transform), $"{property}.x", x);
        clip.SetCurve("", typeof(Transform), $"{property}.y", y);
        clip.SetCurve("", typeof(Transform), $"{property}.z", z);
    }

    private static void SetColor(AnimationClip clip, PoseKey[] keys)
    {
        AnimationCurve r = new AnimationCurve();
        AnimationCurve g = new AnimationCurve();
        AnimationCurve b = new AnimationCurve();
        AnimationCurve a = new AnimationCurve();
        foreach (PoseKey key in keys)
        {
            r.AddKey(key.Time, key.Color.r);
            g.AddKey(key.Time, key.Color.g);
            b.AddKey(key.Time, key.Color.b);
            a.AddKey(key.Time, key.Color.a);
        }
        clip.SetCurve("", typeof(SpriteRenderer), "m_Color.r", r);
        clip.SetCurve("", typeof(SpriteRenderer), "m_Color.g", g);
        clip.SetCurve("", typeof(SpriteRenderer), "m_Color.b", b);
        clip.SetCurve("", typeof(SpriteRenderer), "m_Color.a", a);
    }

    private static void SetLoop(AnimationClip clip, bool loop)
    {
        AnimationClipSettings settings = AnimationUtility.GetAnimationClipSettings(clip);
        settings.loopTime = loop;
        AnimationUtility.SetAnimationClipSettings(clip, settings);
    }

    private static void ConfigureSprite(string path)
    {
        TextureImporter importer = AssetImporter.GetAtPath(path) as TextureImporter;
        if (importer == null)
            return;
        importer.textureType = TextureImporterType.Sprite;
        importer.spriteImportMode = SpriteImportMode.Single;
        importer.spritePixelsPerUnit = 80f;
        importer.filterMode = FilterMode.Point;
        importer.textureCompression = TextureImporterCompression.Uncompressed;
        importer.mipmapEnabled = false;
        importer.SaveAndReimport();
    }

    private static void ConfigureBossFrames(string frameSet)
    {
        string root = $"{BossFrameRoot}/{frameSet}";
        if (!Directory.Exists(root))
            return;
        foreach (string path in Directory.GetFiles(root, "*.png", SearchOption.AllDirectories))
            ConfigureSprite(Normalize(path));
    }

    private static List<Sprite> LoadSprites(string folder)
    {
        return Directory.Exists(folder)
            ? Directory.GetFiles(folder, "*.png").OrderBy(path => path).Select(path => AssetDatabase.LoadAssetAtPath<Sprite>(Normalize(path))).Where(sprite => sprite != null).ToList()
            : new List<Sprite>();
    }

    private static void DeletePrefabs(string folder)
    {
        if (!Directory.Exists(folder))
            return;
        foreach (string path in Directory.GetFiles(folder, "*.prefab"))
            AssetDatabase.DeleteAsset(Normalize(path));
    }

    private static bool GeneratedLooksCurrent()
    {
        return Directory.Exists(SmallPrefabRoot)
               && Directory.Exists(BossPrefabRoot)
               && Directory.GetFiles(SmallPrefabRoot, "*.prefab").Length == 20
               && Directory.GetFiles(BossPrefabRoot, "*.prefab").Length == 10
               && File.Exists($"{AnimationRoot}/Bosses/IdleBossAbyssDemon/IdleBossAbyssDemon.controller")
               && Directory.Exists($"{BossFrameRoot}/ForestEntBrute")
               && File.Exists($"{AnimationRoot}/Small Enemies/IdleSlimeA/0_Idle.anim")
               && File.ReadAllText($"{AnimationRoot}/Small Enemies/IdleSlimeA/0_Idle.anim").Contains("value: 2.35");
    }

    private static void AddAny(AnimatorStateMachine sm, AnimatorState state, string trigger, bool self = true)
    {
        AnimatorStateTransition transition = sm.AddAnyStateTransition(state);
        transition.hasExitTime = false;
        transition.duration = 0f;
        transition.canTransitionToSelf = self;
        transition.AddCondition(AnimatorConditionMode.If, 0f, trigger);
    }

    private static void ReturnToIdle(AnimatorState state, AnimatorState idle)
    {
        AnimatorStateTransition transition = state.AddTransition(idle);
        transition.hasExitTime = true;
        transition.exitTime = 1f;
        transition.duration = 0f;
    }

    private static void SetString(SerializedObject so, string name, string value)
    {
        SerializedProperty property = so.FindProperty(name);
        if (property != null) property.stringValue = value;
    }

    private static void SetInt(SerializedObject so, string name, int value)
    {
        SerializedProperty property = so.FindProperty(name);
        if (property != null) property.intValue = value;
    }

    private static void SetFloat(SerializedObject so, string name, float value)
    {
        SerializedProperty property = so.FindProperty(name);
        if (property != null) property.floatValue = value;
    }

    private static void SetBool(SerializedObject so, string name, bool value)
    {
        SerializedProperty property = so.FindProperty(name);
        if (property != null) property.boolValue = value;
    }

    private static void TrySetTag(GameObject obj, string tag)
    {
        try { obj.tag = tag; }
        catch { Debug.LogWarning($"IdleBattlerPrefabBuilder: tag '{tag}' chua ton tai."); }
    }

    private static string SplitName(string value) => System.Text.RegularExpressions.Regex.Replace(value, "([a-z])([A-Z])", "$1 $2");
    private static string ToId(string value) => new string(value.Where(char.IsLetterOrDigit).ToArray());
    private static string Normalize(string path) => path.Replace("\\", "/");
    private static void EnsureDirectory(string path) { if (!Directory.Exists(path)) Directory.CreateDirectory(path); }
}
