using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TerrainScanner : MonoBehaviour
{
    [Header("파동 비주얼 프리팹")]
    public GameObject TerrainScannerPrefab;

    [Header("파동 파라미터")]
    [Tooltip("파동 진행 시간(초)")]
    public float duration = 3f;

    [Tooltip("파동이 도달할 최대 반경(파티클 startSize, Trigger 반경)")]
    public float maxRadius = 20f;

    [Header("히트 필터(선택)")]
    [Tooltip("스캔 대상으로 인정할 레이어(0이면 전체 허용)")]
    public LayerMask hitLayer;

    [Header("Line of Sight(벽 뒤 스캔 차단)")]
    [Tooltip("체크를 켜면 차단 레이어 뒤에 있는 ScanableItem은 스캔 차단")]
    public bool requireLineOfSight = true;

    [Tooltip("벽/지형 등 시야를 막는 레이어 마스크")]
    public LayerMask occlusionMask;

    [Tooltip("레이 시작/도착 지점을 살짝 띄우는 값(바닥 간섭 완화)")]
    public float losEyeOffset = 0.05f;

    /// 파동이 '가장 먼저' 닿은 ScanableItem 알림
    public event Action<ScanableItem> OnFirstHit;

    /// 파동 시작(현재 Transform.position 기준)
    public void SpawnTerrainScanner()
    {
        if (TerrainScannerPrefab == null)
        {
            Debug.LogWarning("[TerrainScanner] TerrainScannerPrefab 미지정");
            return;
        }

        // 1) 파동 프리팹 생성
        GameObject pulse = Instantiate(TerrainScannerPrefab, transform.position, Quaternion.identity);

        // 2) 파티클 설정(수명/크기 세팅 + 일정시간 후 파괴)
        var ps = pulse.GetComponentInChildren<ParticleSystem>();
        if (ps != null)
        {
            var main = ps.main;
            main.startLifetime = duration;
            main.startSize = maxRadius;
        }
        else
        {
            Debug.Log("[TerrainScanner] 파티클 디버그 로그");
        }
        Destroy(pulse, duration + 1f);

        // 3) Trigger
        var sc = pulse.GetComponent<SphereCollider>();
        if (sc == null) sc = pulse.AddComponent<SphereCollider>();
        sc.isTrigger = true;
        sc.radius = 0.01f;

        // 4) 반경 확장 & 최초 히트 감지 
        var driver = pulse.AddComponent<TerrainScannerPulseDriver>();
        driver.Init(this, sc, duration, maxRadius, hitLayer);
    }

    internal void ReportFirstHit(ScanableItem item)
    {
        OnFirstHit?.Invoke(item);
    }
    internal bool PassLineOfSight(ScanableItem item, Vector3 origin)
    {
        if (!requireLineOfSight) return true;
        if (occlusionMask.value == 0) return true; 
        if (item == null) return false;

        Vector3 from = origin + Vector3.up * losEyeOffset;
        Vector3 to = item.GetScanWorldPoint() + Vector3.up * losEyeOffset;

        Vector3 dir = to - from;
        float dist = dir.magnitude;
        if (dist <= 0.05f) return true;

        // 차단 레이어가 먼저 맞으면 실패
        if (Physics.Raycast(from, dir.normalized, out RaycastHit hit, dist, occlusionMask, QueryTriggerInteraction.Ignore))
        {

            if (hit.collider != null && hit.collider.transform.IsChildOf(item.transform))
                return true;

            return false;
        }

        return true;
    }
}

internal class TerrainScannerPulseDriver : MonoBehaviour
{
    TerrainScanner owner;
    SphereCollider trigger;
    float duration;
    float maxRadius;
    LayerMask hitMask;

    float elapsed;
    bool firstHitSent;
    const float MIN_R = 0.01f;

    public void Init(TerrainScanner owner, SphereCollider trigger, float duration, float maxRadius, LayerMask hitMask)
    {
        this.owner = owner;
        this.trigger = trigger;
        this.duration = Mathf.Max(0.05f, duration);
        this.maxRadius = Mathf.Max(MIN_R, maxRadius);
        this.hitMask = hitMask;

        elapsed = 0f;
        firstHitSent = false;
        if (this.trigger != null) this.trigger.radius = MIN_R;
    }

    void Update()
    {
        if (trigger == null) return;
        elapsed += Time.deltaTime;
        float t = Mathf.Clamp01(elapsed / duration);
        trigger.radius = Mathf.Lerp(MIN_R, maxRadius, t);
    }

    void OnTriggerEnter(Collider other)
    {
        if (firstHitSent) return;

        if (hitMask.value != 0)
        {
            int ol = 1 << other.gameObject.layer;
            if ((ol & hitMask.value) == 0) return;
        }

        var item = other.GetComponentInParent<ScanableItem>();
        if (item == null) return;

        if (owner != null && owner.requireLineOfSight && owner.occlusionMask.value != 0)
        {
            if (!owner.PassLineOfSight(item, transform.position))
                return;
        }
        firstHitSent = true;
        owner?.ReportFirstHit(item);
    }
}
