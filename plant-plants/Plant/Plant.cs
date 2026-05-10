using UnityEngine;
using UnityEngine.UI;

public class Plant : MonoBehaviour
{

    public float maxScale = 3.0f; // 최대 성장 수치

    public float growthRate = 1.0f; // 성장 속도

    public float currentScale = 0.0f; // 현재 크기

    private int currentStageIndex = 0; // 현재 성장 단계 인덱스

    public bool isHarvestable = false; // 수확 가능 여부

    public GameObject plant01Prefab; // 초기 식물 프리팹
    public GameObject plant02Prefab; // 성장 상태가 1일 때의 식물 프리팹
    public GameObject plant03Prefab; // 성장 상태가 2 이상일 때의 식물 프리팹
    public GameObject currentPlant; // 현재 식물 오브젝트

    [SerializeField]
    public CountPlant countPlant; // CountPlant 스크립트에 대한 참조

   
    private void Start()
    {      
        // CountPlant 스크립트를 직접 찾기 위해 GameObject.Find 또는 GameObject.FindWithTag를 사용할 수 있습니다.
        GameObject countPlantObj = GameObject.Find("CountPlant"); // CountPlantObject는 CountPlant 스크립트가 포함된 GameObject의 이름입니다.
        // CountPlant 스크립트의 인스턴스를 가져와서 할당
        if (countPlantObj != null)
        {
            countPlant = countPlantObj.GetComponent<CountPlant>(); // CountPlant 스크립트를 가져와서 countPlant에 할당합니다.
        }
        else
        {
            Debug.LogError("CountPlantObject를 찾을 수 없습니다.");
        }
    }

    // 식물 성장 함수
    public void GrowPlant(float growthRate)
    {
        // 현재 크기가 최대 크기에 도달하지 않았다면 계속 성장
        if (currentScale < maxScale)
        {
            currentScale += 1f;
        }
        // 최대 크기에 도달하면 수확 가능 상태로 변경
        if (currentScale >= maxScale)
        {
            isHarvestable = true;
            
            HarvestPlant();       // 식물을 수확하는 함수 호출
            countPlant.HarvestedPlant();

        }
        if (currentPlant != null)
        {
            Destroy(currentPlant);
        }
        // 성장 상태에 따라 적절한 프리팹을 선택
        if (currentScale == 1)
        {
            currentPlant = Instantiate(plant02Prefab, transform.position, transform.rotation);
            Destroy(plant01Prefab);
            
        }
        else if (currentScale >= 2)
        {
            currentPlant = Instantiate(plant03Prefab, transform.position, transform.rotation);
            Destroy(plant02Prefab);
        }
        // 성장 로직 추가
        currentScale += 1;       
    }

    private void HarvestPlant()
    {
        // 수확 가능한 상태라면
        if (isHarvestable)
        {
             
                Debug.Log("TryHarvest01실행");
                // 식물을 수확한 후 초기화
                currentScale = 0.0f;
                transform.localScale = Vector3.zero;

                isHarvestable = false;

                Destroy(plant03Prefab);
        }
        else
        {
            Debug.Log("아직 수확할 수 없는 상태입니다.");
        }
    }
}