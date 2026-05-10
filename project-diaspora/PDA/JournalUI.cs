using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine.UI;
using UnityEngine;

public class JournalUI : MonoBehaviour
{
    public static JournalUI Instance; // 싱글턴 인스턴스

    [Header("UI References")]
    public TextMeshProUGUI titleText;       // 저널 제목 텍스트
    public TextMeshProUGUI bodyText;        // 저널 본문 텍스트
    public Button nextButton;
    public Button prevButton;

    // 잠금 해제된 저널 항목 리스트
    private List<QuestData> unlockedEntries = new List<QuestData>();
    private int currentPage = 0;

    void Awake()
    {
        // 싱글턴 설정
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
            return;
        }
    }

    void Start()
    {
        // 버튼 리스너 연결
        if (nextButton) nextButton.onClick.AddListener(NextPage);
        if (prevButton) prevButton.onClick.AddListener(PrevPage);

        // 시작 시 한 번 UI 동기화 시도
        SyncFromManager();
    }

    // PDA에서 Journal 패널이 켜질 때마다 호출됨
    // 여기서 QuestManager에 쌓여 있던 캐시를 긁어온다.
    void OnEnable()
    {
        SyncFromManager();
    }

    // 다음 페이지
    public void NextPage()
    {
        if (currentPage < unlockedEntries.Count - 1)
        {
            currentPage++;
            UpdateUI();
        }
    }

    // 이전 페이지
    public void PrevPage()
    {
        if (currentPage > 0)
        {
            currentPage--;
            UpdateUI();
        }
    }

    // 현재 페이지에 맞춰 UI 업데이트
    private void UpdateUI()
    {
        if (unlockedEntries.Count == 0)
        {
            // 해금된 저널이 없을 때
            titleText.text = "일지";
            bodyText.text = "아직 기록된 내용이 없습니다.";
            if (nextButton) nextButton.gameObject.SetActive(false);
            if (prevButton) prevButton.gameObject.SetActive(false);
        }
        else
        {
            // 현재 페이지의 퀘스트 데이터 가져오기
            QuestData entry = unlockedEntries[currentPage];

            titleText.text = entry.journalTitle;
            bodyText.text = entry.journalBody;

            // 버튼 활성화/비활성화
            if (prevButton) prevButton.gameObject.SetActive(currentPage > 0);
            if (nextButton) nextButton.gameObject.SetActive(currentPage < unlockedEntries.Count - 1);
        }
    }

    //   QuestManager → JournalUI 동기화
    //   1) QuestManager.Instance.unlockedJournalEntries를 확인
    //   2) 아직 없는 항목만 unlockedEntries에 추가
    //   3) 마지막 항목 페이지로 이동
    //   4) UI 업데이트
    public void SyncFromManager()
    {
        if (QuestManager.Instance != null)
        {
            var cache = QuestManager.Instance.unlockedJournalEntries;

            bool addedSomething = false;

            foreach (var q in cache)
            {
                if (q == null) continue;
                if (!unlockedEntries.Contains(q))
                {
                    unlockedEntries.Add(q);
                    addedSomething = true;
                }
            }

            // 새 항목이 들어왔다면 가장 최근 항목 페이지로 이동
            if (addedSomething && unlockedEntries.Count > 0)
            {
                currentPage = unlockedEntries.Count - 1;
            }
        }

        // 리스트 동기화 후 화면 갱신
        UpdateUI();
    }

}
