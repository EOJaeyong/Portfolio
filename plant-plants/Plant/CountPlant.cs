using System.Collections;
using System.Collections.Generic;
// using UnityEditor.SearchService;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEditor;

public class CountPlant : MonoBehaviour
{
    public int CurrentPlant { get; private set; }  // 현재 수확한 식물 개수
    public int MaxHarvestablePlants = 100; // 수확 가능한 최대 식물 개수
    public int TargetPlantCount = 100; // 목표 식물 개수
    public GameObject NextSecenePanel;  // UI를 담고 있는 패널
    public Button nextSceneButton;  // 다음 씬으로 넘어가는 버튼
    private Alien alienScript; // Alien 스크립트를 참조하기 위한 변수
    private string currentSceneName; // 현재 씬의 이름을 저장하기 위한 변수
    private string nextSceneName; // 다음 씬의 이름을 저장하기 위한 변수

    public SceneLoader sceneLoader; // SceneLoader 스크립트를 참조하기 위한 변수
    public Text CurrentPlantText; // 현재 수확한 식물 개수를 표시할 UI(Text) 요소
    private PlayerHealth playerHealth; // PlayerHealth 스크립트를 참조하기 위한 변수

    public void Start()
    {
        // 현재 씬의 이름 저장
        currentSceneName = SceneManager.GetActiveScene().name;

        sceneLoader = FindObjectOfType<SceneLoader>(); // SceneLoader 스크립트 컴포넌트를 찾아 할당
        nextSceneButton.onClick.AddListener(LoadNextSceneOnClick);  // 버튼 클릭 시 LoadNextScene 함수 호출
        // Alien 스크립트 컴포넌트를 가져와서 참조 변수에 할당
        alienScript = GameObject.FindGameObjectWithTag("Alien").GetComponent<Alien>();

        // 현재 씬의 이름 저장
        currentSceneName = SceneManager.GetActiveScene().name;

        // PlayerHealth 스크립트 컴포넌트를 가져와서 참조 변수에 할당
        playerHealth = GameObject.FindGameObjectWithTag("Player").GetComponent<PlayerHealth>();


        // 초기화 시 UI 업데이트
        UpdateCurrentPlantUI();

    }
    void UpdateCurrentPlantUI()
    {
        if (CurrentPlantText != null)
        {
            CurrentPlantText.text = "현재 수확한 식물 개수: " + CurrentPlant.ToString();
        }
    }

    public void HarvestedPlant()
    {
        CurrentPlant++;
        Debug.Log("총 수확한 식물 개수: " + CurrentPlant);


        UpdateCurrentPlantUI();


        if (CurrentPlant > MaxHarvestablePlants)
        {
            Debug.Log("수확 가능한 최대치를 초과하였습니다!");
        }

        if (CurrentPlant >= TargetPlantCount)
        {
            Debug.Log("목표 식물 개수에 도달했습니다! 다음 씬으로 이동합니다.");
            // 외계인이 총을 발사하지 않도록 설정
            // 플레이어를 일시적으로 무적 상태로 설정
            playerHealth.SetInvincible(true);
            alienScript.SetCanShoot(false);
            NextSecenePanel.SetActive(true);
            alienScript.isShooting = false;
        }
    }
    public void LoadNextSceneOnClick()
    {
        // Debug.Log로 currentSceneName 변수의 값을 확인
        Debug.Log("Current Scene: " + currentSceneName);
        if (currentSceneName == "SampleScene")
        {
            sceneLoader.LoadRD2Scene();
        }
        else if (currentSceneName == "RD2")
        {
            sceneLoader.LoadRD3Scene();
        }
        else if (currentSceneName == "RD3")
        {
            sceneLoader.HappyEnding();
        }
        Debug.Log("로드넥스트씬");


    }
}