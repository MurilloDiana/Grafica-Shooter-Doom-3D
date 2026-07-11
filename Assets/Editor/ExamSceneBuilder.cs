using System.IO;
using UnityEditor;
using UnityEditor.AI;
using UnityEditor.Events;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public static class ExamSceneBuilder
{
    private const string ScenePath = "Assets/Scenes/ParcialShooter3D.unity";

    [MenuItem("Tools/Build Parcial Shooter Scene")]
    public static void Build()
    {
        EnsureFolders();
        Scene scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

        Material floorMat = Material("Floor_Metal", new Color(0.28f, 0.29f, 0.31f));
        Material wallMat = Material("Wall_Concrete", new Color(0.55f, 0.56f, 0.58f));
        Material enemyMat = Material("Enemy_Red", new Color(0.75f, 0.08f, 0.06f));
        Material pickupMat = Material("Pickup_Health", new Color(0.95f, 0.95f, 0.95f));
        Material exitMat = Material("Exit_Green", new Color(0.1f, 0.85f, 0.35f));
        Material weaponMat = Material("Weapon_Dark", new Color(0.08f, 0.08f, 0.09f));

        AudioClip shootClip = SoundOrTone("shootSound.mp3", "Shoot_Beep", 0.07f, 520f);
        AudioClip reloadClip = SoundOrTone("reloadSound.mp3", "Reload_Beep", 0.18f, 220f);
        AudioClip hurtClip = SoundOrTone("hurtSound.mp3", "Hurt_Beep", 0.14f, 110f);
        AudioClip healClip = SoundOrTone("HealSound.mp3", "Heal_Beep", 0.16f, 760f);

        GameObject level = new GameObject("Level");
        CreateMaze(level.transform, floorMat, wallMat);

        GameObject player = CreatePlayer(shootClip, reloadClip, hurtClip, healClip, weaponMat);
        GameObject canvas = CreateCanvas(out Image damageFlash, out Text ammo, out Text enemies, out Text health, out Image healthFill, out Text status, out GameObject gameOver, out GameObject victory);
        PlayerHealth playerHealth = player.GetComponent<PlayerHealth>();
        PlayerWeapon playerWeapon = player.GetComponentInChildren<PlayerWeapon>();
        SetRef(playerHealth, "damageFlash", damageFlash);

        GameObject managerObject = new GameObject("GameManager");
        GameManager manager = managerObject.AddComponent<GameManager>();
        SetRef(manager, "playerHealth", playerHealth);
        SetRef(manager, "playerWeapon", playerWeapon);
        SetRef(manager, "ammoText", ammo);
        SetRef(manager, "enemiesText", enemies);
        SetRef(manager, "healthText", health);
        SetRef(manager, "healthFill", healthFill);
        SetRef(manager, "statusText", status);
        SetRef(manager, "gameOverPanel", gameOver);
        SetRef(manager, "victoryPanel", victory);

        CreateEnemy("Enemy A", new Vector3(18f, 1f, -18f), enemyMat, shootClip);
        CreateEnemy("Enemy B", new Vector3(-18f, 1f, 16f), enemyMat, shootClip);
        CreateEnemy("Enemy C", new Vector3(20f, 1f, 18f), enemyMat, shootClip);
        CreateEnemy("Enemy D", new Vector3(-22f, 1f, -8f), enemyMat, shootClip);
        CreateEnemy("Enemy E", new Vector3(4f, 1f, 6f), enemyMat, shootClip);
        CreateEnemy("Enemy F", new Vector3(23f, 1f, -2f), enemyMat, shootClip);

        CreateHealthPickup(new Vector3(-22f, 0.6f, 3f), pickupMat);
        CreateHealthPickup(new Vector3(17f, 0.6f, 10f), pickupMat);
        CreateExit(new Vector3(0f, 1f, 26.5f), exitMat);
        CreateLights();
        CreateEventSystem();

        UnityEditor.AI.NavMeshBuilder.BuildNavMesh();

        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene, ScenePath);
        EditorBuildSettings.scenes = new[] { new EditorBuildSettingsScene(ScenePath, true) };
        PlayerSettings.productName = "Parcial Practico Shooter 3D";
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
    }

    private static GameObject CreatePlayer(AudioClip shootClip, AudioClip reloadClip, AudioClip hurtClip, AudioClip healClip, Material weaponMat)
    {
        GameObject player = new GameObject("Player");
        player.tag = "Player";
        player.transform.position = new Vector3(0f, 1f, -24f);
        CharacterController controller = player.AddComponent<CharacterController>();
        controller.height = 2f;
        controller.radius = 0.4f;
        controller.center = new Vector3(0f, 1f, 0f);
        AudioSource playerAudio = player.AddComponent<AudioSource>();
        PlayerHealth health = player.AddComponent<PlayerHealth>();
        SetRef(health, "audioSource", playerAudio);
        SetRef(health, "hurtClip", hurtClip);
        SetRef(health, "healClip", healClip);

        GameObject cameraObject = new GameObject("Player Camera");
        cameraObject.transform.SetParent(player.transform);
        cameraObject.transform.localPosition = new Vector3(0f, 1.65f, 0f);
        Camera camera = cameraObject.AddComponent<Camera>();
        camera.fieldOfView = 72f;
        cameraObject.AddComponent<AudioListener>();

        PlayerMotor motor = player.AddComponent<PlayerMotor>();
        SetRef(motor, "cameraPivot", cameraObject.transform);

        GameObject weaponObject = CreateBox("Weapon", new Vector3(0.35f, -0.28f, 0.65f), new Vector3(0.22f, 0.16f, 0.8f), weaponMat, cameraObject.transform, false);
        weaponObject.transform.localRotation = Quaternion.Euler(-5f, 3f, 0f);
        AudioSource weaponAudio = weaponObject.AddComponent<AudioSource>();
        PlayerWeapon weapon = weaponObject.AddComponent<PlayerWeapon>();
        SetInt(weapon, "clipSize", 10);
        SetInt(weapon, "startingReserveAmmo", 40);
        SetRef(weapon, "playerCamera", camera);
        SetRef(weapon, "audioSource", weaponAudio);
        SetRef(weapon, "shootClip", shootClip);
        SetRef(weapon, "reloadClip", reloadClip);

        return player;
    }

    private static void CreateEnemy(string name, Vector3 position, Material material, AudioClip shootClip)
    {
        GameObject enemy = GameObject.CreatePrimitive(PrimitiveType.Capsule);
        enemy.name = name;
        enemy.transform.position = position;
        enemy.transform.localScale = new Vector3(1.1f, 1.1f, 1.1f);
        enemy.GetComponent<Renderer>().sharedMaterial = material;

        NavMeshAgent agent = enemy.AddComponent<NavMeshAgent>();
        agent.speed = 3.2f;
        agent.angularSpeed = 420f;
        agent.stoppingDistance = 12f;

        AudioSource audio = enemy.AddComponent<AudioSource>();
        EnemyController controller = enemy.AddComponent<EnemyController>();
        EnemyHealth health = enemy.AddComponent<EnemyHealth>();
        SetRef(controller, "audioSource", audio);
        SetRef(controller, "shootClip", shootClip);
        SetRef(health, "controller", controller);

        GameObject eye = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        eye.name = "Fire Point";
        eye.transform.SetParent(enemy.transform);
        eye.transform.localPosition = new Vector3(0f, 0.55f, 0.55f);
        eye.transform.localScale = Vector3.one * 0.15f;
        Object.DestroyImmediate(eye.GetComponent<Collider>());
        SetRef(controller, "firePoint", eye.transform);
    }

    private static void CreateHealthPickup(Vector3 position, Material material)
    {
        GameObject root = new GameObject("Botiquin");
        root.transform.position = position;
        BoxCollider trigger = root.AddComponent<BoxCollider>();
        trigger.isTrigger = true;
        trigger.size = new Vector3(1.5f, 1.2f, 1.5f);
        root.AddComponent<HealthPickup>();

        GameObject box = CreateBox("White Box", Vector3.zero, new Vector3(1.1f, 0.28f, 1.1f), material, root.transform, false);
        box.transform.localPosition = Vector3.zero;
        Material red = Material("Pickup_Cross_Red", new Color(0.85f, 0.05f, 0.05f));
        CreateBox("Cross Horizontal", new Vector3(0f, 0.18f, 0f), new Vector3(0.8f, 0.08f, 0.18f), red, root.transform, false);
        CreateBox("Cross Vertical", new Vector3(0f, 0.19f, 0f), new Vector3(0.18f, 0.08f, 0.8f), red, root.transform, false);
    }

    private static void CreateMaze(Transform level, Material floorMat, Material wallMat)
    {
        CreateBox("Floor", new Vector3(0f, -0.05f, 0f), new Vector3(58f, 0.1f, 58f), floorMat, level, true);
        CreateBox("North Wall", new Vector3(0f, 1.5f, 29f), new Vector3(58f, 3f, 1f), wallMat, level, true);
        CreateBox("South Wall", new Vector3(0f, 1.5f, -29f), new Vector3(58f, 3f, 1f), wallMat, level, true);
        CreateBox("East Wall", new Vector3(29f, 1.5f, 0f), new Vector3(1f, 3f, 58f), wallMat, level, true);
        CreateBox("West Wall", new Vector3(-29f, 1.5f, 0f), new Vector3(1f, 3f, 58f), wallMat, level, true);

        CreateBox("Maze Wall A", new Vector3(-18f, 1.25f, -14f), new Vector3(2f, 2.5f, 20f), wallMat, level, true);
        CreateBox("Maze Wall B", new Vector3(-8f, 1.25f, -20f), new Vector3(18f, 2.5f, 2f), wallMat, level, true);
        CreateBox("Maze Wall C", new Vector3(8f, 1.25f, -12f), new Vector3(2f, 2.5f, 18f), wallMat, level, true);
        CreateBox("Maze Wall D", new Vector3(19f, 1.25f, -18f), new Vector3(16f, 2.5f, 2f), wallMat, level, true);
        CreateBox("Maze Wall E", new Vector3(18f, 1.25f, -1f), new Vector3(2f, 2.5f, 18f), wallMat, level, true);
        CreateBox("Maze Wall F", new Vector3(5f, 1.25f, 8f), new Vector3(20f, 2.5f, 2f), wallMat, level, true);
        CreateBox("Maze Wall G", new Vector3(-14f, 1.25f, 8f), new Vector3(2f, 2.5f, 20f), wallMat, level, true);
        CreateBox("Maze Wall H", new Vector3(-4f, 1.25f, 18f), new Vector3(20f, 2.5f, 2f), wallMat, level, true);
        CreateBox("Maze Wall I", new Vector3(18f, 1.25f, 18f), new Vector3(2f, 2.5f, 14f), wallMat, level, true);
        CreateBox("Maze Wall J", new Vector3(-23f, 1.25f, -1f), new Vector3(10f, 2.5f, 2f), wallMat, level, true);
        CreateBox("Maze Wall K", new Vector3(0f, 1.25f, -2f), new Vector3(12f, 2.5f, 2f), wallMat, level, true);
        CreateBox("Maze Wall L", new Vector3(24f, 1.25f, 8f), new Vector3(9f, 2.5f, 2f), wallMat, level, true);

        CreateBox("Cover Block 1", new Vector3(-24f, 0.8f, -22f), new Vector3(4f, 1.6f, 2f), wallMat, level, true);
        CreateBox("Cover Block 2", new Vector3(23f, 0.8f, 22f), new Vector3(4f, 1.6f, 2f), wallMat, level, true);
        CreateBox("Cover Block 3", new Vector3(-7f, 0.8f, 3f), new Vector3(3f, 1.6f, 3f), wallMat, level, true);
        CreateBox("Cover Block 4", new Vector3(11f, 0.8f, 23f), new Vector3(5f, 1.6f, 2f), wallMat, level, true);
    }

    private static void CreateExit(Vector3 position, Material material)
    {
        GameObject exit = CreateBox("Exit Zone - Meta", position, new Vector3(4f, 2f, 0.25f), material, null, false);
        BoxCollider collider = exit.GetComponent<BoxCollider>();
        collider.isTrigger = true;
        exit.AddComponent<ExitZone>();
    }

    private static GameObject CreateCanvas(out Image damageFlash, out Text ammo, out Text enemies, out Text health, out Image healthFill, out Text status, out GameObject gameOver, out GameObject victory)
    {
        GameObject canvasObject = new GameObject("HUD Canvas");
        Canvas canvas = canvasObject.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvasObject.AddComponent<CanvasScaler>().uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        canvasObject.AddComponent<GraphicRaycaster>();

        ammo = Text("Ammo Text", canvasObject.transform, new Vector2(20f, -70f), TextAnchor.UpperLeft, 20);
        ammo.rectTransform.sizeDelta = new Vector2(900f, 70f);
        enemies = Text("Enemies Text", canvasObject.transform, new Vector2(20f, -108f), TextAnchor.UpperLeft, 22);
        health = Text("Health Text", canvasObject.transform, new Vector2(20f, -145f), TextAnchor.UpperLeft, 22);
        healthFill = HealthBar(canvasObject.transform);
        status = Text("Status Text", canvasObject.transform, new Vector2(0f, -58f), TextAnchor.UpperCenter, 26);
        status.text = "Elimina enemigos y llega a la meta";

        damageFlash = Image("Damage Flash", canvasObject.transform, Color.clear);
        RectTransform flashRect = damageFlash.rectTransform;
        flashRect.anchorMin = Vector2.zero;
        flashRect.anchorMax = Vector2.one;
        flashRect.offsetMin = Vector2.zero;
        flashRect.offsetMax = Vector2.zero;
        damageFlash.raycastTarget = false;

        Text crosshair = Text("Crosshair", canvasObject.transform, Vector2.zero, TextAnchor.MiddleCenter, 28);
        crosshair.text = "+";

        gameOver = Panel("Game Over Panel", canvasObject.transform, "GAME OVER", "Reintentar");
        victory = Panel("Victory Panel", canvasObject.transform, "VICTORIA", "Jugar otra vez");
        Button retryA = gameOver.GetComponentInChildren<Button>(true);
        Button retryB = victory.GetComponentInChildren<Button>(true);
        UnityActionAdd(retryA);
        UnityActionAdd(retryB);
        gameOver.SetActive(false);
        victory.SetActive(false);

        return canvasObject;
    }

    private static void UnityActionAdd(Button button)
    {
        RestartButton restartButton = button.gameObject.AddComponent<RestartButton>();
        UnityEventTools.AddPersistentListener(button.onClick, restartButton.Restart);
    }

    private static GameObject Panel(string name, Transform parent, string title, string buttonText)
    {
        Image panel = Image(name, parent, new Color(0f, 0f, 0f, 0.78f));
        RectTransform rect = panel.rectTransform;
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.offsetMin = Vector2.zero;
        rect.offsetMax = Vector2.zero;

        Text label = Text("Title", panel.transform, new Vector2(0f, 60f), TextAnchor.MiddleCenter, 46);
        label.text = title;

        GameObject buttonObject = new GameObject("Retry Button");
        buttonObject.transform.SetParent(panel.transform, false);
        Image buttonImage = buttonObject.AddComponent<Image>();
        buttonImage.color = new Color(0.08f, 0.38f, 0.52f, 1f);
        Button button = buttonObject.AddComponent<Button>();
        RectTransform buttonRect = buttonObject.GetComponent<RectTransform>();
        buttonRect.anchorMin = new Vector2(0.5f, 0.5f);
        buttonRect.anchorMax = new Vector2(0.5f, 0.5f);
        buttonRect.anchoredPosition = new Vector2(0f, -35f);
        buttonRect.sizeDelta = new Vector2(210f, 54f);

        Text text = Text("Button Text", buttonObject.transform, Vector2.zero, TextAnchor.MiddleCenter, 24);
        text.text = buttonText;
        return panel.gameObject;
    }

    private static Text Text(string name, Transform parent, Vector2 anchoredPosition, TextAnchor anchor, int size)
    {
        GameObject go = new GameObject(name);
        go.transform.SetParent(parent, false);
        Text text = go.AddComponent<Text>();
        text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        text.fontSize = size;
        text.color = Color.white;
        text.alignment = anchor;
        text.text = name;

        RectTransform rect = text.rectTransform;
        rect.anchorMin = Anchor(anchor);
        rect.anchorMax = Anchor(anchor);
        rect.anchoredPosition = anchoredPosition;
        rect.sizeDelta = new Vector2(520f, 70f);
        return text;
    }

    private static Image Image(string name, Transform parent, Color color)
    {
        GameObject go = new GameObject(name);
        go.transform.SetParent(parent, false);
        Image image = go.AddComponent<Image>();
        image.color = color;
        return image;
    }

    private static Image HealthBar(Transform parent)
    {
        Image background = Image("Health Bar Background", parent, new Color(0.08f, 0.08f, 0.08f, 0.85f));
        RectTransform backgroundRect = background.rectTransform;
        backgroundRect.anchorMin = new Vector2(0f, 1f);
        backgroundRect.anchorMax = new Vector2(0f, 1f);
        backgroundRect.pivot = new Vector2(0f, 1f);
        backgroundRect.anchoredPosition = new Vector2(20f, -178f);
        backgroundRect.sizeDelta = new Vector2(250f, 22f);

        Image fill = Image("Health Bar Fill", background.transform, new Color(0.1f, 0.85f, 0.35f, 1f));
        fill.type = UnityEngine.UI.Image.Type.Filled;
        fill.fillMethod = UnityEngine.UI.Image.FillMethod.Horizontal;
        fill.fillOrigin = (int)UnityEngine.UI.Image.OriginHorizontal.Left;
        fill.fillAmount = 1f;

        RectTransform fillRect = fill.rectTransform;
        fillRect.anchorMin = Vector2.zero;
        fillRect.anchorMax = Vector2.one;
        fillRect.offsetMin = new Vector2(2f, 2f);
        fillRect.offsetMax = new Vector2(-2f, -2f);
        return fill;
    }

    private static Vector2 Anchor(TextAnchor anchor)
    {
        switch (anchor)
        {
            case TextAnchor.UpperLeft: return new Vector2(0f, 1f);
            case TextAnchor.UpperCenter: return new Vector2(0.5f, 1f);
            case TextAnchor.MiddleCenter: return new Vector2(0.5f, 0.5f);
            default: return new Vector2(0.5f, 0.5f);
        }
    }

    private static GameObject CreateBox(string name, Vector3 position, Vector3 scale, Material material, Transform parent, bool navigationStatic)
    {
        GameObject go = GameObject.CreatePrimitive(PrimitiveType.Cube);
        go.name = name;
        go.transform.SetParent(parent, false);
        go.transform.localPosition = position;
        go.transform.localScale = scale;
        if (material != null) go.GetComponent<Renderer>().sharedMaterial = material;
        if (navigationStatic) GameObjectUtility.SetStaticEditorFlags(go, StaticEditorFlags.NavigationStatic);
        return go;
    }

    private static void CreateLights()
    {
        GameObject sun = new GameObject("Directional Light");
        Light light = sun.AddComponent<Light>();
        light.type = LightType.Directional;
        light.intensity = 1.2f;
        sun.transform.rotation = Quaternion.Euler(50f, -30f, 0f);

        RenderSettings.ambientLight = new Color(0.35f, 0.36f, 0.38f);
    }

    private static void CreateEventSystem()
    {
        GameObject eventSystem = new GameObject("EventSystem");
        eventSystem.AddComponent<EventSystem>();
        eventSystem.AddComponent<StandaloneInputModule>();
    }

    private static Material Material(string name, Color color)
    {
        string path = $"Assets/Materials/{name}.mat";
        Material material = AssetDatabase.LoadAssetAtPath<Material>(path);
        if (material == null)
        {
            material = new Material(Shader.Find("Universal Render Pipeline/Lit") ?? Shader.Find("Standard"));
            AssetDatabase.CreateAsset(material, path);
        }

        material.color = color;
        EditorUtility.SetDirty(material);
        return material;
    }

    private static AudioClip Tone(string name, float duration, float frequency)
    {
        string path = $"Assets/Sounds/{name}.asset";
        AudioClip existing = AssetDatabase.LoadAssetAtPath<AudioClip>(path);
        if (existing != null) return existing;

        int sampleRate = 44100;
        int length = Mathf.CeilToInt(duration * sampleRate);
        float[] data = new float[length];
        for (int i = 0; i < length; i++)
        {
            float t = i / (float)sampleRate;
            float envelope = 1f - (i / (float)length);
            data[i] = Mathf.Sin(2f * Mathf.PI * frequency * t) * 0.25f * envelope;
        }

        AudioClip clip = AudioClip.Create(name, length, 1, sampleRate, false);
        clip.SetData(data, 0);
        AssetDatabase.CreateAsset(clip, path);
        return clip;
    }

    private static AudioClip SoundOrTone(string fileName, string fallbackName, float duration, float frequency)
    {
        string path = $"Assets/Sounds/{fileName}";
        AudioClip clip = AssetDatabase.LoadAssetAtPath<AudioClip>(path);
        if (clip != null) return clip;

        if (File.Exists(path))
        {
            AssetDatabase.ImportAsset(path, ImportAssetOptions.ForceSynchronousImport);
            clip = AssetDatabase.LoadAssetAtPath<AudioClip>(path);
            if (clip != null) return clip;
        }

        return Tone(fallbackName, duration, frequency);
    }

    private static void SetRef(Object target, string propertyName, Object value)
    {
        SerializedObject so = new SerializedObject(target);
        SerializedProperty prop = so.FindProperty(propertyName);
        prop.objectReferenceValue = value;
        so.ApplyModifiedPropertiesWithoutUndo();
        EditorUtility.SetDirty(target);
    }

    private static void SetInt(Object target, string propertyName, int value)
    {
        SerializedObject so = new SerializedObject(target);
        SerializedProperty prop = so.FindProperty(propertyName);
        prop.intValue = value;
        so.ApplyModifiedPropertiesWithoutUndo();
        EditorUtility.SetDirty(target);
    }

    private static void EnsureFolders()
    {
        Directory.CreateDirectory("Assets/Scenes");
        Directory.CreateDirectory("Assets/Materials");
        Directory.CreateDirectory("Assets/Sounds");
    }
}
