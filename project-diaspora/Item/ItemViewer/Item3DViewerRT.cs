using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Item3DViewerRT : MonoBehaviour
{
    public static Item3DViewerRT Instance { get; private set; }

    [Header("Layer Filtering")]
    [SerializeField] string itemLayerName = "ItemViewer"; 
    int _itemLayer = -1;

    [Header("Viewer Camera (RenderTexture)")]
    [SerializeField] float camFov = 35f;
    [SerializeField] float nearClip = 0.01f;
    [SerializeField] float farClip = 50f;
    [SerializeField] bool transparentBackground = true;         
    [SerializeField] Color rtClearColor = new Color(0, 0, 0, 70);    

    [Header("Fit")]
    [SerializeField] float targetScreenHeightFraction = 0.5f;    
    [SerializeField] float minDistance = 0.4f;
    [SerializeField] float maxDistance = 3.0f;
    [SerializeField] float margin = 0.10f;                     

    [Header("Rotation")]
    [SerializeField] Vector2 dragSensitivity = new Vector2(0.3f, 0.3f);
    [SerializeField] float dragClickThreshold = 6f;         

    [Header("UI")]
    [SerializeField] int overlaySortingOrder = 50000;          
    [SerializeField] bool hideAllInGameUI = true;                
    [SerializeField] bool viewerFullscreen = false;              
    [SerializeField] Vector2 insetMin = new Vector2(0.15f, 0.15f);
    [SerializeField] Vector2 insetMax = new Vector2(0.85f, 0.85f);

    [Header("Player Lock (FirstPersonController)")]
    [SerializeField] bool disablePlayerLookWhenViewing = true;   
    [SerializeField] bool disablePlayerMoveWhenViewing = false;

    [Header("Pause Settings (시간만 정지)")]
    [SerializeField] bool pauseGameWhenViewing = true;
    [SerializeField] bool pauseAudioListener = false;

    // 저장 슬롯
    float _prevTimeScale = 1f;
    bool _pausedByViewer = false;

    [SerializeField] bool isolatePreview = true;                
    [System.Serializable] struct SavedRB { public Rigidbody rb; public bool wasKinematic; public bool usedGravity; public RigidbodyConstraints constraints; }
    [System.Serializable] struct SavedAnim { public Animator anim; public bool wasEnabled; public bool applyRootMotion; }
    [System.Serializable] struct SavedCol { public Collider col; public bool wasEnabled; }
    [System.Serializable] struct SavedBeh { public Behaviour beh; public bool wasEnabled; }
    readonly List<SavedRB> _savedRBs = new();
    readonly List<SavedAnim> _savedAnims = new();
    readonly List<SavedCol> _savedCols = new();
    readonly List<SavedBeh> _savedBehs = new();
    int _previewLayer = -1; 

  
    Camera viewerCam;             
    RenderTexture rt;
    Canvas viewerCanvas;           
    RawImage viewerImage;          
    RectTransform viewerImageRT;
    Transform pivot;             
    Transform modelRoot;           
    GameObject currentModel;
    Light dirLight;

    bool isActive;
    bool dragging;
    bool armed; 
    Vector3 pressPos;
    Vector3 lastMousePos;
    Vector2 accumulatedEuler;

    readonly List<GameObject> disabledUIs = new();

    FirstPersonController _fpc;
    bool _prevCameraCanMove;
    bool _prevPlayerCanMove;
    bool _prevLockCursor;

    void OnEnable()
    {
        UIExclusiveManager.CloseViewerRequested += ForceCloseByManager;
    }

    void OnDisable()
    {
        UIExclusiveManager.CloseViewerRequested -= ForceCloseByManager;
    }

    private void ForceCloseByManager()
    {
        if (isActive) Hide();
    }


    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);
        var camGO = new GameObject("[RTViewerCamera]");

        viewerCam = camGO.AddComponent<Camera>();
        viewerCam.fieldOfView = camFov;
        viewerCam.nearClipPlane = nearClip;
        viewerCam.farClipPlane = farClip;
        viewerCam.transform.position = Vector3.zero;
        viewerCam.transform.rotation = Quaternion.identity;
        viewerCam.clearFlags = CameraClearFlags.SolidColor;
        viewerCam.backgroundColor = transparentBackground ? rtClearColor : new Color(0, 0, 0, 1);
        viewerCam.allowHDR = false;
        viewerCam.allowMSAA = false;
        viewerCam.enabled = false;

        DontDestroyOnLoad(camGO);
        var pivotGO = new GameObject("[RTViewerPivot]");
        pivot = pivotGO.transform;
        pivot.SetParent(viewerCam.transform, false);
        pivot.localPosition = Vector3.forward * 1f; 
        pivot.localRotation = Quaternion.identity;
        DontDestroyOnLoad(pivotGO);

        var rootGO = new GameObject("ModelRoot");
        modelRoot = rootGO.transform;
        modelRoot.SetParent(pivot, false);

        DontDestroyOnLoad(rootGO);
        CreateViewerCanvas();
        SetViewerCanvasActive(false);

        _itemLayer = LayerMask.NameToLayer(itemLayerName);
        if (_itemLayer == -1)
        {
            Debug.LogError($"[Item3DViewerRT] Layer '{itemLayerName}'를 Project Settings > Tags and Layers 에 먼저 만들어 주세요.");
           
            viewerCam.cullingMask = 0;
        }
        else
        {
           
            viewerCam.cullingMask = 1 << _itemLayer;
        }

    }

    void CreateViewerCanvas()
    {
        var go = new GameObject("[RTViewerCanvas]");
        viewerCanvas = go.AddComponent<Canvas>();
        viewerCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
        viewerCanvas.sortingOrder = overlaySortingOrder;
        DontDestroyOnLoad(go);

        var scaler = go.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);

        go.AddComponent<UnityEngine.UI.GraphicRaycaster>();

        var raw = new GameObject("RTImage");
        raw.transform.SetParent(go.transform, false);
        viewerImage = raw.AddComponent<RawImage>();
        viewerImage.color = Color.white;          
        viewerImage.raycastTarget = true;
        viewerImageRT = raw.GetComponent<RectTransform>();

        if (viewerFullscreen)
        {
            viewerImageRT.anchorMin = Vector2.zero;
            viewerImageRT.anchorMax = Vector2.one;
        }
        else
        {
            viewerImageRT.anchorMin = insetMin;
            viewerImageRT.anchorMax = insetMax;
        }
        viewerImageRT.offsetMin = Vector2.zero;
        viewerImageRT.offsetMax = Vector2.zero;
    }

    void SetViewerCanvasActive(bool on)
    {
        if (viewerCanvas != null) viewerCanvas.enabled = on;
        if (viewerImage != null) viewerImage.enabled = on;
    }

    public void Show(GameObject itemPrefab)
    {
        if (itemPrefab == null) { Debug.LogWarning("[Item3DViewerRT] Show: itemPrefab == null"); return; }
        if (!UIExclusiveManager.TryOpen(UIKind.Viewer)) return;
        if (isActive) Hide();

        
        if (hideAllInGameUI)
        {
            disabledUIs.Clear();
            foreach (var canvas in FindObjectsOfType<Canvas>(true))
            {
                if (canvas == viewerCanvas) continue;
                if (!canvas.isActiveAndEnabled) continue;
                canvas.gameObject.SetActive(false);
                disabledUIs.Add(canvas.gameObject);
            }
        }

        pivot.localPosition = Vector3.forward * 1f;
        pivot.localRotation = Quaternion.identity;
        modelRoot.localPosition = Vector3.zero;
        modelRoot.localRotation = Quaternion.identity;
        accumulatedEuler = Vector2.zero;

        
        if (currentModel != null) Destroy(currentModel);
        currentModel = Instantiate(itemPrefab, modelRoot);
        currentModel.transform.localPosition = Vector3.zero;
        currentModel.transform.localRotation = Quaternion.identity;
        currentModel.transform.localScale = Vector3.one;

        if (_itemLayer != -1)
        {
            SetLayerRecursively(currentModel, _itemLayer);
        }

        var bounds = CalculateBounds(currentModel);
        Vector3 centerOffset = bounds.center - currentModel.transform.position;
        currentModel.transform.localPosition = -centerOffset;

        FreezeForPreview(currentModel);

        float dist = ComputeDistanceForBounds(bounds);

        pivot.localPosition = new Vector3(0, 0, dist);
        pivot.localRotation = Quaternion.identity;

        AllocateRT();

        viewerCam.targetTexture = rt;
        viewerImage.texture = rt;
        viewerCam.enabled = true;
    
        SetViewerCanvasActive(true);
        _fpc = FindObjectOfType<FirstPersonController>(true);
        if (_fpc != null)
        {
            _prevCameraCanMove = _fpc.cameraCanMove;
            _prevPlayerCanMove = _fpc.playerCanMove;
            _prevLockCursor = _fpc.lockCursor;

            if (disablePlayerLookWhenViewing) _fpc.cameraCanMove = false;
            if (disablePlayerMoveWhenViewing) _fpc.playerCanMove = false;

            _fpc.lockCursor = false;
        }

        if (pauseGameWhenViewing)
        {
            _prevTimeScale = Time.timeScale;
            Time.timeScale = 0f;
            if (pauseAudioListener) AudioListener.pause = true;
            _pausedByViewer = true;
        }

        isActive = true;
        dragging = false;
        armed = false;

        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;

        UIExclusiveManager.NotifyOpened(UIKind.Viewer);
    }

    public void Hide()
    {
        if (!isActive) return;
        if (currentModel != null) RestorePreviewState();
        if (currentModel != null) { Destroy(currentModel); currentModel = null; }
        if (dirLight != null) dirLight.enabled = false;
        if (viewerCam != null) viewerCam.enabled = false;
        if (rt != null)
        {
            viewerCam.targetTexture = null;
            rt.Release();
            Destroy(rt);
            rt = null;
        }

        foreach (var go in disabledUIs)
            if (go != null) go.SetActive(true);
        disabledUIs.Clear();
   
        if (_fpc != null)
        {
            _fpc.cameraCanMove = _prevCameraCanMove;
            _fpc.playerCanMove = _prevPlayerCanMove;
            _fpc.lockCursor = _prevLockCursor;
            _fpc = null;
        }

        if (_pausedByViewer)
        {
            if (pauseAudioListener) AudioListener.pause = false;
            Time.timeScale = _prevTimeScale;
            _pausedByViewer = false;
        }

        Cursor.lockState = CursorLockMode.Locked; 
        Cursor.visible = false; 

        SetViewerCanvasActive(false);
        isActive = false;
        UIExclusiveManager.NotifyClosed(UIKind.Viewer);
    }

    void Update()
    {
        if (!isActive) return;

        if (Input.GetMouseButtonDown(0))
        {
            pressPos = Input.mousePosition;
            lastMousePos = pressPos;
            dragging = false;
            armed = RectTransformUtility.RectangleContainsScreenPoint(viewerImageRT, pressPos);
        }

        if (Input.GetMouseButton(0))
        {
            var cur = Input.mousePosition;
            var delta = (Vector2)(cur - lastMousePos);
            if (!dragging && ((Vector2)(cur - pressPos)).sqrMagnitude > dragClickThreshold * dragClickThreshold)
                dragging = true;

            if (armed && dragging && modelRoot != null)
            {
                accumulatedEuler.x += delta.y * dragSensitivity.y;  
                accumulatedEuler.y -= delta.x * dragSensitivity.x;  
                modelRoot.localRotation = Quaternion.Euler(accumulatedEuler.x, accumulatedEuler.y, 0f);
            }
            lastMousePos = cur;
        }

        if (Input.GetMouseButtonUp(0))
        {
            var up = Input.mousePosition;
            var moved = ((Vector2)(up - pressPos)).sqrMagnitude;
            if (!dragging && moved <= dragClickThreshold * dragClickThreshold)
            {
                Hide(); 
                return;
            }
        }

        if (Input.GetMouseButtonDown(1))
        {
            Hide();
        }

    }

    float ComputeDistanceForBounds(Bounds b)
    {
        float maxSize = Mathf.Max(b.size.x, b.size.y, b.size.z);
        if (maxSize < 1e-5f) maxSize = 0.1f;

        float fovRad = camFov * Mathf.Deg2Rad;
        float worldHeightAt1m = 2f * Mathf.Tan(fovRad * 0.5f);     
        float desiredWorldHeight = maxSize / (1f - margin);
        float neededDistance = (desiredWorldHeight / worldHeightAt1m) / Mathf.Clamp01(targetScreenHeightFraction);
        return Mathf.Clamp(neededDistance, minDistance, maxDistance);
    }

    void AllocateRT()
    {
        int w = Mathf.Max(512, Screen.width);
        int h = Mathf.Max(512, Screen.height);

        if (rt != null && (rt.width != w || rt.height != h))
        {
            viewerCam.targetTexture = null;
            rt.Release();
            Destroy(rt);
            rt = null;
        }
        if (rt == null)
        {
            rt = new RenderTexture(w, h, 24, RenderTextureFormat.ARGB32)
            {
                name = "RT_ItemViewer",
                antiAliasing = 2,
                useMipMap = false,
                autoGenerateMips = false
            };
            rt.Create();
        }
    }

    Bounds CalculateBounds(GameObject go)
    {
        Renderer[] renderers = go.GetComponentsInChildren<Renderer>(true);
        if (renderers.Length == 0)
            return new Bounds(go.transform.position, Vector3.one * 0.1f);

        Bounds b = renderers[0].bounds;
        for (int i = 1; i < renderers.Length; i++)
            b.Encapsulate(renderers[i].bounds);

        return b;
    }

    
    void FreezeForPreview(GameObject root)
    {
     
        if (_previewLayer == -1) _previewLayer = LayerMask.NameToLayer("ItemViewer");
        if (_previewLayer != -1) SetLayerRecursively(root, _previewLayer);
  
        foreach (var rb in root.GetComponentsInChildren<Rigidbody>(true))
        {
            _savedRBs.Add(new SavedRB { rb = rb, wasKinematic = rb.isKinematic, usedGravity = rb.useGravity, constraints = rb.constraints });
            rb.useGravity = false;
            rb.isKinematic = true;
            rb.constraints = RigidbodyConstraints.FreezeAll;
        }

        foreach (var col in root.GetComponentsInChildren<Collider>(true))
        {
            _savedCols.Add(new SavedCol { col = col, wasEnabled = col.enabled });
            col.enabled = false;
        }

        foreach (var anim in root.GetComponentsInChildren<Animator>(true))
        {
            _savedAnims.Add(new SavedAnim { anim = anim, wasEnabled = anim.enabled, applyRootMotion = anim.applyRootMotion });
            anim.applyRootMotion = false;
            anim.enabled = false;
        }

        if (!isolatePreview) return;

        foreach (var beh in root.GetComponentsInChildren<Behaviour>(true))
        {
            if (beh == null) continue;
            if (beh is Animator) continue;
            if (beh is Light) continue;
            if (beh is Canvas) continue;
            if (beh is Graphic) continue;
            if (beh is Camera) continue;

            _savedBehs.Add(new SavedBeh { beh = beh, wasEnabled = beh.enabled });
            beh.enabled = false;
        }
    }

    void RestorePreviewState()
    {
        for (int i = 0; i < _savedBehs.Count; i++) if (_savedBehs[i].beh) _savedBehs[i].beh.enabled = _savedBehs[i].wasEnabled;
        _savedBehs.Clear();

        for (int i = 0; i < _savedAnims.Count; i++) if (_savedAnims[i].anim)
            {
                _savedAnims[i].anim.applyRootMotion = _savedAnims[i].applyRootMotion;
                _savedAnims[i].anim.enabled = _savedAnims[i].wasEnabled;
            }
        _savedAnims.Clear();

        for (int i = 0; i < _savedCols.Count; i++) if (_savedCols[i].col) _savedCols[i].col.enabled = _savedCols[i].wasEnabled;
        _savedCols.Clear();

        for (int i = 0; i < _savedRBs.Count; i++) if (_savedRBs[i].rb)
            {
                _savedRBs[i].rb.useGravity = _savedRBs[i].usedGravity;
                _savedRBs[i].rb.isKinematic = _savedRBs[i].wasKinematic;
                _savedRBs[i].rb.constraints = _savedRBs[i].constraints;
            }
        _savedRBs.Clear();
    }

    void SetLayerRecursively(GameObject go, int layer)
    {
        go.layer = layer;
        foreach (Transform t in go.transform) SetLayerRecursively(t.gameObject, layer);
    }
}
