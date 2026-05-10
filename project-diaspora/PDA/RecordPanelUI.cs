using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class RecordPanelUI : MonoBehaviour
{
    [Header("데이터/코어")]
    [SerializeField] private RecordManager manager;          
    [SerializeField] private List<RecordData> allRecords;     // 매니저 대신 직접 연결 가능

    [Header("그리드 설정")]
    [SerializeField] private int columns = 3;
    [SerializeField] private int rows = 3;
    [SerializeField] private GridAutoFitter autoFitter;       

    [Header("UI 참조")]
    [SerializeField] private RectTransform gridContent;       // GridLayoutGroup 위치
    [SerializeField] private GridLayoutGroup grid;
    [SerializeField] private ScrollRect scroll;
    [SerializeField] private RecordSlotUI slotPrefab;         // 기록용 슬롯 프리팹(아래 스크립트)
    [SerializeField] private Sprite questionSprite;           // 잠금 표시
    [SerializeField] private Image detailsIcon;               // 우측 본문 영역: 아이콘
    [SerializeField] private TMP_Text detailsTitle;           // 제목
    [SerializeField] private TMP_Text detailsBody;            // 본문

    // 내부 상태
    private readonly List<RecordSlotUI> slots = new();
    private readonly Dictionary<int, RecordData> dataById = new();
    private bool initialized = false;
    private bool gridBuilt = false;

    void Awake()
    {
        if (!manager) manager = FindObjectOfType<RecordManager>(true);

        EnsureInitialized();

        
        if (gameObject.activeInHierarchy)
            BuildAndPaint();
    }

    void OnEnable()
    {
        EnsureInitialized();
        BuildAndPaint();

        // 해금 이벤트 구독 → 플레이 중 새 기록 습득 시 즉시 반영
        if (manager) manager.OnUnlocked += OnRecordUnlocked;
    }

    void OnDisable()
    {
        if (manager) manager.OnUnlocked -= OnRecordUnlocked;
    }

    private void OnRecordUnlocked(int id)
    {
        // 슬롯이 이미 있다면 해당 ID를 첫 빈 칸에 그려줌
        if (gridBuilt && slots.Count > 0) TryPaintOne(id);
        // 상세는 자동으로 열 필요 없으면 생략
    }

    private void EnsureInitialized()
    {
        if (initialized) return;

        dataById.Clear();

        // 우선순위: 매니저 → allRecords
        if (manager)
        {
            foreach (var r in manager.GetType()
                                      .GetField("allRecords", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
                                      .GetValue(manager) as List<RecordData>)
            {
                if (!r) continue;
                if (!dataById.ContainsKey(r.id)) dataById.Add(r.id, r);
            }
        }
        else
        {
            foreach (var r in allRecords)
            {
                if (!r) continue;
                if (!dataById.ContainsKey(r.id)) dataById.Add(r.id, r);
            }
        }

        initialized = true;
    }

    private void BuildAndPaint()
    {
        if (!gridBuilt) BuildGrid();
        FillAllWithQuestion();
        RepaintFromCore();
        autoFitter?.SetGrid(columns, rows);
        ClearDetails();
    }

    private void BuildGrid()
    {
        grid.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
        grid.constraintCount = Mathf.Max(1, columns);

        for (int i = slots.Count - 1; i >= 0; i--)
            if (slots[i]) DestroyImmediate(slots[i].gameObject);
        slots.Clear();

        int total = Mathf.Max(1, rows) * Mathf.Max(1, columns);
        for (int i = 0; i < total; i++)
        {
            var slot = Instantiate(slotPrefab, gridContent);
            slot.Init(OnSlotClicked);
            slots.Add(slot);
        }
        gridBuilt = true;
    }

    private void FillAllWithQuestion()
    {
        foreach (var s in slots)
            s.ShowQuestion(questionSprite);
    }

    private void RepaintFromCore()
    {
        if (!manager) return;
        foreach (var id in manager.AllUnlockedIds())
            TryPaintOne(id);
    }

    private void TryPaintOne(int id)
    {
        if (!dataById.TryGetValue(id, out var data)) return;

        // 이미 칠해진 슬롯(= 같은 ID)은 건너뜀
        foreach (var s in slots) if (s.RecordId == id) return;

        // 첫 빈 칸 찾기
        RecordSlotUI empty = null;
        foreach (var s in slots) { if (s.RecordId < 0) { empty = s; break; } }

        if (empty == null)
        {
            Debug.LogWarning("[RecordPanelUI] 빈 슬롯 없음 → rows/columns 확장 또는 페이지 필요");
            return;
        }

        empty.ShowRecord(data);
    }

    private void OnSlotClicked(int recordId)
    {
        if (!dataById.TryGetValue(recordId, out var data)) return;

        if (detailsIcon) detailsIcon.sprite = data.icon;
        if (detailsTitle) detailsTitle.text = data.title;
        if (detailsBody) detailsBody.text = data.content;
    }

    private void ClearDetails()
    {
        if (detailsIcon) detailsIcon.sprite = null;
        if (detailsTitle) detailsTitle.text = "";
        if (detailsBody) detailsBody.text = "";
    }
}
