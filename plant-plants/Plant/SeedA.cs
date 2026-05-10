using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SeedA : MonoBehaviour
{
    public GameObject[] growthStages; // 성장 단계에 따른 모델 배열
    private int currentStageIndex = 0; // 현재 성장 단계 인덱스

    private void Start()
    {
        // 초기 성장 단계 모델 활성화
        UpdateGrowthStage();
    }

    private void UpdateGrowthStage()
    {
        // 이전 성장 단계 모델 비활성화
        for (int i = 0; i < growthStages.Length; i++)
        {
            growthStages[i].SetActive(i == currentStageIndex);
        }
    }
    
    // 성장하는 함수
    public void GrowPlant()
    {
        // 현재 성장 단계 인덱스 증가
        currentStageIndex++;

        // 최대 성장 단계에 도달하면 종료
        if (currentStageIndex >= growthStages.Length)
        {
            Debug.Log("식물이 최대 성장했습니다.");
            return;
        }

        // 성장 단계에 따른 모델 변경
        UpdateGrowthStage();
    }
}
