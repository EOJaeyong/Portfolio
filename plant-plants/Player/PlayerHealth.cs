using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

public class PlayerHealth : MonoBehaviour
{
    public int startingHealth = 100; // 시작 체력
    public int currentHealth { get; private set; } // 현재 체력
    public int maxHealth = 100; // 최대 체력
    public Image damageImage; // UI 이미지를 참조할 변수
    private Coroutine damageCoroutine; // 코루틴 참조 변수

    public AudioSource audioSource;     // AudioSource 컴포넌트
    public AudioClip recoverhealthSound;      //  오디오 클립
    private bool invincible = false; // 무적 상태 여부를 나타내는 변수

    void Start()
    {
        currentHealth = startingHealth;

    }

    // 플레이어가 피해를 받을 때 호출되는 함수
    public void TakeDamage(int amount)
    {

        // 무적 상태일 때는 피해를 받지 않음
        if (invincible)
            return;

        currentHealth -= amount;

        damageImage.gameObject.SetActive(true);

        // 기존에 실행 중인 코루틴이 있으면 중지
        if (damageCoroutine != null)
        {
            StopCoroutine(damageCoroutine);
        }

        // 0.5초 후에 UI 이미지를 비활성화하는 코루틴 시작
        damageCoroutine = StartCoroutine(DisableDamageImageAfterDelay());

        // 체력이 0 이하로 떨어졌을 때 처리
        if (currentHealth <= 0)
        {
            Die();
        }
    }

    // 플레이어가 사망했을 때 호출되는 함수
    void Die()
    {

        Debug.Log("Player died!");

        // 게임 오버 씬으로 전환
        SceneManager.LoadScene("BadEnding");


    }

    //체력 회복 메서드
    public void RecoverHealth(int amount)
    {
        currentHealth += amount;

        //체력이 최대 체력을 넘지 않도록 처리
        if (currentHealth > maxHealth)
        {
            currentHealth = maxHealth;
        }
    }

    //아이템 충돌 처리
    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("HpItem"))
        {
            Debug.Log("Hp +20");

            //체력 회복 처리
            RecoverHealth(20);

            audioSource.PlayOneShot(recoverhealthSound);

            //HpItem 오브젝트 제거
            Destroy(other.gameObject);
        }
    }
    // 피해 UI 이미지를 일정 시간 후에 비활성화하는 코루틴 메서드
    IEnumerator DisableDamageImageAfterDelay()
    {
        yield return new WaitForSeconds(0.5f);

        if (damageImage != null)
        {
            damageImage.gameObject.SetActive(false);
        }
    }

    // 무적 상태 설정
    public void SetInvincible(bool value)
    {
        invincible = value;
    }
}
