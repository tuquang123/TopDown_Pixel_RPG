using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.Animations;
using UnityEngine;

public static class BossSkillPrefabSetup
{
    private const string PrefabRoot = "Assets/Prefab/Enemy/WebBosses";
    private const string VfxRoot = "Assets/Prefab/VFX";
    private const string BossAnimRoot = "Assets/Animations/WebBosses";
    private const string SmallSlimeAnimRoot = "Assets/Animations/Enemies/SmallSlime";
    private const string SlimeSpriteRoot = "Assets/Art/BossArt/WebBosses/SkeletonBoss/characters/Characters/Slime/Slime";
    private const string GravokPrefab = "Assets/Prefab/Enemy/Gravok.prefab";
    private const string SmallSlimePrefabPath = "Assets/Prefab/Enemy/Small Slime.prefab";
    private const string GoblinBossPrefabPath = PrefabRoot + "/Goblin King Boss.prefab";
    private const string AutoSetupSessionKey = "BossSkillPrefabSetup.AutoSetupQueued.v1";

    [MenuItem("Tools/Bosses/Setup Boss Skills")]
    public static void SetupFromMenu()
    {
        Setup();
    }

    [InitializeOnLoadMethod]
    private static void AutoSetupWhenImported()
    {
        if (SessionState.GetBool(AutoSetupSessionKey, false))
            return;

        if (!Directory.Exists(PrefabRoot))
            return;

        if (GeneratedSetupLooksCurrent())
            return;

        SessionState.SetBool(AutoSetupSessionKey, true);
        EditorApplication.delayCall += () =>
        {
            try
            {
                Setup();
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"BossSkillPrefabSetup failed: {ex}");
            }
        };
    }

    public static void Setup()
    {
        EnsureDirectory(PrefabRoot);
        EnsureDirectory(VfxRoot);
        EnsureDirectory(SmallSlimeAnimRoot);

        GameObject lightning = CreateAreaStrikePrefab("Boss Lightning Strike", BossAreaStrikeVisual.Lightning);
        GameObject stoneSpike = CreateAreaStrikePrefab("Boss Stone Spike Strike", BossAreaStrikeVisual.StoneSpike);
        GameObject smallSlime = CreateSmallSlimePrefab();
        GameObject goblinBoss = EnsureGoblinBossPrefab();
        GameObject gravok = AssetDatabase.LoadAssetAtPath<GameObject>(GravokPrefab);

        ConfigureBoss(PrefabRoot + "/Boss.prefab", BossSkillProfile.SummonGravok, gravok, smallSlime, lightning, stoneSpike, 9f, 2, 5, 1.15f, 2.2f, "Bringer Of Death");
        ConfigureBoss(PrefabRoot + "/Boss 1.prefab", BossSkillProfile.LightningStorm, gravok, smallSlime, lightning, stoneSpike, 7f, 2, 5, 1.2f, 2.2f, "Boss 1");
        ConfigureBoss(PrefabRoot + "/Boss Creature.prefab", BossSkillProfile.CriticalBite, gravok, smallSlime, lightning, stoneSpike, 6f, 2, 5, 1.15f, 2.2f);
        ConfigureBoss(PrefabRoot + "/King Slime.prefab", BossSkillProfile.SummonSlimes, gravok, smallSlime, lightning, stoneSpike, 9f, 2, 5, 1.15f, 2.2f);
        ConfigureBoss(PrefabRoot + "/Mecha Stone Golem.prefab", BossSkillProfile.StoneSpikes, gravok, smallSlime, lightning, stoneSpike, 7.5f, 2, 5, 1.1f, 2.2f);
        ConfigureBoss(PrefabRoot + "/Undead Executioner.prefab", BossSkillProfile.PhaseVanish, gravok, smallSlime, lightning, stoneSpike, 8f, 2, 5, 1.15f, 2.4f);
        ConfigureBoss(PrefabRoot + "/Skeleton Boss.prefab", BossSkillProfile.Basic, gravok, smallSlime, lightning, stoneSpike, 8f, 2, 5, 1.15f, 2.2f);
        ConfigureBoss(GoblinBossPrefabPath, BossSkillProfile.Basic, gravok, smallSlime, lightning, stoneSpike, 8f, 2, 5, 1.15f, 2.2f);

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
    }

    private static GameObject CreateAreaStrikePrefab(string displayName, BossAreaStrikeVisual visual)
    {
        string path = $"{VfxRoot}/{displayName}.prefab";
        GameObject root = new GameObject(displayName);
        BossAreaStrikeEffect effect = root.AddComponent<BossAreaStrikeEffect>();
        SerializedObject serializedEffect = new SerializedObject(effect);
        serializedEffect.FindProperty("visual").enumValueIndex = (int)visual;
        serializedEffect.ApplyModifiedPropertiesWithoutUndo();

        GameObject prefab = PrefabUtility.SaveAsPrefabAsset(root, path);
        Object.DestroyImmediate(root);
        return prefab;
    }

    private static GameObject CreateSmallSlimePrefab()
    {
        AnimatorController controller = CreateSlimeController();
        GameObject root = new GameObject("Small Slime");
        root.tag = "Enemy";
        root.layer = LayerMask.NameToLayer("Enemy");

        BoxCollider2D collider = root.AddComponent<BoxCollider2D>();
        collider.offset = new Vector2(0f, -0.08f);
        collider.size = new Vector2(0.45f, 0.35f);

        Rigidbody2D rb = root.AddComponent<Rigidbody2D>();
        rb.gravityScale = 0f;
        rb.freezeRotation = true;
        rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
        rb.interpolation = RigidbodyInterpolation2D.Interpolate;

        EnemyAI ai = root.AddComponent<EnemyAI>();
        SerializedObject serializedAi = new SerializedObject(ai);
        serializedAi.FindProperty("enemyName").stringValue = "Small Slime";
        serializedAi.FindProperty("maxHealth").intValue = 120;
        serializedAi.FindProperty("attackDamage").intValue = 18;
        serializedAi.FindProperty("attackRange").floatValue = 0.85f;
        serializedAi.FindProperty("detectionRange").floatValue = 12f;
        serializedAi.FindProperty("attackCooldown").floatValue = 1.25f;
        serializedAi.FindProperty("moveSpeed").floatValue = 2.1f;
        serializedAi.FindProperty("requireHitToAggro").boolValue = false;
        serializedAi.FindProperty("enablePatrol").boolValue = false;
        serializedAi.FindProperty("exp").intValue = 1;
        serializedAi.ApplyModifiedPropertiesWithoutUndo();

        GameObject unitRoot = new GameObject("UnitRoot");
        unitRoot.transform.SetParent(root.transform, false);
        unitRoot.transform.localScale = Vector3.one * 2.6f;
        unitRoot.AddComponent<CommonAnimationEvents>();

        SpriteRenderer renderer = unitRoot.AddComponent<SpriteRenderer>();
        renderer.sortingOrder = 25;
        renderer.sprite = LoadSprites($"{SlimeSpriteRoot}/Idle.png").FirstOrDefault();

        Animator animator = unitRoot.AddComponent<Animator>();
        animator.runtimeAnimatorController = controller;

        GameObject prefab = PrefabUtility.SaveAsPrefabAsset(root, SmallSlimePrefabPath);
        Object.DestroyImmediate(root);
        return prefab;
    }

    private static GameObject EnsureGoblinBossPrefab()
    {
        GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(GoblinBossPrefabPath);
        if (prefab != null)
            return prefab;

        WebBossPrefabBuilder.Build();
        return AssetDatabase.LoadAssetAtPath<GameObject>(GoblinBossPrefabPath);
    }

    private static AnimatorController CreateSlimeController()
    {
        string controllerPath = $"{SmallSlimeAnimRoot}/SmallSlime.controller";
        AssetDatabase.DeleteAsset(controllerPath);

        AnimationClip idle = CreateClip($"{SmallSlimeAnimRoot}/0_Idle.anim", LoadSprites($"{SlimeSpriteRoot}/Idle.png"), true);
        AnimationClip move = CreateClip($"{SmallSlimeAnimRoot}/1_Move.anim", LoadSprites($"{SlimeSpriteRoot}/Walk.png"), true);
        AnimationClip attack = CreateClip($"{SmallSlimeAnimRoot}/2_Attack.anim", LoadSprites($"{SlimeSpriteRoot}/Walk.png"), false, true);
        AnimationClip damaged = CreateClip($"{SmallSlimeAnimRoot}/3_Damaged.anim", LoadSprites($"{SlimeSpriteRoot}/Idle.png"), false);
        AnimationClip death = CreateClip($"{SmallSlimeAnimRoot}/4_Death.anim", LoadSprites($"{SlimeSpriteRoot}/Death.png"), false);

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
        return controller;
    }

    private static AnimationClip CreateClip(string path, List<Sprite> sprites, bool loop, bool attackEvent = false)
    {
        AssetDatabase.DeleteAsset(path);
        AnimationClip clip = new AnimationClip { frameRate = 10f };
        EditorCurveBinding binding = EditorCurveBinding.PPtrCurve("", typeof(SpriteRenderer), "m_Sprite");
        ObjectReferenceKeyframe[] frames = new ObjectReferenceKeyframe[Mathf.Max(1, sprites.Count)];

        for (int i = 0; i < frames.Length; i++)
        {
            frames[i] = new ObjectReferenceKeyframe
            {
                time = i / clip.frameRate,
                value = sprites.Count > 0 ? sprites[Mathf.Min(i, sprites.Count - 1)] : null
            };
        }

        AnimationUtility.SetObjectReferenceCurve(clip, binding, frames);
        AnimationClipSettings settings = AnimationUtility.GetAnimationClipSettings(clip);
        settings.loopTime = loop;
        AnimationUtility.SetAnimationClipSettings(clip, settings);

        if (attackEvent)
        {
            AnimationUtility.SetAnimationEvents(clip, new[]
            {
                new AnimationEvent { functionName = "OnAttackHit", time = 0.2f }
            });
        }

        AssetDatabase.CreateAsset(clip, path);
        return clip;
    }

    private static void ConfigureBoss(
        string prefabPath,
        BossSkillProfile profile,
        GameObject gravok,
        GameObject smallSlime,
        GameObject lightning,
        GameObject stoneSpike,
        float cooldown,
        int gravokCount,
        int slimeCount,
        float radius,
        float vanishTime,
        string displayName = null)
    {
        GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);
        if (prefab == null)
            return;

        BossAI boss = prefab.GetComponent<BossAI>();
        if (boss == null)
            return;

        SerializedObject serializedBoss = new SerializedObject(boss);
        if (!string.IsNullOrWhiteSpace(displayName))
            serializedBoss.FindProperty("enemyName").stringValue = displayName;

        serializedBoss.FindProperty("skillProfile").enumValueIndex = (int)profile;
        serializedBoss.FindProperty("minionPrefab").objectReferenceValue = gravok;
        serializedBoss.FindProperty("slimeMinionPrefab").objectReferenceValue = smallSlime;
        serializedBoss.FindProperty("lightningPrefab").objectReferenceValue = lightning;
        serializedBoss.FindProperty("stoneSpikePrefab").objectReferenceValue = stoneSpike;
        serializedBoss.FindProperty("specialCooldown").floatValue = cooldown;
        serializedBoss.FindProperty("gravokSummonCount").intValue = gravokCount;
        serializedBoss.FindProperty("slimeSummonCount").intValue = slimeCount;
        serializedBoss.FindProperty("areaSkillRadius").floatValue = radius;
        serializedBoss.FindProperty("vanishDuration").floatValue = vanishTime;
        serializedBoss.FindProperty("requireHitToAggro").boolValue = false;
        serializedBoss.FindProperty("enablePatrol").boolValue = false;
        serializedBoss.ApplyModifiedPropertiesWithoutUndo();

        EditorUtility.SetDirty(prefab);
    }

    private static List<Sprite> LoadSprites(string assetPath)
    {
        return AssetDatabase.LoadAllAssetsAtPath(assetPath)
            .OfType<Sprite>()
            .OrderBy(sprite => sprite.name)
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

    private static bool GeneratedSetupLooksCurrent()
    {
        return File.Exists(SmallSlimePrefabPath)
               && File.Exists(GoblinBossPrefabPath)
               && File.Exists($"{VfxRoot}/Boss Lightning Strike.prefab")
               && File.Exists($"{VfxRoot}/Boss Stone Spike Strike.prefab")
               && BossHasProfile(PrefabRoot + "/Boss.prefab", BossSkillProfile.SummonGravok)
               && BossHasProfile(PrefabRoot + "/Boss 1.prefab", BossSkillProfile.LightningStorm)
               && BossHasProfile(PrefabRoot + "/Boss Creature.prefab", BossSkillProfile.CriticalBite)
               && BossHasProfile(PrefabRoot + "/King Slime.prefab", BossSkillProfile.SummonSlimes)
               && BossHasProfile(PrefabRoot + "/Mecha Stone Golem.prefab", BossSkillProfile.StoneSpikes)
               && BossHasProfile(PrefabRoot + "/Undead Executioner.prefab", BossSkillProfile.PhaseVanish)
               && BossHasProfile(PrefabRoot + "/Skeleton Boss.prefab", BossSkillProfile.Basic)
               && BossHasProfile(GoblinBossPrefabPath, BossSkillProfile.Basic);
    }

    private static bool BossHasProfile(string prefabPath, BossSkillProfile profile)
    {
        GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);
        if (prefab == null)
            return false;

        BossAI boss = prefab.GetComponent<BossAI>();
        if (boss == null)
            return false;

        SerializedObject serializedBoss = new SerializedObject(boss);
        SerializedProperty property = serializedBoss.FindProperty("skillProfile");
        return property != null && property.enumValueIndex == (int)profile;
    }

    private static void EnsureDirectory(string path)
    {
        if (!Directory.Exists(path))
            Directory.CreateDirectory(path);
    }
}
