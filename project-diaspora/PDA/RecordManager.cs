using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RecordManager : MonoBehaviour
{
    public static RecordManager Instance { get; private set; }

    [Header("모든 기록 데이터(SO)")]
    [SerializeField] private List<RecordData> allRecords;

    // id -> data 맵 
    private readonly Dictionary<int, RecordData> dataById = new();

    // 획득(해금) 상태
    private readonly HashSet<int> unlocked = new();

    // UI가 구독해서 갱신할 이벤트
    public event Action<int> OnUnlocked;

    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);

        // 데이터 맵 구성
        dataById.Clear();
        foreach (var r in allRecords)
        {
            if (!r) continue;
            if (!dataById.ContainsKey(r.id)) dataById.Add(r.id, r);
            else Debug.LogWarning($"[RecordManager] 중복 ID: {r.id} - {r.name}");
        }

      
    }

    public bool IsUnlocked(int id) => unlocked.Contains(id);

    public RecordData GetData(int id)
        => dataById.TryGetValue(id, out var d) ? d : null;

    public void Unlock(int id)
    {
        if (!dataById.ContainsKey(id))
        {
            Debug.LogWarning($"[RecordManager] 알 수 없는 ID: {id}");
            return;
        }
        if (!unlocked.Add(id)) return; 

        OnUnlocked?.Invoke(id);
        Debug.Log($"[RecordManager] 기록 해금: {id} - {dataById[id].title}");
    }

    public IEnumerable<int> AllUnlockedIds() => unlocked;
}
