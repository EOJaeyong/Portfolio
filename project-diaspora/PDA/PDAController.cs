using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Linq;

public enum PDATab { Home, Codex, Record, Journal }

public class PDAController : MonoBehaviour
{

    [Header("Auto-Assign by Names (빈 값이면 이름으로 찾아 자동 할당)")]
    [SerializeField] private string pdaRootName = "Report_UI";
    [SerializeField] private string homePanelName = "HomePanel";
    [SerializeField] private string codexPanelName = "CodexPanel";
    [SerializeField] private string recordPanelName = "RecordPanel";
    [SerializeField] private string journalPanelName = "JournalPanel";

    [Header("Optional: Player/UI 경로 (버튼 자동 연결용)")]
    [Tooltip("Player 오브젝트 이름. 비우면 'Player'를 찾습니다.")]
    [SerializeField] private string playerRootName = "Player_Demo";
    [Tooltip("Player 하위 UI 경로 (예: \"PDA\" / \"UI\"). 비우면 'ui'를 근사 탐색합니다.")]
    [SerializeField] private string uiRootPath = "PDA";

    [Header("Button Names (여러 개를 모두 바인딩)")]
    [SerializeField] private string homeButtonName = "HomeButton";
    [SerializeField] private string codexButtonName = "CodexButton";
    [SerializeField] private string recordButtonName = "RecordButton";
    [SerializeField] private string journalButtonName = "JournalButton";

    [Header("Refs (자동 할당 대상)")]
    [SerializeField] private GameObject pdaRoot;
    [SerializeField] private GameObject homePanel, codexPanel, recordPanel, journalPanel;

    [Header("Startup")]
    [SerializeField] private bool startClosed = true;                // 시작 시 PDA UI를 닫기
    [SerializeField] private bool keepClosedOnSceneLoad = true;      // 씬 로드 후에도 닫힌 상태 유지

    [Header("Rebind Settings on Scene Load")]
    [Tooltip("씬 로드 후 재시도 기간(초) 동안 주기적으로 찾습니다.")]
    [SerializeField] private float rebindTimeoutSeconds = 3f;
    [Tooltip("재시도 간격(초)")]
    [SerializeField] private float rebindIntervalSeconds = 0.1f;

    [SerializeField] private FirstPersonController playerController;

    [Header("First Open UI")]
    [Tooltip("최초 1회만 띄울 안내 UI")]
    [SerializeField] private GameObject firstTimeIntroUI;

    private static bool hasOpenedPDA = false;

    private bool isToggling = false;

    [Header("Sound")]
    [SerializeField] private AudioSource pdaAudioSource;
    [SerializeField] private AudioClip openClip;
    [SerializeField] private AudioClip closeClip;

    private bool isFirstTimeIntroOpen = false;
    void OnEnable()
    {
        UIExclusiveManager.ClosePDARequested += ForceCloseByManager;
        SceneManager.sceneLoaded += OnSceneLoaded_Rebind;
    }

    void OnDisable()
    {
        CloseFirstTimeUI();

        UIExclusiveManager.ClosePDARequested -= ForceCloseByManager;
        SceneManager.sceneLoaded -= OnSceneLoaded_Rebind;

        // 정지 상태로 종료되지 않도록 복구
        if (Time.timeScale == 0f) Time.timeScale = 1f;
        AudioListener.pause = false;
    }

    private void ForceCloseByManager()
    {
        if (pdaRoot != null && pdaRoot.activeSelf)
        {
            CloseNow();
            Time.timeScale = 1f; 
            AudioListener.pause = false;
            UIExclusiveManager.NotifyClosed(UIKind.PDA);
       
        }
    }

    private void CloseNow()
    {
        if (pdaRoot) pdaRoot.SetActive(false);
        CloseFirstTimeUI();
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;
        if (playerController == null) ResolvePlayerController();
        if (playerController != null) playerController.cameraCanMove = true;

        if (pdaAudioSource != null && closeClip != null)
            pdaAudioSource.PlayOneShot(closeClip);

        

    }

    private void OpenNow()
    {
        if (pdaRoot) pdaRoot.SetActive(true);
        ShowTab(PDATab.Home);
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;
        if (playerController == null) ResolvePlayerController();
        if (playerController != null) playerController.cameraCanMove = false;

        if (pdaAudioSource != null && openClip != null)
            pdaAudioSource.PlayOneShot(openClip);

        
    }

    void Awake()
    {
        AutoAssignPanelsByName();
        AutoWireButtons_AllMatches();   // 여러 개 버튼 동시 바인딩
        if (startClosed) ForceClosedAtStartup();
        ResolvePlayerController();
    }

    private void ForceClosedAtStartup()
    {
        SetPanels(false, false, false, false);
        if (pdaRoot != null) pdaRoot.SetActive(false);
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.F1))
            TryToggle();

    }

    void TryToggle()
    {
        if (isToggling) return;
        StartCoroutine(ToggleRoutine());
    }

    IEnumerator ToggleRoutine()
    {
        isToggling = true;
        yield return null; // 1프레임 대기 → F1 입력 중복 방지

        Toggle(); // 원래 존재하던 Toggle() 호출

        // 0.1초 정도 딜레이 줘서 UIExclusive와 충돌 방지
        yield return new WaitForSecondsRealtime(0.1f);
        isToggling = false;
    }

    // 공개 탭 전환 API 
    public void ShowHome() { SetPanels(true, false, false, false); }
    public void ShowCodex() { SetPanels(false, true, false, false); }
    public void ShowRecord() { SetPanels(false, false, true, false); }
    public void ShowJournal() { SetPanels(false, false, false, true); }

    public void ShowTab(PDATab tab)
    {
        if (homePanel) homePanel.SetActive(tab == PDATab.Home);
        if (codexPanel) codexPanel.SetActive(tab == PDATab.Codex);
        if (recordPanel) recordPanel.SetActive(tab == PDATab.Record);
        if (journalPanel) journalPanel.SetActive(tab == PDATab.Journal);
    }

    void Toggle()
    {
        if (pdaRoot == null)
        {
            Debug.LogWarning("[PDA] pdaRoot가 비어 있습니다. 이름/자동할당 확인");
            return;
        }
        QuestManager.Instance.ReportProgress(GoalType.Action_UI, "PDA");
        if (!pdaRoot.activeSelf)
        {
            if (!UIExclusiveManager.TryOpen(UIKind.PDA)) return;
            OpenNow();

            if (!hasOpenedPDA)
            {
                ShowFirstTimeUI(); // 안내창 띄우기
                hasOpenedPDA = true; // "이제 열어봤음"으로 표시

            }
            Time.timeScale = 0f;
            //AudioListener.pause = true;
            UIExclusiveManager.NotifyOpened(UIKind.PDA);
        }
        else
        {
            CloseNow();
            CloseFirstTimeUI();
            Time.timeScale = 1f;
            //AudioListener.pause = false;
            UIExclusiveManager.NotifyClosed(UIKind.PDA);
        }

      //  QuestManager.Instance.ReportProgress(GoalType.Action_UI, "PDA");
    }

    private void ShowFirstTimeUI()
    {
        if (firstTimeIntroUI != null)
        {
            firstTimeIntroUI.SetActive(true); 

            if (!isFirstTimeIntroOpen)
            {
                isFirstTimeIntroOpen = true;
                UIExclusiveManager.PushEscBlock(); 
            }

            Debug.Log("[PDA] 이번 게임 실행 중 최초 오픈! 안내창을 띄웁니다.");
        }
    }
    public void RegisterFirstTimeUI(GameObject uiInstance)
    {
        firstTimeIntroUI = uiInstance;
        if (firstTimeIntroUI != null) firstTimeIntroUI.SetActive(false);

        Debug.Log("[PDA] FirstTimeIntroUI가 성공적으로 연결되었습니다: " + uiInstance.name);
    }

    // 안내창 닫기 버튼용 함수
    public void CloseFirstTimeUI()
    {
        if (firstTimeIntroUI != null)
        {
            firstTimeIntroUI.SetActive(false); 
        }

        if (isFirstTimeIntroOpen)
        {
            isFirstTimeIntroOpen = false;
            UIExclusiveManager.PopEscBlock();
        }
    }
    void SetPanels(bool h, bool c, bool r, bool j)
    {
        if (homePanel) homePanel.SetActive(h);
        if (codexPanel) codexPanel.SetActive(c);
        if (recordPanel) recordPanel.SetActive(r);
        if (journalPanel) journalPanel.SetActive(j);
    }

    // 자동 할당 & 버튼 연결
    void AutoAssignPanelsByName()
    {
        if (pdaRoot == null)
            pdaRoot = FindInSceneIncludingInactive(pdaRootName);

        if (homePanel == null) homePanel = FindChildByNameIncludingInactive(pdaRoot, homePanelName);
        if (codexPanel == null) codexPanel = FindChildByNameIncludingInactive(pdaRoot, codexPanelName);
        if (recordPanel == null) recordPanel = FindChildByNameIncludingInactive(pdaRoot, recordPanelName);
        if (journalPanel == null) journalPanel = FindChildByNameIncludingInactive(pdaRoot, journalPanelName);
    }

    GameObject FindChildByNameIncludingInactive(GameObject root, string name)
    {
        if (root == null || string.IsNullOrEmpty(name)) return null;
        var t = FindDeepChildByNameIncludingInactive(root.transform, name);
        return t ? t.gameObject : null;
    }

    /// <summary>
    /// 동일 이름의 버튼들을 "uiRoot 하위 + 씬 전역(비활성 포함)"에서 모두 수집해 일괄 바인딩
    /// </summary>
    void AutoWireButtons_AllMatches()
    {
        // 중복 방지를 위해 HashSet 사용
        var bound = new HashSet<Button>();

        // 1) uiRoot 하위에서 모두 수집
        Transform uiRoot = ResolveUIRoot();
        if (uiRoot != null)
        {
            CollectAndBindAll(uiRoot, homeButtonName, ShowHome, "ShowHome", bound);
            CollectAndBindAll(uiRoot, codexButtonName, ShowCodex, "ShowCodex", bound);
            CollectAndBindAll(uiRoot, recordButtonName, ShowRecord, "ShowRecord", bound);
            CollectAndBindAll(uiRoot, journalButtonName, ShowJournal, "ShowJournal", bound);
        }

        // 2) 씬 전역(비활성 포함)에서 누락분 보강 수집
        CollectAndBindAll_InScene(homeButtonName, ShowHome, "ShowHome", bound);
        CollectAndBindAll_InScene(codexButtonName, ShowCodex, "ShowCodex", bound);
        CollectAndBindAll_InScene(recordButtonName, ShowRecord, "ShowRecord", bound);
        CollectAndBindAll_InScene(journalButtonName, ShowJournal, "ShowJournal", bound);

        if (bound.Count == 0)
        {
            Debug.LogWarning("[PDA] 바인딩된 버튼이 없습니다. 이름/경로를 확인하세요.");
        }
    }

    /// <summary>
    /// uiRoot 하위에서 이름이 정확히 같은 버튼들을 모두 찾아 바인딩
    /// </summary>
    void CollectAndBindAll(Transform root, string targetName, UnityEngine.Events.UnityAction action, string label, HashSet<Button> acc)
    {
        if (root == null || string.IsNullOrEmpty(targetName)) return;

        var all = root.GetComponentsInChildren<Button>(true);
        foreach (var b in all)
        {
            if (b == null) continue;
            // 우선 정확 일치, 없으면 Contains 보조(디자이너 편의)
            if (b.name == targetName || b.name.Contains(targetName))
            {
                TryBind(b, action, label);
                acc.Add(b);
            }
        }
    }

    /// <summary>
    /// 씬 전역(비활성 포함)에서 이름이 같은 버튼들을 모두 찾아 바인딩(이미 바인딩된 것은 제외)
    /// </summary>
    void CollectAndBindAll_InScene(string targetName, UnityEngine.Events.UnityAction action, string label, HashSet<Button> acc)
    {
        if (string.IsNullOrEmpty(targetName)) return;

        foreach (var b in FindButtonsInSceneIncludingInactive(targetName))
        {
            if (b == null || acc.Contains(b)) continue;
            TryBind(b, action, label);
            acc.Add(b);
        }
    }

    Transform ResolveUIRoot()
    {
        Transform playerRoot = null;
        if (!string.IsNullOrEmpty(playerRootName))
        {
            var playerGO = GameObject.Find(playerRootName);
            if (playerGO != null) playerRoot = playerGO.transform;
        }
        if (playerRoot == null) return null;

        Transform uiRoot = null;
        if (!string.IsNullOrEmpty(uiRootPath))
        {
            uiRoot = playerRoot.Find(uiRootPath);
            if (uiRoot == null)
            {
                // 경로가 틀렸을 때: 'ui'로 근사 탐색
                uiRoot = FindDeepChildByName(playerRoot, "ui");
            }
        }
        return uiRoot;
    }

    void TryBind(Button btn, UnityEngine.Events.UnityAction action, string label)
    {
        if (btn == null) { Debug.LogWarning($"[PDA] 버튼({label})을 찾지 못했습니다."); return; }
        btn.onClick.RemoveAllListeners();
        btn.onClick.AddListener(action);
        Debug.Log($"[PDA] '{btn.name}' → {label}() 연결 완료");
    }

    static Transform FindDeepChildByName(Transform parent, string name)
    {
        if (parent == null) return null;
        for (int i = 0; i < parent.childCount; i++)
        {
            var c = parent.GetChild(i);
            if (c.name == name) return c;
            var r = FindDeepChildByName(c, name);
            if (r != null) return r;
        }
        return null;
    }

    // 씬 전역(비활성 포함) 탐색 유틸 
    static GameObject FindInSceneIncludingInactive(string targetName)
    {
        if (string.IsNullOrEmpty(targetName)) return null;

        var scene = SceneManager.GetActiveScene();
        var roots = scene.GetRootGameObjects();
        foreach (var root in roots)
        {
            var t = FindDeepChildByNameIncludingInactive(root.transform, targetName);
            if (t != null) return t.gameObject;
            if (root.name == targetName) return root;
        }
        return null;
    }

    static Transform FindDeepChildByNameIncludingInactive(Transform parent, string targetName)
    {
        if (parent == null) return null;
        if (parent.name == targetName) return parent;
        for (int i = 0; i < parent.childCount; i++)
        {
            var c = parent.GetChild(i);
            var r = FindDeepChildByNameIncludingInactive(c, targetName);
            if (r != null) return r;
        }
        return null;
    }

    static IEnumerable<Button> FindButtonsInSceneIncludingInactive(string targetName)
    {
        var results = new List<Button>();
        var scene = SceneManager.GetActiveScene();
        foreach (var root in scene.GetRootGameObjects())
        {
            // 모든 버튼을 비활성 포함으로 긁어와 필터
            var buttons = root.GetComponentsInChildren<Button>(true);
            foreach (var b in buttons)
            {
                if (!b) continue;
                if (b.name == targetName || b.name.Contains(targetName))
                    results.Add(b);
            }
        }
        return results.Distinct(); // 혹시 중복 방지
    }

    private void OnSceneLoaded_Rebind(Scene scene, LoadSceneMode mode)
    {
        StartCoroutine(RebindRoutine());
    }

    private IEnumerator RebindRoutine()
    {
        float deadline = Time.time + Mathf.Max(0.5f, rebindTimeoutSeconds);
        pdaRoot = null;
        homePanel = codexPanel = recordPanel = journalPanel = null;

        while (Time.time < deadline)
        {
            AutoAssignPanelsByName();

            if (pdaRoot != null)
            {
                AutoWireButtons_AllMatches();    // 재바인딩 시에도 전부 연결
                if (keepClosedOnSceneLoad) ForceClosedAtStartup();
                yield break;
            }

            yield return new WaitForSeconds(rebindIntervalSeconds);
        }
        Debug.LogWarning("[PDA] 씬 로드 후 제한 시간 내에 PDA 루트를 찾지 못했습니다.");
    }

    void ResolvePlayerController()
    {
        if (playerController != null) return;
        var tagged = GameObject.FindGameObjectWithTag("Player");
        if (tagged != null) playerController = tagged.GetComponentInChildren<FirstPersonController>(true);
    }

}
