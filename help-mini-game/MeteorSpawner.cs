using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MeteorSpawner : MonoBehaviour
{
    // 운석 프리팹
    public GameObject meteorPrefab;

    // 생성 범위
    public float spawnRangeX = 5f;
    public float spawnY = 6f;

    // 낙하 속도 간격
    public float minSpeed = 3f;
    public float maxSpeed = 8f;

    // 생성 간격
    public float spawnInterval = 1f;

    private bool isSpawning = false;

    void Start()
    {
        StartSpawning();
    }

    public void StartSpawning()
    {
        if (!isSpawning)
        {
            isSpawning = true;
            StartCoroutine(SpawnRoutine());
        }
    }

    public void StopSpawning()
    {
        isSpawning = false;
        StopAllCoroutines();
    }

    IEnumerator SpawnRoutine()
    {
        while (isSpawning)
        {
            SpawnMeteor();
            yield return new WaitForSeconds(spawnInterval);
        }
    }

    void SpawnMeteor()
    {
        // 화면 상단의 랜덤 X 위치에 운석 생성
        float spawnX = Random.Range(-spawnRangeX, spawnRangeX);

        GameObject meteor = Instantiate(
            meteorPrefab,
            new Vector3(spawnX, spawnY, 0f),
            Quaternion.identity
        );

        // Rigidbody2D를 사용해 낙하 속도 랜덤화
        Rigidbody2D rb = meteor.GetComponent<Rigidbody2D>();

        rb.velocity = Vector3.down * Random.Range(minSpeed, maxSpeed);
    }
}
