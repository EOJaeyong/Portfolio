using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
public class ArtifactCodexUI : MonoBehaviour
{
    [Header("데이터")]
    [Tooltip("게임에 존재하는 모든 유물 데이터")]
    [SerializeField] private List<ArtifactData> allArtifacts;

    [Header("그리드 설정")]
    [SerializeField] private int columns = 3;
    [SerializeField] private int rows = 3;
   
    [Header("UI 참조")]
    [SerializeField] private RectTransform gridContent;
    [SerializeField] private GridLayoutGroup grid; 
    [SerializeField] private CodexSlotUI slotPrefab;    // 슬롯 프리팹
    [SerializeField] private Sprite questionSprite;     // ? 스프라이트
    [SerializeField] private ArtifactDetailsPanel detailsPanel;

    // 내부 상태
    private readonly List<CodexSlotUI> slots = new();
    private readonly HashSet<int> registered = new(); // 등록된 유물 id 
    private readonly Dictionary<int, ArtifactData> dataById = new();

    // 초기화/그리드 상태 플래그
    private bool initialized = false;
    private bool gridBuilt = false;

    private readonly HashSet<int> scanned = new();
    private readonly Dictionary<int, string> scannedTexts = new();

    [SerializeField] private RectTransform viewport;
    [SerializeField] private Scrollbar verticalScrollbar;

    [SerializeField] private bool vbarOverlays = true;

    [Header("외곽 여백(마진)")]
    [SerializeField] private float marginLeft = 12f;
    [SerializeField] private float marginRight = 12f;
    [SerializeField] private float marginTop = 12f;
    [SerializeField] private float marginBottom = 12f;

    [Header("셀 간격(Spacing)")]
    [SerializeField] private float cellSpacingX = 6f;
    [SerializeField] private float cellSpacingY = 6f;

    void Awake()
    {

        EnsureInitialized();

        if (gameObject.activeInHierarchy)
        {
            BuildGrid();
            FillAllWithQuestion();
            detailsPanel?.Clear();
            RepaintFromRegistered();
            AdjustLayout(); 
        }
    }

    // 패널이 처음 열릴 때 
    void OnEnable()
    {
        EnsureInitialized();

        StartCoroutine(BootstrapAfterLayout());

        if (!gridBuilt)
        {
            BuildGrid();
            FillAllWithQuestion();
            detailsPanel?.Clear(); 
            AdjustLayout();
        }

        // 꺼진 동안 등록된 유물들을 UI에 반영
        RepaintFromRegistered();
    }


    IEnumerator BootstrapAfterLayout()
    {

        EnsureInitialized();

     
        Canvas.ForceUpdateCanvases();

        yield return null;
        AdjustLayout(); 
        if (!gridBuilt)
        {
            BuildGrid();
            FillAllWithQuestion();
            detailsPanel?.Clear();
            gridBuilt = true;
        }

        if (gridContent)
            LayoutRebuilder.ForceRebuildLayoutImmediate(gridContent);

 

        Canvas.ForceUpdateCanvases();

        RepaintFromRegistered();
    }


    // 데이터 맵을 어디서든 강제로 준비
    private void EnsureInitialized()
    {
        if (initialized) return;

        dataById.Clear();
        foreach (var a in allArtifacts)
        {
            if (!a) continue;
            if (!dataById.ContainsKey(a.id))
                dataById.Add(a.id, a);
            else
                Debug.LogWarning($"Artifact id 중복: {a.id} - {a.name}");
        }
        initialized = true;
    }


    /// 인스펙터에서 columns/rows 바꾸면, 에디터에서 바로 재배치하고 싶을 때 사용(선택).

#if UNITY_EDITOR
    void OnValidate()
    {
        if (grid != null)
        {
            grid.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
            grid.constraintCount = Mathf.Max(1, columns);
            AdjustLayout(); // 에디터에서도 즉시 반영
        }
    }
#endif

    // 그리드 빌드
    private void BuildGrid()
    {
        // GridLayoutGroup 기본 세팅(열 고정)
        grid.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
        grid.constraintCount = Mathf.Max(1, columns);

        // [수정: 그리드 정렬을 좌상단으로 고정]
        grid.childAlignment = TextAnchor.UpperLeft;
        grid.startCorner = GridLayoutGroup.Corner.UpperLeft;

        // 기존 슬롯 정리
        for (int i = slots.Count - 1; i >= 0; i--)
        {
            if (slots[i]) DestroyImmediate(slots[i].gameObject);
        }
        slots.Clear();

        // 총 슬롯 개수 = rows * columns
        int total = Mathf.Max(1, rows) * Mathf.Max(1, columns);
        for (int i = 0; i < total; i++)
        {
            var slot = Instantiate(slotPrefab, gridContent);
            slot.Init(OnSlotClicked);
            slots.Add(slot);
        }

        gridBuilt = true;
        AdjustLayout(); // AdjustCellSizeToFill() 대신 AdjustLayout() 호출

    }

    //  초기 상태: 전부 ?로 채우기 
    private void FillAllWithQuestion()
    {
        foreach (var s in slots)
            s.ShowQuestion(questionSprite);
    }


    private void RepaintFromRegistered()
    {
        // 먼저 '?'로 초기화
        FillAllWithQuestion();

        // 등록된 항목들을 순서대로 칠함
        foreach (var id in registered)
            TryPaintOne(id);
    }

    //  첫 번째 빈 슬롯에 해당 id의 아이콘 등록
    private void TryPaintOne(int artifactId)
    {
        if (!dataById.TryGetValue(artifactId, out var data)) return;

        CodexSlotUI empty = null;
        foreach (var s in slots)
        {
            if (s.ArtifactId < 0) { empty = s; break; }
        }
        if (empty == null)
        {
            Debug.LogWarning("빈 슬롯이 없습니다. rows/columns를 늘리거나 페이지/스크롤을 추가하세요.");
            return;
        }

        empty.ShowArtifact(data);
    }

    // 외부에서 호출: 유물 획득 시 등록
    public void RegisterArtifact(int artifactId)
    {
        // 패널이 꺼져 있어도 데이터 맵을 준비
        EnsureInitialized();

        // 이미 등록된 유물은 무시(중복 등록 X)
        if (registered.Contains(artifactId)) return;

        // 데이터 검증
        if (!dataById.TryGetValue(artifactId, out var data))
        {
            Debug.LogWarning($"알 수 없는 유물 ID: {artifactId}. allArtifacts에 등록되어 있는지 확인하세요.");
            return;
        }

        // 등록 집합에 먼저 추가(그리드가 없어도 상태는 유지)
        registered.Add(artifactId);

        // 그리드가 아직 안 만들어졌다면 패널이 열릴 때 자동 반영
        if (!gridBuilt || slots.Count == 0) return;

        // 첫 번째 빈 슬롯(= ‘?’ 슬롯)을 찾음
        CodexSlotUI empty = null;
        foreach (var s in slots)
        {
            if (s.ArtifactId < 0) { empty = s; break; }
        }

        if (empty == null)
        {
            Debug.LogWarning("빈 슬롯이 없습니다. rows/columns를 늘리거나 페이지/스크롤을 추가하세요.");
            return;
        }

        // 빈 슬롯을 해당 유물로 전환
        empty.ShowArtifact(data);

       
    }
    public void MarkAsScanned(int artifactId, string scanText)
    {
        scanned.Add(artifactId);
        scannedTexts[artifactId] = scanText;

        if (dataById.TryGetValue(artifactId, out var data))
        {
            detailsPanel?.Show(data.icon, data.displayName, data.description, scanText, true);
        }
    }
    //  슬롯 클릭 → 상세창 표시 
    private void OnSlotClicked(int artifactId)
    {
        if (!dataById.TryGetValue(artifactId, out var data)) return;
        bool isScanned = scanned.Contains(artifactId);

        string text = "???";
        if (isScanned && scannedTexts.TryGetValue(artifactId, out var s)) text = s;

        detailsPanel?.Show(data.icon, data.displayName, data.description, text, isScanned);
    }

    private void AdjustLayout()
    {
        if (!grid) return;
        var basis = viewport ? viewport : gridContent;
        if (!basis) return;

        float width = basis.rect.width;
        float height = basis.rect.height;
        if (width <= 0f || height <= 0f) return;

        //  padding과 spacing을 float로 반영
        var pad = new RectOffset(
            Mathf.RoundToInt(marginLeft),
            Mathf.RoundToInt(marginRight),
            Mathf.RoundToInt(marginTop),
            Mathf.RoundToInt(marginBottom)
        );
        grid.padding = pad;
        grid.spacing = new Vector2(cellSpacingX, cellSpacingY);

        //스크롤바 폭 처리
        float vbar = 0f;
        if (!vbarOverlays && verticalScrollbar)
        {
            var rt = verticalScrollbar.transform as RectTransform;
            var le = verticalScrollbar.GetComponent<LayoutElement>();
            if (le && le.preferredWidth > 0f) vbar = le.preferredWidth;
            else if (rt) vbar = Mathf.Abs(rt.rect.width);
            if (vbar <= 0f) vbar = 18f;
        }

        // 셀 크기 계산
        float gapW = grid.spacing.x * Mathf.Max(0, columns - 1);
        float gapH = grid.spacing.y * Mathf.Max(0, rows - 1);

        float usableW = width - (vbarOverlays ? 0f : vbar);
        float cellW = (usableW - pad.left - pad.right - gapW) / Mathf.Max(1, columns);
        float cellH = cellW; // 정사각, 원하면 비율 적용 가능

        grid.childAlignment = TextAnchor.UpperLeft;
        grid.startCorner = GridLayoutGroup.Corner.UpperLeft;
        grid.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
        grid.constraintCount = Mathf.Max(1, columns);
        grid.cellSize = new Vector2(cellW, cellH);

        // Content 크기 계산
        if (gridContent)
        {
            float contentHeight = pad.top + pad.bottom + gapH + rows * cellH;
            gridContent.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, contentHeight);
            gridContent.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, width);
            gridContent.anchoredPosition = Vector2.zero;
        }
    }

}