using UnityEngine;
using UnityEngine.UI;

public class FinalSplitScreenController : MonoBehaviour
{
    [Header("--- LINKOWANIE ---")]
    public Transform player1;
    public Transform player2;
    public Camera cam1;
    public Camera cam2;

    [Header("--- ZOOM (ZBLIŻENIE) ---")]
    [Tooltip("Zoom, gdy gracze są RAZEM. Kamera patrzy na środek między nimi.")]
    [Range(2f, 40f)] public float mergedZoom = 15.0f; // Większa wartość, żeby widzieć obu

    [Tooltip("Zoom, gdy gracze są OSOBNO. Kamera skupia się na jednym graczu.")]
    [Range(2f, 40f)] public float splitZoom = 8.0f;

    [Header("--- KONFIGURACJA PRZEJŚĆ ---")]
    [Tooltip("Szybkość przejścia między trybami (Zoom, Pozycja, Alpha).")]
    public float transitionSpeed = 2.0f;

    [Tooltip("Dystans, przy którym ekran pęka na pół.")]
    public float splitDistance = 10.0f; // Zwiększ to, jeśli chcesz dłużej widzieć wspólny ekran

    [Tooltip("Rozsuwanie kamer przy podziale (żeby nie wchodzić w linię).")]
    public float splitSeparation = 3.0f;

    [Header("--- WYGLĄD ---")]
    public float lineWidth = 15.0f;
    public bool invertRotation = false;

    // --- ZMIENNE WEWNĘTRZNE ---
    private RenderTexture rt1, rt2;
    private CanvasGroup splitCanvasGroup;      
    private CanvasGroup backgroundCanvasGroup; 

    private RectTransform rotatorPivot;
    private RectTransform mask1, mask2;
    private RectTransform content1, content2;
    
    private Vector3 velCam1, velCam2;
    private float currentSep = 0f;
    private float sepVel;
    
    private float currentZoom;
    private float zoomVel;

    void Start()
    {
        // 1. SETUP KAMER
        if (cam1.transform.parent != null) cam1.transform.parent = null;
        if (cam2.transform.parent != null) cam2.transform.parent = null;
        
        cam1.transform.localScale = Vector3.one;
        cam2.transform.localScale = Vector3.one;
        cam1.orthographic = true;
        cam2.orthographic = true;
        
        currentZoom = mergedZoom;

        // Wyłącz inne kamery
        foreach (Camera c in FindObjectsOfType<Camera>())
        {
            if (c != cam1 && c != cam2 && c.GetComponent<FinalSplitScreenController>() == null)
                c.enabled = false;
        }

        SetupRenderTextures();
        CreateOutputCamera();
        BuildNewUI();
    }

    void LateUpdate()
    {
        if (!player1 || !player2) return;

        Vector3 p1 = player1.position;
        Vector3 p2 = player2.position;
        Vector3 delta = p2 - p1;
        float dist = delta.magnitude;
        Vector3 dir = delta.normalized;
        Vector3 midpoint = (p1 + p2) / 2.0f; // Punkt środkowy

        // Decyzja: Split czy Merge?
        bool isSplit = dist > splitDistance;

        // --- A. OBSŁUGA ZOOMU ---
        float targetZoom = isSplit ? splitZoom : mergedZoom;
        currentZoom = Mathf.SmoothDamp(currentZoom, targetZoom, ref zoomVel, 0.5f / transitionSpeed);

        cam1.orthographicSize = currentZoom;
        cam2.orthographicSize = currentZoom;


        // --- B. POZYCJONOWANIE KAMER (TU JEST ZMIANA) ---
        
        // Oblicz separację tylko dla trybu SPLIT
        float targetSep = isSplit ? splitSeparation : 0f;
        currentSep = Mathf.SmoothDamp(currentSep, targetSep, ref sepVel, 0.5f / transitionSpeed);

        Vector3 targetPos1, targetPos2;

        if (isSplit)
        {
            // TRYB SPLIT: Każda kamera śledzi swojego gracza + separacja
            targetPos1 = p1 + (dir * currentSep);
            targetPos2 = p2 - (dir * currentSep);
        }
        else
        {
            // TRYB MERGED: Obie kamery patrzą na WSPÓLNY ŚRODEK
            targetPos1 = midpoint;
            targetPos2 = midpoint;
        }

        // Dodajemy Z=-10
        targetPos1.z = -10;
        targetPos2.z = -10;

        // Aplikujemy ruch (SmoothDamp zapewnia płynne przejście między trybami)
        cam1.transform.position = Vector3.SmoothDamp(cam1.transform.position, targetPos1, ref velCam1, 0.2f);
        cam2.transform.position = Vector3.SmoothDamp(cam2.transform.position, targetPos2, ref velCam2, 0.2f);


        // --- C. UI I PRZEZROCZYSTOŚĆ ---
        float angleRad = Mathf.Atan2(delta.y, delta.x);
        float angleDeg = angleRad * Mathf.Rad2Deg;
        if (invertRotation) angleDeg *= -1;

        float lerpSpeed = Time.deltaTime * transitionSpeed * 2f;

        if (isSplit)
        {
            // Wchodzimy w Split
            splitCanvasGroup.alpha = Mathf.Lerp(splitCanvasGroup.alpha, 1, lerpSpeed);
            backgroundCanvasGroup.alpha = Mathf.Lerp(backgroundCanvasGroup.alpha, 0, lerpSpeed);
            
            rotatorPivot.localRotation = Quaternion.Euler(0, 0, angleDeg);
            content1.localRotation = Quaternion.Euler(0, 0, -angleDeg);
            content2.localRotation = Quaternion.Euler(0, 0, -angleDeg);
        }
        else
        {
            // Wchodzimy w Merge
            splitCanvasGroup.alpha = Mathf.Lerp(splitCanvasGroup.alpha, 0, lerpSpeed);
            backgroundCanvasGroup.alpha = Mathf.Lerp(backgroundCanvasGroup.alpha, 1, lerpSpeed);
            
            rotatorPivot.localRotation = Quaternion.Lerp(rotatorPivot.localRotation, Quaternion.identity, lerpSpeed);
            content1.localRotation = Quaternion.Lerp(content1.localRotation, Quaternion.identity, lerpSpeed);
            content2.localRotation = Quaternion.Lerp(content2.localRotation, Quaternion.identity, lerpSpeed);
        }
    }

    // ========================================================================
    // BUDOWA UI
    // ========================================================================
    void BuildNewUI()
    {
        GameObject goCanvas = new GameObject("Split_UI_Canvas");
        Canvas cv = goCanvas.AddComponent<Canvas>();
        cv.renderMode = RenderMode.ScreenSpaceOverlay;
        goCanvas.AddComponent<CanvasScaler>();

        // TŁO
        GameObject bgObj = CreateRawImage("Background_Merged", rt1, goCanvas.transform);
        Stretch(bgObj.GetComponent<RectTransform>());
        backgroundCanvasGroup = bgObj.AddComponent<CanvasGroup>(); 
        backgroundCanvasGroup.alpha = 1;

        // SPLIT
        GameObject splitRoot = new GameObject("Split_Container");
        splitRoot.transform.SetParent(goCanvas.transform, false);
        Stretch(splitRoot.AddComponent<RectTransform>());
        splitCanvasGroup = splitRoot.AddComponent<CanvasGroup>();
        splitCanvasGroup.alpha = 0;

        // ROTATOR
        GameObject rotator = new GameObject("Rotator_Pivot");
        rotator.transform.SetParent(splitRoot.transform, false);
        rotatorPivot = rotator.AddComponent<RectTransform>();
        rotatorPivot.anchorMin = new Vector2(0.5f, 0.5f);
        rotatorPivot.anchorMax = new Vector2(0.5f, 0.5f);
        rotatorPivot.sizeDelta = new Vector2(Screen.width * 3, Screen.height * 3);

        // LEWA
        mask1 = CreateMask("Mask_Left", rotator.transform, true);
        GameObject c1 = CreateRawImage("Content_P1", rt1, mask1.transform);
        content1 = c1.GetComponent<RectTransform>();
        SetupContentRect(content1, true);

        // PRAWA
        mask2 = CreateMask("Mask_Right", rotator.transform, false);
        GameObject c2 = CreateRawImage("Content_P2", rt2, mask2.transform);
        content2 = c2.GetComponent<RectTransform>();
        SetupContentRect(content2, false);

        // LINIA
        GameObject line = new GameObject("Separator_Line");
        line.transform.SetParent(rotator.transform, false);
        Image li = line.AddComponent<Image>();
        li.color = Color.black;
        RectTransform lineRect = line.GetComponent<RectTransform>();
        lineRect.anchorMin = new Vector2(0.5f, 0);
        lineRect.anchorMax = new Vector2(0.5f, 1);
        lineRect.sizeDelta = new Vector2(lineWidth, 0);
    }

    RectTransform CreateMask(string name, Transform parent, bool isLeft)
    {
        GameObject go = new GameObject(name);
        go.transform.SetParent(parent, false);
        RectTransform rt = go.AddComponent<RectTransform>();
        rt.anchorMin = new Vector2(0, 0);
        rt.anchorMax = new Vector2(1, 1);
        rt.pivot = new Vector2(0.5f, 0.5f);
        Image img = go.AddComponent<Image>();
        img.color = Color.white;
        go.AddComponent<Mask>().showMaskGraphic = false;

        if (isLeft) rt.anchorMax = new Vector2(0.5f, 1);
        else rt.anchorMin = new Vector2(0.5f, 0);
        
        rt.offsetMin = Vector2.zero;
        rt.offsetMax = Vector2.zero;
        return rt;
    }

    void SetupContentRect(RectTransform content, bool isLeftMask)
    {
        content.anchorMin = new Vector2(0.5f, 0.5f);
        content.anchorMax = new Vector2(0.5f, 0.5f);
        content.pivot = new Vector2(0.5f, 0.5f);
        content.sizeDelta = rotatorPivot.sizeDelta;
        
        float shift = rotatorPivot.sizeDelta.x * 0.25f;
        if (isLeftMask) content.anchoredPosition = new Vector2(shift, 0);
        else content.anchoredPosition = new Vector2(-shift, 0);
    }

    void SetupRenderTextures()
    {
        if(rt1) rt1.Release();
        if(rt2) rt2.Release();
        rt1 = new RenderTexture(Screen.width, Screen.height, 24);
        rt2 = new RenderTexture(Screen.width, Screen.height, 24);
        rt1.antiAliasing = 2; rt2.antiAliasing = 2;
        cam1.targetTexture = rt1;
        cam2.targetTexture = rt2;
    }

    void CreateOutputCamera()
    {
        GameObject outCam = new GameObject("Output_Cam_Final");
        Camera c = outCam.AddComponent<Camera>();
        c.cullingMask = 0;
        c.clearFlags = CameraClearFlags.SolidColor;
        c.backgroundColor = Color.black;
        c.orthographic = true;
        c.depth = 100;
        if(outCam.GetComponent<AudioListener>()) Destroy(outCam.GetComponent<AudioListener>());
    }

    GameObject CreateRawImage(string name, Texture t, Transform p)
    {
        GameObject g = new GameObject(name);
        g.transform.SetParent(p, false);
        RawImage ri = g.AddComponent<RawImage>();
        ri.texture = t;
        return g;
    }

    void Stretch(RectTransform rt)
    {
        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.one;
        rt.offsetMin = Vector2.zero;
        rt.offsetMax = Vector2.zero;
    }
}