using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bullet : MonoBehaviour
{
    public float bulletSpeed = 20f; // 총알 발사 속도
    public int damage = 20; // 총알 데미지
    public float fireRate = 2f; // 총알 발사 간격

    void Start()
    {
        Rigidbody bulletRigidbody = GetComponent<Rigidbody>();
        if (bulletRigidbody != null)
        {
            Destroy(gameObject, 3f); // 총알 오브젝트를 3초 후에 파괴합니다.
        }
    }

    public void Fire(Vector3 direction)
    {
        // 코루틴을 사용하여 일정 간격으로 총알을 발사합니다.
        StartCoroutine(FireRoutine(direction));
    }

    IEnumerator FireRoutine(Vector3 direction)
    {
        while (true)
        {
            // 총알을 발사합니다.
            Rigidbody bulletRigidbody = GetComponent<Rigidbody>();
            if (bulletRigidbody != null)
            {
                bulletRigidbody.velocity = direction * bulletSpeed;

            }

            // 다음 발사를 기다립니다.
            yield return new WaitForSeconds(fireRate);
        }
    }

    void OnTriggerEnter(Collider other)
    {
        // 충돌한 대상이 플레이어인지 확인합니다.
        if (other.CompareTag("Player"))
        {
            Debug.Log("플레이어 충돌 감지");
            // 플레이어에게 데미지를 입힙니다.
            PlayerHealth playerHealth = other.GetComponent<PlayerHealth>();
            if (playerHealth != null)
            {
                Debug.Log("플레이어 체력 -20");
                playerHealth.TakeDamage(damage);
            }

            // 총알을 파괴합니다.
            Destroy(gameObject);
        }
    }
}
