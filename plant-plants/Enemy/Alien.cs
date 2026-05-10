using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Alien : MonoBehaviour
{
    public GameObject bulletPrefab; // 총알 프리팹
    public Transform bulletSpawnPoint; // 총알이 발사될 위치
    public float bulletSpeed = 10f; // 총알 발사 속도
    public float detectionRange = 10f; // 외계인의 감지 범위

    public float rotationSpeed = 5f; // 외계인의 회전 속도

    public float swayAmount = 10f; // 몸통의 좌우로 움직이는 최대 각도
    public float swaySpeed = 2f; // 몸통의 좌우로 움직이는 속도

    private Quaternion startRotation; // 시작할 때의 회전값

    private GameObject player; // 플레이어 객체
    public bool isShooting = false; // 총알 발사 여부를 나타내는 변수
    private bool canShoot = true; // 외계인이 총을 발사할 수 있는지 여부를 나타내는 변수

    public AudioSource audioSource;     // AudioSource 컴포넌트
    public AudioClip shoothingSound;      //  오디오 클립

    void Start()
    {
       // InvokeRepeating("FireBullet", 0f, 2f); // 시작 후 2초마다 FireBullet 메서드를 반복 호출
        player = GameObject.FindGameObjectWithTag("Player");

        startRotation = transform.rotation; // 시작할 때의 회전값 저장
    }

    void Update()
    {
        if (player != null)
        {
            // 플레이어와 외계인의 거리를 계산
            float distanceToPlayer = Vector3.Distance(transform.position, player.transform.position);

            // 플레이어가 감지 범위 내에 있으면 총알 발사
            if (distanceToPlayer <= detectionRange && !isShooting && canShoot)
            {               
                isShooting = true;
                InvokeRepeating("Shoot", 0f, 2f); // 2초마다 Shoot 메서드를 반복 호출
            }
            else if (distanceToPlayer > detectionRange && isShooting)
            {
                isShooting = false;
                CancelInvoke("Shoot"); // 발사 중지
            }
            // 플레이어를 바라보는 회전
            RotateTowardsPlayer();          
        }
    }

    public void SetCanShoot(bool canShoot)
    {
        this.canShoot = canShoot;
    }


    void RotateTowardsPlayer()
    {
        Vector3 direction = (player.transform.position - transform.position).normalized;
        Quaternion lookRotation = Quaternion.LookRotation(direction);
        // Y 축 방향 회전 제한
        lookRotation.eulerAngles = new Vector3(0, lookRotation.eulerAngles.y, 0);
        transform.rotation = Quaternion.Slerp(transform.rotation, lookRotation, rotationSpeed * Time.deltaTime);
    }



    //Vector3 direction = (player.transform.position - transform.position).normalized;
    //Quaternion lookRotation = Quaternion.LookRotation(direction);
    //transform.rotation = Quaternion.Slerp(transform.rotation, lookRotation, rotationSpeed * Time.deltaTime);

    void Shoot()
    {
        GameObject bullet = Instantiate(bulletPrefab, bulletSpawnPoint.position, Quaternion.identity);
        Rigidbody bulletRigidbody = bullet.GetComponent<Rigidbody>();
        if (bulletRigidbody != null)
        {
            audioSource.PlayOneShot(shoothingSound);
            Vector3 direction = (player.transform.position - bulletSpawnPoint.position).normalized;
            bulletRigidbody.velocity = direction * bulletSpeed;
        }
    }
    //private void FireBullet()
    //{
    //    GameObject bullet = Instantiate(bulletPrefab, bulletSpawnPoint.position, Quaternion.identity);
    //    Rigidbody bulletRigidbody = bullet.GetComponent<Rigidbody>();
    //    if (bulletRigidbody != null)
    //    {
    //        Vector3 direction = (PlayerPosition() - bulletSpawnPoint.position).normalized;
    //        bulletRigidbody.velocity = direction * bulletSpeed;
    //    }
    //}

    private Vector3 PlayerPosition()
    {
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            return player.transform.position;
        }
        return Vector3.zero;
    }
}

