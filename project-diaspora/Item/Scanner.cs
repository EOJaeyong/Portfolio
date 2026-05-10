using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Scanner : MonoBehaviour
{
    [Header("스캐너 설정")]
    public KeyCode scanKey = KeyCode.Space; // 스캔 키
    public TerrainScanner terrainScanner;  // TerrainScanner를 연결할 변수

    [Header("사운드 설정")]
    public AudioClip scanSound;            // 스캔 사운드 클립
    private AudioSource audioSource;       // 오디오 소스

    [Header("벽 뒤 스캔 차단")]
    [Tooltip("체크 시, occlusionMask에 포함된 벽/지형 뒤의 ScanableItem은 스캔 정보가 뜨지 않습니다.")]
    public bool requireLineOfSight = true;

    [Tooltip("시야를 가리는 레이어(예: Wall, Environment, Terrain)")]
    public LayerMask occlusionMask;

    private float scanCooldown = 3f;
    private float lastScanTime = -Mathf.Infinity;

    private Camera cam;

    void Start()
    {
        cam = Camera.main;

        // AudioSource 컴포넌트가 없으면 자동으로 추가
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }

        if (terrainScanner != null)
        {
            terrainScanner.requireLineOfSight = requireLineOfSight;
            terrainScanner.occlusionMask = occlusionMask;

            // 중복 구독 방지
            terrainScanner.OnFirstHit -= HandleFirstHit;
            terrainScanner.OnFirstHit += HandleFirstHit;
        }
        else
        {
            Debug.LogWarning("[Scanner] TerrainScanner 미지정");
        }
    }

    void Update()
    {
        if (Input.GetKeyDown(scanKey) && Time.time - lastScanTime >= scanCooldown)
        {
            lastScanTime = Time.time;

            if (scanSound != null) audioSource.PlayOneShot(scanSound);

            if (terrainScanner != null)
            {
                // 파동 중심을 플레이어/카메라 위치로
                Transform center = cam != null ? cam.transform : transform;
                terrainScanner.transform.position = center.position;

                terrainScanner.requireLineOfSight = requireLineOfSight;
                terrainScanner.occlusionMask = occlusionMask;

                terrainScanner.SpawnTerrainScanner();

                if (QuestManager.Instance.currentQuest.id == 3)
                {
                    QuestManager.Instance.CompleteQuest();
                }
                else
                {
                    Debug.Log("3 dsla");
                }

            }

            StopAllCoroutines();
            StartCoroutine(HideAllAfterDelay(scanCooldown));
        }
    }
    private void HandleFirstHit(ScanableItem item)
    {
        HideAllScanUI();

        if (item != null)
        {
            item.ShowScanUI(); // HUD + 아이템 위 UI

            // (원본) 유물 식별 → 코덱스 스캔 완료/문구 전달
            var link = item.GetComponentInParent<ArtifactLink>();
            if (link != null && link.Data != null)
            {
                var codex = FindObjectOfType<ArtifactCodexUI>(true);
                if (codex != null)
                {
                    codex.MarkAsScanned(link.Data.id, item.scanText);
                }
            }
        }

        LogHitAndNext(item);
    }
    private void LogHitAndNext(ScanableItem first)
    {
        // 스캔 시작점과 동일한 기준 사용(카메라가 있으면 카메라 위치, 없으면 스캐너 오브젝트 위치)
        Vector3 origin = (cam != null ? cam.transform.position : transform.position);

        // 씬의 모든 ScanableItem 수집
        var items = FindObjectsOfType<ScanableItem>();
        var list = new System.Collections.Generic.List<(ScanableItem it, float dist)>();
        foreach (var it in items)
        {
            if (it == null) continue;
            float d = Vector3.Distance(origin, it.transform.position);
            list.Add((it, d));
        }

        // 거리 오름차순 정렬
        list.Sort((a, b) => a.dist.CompareTo(b.dist));

        // first의 인덱스 찾기 (없다면 가장 가까운 걸 first로 간주)
        int idx = list.FindIndex(e => e.it == first);
        if (idx < 0) idx = 0;

        // 1) 먼저 맞은 아이템
        var firstEntry = list[idx];
        Debug.Log($"[ScanDebug] 1st: '{firstEntry.it.name}'  distance={firstEntry.dist:F2} m");

        // 2) 그 다음 가까운 아이템(거리 더 먼 것 중 첫 번째)
        bool printedSecond = false;
        for (int i = idx + 1; i < list.Count; i++)
        {
            var secondEntry = list[i];
            // '정면만' 보고 싶으면 아래 주석 해제:
            // if (cam != null) {
            //     Vector3 dir = (secondEntry.it.transform.position - origin).normalized;
            //     if (Vector3.Dot(cam.transform.forward, dir) <= 0f) continue; // 카메라 뒤는 스킵
            // }

            Debug.Log($"[ScanDebug] 2nd: '{secondEntry.it.name}'  distance={secondEntry.dist:F2} m");
            printedSecond = true;
            break;
        }

        if (!printedSecond)
        {
            Debug.Log("[ScanDebug] 2nd: (없음) 스캔 가능한 다른 아이템을 찾지 못했습니다.");
        }
    }
    IEnumerator HideAllAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        HideAllScanUI();
    }

    private void HideAllScanUI()
    {
        var items = FindObjectsOfType<ScanableItem>();
        foreach (var it in items)
        {
            if (it != null && it.scanUIInstance != null)
                it.HideScanUI();
        }
    }
  
}