using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.SceneManagement;

public class SceneSpawnManager : MonoBehaviour
{
    /// <summary>
    /// 씬 진입 시 몬스터 랜덤 생성 및 씬 나가면 몬스터 삭제 
    /// 박스 콜라이더로 스폰 구역을 정하면 해당 구역에서 스폰 가능 스폰 구역이 없으면 NavMesh위 랜덤 생성
    /// </summary>
    [System.Serializable]
    public class SpawnRule
    {
        [Tooltip("이 규칙이 적용될 씬 이름")]
        public string sceneName = "";

        [Tooltip("스폰할 몬스터 프리팹들 (스피릿, 코일헤드 등)")]
        public GameObject[] monsterPrefabs;

        [Tooltip("씬 입장 시 스폰할 최소/최대 마릿수")]
        public int spawnCountMin = 5;
        public int spawnCountMax = 10;

        [Tooltip("스폰 가능한 존. 지정하면 이 영역 안에서만 스폰됨. 비워두면 NavMesh 전체에서 랜덤 스폰")]
        public BoxCollider[] spawnAreas;

        [Tooltip("스폰 시도 시 NavMesh.SamplePosition 허용 반경")]
        public float sampleRadius = 4f;

        [Tooltip("스폰 시도 최대 횟수(포인트가 NavMesh 위에 안 잡힐 때 대비)")]
        public int maxAttempts = 15;

        [Tooltip("씬 떠날 때 생성 몬스터 전부 제거할지")]
        public bool clearOnSceneUnload = true;
    }

    [Header("씬별 스폰 규칙 목록")]
    public List<SpawnRule> rules = new List<SpawnRule>();

    [Header("생성된 몬스터 트래킹")]
    public List<GameObject> spawned = new List<GameObject>();

    private void Awake()
    {
        DontDestroyOnLoad(gameObject);
        SceneManager.sceneLoaded += OnSceneLoaded;
        SceneManager.sceneUnloaded += OnSceneUnloaded;
    }

    private void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
        SceneManager.sceneUnloaded -= OnSceneUnloaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        var rule = FindRule(scene.name);
        if (rule == null) return;

        int count = Random.Range(rule.spawnCountMin, rule.spawnCountMax + 1);
        for (int i = 0; i < count; i++)
        {
            // 프리팹 랜덤 선택
            var prefab = PickRandom(rule.monsterPrefabs);
            if (prefab == null) continue;

            // 위치 뽑기
            if (TryGetRandomSpawnPoint(rule, out Vector3 pos))
            {
                var go = Instantiate(prefab, pos, Quaternion.identity);
                spawned.Add(go);
            }
        }
    }

    private void OnSceneUnloaded(Scene scene)
    {
        var rule = FindRule(scene.name);
        if (rule == null || !rule.clearOnSceneUnload) return;

        // 이 매니저가 생성한 몬스터만 제거
        for (int i = spawned.Count - 1; i >= 0; i--)
        {
            if (spawned[i] != null)
            {
                Destroy(spawned[i]);
            }
            spawned.RemoveAt(i);
        }
    }

    private SpawnRule FindRule(string sceneName)
    {
        for (int i = 0; i < rules.Count; i++)
        {
            if (rules[i] != null && rules[i].sceneName == sceneName)
                return rules[i];
        }
        return null;
    }

    private GameObject PickRandom(GameObject[] arr)
    {
        if (arr == null || arr.Length == 0) return null;
        return arr[Random.Range(0, arr.Length)];
    }

    private bool TryGetRandomSpawnPoint(SpawnRule rule, out Vector3 pos)
    {
        // 스폰 존이 있으면 스폰 존 안에서 뽑기
        if (rule.spawnAreas != null && rule.spawnAreas.Length > 0)
        {
            for (int attempt = 0; attempt < rule.maxAttempts; attempt++)
            {
                var area = rule.spawnAreas[Random.Range(0, rule.spawnAreas.Length)];
                if (area == null) continue;

                Vector3 rand = new Vector3(
                    Random.Range(area.bounds.min.x, area.bounds.max.x),
                    area.bounds.center.y,
                    Random.Range(area.bounds.min.z, area.bounds.max.z)
                );

                if (NavMesh.SamplePosition(rand, out var hit, rule.sampleRadius, NavMesh.AllAreas))
                {
                    pos = hit.position;
                    return true;
                }
            }
        }

        // 스폰 존이 없으면 NavMesh에서 뽑기
        var tri = NavMesh.CalculateTriangulation();
        if (tri.vertices != null && tri.indices != null && tri.indices.Length >= 3)
        {
            for (int attempt = 0; attempt < rule.maxAttempts; attempt++)
            {
                
                int t = Random.Range(0, tri.indices.Length / 3);
                var i0 = tri.indices[t * 3 + 0];
                var i1 = tri.indices[t * 3 + 1];
                var i2 = tri.indices[t * 3 + 2];

                Vector3 a = tri.vertices[i0];
                Vector3 b = tri.vertices[i1];
                Vector3 c = tri.vertices[i2];

                Vector2 r = Random.insideUnitCircle;
                float u = Mathf.Abs(r.x);
                float v = Mathf.Abs(r.y);
                if (u + v > 1f) { u = 1f - u; v = 1f - v; }
                Vector3 p = a + u * (b - a) + v * (c - a);

                if (NavMesh.SamplePosition(p, out var hit, rule.sampleRadius, NavMesh.AllAreas))
                {
                    pos = hit.position;
                    return true;
                }
            }
        }

        pos = default;
        return false;
    }

}
