using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.AI;

public class MonsterSpawner : MonoBehaviour
{
    public static MonsterSpawner Instance;

    [Header("몬스터 설정")]
    public GameObject monsterPrefab;
    public int monsterCount = 15;

    private List<GameObject> spawnedMonsters = new List<GameObject>();

    void Awake()
    {
        Instance = this;
    }

    public void SpawnMonsters()
    {
        ClearAllMonsters();
        StartCoroutine(SpawnMonstersCoroutine());
    }

    private IEnumerator SpawnMonstersCoroutine()
    {
        int spawned = 0;
        int attempts = 0;
        int maxAttempts = monsterCount * 50; // 충분한 시도

        Terrain terrain = Terrain.activeTerrain;
        if (terrain == null)
        {
            Debug.LogError("Terrain을 찾을 수 없습니다!");
            yield break;
        }

        Vector3 terrainPos = terrain.transform.position;
        Vector3 terrainSize = terrain.terrainData.size;

        while (spawned < monsterCount && attempts < maxAttempts)
        {
            attempts++;

            // 랜덤 위치 (Terrain 위)
            float x = Random.Range(terrainPos.x, terrainPos.x + terrainSize.x);
            float z = Random.Range(terrainPos.z, terrainPos.z + terrainSize.z);
            float y = terrain.SampleHeight(new Vector3(x, 0, z)) + terrainPos.y;
            Vector3 randomPoint = new Vector3(x, y, z);

            // NavMesh 위인지 확인
            NavMeshHit hit;
            if (NavMesh.SamplePosition(randomPoint, out hit, 5f, NavMesh.AllAreas))
            {
                GameObject monster = Instantiate(monsterPrefab, hit.position, Quaternion.identity);
                spawnedMonsters.Add(monster);
                spawned++;
                yield return null; // 한 프레임마다 생성 → 렉 방지
            }
        }

        Debug.Log($"{spawnedMonsters.Count}마리 생성 완료!");
    }

    public void ClearAllMonsters()
    {
        foreach (var m in spawnedMonsters)
        {
            if (m != null)
                Destroy(m);
        }
        spawnedMonsters.Clear();
        Debug.Log("모든 몬스터 제거 완료");
    }

    private void OnDrawGizmosSelected()
    {
        Terrain terrain = Terrain.activeTerrain;
        if (terrain != null)
        {
            Gizmos.color = new Color(0, 1, 0, 0.25f);
            Vector3 center = terrain.transform.position + terrain.terrainData.size * 0.5f;
            Gizmos.DrawCube(center, terrain.terrainData.size);
        }
    }
}
