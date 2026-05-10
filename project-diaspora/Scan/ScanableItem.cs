using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// 아이템 오브젝트에 uiTarget이름의 빈 오브젝트를 추가(ui의 생성될 위치)
/// </summary>
public class ScanableItem : MonoBehaviour
{
    [Header("아이템 데이터")]
    public Item itemData;

    [HideInInspector]
    public GameObject scanUIInstance;

    [Header("사운드 설정")]
    public AudioClip[] scanSounds;      // 여러 개의 사운드를 넣을 수 있는 배열
    private AudioSource audioSource;

    [Header("스캔 HUD 텍스트")]
    [TextArea(2, 4)]
    public string scanText;   // 아이템별 커스텀 문구. 비우면 itemData에서 대체

    [Tooltip("UI가 생성/배치될 기준. 비우면 이 오브젝트 transform 사용.")]
    public Transform uiAnchor;

    [Tooltip("앵커 기준 위치 오프셋(로컬 아님, 월드 기준).")]
    public Vector3 uiOffset = new Vector3(0f, 0.6f, 0f);

    [Header("아이템 위 UI(월드-스페이스)")]
    [Tooltip("아이템 위에 표시할 월드-스페이스 UI 프리팹(월드 스페이스 Canvas 루트가 있는 프리팹).")]
    public GameObject worldScanUIPrefab;

    [Header("Line of Sight(선택)")]
    [Tooltip("시야/차단 판정에 사용할 기준 지점. 비우면 uiAnchor 또는 transform 사용.")]
    public Transform scanPoint;

    private void Awake()
    {
        // AudioSource 설정
        audioSource = gameObject.AddComponent<AudioSource>();

    }

    /// <summary>
    /// 시야 차단 체크용 월드 좌표 반환
    /// </summary>
    public Vector3 GetScanWorldPoint()
    {
        if (scanPoint != null) return scanPoint.position;
        if (uiAnchor != null) return uiAnchor.position;
        return transform.position;
    }

    private string BuildScanText()
    {
        if (!string.IsNullOrWhiteSpace(scanText)) return scanText;
        if (itemData != null)
        {
            if (!string.IsNullOrWhiteSpace(itemData.itemTale)) return itemData.itemTale;  // 스토리/설명
            if (!string.IsNullOrWhiteSpace(itemData.itemName)) return itemData.itemName;  // 이름
        }
        return "스캔 가능한 오브젝트"; 
    }

    /// <summary>
    /// UI를 활성화하고 소리를 재생
    /// </summary>
    public void ShowScanUI()
    {

        // 1) 상단 텍스트 HUD
        if (TextScanHUD.Instance != null)
            TextScanHUD.Instance.Show(BuildScanText());

        // 2) 아이템 위 UI(월드-스페이스)
        if (worldScanUIPrefab != null)
        {
            if (scanUIInstance == null)
            {
                Transform anchor = uiAnchor != null ? uiAnchor : transform;
                scanUIInstance = Instantiate(worldScanUIPrefab, anchor.position + uiOffset, Quaternion.identity);
                // 프리팹 루트에 SimpleWorldScanUI가 없다면 자동으로 붙임(문구/빌보드 제어)
                var simple = scanUIInstance.GetComponent<SimpleWorldScanUI>();
                if (simple == null) simple = scanUIInstance.AddComponent<SimpleWorldScanUI>();
                simple.Bind(anchor, uiOffset);        
            }
            else
            {
                var simple = scanUIInstance.GetComponent<SimpleWorldScanUI>();
                if (simple != null)
                {
                    simple.Bind(uiAnchor != null ? uiAnchor : transform, uiOffset);      
                }
                scanUIInstance.transform.gameObject.SetActive(true);
            }
        }

        // 3) 사운드
        PlayRandomSound();
    }

    /// <summary>
    /// UI를 비활성화
    /// </summary>
    public void HideScanUI()
    {

        // 월드-스페이스 UI
        if (scanUIInstance != null)
            scanUIInstance.SetActive(false);

        // 상단 텍스트 HUD
        if (TextScanHUD.Instance != null)
            TextScanHUD.Instance.Hide();
    }

    /// <summary>
    /// 랜덤 사운드 재생
    /// </summary>
    private void PlayRandomSound()
    {
        if (scanSounds != null && scanSounds.Length > 0 && audioSource != null)
        {
            AudioClip clip = scanSounds[Random.Range(0, scanSounds.Length)];
            audioSource.PlayOneShot(clip);
        }
    }

    public void OnPickedUp() => CleanupScanUI();

    private void CleanupScanUI()
    {
        if (scanUIInstance != null)
        {
            Destroy(scanUIInstance);
            scanUIInstance = null;
        }
        if (TextScanHUD.Instance != null)
            TextScanHUD.Instance.Hide();
    }

    /// <summary>
    /// 간단한 월드-스페이스 스캔 UI 컨트롤러:
    /// - 앵커(아이템) 위치 + 오프셋을 따라감
    /// - 메인 카메라를 계속 바라봄(빌보드)
    /// - TextMeshProUGUI(있으면)로 문구 설정
    /// </summary>
    public class SimpleWorldScanUI : MonoBehaviour
    {
        Transform anchor;
        Vector3 offset;
        Camera cam;

        // 앵커와 오프셋 바인딩
        public void Bind(Transform anchor, Vector3 offset)
        {
            this.anchor = anchor;
            this.offset = offset;
            cam = Camera.main;
        }

        void LateUpdate()
        {
            if (anchor != null)
                transform.position = anchor.position + offset;

            // 빌보드 처리: 카메라 바라보게
            if (cam == null) cam = Camera.main;
            if (cam != null)
            {
                Vector3 dir = (transform.position - cam.transform.position).normalized;
                if (dir.sqrMagnitude > 0.0001f)
                    transform.rotation = Quaternion.LookRotation(dir, Vector3.up);
            }
        }
    }
}