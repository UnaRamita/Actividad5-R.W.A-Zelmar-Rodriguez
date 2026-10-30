using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.UI;

// Construye los assets y la escena del nivel "Rodolfo was alone" usando solo primitivas de Unity.
public static class RodolfoLevelBuilder
{
    private const string ArtPath = "Assets/Art/GUI";
    private const string MaterialsPath = ArtPath + "/Materials";
    private const string PrefabsPath = ArtPath + "/Prefabs";
    private const string TexturesPath = ArtPath + "/Textures";
    private const string ScenePath = "Assets/Scenes/RodolfoWasAlone.unity";

    private static readonly Color BlockColor = Hex("#111111");
    private static readonly Color RodolfoColor = Hex("#E9573F");
    private static readonly Color BackgroundColor = Hex("#5D7E8A");
    private static readonly Color BackgroundShapeColor = Hex("#6C8E9A");

    [MenuItem("Tools/Rodolfo was alone/Construir nivel")]
    public static void Build()
    {
        CreateFolders();
        Texture2D triangle = CreateTriangleTexture();

        Material blockMaterial = CreateMaterial("Block", BlockColor, null);
        Material rodolfoMaterial = CreateMaterial("RodolfoBody", RodolfoColor, null);
        Material indicatorMaterial = CreateMaterial("RodolfoIndicator", Color.white, triangle);
        Material markerMaterial = CreateMaterial("LevelEndMarker", Color.white, null);
        Material backgroundMaterial = CreateMaterial("Background", BackgroundColor, null);
        Material backgroundShapeMaterial = CreateMaterial("BackgroundShape", BackgroundShapeColor, null);
        PhysicsMaterial rodolfoPhysics = CreatePhysicsMaterial();

        GameObject rodolfoPrefab = CreateRodolfoPrefab(rodolfoMaterial, indicatorMaterial, rodolfoPhysics);
        CreateScene(rodolfoPrefab, blockMaterial, markerMaterial, backgroundMaterial, backgroundShapeMaterial);

        AssetDatabase.SaveAssets();
        Debug.Log("[RodolfoLevelBuilder] Nivel construido en " + ScenePath);
    }

    private static void CreateFolders()
    {
        CreateFolder("Assets", "Art");
        CreateFolder("Assets/Art", "GUI");
        CreateFolder(ArtPath, "Materials");
        CreateFolder(ArtPath, "Prefabs");
        CreateFolder(ArtPath, "Textures");
    }

    private static void CreateFolder(string parent, string name)
    {
        if (!AssetDatabase.IsValidFolder(parent + "/" + name))
        {
            AssetDatabase.CreateFolder(parent, name);
        }
    }

    // Triangulo blanco invertido (punta hacia abajo) sobre fondo transparente.
    private static Texture2D CreateTriangleTexture()
    {
        const int size = 128;
        Texture2D texture = new Texture2D(size, size, TextureFormat.RGBA32, false);
        for (int y = 0; y < size; y++)
        {
            // En la fila superior el triangulo ocupa todo el ancho y se angosta hacia abajo.
            float halfWidth = (y + 0.5f) / size * (size / 2.0f);
            for (int x = 0; x < size; x++)
            {
                float distance = Mathf.Abs(x + 0.5f - size / 2.0f);
                float alpha = Mathf.Clamp01(halfWidth - distance + 0.5f);
                texture.SetPixel(x, y, new Color(1, 1, 1, alpha));
            }
        }
        texture.Apply();

        string path = TexturesPath + "/IndicatorTriangle.png";
        System.IO.File.WriteAllBytes(path, texture.EncodeToPNG());
        Object.DestroyImmediate(texture);
        AssetDatabase.ImportAsset(path);

        TextureImporter importer = (TextureImporter)AssetImporter.GetAtPath(path);
        importer.textureType = TextureImporterType.Default;
        importer.alphaIsTransparency = true;
        importer.mipmapEnabled = false;
        importer.wrapMode = TextureWrapMode.Clamp;
        importer.SaveAndReimport();
        return AssetDatabase.LoadAssetAtPath<Texture2D>(path);
    }

    private static Material CreateMaterial(string name, Color color, Texture2D texture)
    {
        string path = MaterialsPath + "/" + name + ".mat";
        Material material = AssetDatabase.LoadAssetAtPath<Material>(path);
        if (material == null)
        {
            material = new Material(Shader.Find("Universal Render Pipeline/Unlit"));
            AssetDatabase.CreateAsset(material, path);
        }
        material.SetColor("_BaseColor", color);

        if (texture != null)
        {
            // Recorte por alfa: solo se dibuja el triangulo, el fondo queda transparente.
            material.SetTexture("_BaseMap", texture);
            material.SetFloat("_AlphaClip", 1);
            material.SetFloat("_Cutoff", 0.5f);
            material.EnableKeyword("_ALPHATEST_ON");
            material.renderQueue = (int)RenderQueue.AlphaTest;
        }
        EditorUtility.SetDirty(material);
        return material;
    }

    // Sin friccion para que Rodolfo no quede "pegado" a las paredes al saltar contra ellas.
    private static PhysicsMaterial CreatePhysicsMaterial()
    {
        string path = MaterialsPath + "/RodolfoPhysics.asset";
        PhysicsMaterial material = AssetDatabase.LoadAssetAtPath<PhysicsMaterial>(path);
        if (material == null)
        {
            material = new PhysicsMaterial("RodolfoPhysics");
            AssetDatabase.CreateAsset(material, path);
        }
        material.dynamicFriction = 0;
        material.staticFriction = 0;
        material.bounciness = 0;
        material.frictionCombine = PhysicsMaterialCombine.Minimum;
        material.bounceCombine = PhysicsMaterialCombine.Minimum;
        EditorUtility.SetDirty(material);
        return material;
    }

    private static GameObject CreateRodolfoPrefab(Material bodyMaterial, Material indicatorMaterial, PhysicsMaterial physics)
    {
        GameObject root = new GameObject("Rodolfo");

        Rigidbody body = root.AddComponent<Rigidbody>();
        body.mass = 1;
        body.useGravity = true;
        body.interpolation = RigidbodyInterpolation.Interpolate;
        body.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;
        body.constraints = RigidbodyConstraints.FreezePositionZ | RigidbodyConstraints.FreezeRotation;

        BoxCollider collider = root.AddComponent<BoxCollider>();
        collider.size = new Vector3(1, 2, 1);
        collider.sharedMaterial = physics;

        root.AddComponent<RodolfoBehaviour>();
        root.AddComponent<RodolfoController>();

        GameObject visualBody = CreatePrimitive(PrimitiveType.Cube, "Body", root.transform, Vector3.zero, new Vector3(1, 2, 1), bodyMaterial);
        visualBody.transform.localPosition = Vector3.zero;

        CreatePrimitive(PrimitiveType.Quad, "Indicator", root.transform, new Vector3(0, 1.5f, 0), new Vector3(0.55f, 0.4f, 1), indicatorMaterial);

        string path = PrefabsPath + "/Rodolfo.prefab";
        GameObject prefab = PrefabUtility.SaveAsPrefabAsset(root, path);
        Object.DestroyImmediate(root);
        return prefab;
    }

    private static void CreateScene(GameObject rodolfoPrefab, Material blockMaterial, Material markerMaterial,
        Material backgroundMaterial, Material backgroundShapeMaterial)
    {
        EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

        // Capa de fondo: color liso y franjas suaves que contrastan con los bloques negros.
        Transform background = new GameObject("Background").transform;
        CreateBlock("BackgroundPlane", background, -20, 112, -10, 30, backgroundMaterial, 10, 1, false);
        CreateBlock("BackgroundShape_01", background, 6, 12, -10, 30, backgroundShapeMaterial, 8, 1, false);
        CreateBlock("BackgroundShape_02", background, 26, 36, -10, 30, backgroundShapeMaterial, 8, 1, false);
        CreateBlock("BackgroundShape_03", background, 52, 56, -10, 30, backgroundShapeMaterial, 8, 1, false);
        CreateBlock("BackgroundShape_04", background, 70, 80, -10, 30, backgroundShapeMaterial, 8, 1, false);

        // Cuerpo del nivel: bloques negros (xMin, xMax, yMin, yMax). El suelo esta en y = 0.
        Transform level = new GameObject("Level").transform;
        CreateBlock("Floor", level, -4, 92, -2, 0, blockMaterial);
        CreateBlock("Wall_Left", level, -4, -2, 0, 14, blockMaterial);
        CreateBlock("Wall_Right", level, 90, 92, 0, 14, blockMaterial);
        CreateBlock("Step_01", level, 4, 6, 0, 1, blockMaterial);
        CreateBlock("Wall_01", level, 9, 11, 0, 2, blockMaterial);
        CreateBlock("Platform_Raised_01", level, 14, 20, 0, 2.5f, blockMaterial);
        CreateBlock("Platform_Floating_01", level, 23, 27, 2.25f, 2.75f, blockMaterial);
        CreateBlock("Platform_Floating_02", level, 30, 34, 4.75f, 5.25f, blockMaterial);
        CreateBlock("Wall_Tall_01", level, 37, 39, 0, 6.5f, blockMaterial);
        CreateBlock("Step_02", level, 43, 45, 0, 1.5f, blockMaterial);
        CreateBlock("Ledge_01", level, 47, 56, 0, 3, blockMaterial);
        CreateBlock("Ledge_02", level, 60, 90, 0, 3, blockMaterial);
        CreateBlock("Platform_Floating_03", level, 66, 69, 5.25f, 5.75f, blockMaterial);
        CreateBlock("Platform_Floating_04", level, 72, 75, 7.75f, 8.25f, blockMaterial);
        CreateBlock("Goal_Tower", level, 78, 90, 3, 8.25f, blockMaterial);

        // Punto de inicio y Rodolfo (instancia del prefab).
        Transform startPoint = new GameObject("StartPoint").transform;
        startPoint.position = new Vector3(0, 1, 0);
        GameObject rodolfo = (GameObject)PrefabUtility.InstantiatePrefab(rodolfoPrefab);
        rodolfo.transform.position = startPoint.position;

        GameObject successMessage = CreateInterface();
        CreateLevelEndMarker(markerMaterial, successMessage, new Vector3(86, 9.25f, 0));
        CreateCamera(rodolfo.transform, level);

        EditorSceneManager.SaveScene(EditorSceneManager.GetActiveScene(), ScenePath);
        EditorBuildSettings.scenes = new[] { new EditorBuildSettingsScene(ScenePath, true) };
    }

    private static void CreateLevelEndMarker(Material markerMaterial, GameObject successMessage, Vector3 position)
    {
        GameObject marker = new GameObject("LevelEndMarker");
        marker.transform.position = position;

        // Area de deteccion con las mismas dimensiones que el cuerpo de Rodolfo.
        BoxCollider area = marker.AddComponent<BoxCollider>();
        area.size = new Vector3(1, 2, 1);
        area.isTrigger = true;

        LevelEndMarker endMarker = marker.AddComponent<LevelEndMarker>();
        endMarker.successMessage = successMessage;

        // Rectangulo blanco apenas detras de la capa de accion, para que Rodolfo quede por delante al llegar.
        CreatePrimitive(PrimitiveType.Cube, "Visual", marker.transform, new Vector3(0, 0, 0.75f), new Vector3(1, 2, 0.5f), markerMaterial);
    }

    private static void CreateCamera(Transform target, Transform level)
    {
        GameObject cameraObject = new GameObject("Main Camera");
        cameraObject.tag = "MainCamera";
        cameraObject.transform.position = new Vector3(0, 4, -10);

        Camera camera = cameraObject.AddComponent<Camera>();
        camera.orthographic = true;
        camera.orthographicSize = 6;
        camera.nearClipPlane = 0.3f;
        camera.farClipPlane = 50;
        camera.clearFlags = CameraClearFlags.SolidColor;
        camera.backgroundColor = BackgroundColor;

        CameraFollow follow = cameraObject.AddComponent<CameraFollow>();
        follow.target = target;
        follow.levelRoot = level;
    }

    private static GameObject CreateInterface()
    {
        Font font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");

        GameObject canvasObject = new GameObject("HUD");
        Canvas canvas = canvasObject.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        CanvasScaler scaler = canvasObject.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);
        scaler.matchWidthOrHeight = 0.5f;

        Text controls = CreateText("ControlsText", canvasObject.transform, font, 30, TextAnchor.UpperLeft,
            "RODOLFO WAS ALONE\n" +
            "Moverse: Flechas  o  A / D        Saltar: Espacio\n" +
            "Llevá a Rodolfo hasta el marcador blanco.");
        RectTransform controlsRect = controls.rectTransform;
        controlsRect.anchorMin = new Vector2(0, 1);
        controlsRect.anchorMax = new Vector2(0, 1);
        controlsRect.pivot = new Vector2(0, 1);
        controlsRect.anchoredPosition = new Vector2(40, -30);
        controlsRect.sizeDelta = new Vector2(1100, 160);

        // Franja en el cuarto inferior con el mensaje de exito (oculta hasta completar el nivel),
        // para no tapar a Rodolfo sobre el marcador.
        GameObject success = new GameObject("SuccessMessage", typeof(RectTransform));
        success.transform.SetParent(canvasObject.transform, false);
        Image band = success.AddComponent<Image>();
        band.color = new Color(0, 0, 0, 0.8f);
        RectTransform bandRect = success.GetComponent<RectTransform>();
        bandRect.anchorMin = new Vector2(0, 0.25f);
        bandRect.anchorMax = new Vector2(1, 0.25f);
        bandRect.sizeDelta = new Vector2(0, 260);

        Text title = CreateText("SuccessTitle", success.transform, font, 84, TextAnchor.MiddleCenter, "¡NIVEL COMPLETADO!");
        title.rectTransform.anchorMin = new Vector2(0, 0.5f);
        title.rectTransform.anchorMax = new Vector2(1, 1);
        title.rectTransform.sizeDelta = Vector2.zero;

        Text subtitle = CreateText("SuccessSubtitle", success.transform, font, 36, TextAnchor.MiddleCenter,
            "Rodolfo llegó al final del recorrido.");
        subtitle.rectTransform.anchorMin = new Vector2(0, 0);
        subtitle.rectTransform.anchorMax = new Vector2(1, 0.5f);
        subtitle.rectTransform.sizeDelta = Vector2.zero;

        success.SetActive(false);
        return success;
    }

    private static Text CreateText(string name, Transform parent, Font font, int size, TextAnchor alignment, string content)
    {
        GameObject textObject = new GameObject(name, typeof(RectTransform));
        textObject.transform.SetParent(parent, false);
        Text text = textObject.AddComponent<Text>();
        text.font = font;
        text.fontSize = size;
        text.alignment = alignment;
        text.color = Color.white;
        text.text = content;
        return text;
    }

    // Bloque rectangular definido por sus limites en X e Y.
    private static GameObject CreateBlock(string name, Transform parent, float xMin, float xMax, float yMin, float yMax,
        Material material, float z = 0, float depth = 1, bool withCollider = true)
    {
        Vector3 position = new Vector3((xMin + xMax) / 2, (yMin + yMax) / 2, z);
        Vector3 scale = new Vector3(xMax - xMin, yMax - yMin, depth);
        GameObject block = CreatePrimitive(PrimitiveType.Cube, name, parent, position, scale, material);
        if (withCollider)
        {
            block.AddComponent<BoxCollider>();
        }
        block.isStatic = true;
        return block;
    }

    // Primitiva sin collider propio (los colliders se agregan solo donde hacen falta).
    private static GameObject CreatePrimitive(PrimitiveType type, string name, Transform parent, Vector3 localPosition,
        Vector3 localScale, Material material)
    {
        GameObject primitive = GameObject.CreatePrimitive(type);
        primitive.name = name;
        Object.DestroyImmediate(primitive.GetComponent<Collider>());
        primitive.transform.SetParent(parent, false);
        primitive.transform.localPosition = localPosition;
        primitive.transform.localScale = localScale;
        Renderer renderer = primitive.GetComponent<Renderer>();
        renderer.sharedMaterial = material;
        renderer.shadowCastingMode = ShadowCastingMode.Off;
        renderer.receiveShadows = false;
        return primitive;
    }

    private static Color Hex(string hex)
    {
        ColorUtility.TryParseHtmlString(hex, out Color color);
        return color;
    }
}
