using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.Animations;
using UnityEngine;

public static class WebBossPrefabBuilder
{
    private const string TemplatePrefab = "Assets/Prefab/Enemy/WebBosses/Boss.prefab";
    private const string FrameRoot = "Assets/Art/BossArt/WebBossFrames";
    private const string AnimationRoot = "Assets/Animations/WebBosses";
    private const string PrefabRoot = "Assets/Prefab/Enemy/WebBosses";
    private const float FrameRate = 12f;
    private const bool AutoBuildOnEditorLoad = false;
    private const string AutoBuildSessionKey = "WebBossPrefabBuilder.AutoBuildQueued.v7";

    private sealed class BossSpec
    {
        public string Id;
        public string DisplayName;
        public float VisualScale;
        public bool InvertFacingDirection;
        public bool PlayMeleeSlashVfx;
        public Vector3 UnitRootOffset;
        public Vector3 ShadowOffset;
        public Vector3 ShadowScale;
        public Vector2 ColliderOffset;
        public Vector2 ColliderSize;
    }

    [MenuItem("Tools/Bosses/Build Web Boss Prefabs")]
    public static void BuildFromMenu()
    {
        Build();
    }

    [InitializeOnLoadMethod]
    private static void AutoBuildWhenImported()
    {
        if (!AutoBuildOnEditorLoad)
            return;

        if (SessionState.GetBool(AutoBuildSessionKey, false))
            return;

        if (!Directory.Exists(FrameRoot))
            return;

        if (GeneratedBossesAreCurrent())
            return;

        SessionState.SetBool(AutoBuildSessionKey, true);
        EditorApplication.delayCall += () =>
        {
            try
            {
                Debug.Log("WebBossPrefabBuilder: auto build started.");
                Build();
                Debug.Log("WebBossPrefabBuilder: auto build finished.");
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"WebBossPrefabBuilder auto build failed: {ex}");
            }
        };
    }

    public static void Build()
    {
        EnsureDirectory(AnimationRoot);
        EnsureDirectory(PrefabRoot);

        foreach (BossSpec spec in Specs())
        {
            ConfigureFrameImports(spec);
            AnimatorController controller = CreateController(spec);
            CreatePrefab(spec, controller);
        }

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
    }

    private static IEnumerable<BossSpec> Specs()
    {
        yield return new BossSpec
        {
            Id = "UndeadExecutioner",
            DisplayName = "Undead Executioner",
            VisualScale = 2.2f,
            InvertFacingDirection = true,
            UnitRootOffset = new Vector3(0f, -0.15f, 0f),
            ShadowOffset = new Vector3(0f, -0.55f, 0f),
            ShadowScale = new Vector3(0.045f, 0.014f, 1f),
            ColliderOffset = new Vector2(0f, -0.45f),
            ColliderSize = new Vector2(0.9f, 1.35f)
        };
        yield return new BossSpec
        {
            Id = "MechaStoneGolem",
            DisplayName = "Mecha Stone Golem",
            VisualScale = 2.4f,
            InvertFacingDirection = true,
            UnitRootOffset = new Vector3(0f, -0.2f, 0f),
            ShadowOffset = new Vector3(0f, -0.58f, 0f),
            ShadowScale = new Vector3(0.05f, 0.016f, 1f),
            ColliderOffset = new Vector2(0f, -0.45f),
            ColliderSize = new Vector2(1.05f, 1.35f)
        };
        yield return new BossSpec
        {
            Id = "EngveeBossCreature",
            DisplayName = "Boss Creature",
            VisualScale = 1.55f,
            InvertFacingDirection = true,
            UnitRootOffset = new Vector3(0f, -0.45f, 0f),
            ShadowOffset = new Vector3(0f, -0.55f, 0f),
            ShadowScale = new Vector3(0.06f, 0.018f, 1f),
            ColliderOffset = new Vector2(0f, -0.35f),
            ColliderSize = new Vector2(1.35f, 1.45f)
        };
        yield return new BossSpec
        {
            Id = "SkeletonBoss",
            DisplayName = "Skeleton Boss",
            VisualScale = 2.5f,
            InvertFacingDirection = true,
            UnitRootOffset = new Vector3(0f, -0.25f, 0f),
            ShadowOffset = new Vector3(0f, -0.52f, 0f),
            ShadowScale = new Vector3(0.045f, 0.014f, 1f),
            ColliderOffset = new Vector2(0f, -0.42f),
            ColliderSize = new Vector2(0.9f, 1.25f)
        };
        yield return new BossSpec
        {
            Id = "KingSlime",
            DisplayName = "King Slime",
            VisualScale = 2.7f,
            InvertFacingDirection = true,
            UnitRootOffset = new Vector3(0f, -0.35f, 0f),
            ShadowOffset = new Vector3(0f, -0.55f, 0f),
            ShadowScale = new Vector3(0.04f, 0.012f, 1f),
            ColliderOffset = new Vector2(0f, -0.35f),
            ColliderSize = new Vector2(0.95f, 0.95f)
        };
        yield return new BossSpec
        {
            Id = "GoblinKing",
            DisplayName = "Goblin King Boss",
            VisualScale = 2.7f,
            InvertFacingDirection = true,
            PlayMeleeSlashVfx = true,
            UnitRootOffset = new Vector3(0f, -0.25f, 0f),
            ShadowOffset = new Vector3(0f, -0.52f, 0f),
            ShadowScale = new Vector3(0.045f, 0.014f, 1f),
            ColliderOffset = new Vector2(0f, -0.42f),
            ColliderSize = new Vector2(0.95f, 1.2f)
        };
    }

    private static void ConfigureFrameImports(BossSpec spec)
    {
        foreach (string path in Directory.GetFiles($"{FrameRoot}/{spec.Id}", "*.png", SearchOption.AllDirectories))
        {
            string assetPath = Normalize(path);
            TextureImporter importer = AssetImporter.GetAtPath(assetPath) as TextureImporter;
            if (importer == null)
                continue;

            importer.textureType = TextureImporterType.Sprite;
            importer.spriteImportMode = SpriteImportMode.Single;
            importer.spritePixelsPerUnit = 100f;
            importer.filterMode = FilterMode.Point;
            importer.textureCompression = TextureImporterCompression.Uncompressed;
            importer.mipmapEnabled = false;
            importer.SaveAndReimport();
        }
    }

    private static AnimatorController CreateController(BossSpec spec)
    {
        string dir = $"{AnimationRoot}/{spec.Id}";
        EnsureDirectory(dir);

        AnimationClip idle = CreateClip(spec, "0_Idle", "Idle", true);
        AnimationClip move = CreateClip(spec, "1_Move", "Move", true);
        AnimationClip attack = CreateClip(spec, "2_Attack", "Attack", false, true);
        AnimationClip damaged = CreateClip(spec, "3_Damaged", "Hit", false);
        AnimationClip death = CreateClip(spec, "4_Death", "Death", false);

        string controllerPath = $"{dir}/{spec.Id}.controller";
        AssetDatabase.DeleteAsset(controllerPath);
        AnimatorController controller = AnimatorController.CreateAnimatorControllerAtPath(controllerPath);
        controller.AddParameter("1_Move", AnimatorControllerParameterType.Bool);
        controller.AddParameter("2_Attack", AnimatorControllerParameterType.Trigger);
        controller.AddParameter("3_Damaged", AnimatorControllerParameterType.Trigger);
        controller.AddParameter("4_Death", AnimatorControllerParameterType.Trigger);
        controller.AddParameter("8_Attack", AnimatorControllerParameterType.Trigger);

        AnimatorStateMachine sm = controller.layers[0].stateMachine;
        AnimatorState idleState = sm.AddState("0_Idle", new Vector3(250, 80, 0));
        AnimatorState moveState = sm.AddState("1_Move", new Vector3(250, 190, 0));
        AnimatorState attackState = sm.AddState("2_Attack", new Vector3(520, 80, 0));
        AnimatorState damagedState = sm.AddState("3_Damaged", new Vector3(520, 190, 0));
        AnimatorState deathState = sm.AddState("4_Death", new Vector3(520, 300, 0));

        idleState.motion = idle;
        moveState.motion = move;
        attackState.motion = attack;
        damagedState.motion = damaged;
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

        AddAnyTrigger(sm, attackState, "2_Attack");
        AddAnyTrigger(sm, attackState, "8_Attack");
        AddAnyTrigger(sm, damagedState, "3_Damaged");
        AddAnyTrigger(sm, deathState, "4_Death", false);

        AddReturnToIdle(attackState, idleState);
        AddReturnToIdle(damagedState, idleState);

        EditorUtility.SetDirty(controller);
        return controller;
    }

    private static AnimationClip CreateClip(BossSpec spec, string clipName, string frameFolder, bool loop, bool addAttackEvent = false)
    {
        string clipPath = $"{AnimationRoot}/{spec.Id}/{clipName}.anim";
        AssetDatabase.DeleteAsset(clipPath);

        List<Sprite> sprites = LoadSprites($"{FrameRoot}/{spec.Id}/{frameFolder}");
        if (sprites.Count == 0)
            sprites = LoadSprites($"{FrameRoot}/{spec.Id}/Idle");

        AnimationClip clip = new AnimationClip { frameRate = FrameRate };
        EditorCurveBinding binding = EditorCurveBinding.PPtrCurve("", typeof(SpriteRenderer), "m_Sprite");
        ObjectReferenceKeyframe[] frames = new ObjectReferenceKeyframe[sprites.Count];

        for (int i = 0; i < sprites.Count; i++)
        {
            frames[i] = new ObjectReferenceKeyframe
            {
                time = i / FrameRate,
                value = sprites[i]
            };
        }

        AnimationUtility.SetObjectReferenceCurve(clip, binding, frames);

        AnimationClipSettings settings = AnimationUtility.GetAnimationClipSettings(clip);
        settings.loopTime = loop;
        AnimationUtility.SetAnimationClipSettings(clip, settings);

        if (addAttackEvent && sprites.Count > 0)
        {
            AnimationEvent hitEvent = new AnimationEvent
            {
                functionName = "OnAttackHit",
                time = Mathf.Max(0f, (sprites.Count * 0.55f) / FrameRate)
            };
            AnimationUtility.SetAnimationEvents(clip, new[] { hitEvent });
        }

        AssetDatabase.CreateAsset(clip, clipPath);
        return clip;
    }

    private static void CreatePrefab(BossSpec spec, AnimatorController controller)
    {
        GameObject root = PrefabUtility.LoadPrefabContents(TemplatePrefab);
        root.name = spec.DisplayName;

        Transform enemy = root.transform.Find("Enemy");
        Transform unitRoot = root.transform.Find("Enemy/UnitRoot");
        Transform shadowGroup = root.transform.Find("Enemy/UnitRoot/Shadow");
        Transform shadow = root.transform.Find("Enemy/UnitRoot/Shadow/Shadow");

        if (enemy != null)
            enemy.localScale = new Vector3(spec.VisualScale, spec.VisualScale, 1f);

        if (unitRoot != null)
        {
            unitRoot.localPosition = spec.UnitRootOffset;
            unitRoot.localScale = Vector3.one;

            Animator animator = unitRoot.GetComponent<Animator>();
            if (animator != null)
                animator.runtimeAnimatorController = controller;

            SpriteRenderer renderer = unitRoot.GetComponent<SpriteRenderer>();
            if (renderer != null)
                renderer.sprite = LoadSprites($"{FrameRoot}/{spec.Id}/Idle").FirstOrDefault();

            if (unitRoot.GetComponent<PlayerAnimationEvents>() == null)
                unitRoot.gameObject.AddComponent<PlayerAnimationEvents>();
        }

        if (shadowGroup != null)
            shadowGroup.localPosition = spec.ShadowOffset;

        if (shadow != null)
            shadow.localScale = spec.ShadowScale;

        BoxCollider2D collider = root.GetComponent<BoxCollider2D>();
        if (collider != null)
        {
            collider.offset = spec.ColliderOffset;
            collider.size = spec.ColliderSize;
        }

        BossAI boss = root.GetComponent<BossAI>();
        if (boss != null)
        {
            SerializedObject serializedBoss = new SerializedObject(boss);
            serializedBoss.FindProperty("enemyName").stringValue = spec.DisplayName;
            serializedBoss.FindProperty("maxHealth").intValue = 10000;
            serializedBoss.FindProperty("attackDamage").intValue = 100;
            serializedBoss.FindProperty("attackRange").floatValue = 1.5f;
            serializedBoss.FindProperty("detectionRange").floatValue = 20f;
            serializedBoss.FindProperty("moveSpeed").floatValue = 2f;
            serializedBoss.FindProperty("isBoss").boolValue = true;
            serializedBoss.FindProperty("killObjectiveID").stringValue = "Boss";
            SetBool(serializedBoss, "invertFacingDirection", spec.InvertFacingDirection);
            SetBool(serializedBoss, "playMeleeSlashVfx", spec.PlayMeleeSlashVfx);
            ApplyBossSkillDefaults(serializedBoss, spec.Id);
            serializedBoss.ApplyModifiedPropertiesWithoutUndo();
        }

        string prefabPath = $"{PrefabRoot}/{spec.DisplayName}.prefab";
        PrefabUtility.SaveAsPrefabAsset(root, prefabPath);
        PrefabUtility.UnloadPrefabContents(root);
    }

    private static List<Sprite> LoadSprites(string folder)
    {
        if (!Directory.Exists(folder))
            return new List<Sprite>();

        return Directory.GetFiles(folder, "*.png")
            .OrderBy(path => path)
            .Select(path => AssetDatabase.LoadAssetAtPath<Sprite>(Normalize(path)))
            .Where(sprite => sprite != null)
            .ToList();
    }

    private static void AddAnyTrigger(AnimatorStateMachine sm, AnimatorState target, string trigger, bool canTransitionToSelf = true)
    {
        AnimatorStateTransition transition = sm.AddAnyStateTransition(target);
        transition.hasExitTime = false;
        transition.duration = 0f;
        transition.canTransitionToSelf = canTransitionToSelf;
        transition.AddCondition(AnimatorConditionMode.If, 0f, trigger);
    }

    private static void AddReturnToIdle(AnimatorState state, AnimatorState idleState)
    {
        AnimatorStateTransition transition = state.AddTransition(idleState);
        transition.hasExitTime = true;
        transition.exitTime = 1f;
        transition.duration = 0f;
    }

    private static bool AnimationsUseUnitRootPath()
    {
        string sampleClip = $"{AnimationRoot}/UndeadExecutioner/0_Idle.anim";
        if (!File.Exists(sampleClip))
            return false;

        return File.ReadAllText(sampleClip).Contains("path: \n");
    }

    private static bool GeneratedBossesAreCurrent()
    {
        if (!Directory.Exists(PrefabRoot) || !AnimationsUseUnitRootPath())
            return false;

        string[] expectedPrefabs =
        {
            "Undead Executioner.prefab",
            "Mecha Stone Golem.prefab",
            "Boss Creature.prefab",
            "Skeleton Boss.prefab",
            "King Slime.prefab",
            "Goblin King Boss.prefab"
        };

        return expectedPrefabs.All(fileName => File.Exists($"{PrefabRoot}/{fileName}"))
               && File.Exists($"{AnimationRoot}/GoblinKing/GoblinKing.controller")
               && File.ReadAllText($"{PrefabRoot}/Goblin King Boss.prefab").Contains("playMeleeSlashVfx: 1")
               && File.ReadAllText($"{PrefabRoot}/King Slime.prefab").Contains("invertFacingDirection: 1");
    }

    private static void ApplyBossSkillDefaults(SerializedObject serializedBoss, string bossId)
    {
        SerializedProperty profile = serializedBoss.FindProperty("skillProfile");
        if (profile == null)
            return;

        profile.enumValueIndex = bossId switch
        {
            "EngveeBossCreature" => (int)BossSkillProfile.CriticalBite,
            "KingSlime" => (int)BossSkillProfile.SummonSlimes,
            "MechaStoneGolem" => (int)BossSkillProfile.StoneSpikes,
            "UndeadExecutioner" => (int)BossSkillProfile.PhaseVanish,
            _ => (int)BossSkillProfile.Basic
        };

        GameObject gravok = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefab/Enemy/Gravok.prefab");
        GameObject slime = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefab/Enemy/Small Slime.prefab");
        GameObject lightning = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefab/VFX/Boss Lightning Strike.prefab");
        GameObject stoneSpike = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefab/VFX/Boss Stone Spike Strike.prefab");

        SetObject(serializedBoss, "minionPrefab", gravok);
        SetObject(serializedBoss, "slimeMinionPrefab", slime);
        SetObject(serializedBoss, "lightningPrefab", lightning);
        SetObject(serializedBoss, "stoneSpikePrefab", stoneSpike);
        SetFloat(serializedBoss, "specialCooldown", bossId == "EngveeBossCreature" ? 6f : bossId == "MechaStoneGolem" ? 7.5f : 8f);
        SetInt(serializedBoss, "gravokSummonCount", 2);
        SetInt(serializedBoss, "slimeSummonCount", 5);
        SetFloat(serializedBoss, "areaSkillRadius", bossId == "MechaStoneGolem" ? 1.1f : 1.15f);
        SetFloat(serializedBoss, "vanishDuration", bossId == "UndeadExecutioner" ? 2.4f : 2.2f);
        SetBool(serializedBoss, "requireHitToAggro", false);
        SetBool(serializedBoss, "enablePatrol", false);
    }

    private static void SetObject(SerializedObject serializedObject, string propertyName, GameObject value)
    {
        SerializedProperty property = serializedObject.FindProperty(propertyName);
        if (property != null)
            property.objectReferenceValue = value;
    }

    private static void SetFloat(SerializedObject serializedObject, string propertyName, float value)
    {
        SerializedProperty property = serializedObject.FindProperty(propertyName);
        if (property != null)
            property.floatValue = value;
    }

    private static void SetInt(SerializedObject serializedObject, string propertyName, int value)
    {
        SerializedProperty property = serializedObject.FindProperty(propertyName);
        if (property != null)
            property.intValue = value;
    }

    private static void SetBool(SerializedObject serializedObject, string propertyName, bool value)
    {
        SerializedProperty property = serializedObject.FindProperty(propertyName);
        if (property != null)
            property.boolValue = value;
    }

    private static void EnsureDirectory(string path)
    {
        if (!Directory.Exists(path))
            Directory.CreateDirectory(path);
    }

    private static string Normalize(string path)
    {
        return path.Replace("\\", "/");
    }
}
