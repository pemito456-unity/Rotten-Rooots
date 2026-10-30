using UnityEditor;
using UnityEditor.Animations;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

public static class ConfigurePlayerAnimatorFlow
{
    private const string ControllerPath = "Assets/Player_Animator_Flow.controller";
    private const string ClipRoot = "Assets/Animações/Clipes";

    [InitializeOnLoadMethod]
    private static void ConfigureWhenEditorCompiles()
    {
        EditorApplication.delayCall += () =>
        {
            if (AssetDatabase.LoadAssetAtPath<AnimatorController>(ControllerPath) == null)
                Configure();
        };
    }

    [MenuItem("Tools/Rotten Roots/Configurar fluxo do Animator do Player")]
    public static void Configure()
    {
        if (AssetDatabase.LoadAssetAtPath<AnimatorController>(ControllerPath) != null)
        {
            Debug.LogError("Player_Animator_Flow.controller já existe. Renomeie o asset antes de gerar novamente.");
            return;
        }

        AnimatorController controller = AnimatorController.CreateAnimatorControllerAtPath(ControllerPath);
        controller.AddParameter("MoveX", AnimatorControllerParameterType.Float);
        controller.AddParameter("MoveY", AnimatorControllerParameterType.Float);
        controller.AddParameter("IsMoving", AnimatorControllerParameterType.Bool);
        controller.AddParameter("IsDead", AnimatorControllerParameterType.Bool);

        AnimatorStateMachine root = controller.layers[0].stateMachine;
        AnimatorState idle = root.AddState("Idle");
        AnimatorState walking = root.AddState("Walking");
        AnimationClip deathClip = LoadClip("Comum/Death.anim");
        AnimatorState death = root.AddState("Death");
        death.motion = deathClip;
        root.defaultState = idle;

        BlendTree idleTree = CreateDirectionalTree(controller, "Idle_Directional", "Idle");
        BlendTree walkingTree = CreateDirectionalTree(controller, "Walking_Directional", "Walking");
        idle.motion = idleTree;
        walking.motion = walkingTree;

        AnimatorStateTransition startWalking = idle.AddTransition(walking);
        startWalking.hasExitTime = false;
        startWalking.duration = 0.08f;
        startWalking.AddCondition(AnimatorConditionMode.If, 0f, "IsMoving");

        AnimatorStateTransition stopWalking = walking.AddTransition(idle);
        stopWalking.hasExitTime = false;
        stopWalking.duration = 0.08f;
        stopWalking.AddCondition(AnimatorConditionMode.IfNot, 0f, "IsMoving");

        AnimatorStateTransition die = root.AddAnyStateTransition(death);
        die.hasExitTime = false;
        die.duration = 0.05f;
        die.canTransitionToSelf = false;
        die.AddCondition(AnimatorConditionMode.If, 0f, "IsDead");

        AssetDatabase.SaveAssets();

        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player == null)
            player = GameObject.Find("Player");

        if (player == null)
        {
            Debug.LogError("Não encontrei o objeto Player na cena aberta. O controller foi criado, mas não atribuído.");
            return;
        }

        Animator animator = player.GetComponent<Animator>();
        if (animator == null)
            animator = Undo.AddComponent<Animator>(player);

        Undo.RecordObject(animator, "Atribuir Animator do Player");
        animator.runtimeAnimatorController = controller;
        EditorUtility.SetDirty(animator);
        EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene());
        EditorSceneManager.SaveScene(SceneManager.GetActiveScene());

        Debug.Log("Fluxo do Animator configurado: Idle, Walking direcional e Death. Variantes de item serão conectadas depois.");
    }

    private static BlendTree CreateDirectionalTree(AnimatorController controller, string name, string clipPrefix)
    {
        BlendTree tree = new BlendTree
        {
            name = name,
            blendType = BlendTreeType.SimpleDirectional2D,
            blendParameter = "MoveX",
            blendParameterY = "MoveY",
            useAutomaticThresholds = false
        };

        AddDirectionalChild(tree, clipPrefix + "_Frente", new Vector2(0f, -1f));
        AddDirectionalChild(tree, clipPrefix + "_Trás", new Vector2(0f, 1f));
        AddDirectionalChild(tree, clipPrefix + "_Esquerda", new Vector2(-1f, 0f));
        AddDirectionalChild(tree, clipPrefix + "_Direita", new Vector2(1f, 0f));
        AssetDatabase.AddObjectToAsset(tree, controller);
        return tree;
    }

    private static void AddDirectionalChild(BlendTree tree, string clipName, Vector2 direction)
    {
        AnimationClip clip = LoadClip("Normal/" + clipName + ".anim");
        tree.AddChild(clip, direction);
    }

    private static AnimationClip LoadClip(string relativePath)
    {
        string path = ClipRoot + "/" + relativePath;
        AnimationClip clip = AssetDatabase.LoadAssetAtPath<AnimationClip>(path);
        if (clip == null)
            throw new MissingReferenceException("Clip de animação não encontrado: " + path);
        return clip;
    }
}
