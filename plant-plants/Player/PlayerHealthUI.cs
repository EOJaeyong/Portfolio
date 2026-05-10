using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
public class PlayerHealthUI : MonoBehaviour
{
    public Slider healthSlider; // 체력을 표시할 Slider UI 요소
    public PlayerHealth playerHealth; // 플레이어의 체력을 관리하는 스크립트

    void Start()
    {
        // PlayerHealth 스크립트를 찾아 할당합니다.
        playerHealth = GameObject.FindGameObjectWithTag("Player").GetComponent<PlayerHealth>();

        // Slider의 최대값을 플레이어의 시작 체력으로 설정합니다.
        healthSlider.maxValue = playerHealth.startingHealth;

        // 현재 체력을 시작 체력으로 설정합니다.
        healthSlider.value = playerHealth.startingHealth;
    }

    void Update()
    {
        // 현재 체력을 Slider에 반영합니다.
        healthSlider.value = playerHealth.currentHealth;
    }
}
