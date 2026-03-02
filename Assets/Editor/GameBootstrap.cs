#if UNITY_EDITOR
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public static class GameBootstrap
{
    private const string ScenePath = "Assets/Scenes/Main.unity";

    [MenuItem("Tools/Golge Anahtarlari/Build Scene & Levels")]
    public static void BuildAll()
    {
        EnsureFolders();
        CreateLevelPrefabs();
        CreateMainScene();
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        Debug.Log("Gölge Anahtarları setup tamamlandı.");
    }

    private static void EnsureFolders()
    {
        if (!AssetDatabase.IsValidFolder("Assets/Prefabs")) AssetDatabase.CreateFolder("Assets", "Prefabs");
        if (!AssetDatabase.IsValidFolder("Assets/Prefabs/Levels")) AssetDatabase.CreateFolder("Assets/Prefabs", "Levels");
        if (!AssetDatabase.IsValidFolder("Assets/Scenes")) AssetDatabase.CreateFolder("Assets", "Scenes");
    }

    private static void CreateLevelPrefabs()
    {
        CreateLevel("Tutorial", new Vector3(8f, -1.5f), 1, false, false);
        CreateLevel("WallJump", new Vector3(12f, 4f), 2, true, false);
        CreateLevel("MovingPlatform", new Vector3(14f, 1f), 2, false, true);
        CreateLevel("LightGate", new Vector3(17f, 2f), 3, true, false);
        CreateLevel("Final", new Vector3(20f, 3f), 4, true, true);
    }

    private static void CreateLevel(string name, Vector3 doorPos, int keyCount, bool withWalls, bool withMovingPlatform)
    {
        GameObject root = new GameObject(name + "_Level");

        CreateGround(root.transform, new Vector3(0f, -3.5f), new Vector2(30f, 1.2f));
        CreateKillZone(root.transform, new Vector3(0f, -8f), new Vector2(50f, 2f));
        CreateCheckpoint(root.transform, new Vector3(2f, -2.2f));

        if (withWalls)
        {
            CreateWall(root.transform, new Vector3(6f, -1f), new Vector2(1f, 6f));
            CreateWall(root.transform, new Vector3(10f, 1f), new Vector2(1f, 6f));
        }

        if (withMovingPlatform)
        {
            var moving = CreateShadowPlatform(root.transform, new Vector3(8f, -0.5f), new Vector2(3f, 0.6f));
            moving.name = "MovingPlatform";
            moving.AddComponent<MovingPlatform>();
        }

        CreateShadowPlatform(root.transform, new Vector3(4f, -1.2f), new Vector2(3f, 0.5f));
        CreateShadowPlatform(root.transform, new Vector3(11f, 0.8f), new Vector2(3f, 0.5f));

        for (int i = 0; i < keyCount; i++)
        {
            float x = 3f + i * 3.2f;
            float y = -1.5f + (i % 2 == 0 ? 1.6f : 3.2f);
            CreateKey(root.transform, new Vector3(x, y));
        }

        CreateDoor(root.transform, doorPos);

        string path = $"Assets/Prefabs/Levels/{name}.prefab";
        PrefabUtility.SaveAsPrefabAsset(root, path);
        Object.DestroyImmediate(root);
    }

    private static void CreateMainScene()
    {
        Scene scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

        var mainCam = new GameObject("Main Camera");
        var cam = mainCam.AddComponent<Camera>();
        cam.orthographic = true;
        cam.orthographicSize = 5f;
        mainCam.tag = "MainCamera";
        mainCam.transform.position = new Vector3(0f, 0f, -10f);

        new GameObject("Global Light 2D Placeholder");

        var lm = new GameObject("LevelManager").AddComponent<LevelManager>();
        var levelRoot = new GameObject("LevelRoot").transform;
        var spawn = new GameObject("PlayerSpawn").transform;
        spawn.position = new Vector3(-6f, -2f, 0f);

        var player = CreatePlayer(spawn.position);
        var lamp = CreateLamp(new Vector3(-2f, 0f, 0f));

        CreateUI();

        SerializedObject lmObj = new SerializedObject(lm);
        var prefabsProp = lmObj.FindProperty("levelPrefabs");
        string[] prefabNames = { "Tutorial", "WallJump", "MovingPlatform", "LightGate", "Final" };
        prefabsProp.arraySize = prefabNames.Length;
        for (int i = 0; i < prefabNames.Length; i++)
        {
            var prefab = AssetDatabase.LoadAssetAtPath<GameObject>($"Assets/Prefabs/Levels/{prefabNames[i]}.prefab");
            prefabsProp.GetArrayElementAtIndex(i).objectReferenceValue = prefab;
        }

        lmObj.FindProperty("levelRoot").objectReferenceValue = levelRoot;
        lmObj.FindProperty("player").objectReferenceValue = player.GetComponent<PlayerController>();
        lmObj.FindProperty("playerSpawn").objectReferenceValue = spawn;
        lmObj.ApplyModifiedPropertiesWithoutUndo();

        EditorSceneManager.SaveScene(scene, ScenePath);
    }

    private static GameObject CreatePlayer(Vector3 position)
    {
        var player = GameObject.CreatePrimitive(PrimitiveType.Capsule);
        player.name = "Player";
        player.tag = "Player";
        player.transform.position = position;
        Object.DestroyImmediate(player.GetComponent<CapsuleCollider>());

        var rb = player.AddComponent<Rigidbody2D>();
        rb.freezeRotation = true;

        var col = player.AddComponent<BoxCollider2D>();
        col.size = new Vector2(0.8f, 1.8f);

        var pc = player.AddComponent<PlayerController>();

        var groundCheck = new GameObject("GroundCheck").transform;
        groundCheck.SetParent(player.transform);
        groundCheck.localPosition = new Vector3(0f, -0.95f, 0f);

        var wallCheck = new GameObject("WallCheck").transform;
        wallCheck.SetParent(player.transform);
        wallCheck.localPosition = new Vector3(0.45f, 0f, 0f);

        SerializedObject pcObj = new SerializedObject(pc);
        pcObj.FindProperty("groundCheck").objectReferenceValue = groundCheck;
        pcObj.FindProperty("wallCheck").objectReferenceValue = wallCheck;
        pcObj.FindProperty("groundLayer").intValue = LayerMask.GetMask("Default");
        pcObj.ApplyModifiedPropertiesWithoutUndo();

        return player;
    }

    private static GameObject CreateLamp(Vector3 position)
    {
        var lamp = new GameObject("Lamp");
        lamp.transform.position = position;

        var sr = lamp.AddComponent<SpriteRenderer>();
        sr.color = Color.yellow;

        var col = lamp.AddComponent<CircleCollider2D>();
        col.isTrigger = true;
        col.radius = 1.4f;

        var lr = lamp.AddComponent<LineRenderer>();
        lr.material = new Material(Shader.Find("Sprites/Default"));
        lr.startWidth = 0.08f;
        lr.endWidth = 0.02f;
        lr.startColor = Color.yellow;
        lr.endColor = new Color(1f, 1f, 0.5f, 0.2f);

        lamp.AddComponent<LightController>();
        return lamp;
    }

    private static void CreateUI()
    {
        var canvasObj = new GameObject("Canvas");
        var canvas = canvasObj.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvasObj.AddComponent<CanvasScaler>();
        canvasObj.AddComponent<GraphicRaycaster>();

        var textObj = new GameObject("KeyText");
        textObj.transform.SetParent(canvasObj.transform, false);
        var text = textObj.AddComponent<Text>();
        text.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
        text.fontSize = 30;
        text.alignment = TextAnchor.UpperLeft;
        text.text = "Anahtar: 0";
        text.color = Color.white;

        var rt = text.GetComponent<RectTransform>();
        rt.anchorMin = new Vector2(0f, 1f);
        rt.anchorMax = new Vector2(0f, 1f);
        rt.pivot = new Vector2(0f, 1f);
        rt.anchoredPosition = new Vector2(20f, -20f);
        rt.sizeDelta = new Vector2(400f, 80f);

        var ui = new GameObject("UIController").AddComponent<UIController>();
        SerializedObject uiObj = new SerializedObject(ui);
        uiObj.FindProperty("keyText").objectReferenceValue = text;
        uiObj.ApplyModifiedPropertiesWithoutUndo();
    }

    private static void CreateGround(Transform root, Vector3 pos, Vector2 size)
    {
        var go = new GameObject("Ground");
        go.transform.SetParent(root);
        go.transform.position = pos;
        var sr = go.AddComponent<SpriteRenderer>();
        sr.sprite = Sprite.Create(Texture2D.whiteTexture, new Rect(0, 0, 1, 1), Vector2.one * 0.5f, 1f);
        go.transform.localScale = new Vector3(size.x, size.y, 1f);
        go.AddComponent<BoxCollider2D>();
    }

    private static void CreateWall(Transform root, Vector3 pos, Vector2 size)
    {
        var go = new GameObject("Wall");
        go.transform.SetParent(root);
        go.transform.position = pos;
        var sr = go.AddComponent<SpriteRenderer>();
        sr.sprite = Sprite.Create(Texture2D.whiteTexture, new Rect(0, 0, 1, 1), Vector2.one * 0.5f, 1f);
        go.transform.localScale = new Vector3(size.x, size.y, 1f);
        go.AddComponent<BoxCollider2D>();
    }

    private static GameObject CreateShadowPlatform(Transform root, Vector3 pos, Vector2 size)
    {
        var go = new GameObject("ShadowPlatform");
        go.transform.SetParent(root);
        go.transform.position = pos;
        var sr = go.AddComponent<SpriteRenderer>();
        sr.sprite = Sprite.Create(Texture2D.whiteTexture, new Rect(0, 0, 1, 1), Vector2.one * 0.5f, 1f);
        sr.color = new Color(1f, 1f, 1f, 0.4f);
        go.transform.localScale = new Vector3(size.x, size.y, 1f);
        go.AddComponent<BoxCollider2D>();
        go.AddComponent<ShadowPlatform>();
        return go;
    }

    private static void CreateKey(Transform root, Vector3 pos)
    {
        var go = new GameObject("Key");
        go.transform.SetParent(root);
        go.transform.position = pos;
        var sr = go.AddComponent<SpriteRenderer>();
        sr.sprite = Sprite.Create(Texture2D.whiteTexture, new Rect(0, 0, 1, 1), Vector2.one * 0.5f, 1f);
        sr.color = Color.yellow;
        go.transform.localScale = Vector3.one * 0.4f;
        var col = go.AddComponent<CircleCollider2D>();
        col.isTrigger = true;
        go.AddComponent<Key>();
    }

    private static void CreateDoor(Transform root, Vector3 pos)
    {
        var go = new GameObject("Door");
        go.transform.SetParent(root);
        go.transform.position = pos;
        var sr = go.AddComponent<SpriteRenderer>();
        sr.sprite = Sprite.Create(Texture2D.whiteTexture, new Rect(0, 0, 1, 1), Vector2.one * 0.5f, 1f);
        go.transform.localScale = new Vector3(1.2f, 2.2f, 1f);
        var col = go.AddComponent<BoxCollider2D>();
        col.isTrigger = true;
        go.AddComponent<Door>();
    }

    private static void CreateCheckpoint(Transform root, Vector3 pos)
    {
        var go = new GameObject("Checkpoint");
        go.transform.SetParent(root);
        go.transform.position = pos;
        var sr = go.AddComponent<SpriteRenderer>();
        sr.sprite = Sprite.Create(Texture2D.whiteTexture, new Rect(0, 0, 1, 1), Vector2.one * 0.5f, 1f);
        sr.color = Color.cyan;
        go.transform.localScale = new Vector3(0.5f, 1.6f, 1f);
        var col = go.AddComponent<BoxCollider2D>();
        col.isTrigger = true;
        go.AddComponent<Checkpoint>();
    }

    private static void CreateKillZone(Transform root, Vector3 pos, Vector2 size)
    {
        var go = new GameObject("KillZone");
        go.transform.SetParent(root);
        go.transform.position = pos;
        go.transform.localScale = new Vector3(size.x, size.y, 1f);
        var col = go.AddComponent<BoxCollider2D>();
        col.isTrigger = true;
        go.AddComponent<KillZone>();
    }
}
#endif
