using UnityEngine;

public class SprayerController : MonoBehaviour
{
   
    public float growthRate = 0.1f; // 식물의 성장 속도 조절을 위한 변수
    public float raycastDistance = 100f; // Raycast 거리
    public bool holdingSprayer = false; // 분무기를 들고 있는지 여부
    public GameObject SprayerPrefab; // 분무기 오브젝트
    private PlayerController playerController; // PlayerController 스크립트의 인스턴스 변수
    public float currentScale = 0.0f; // 현재 크기
    public AudioSource audioSource;     // AudioSource 컴포넌트
    public AudioClip spraySound;      //  오디오 클립


    void Start()
    {
        // PlayerController의 이벤트에 메서드를 등록
        PlayerController.OnSprayerToggled += UpdateSprayerState;
        // PlayerController 스크립트의 인스턴스 가져오기
        playerController = PlayerController.Instance;

        playerController = FindObjectOfType<PlayerController>();

    }

    public void UseItem()
    {
        Update();
        
        Debug.Log("분무기를 들었습니다");
    }
    void UpdateSprayerState(bool holding)
    {
        holdingSprayer = holding;
    }

    void Update()
    {       
            // 마우스 오른쪽 버튼을 누르고 분무기를 들고 있을 때만
            if (Input.GetMouseButtonDown(1))
            {
                
                RaycastHit hit;
                Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

                // Raycast를 사용하여 마우스가 클릭한 위치에서 식물을 감지
                if (Physics.Raycast(ray, out hit, raycastDistance))
                {
                    // 감지된 오브젝트가 Plant 태그를 가지고 있는지 확인
                    if (hit.collider.gameObject.CompareTag("Plant"))
                    {
                       

                        Plant plant = hit.collider.GetComponent<Plant>();
                        if (plant != null)
                        {
                            // 현재 식물의 성장 상태에 따라 모델 변경
                            //UpdatePlantModel(plant);
                        }
                        //클릭된 오브젝트에게 GrowPlant 메서드 호출하여 식물 성장 촉진
                        hit.collider.GetComponent<Plant>().GrowPlant(currentScale);
                        audioSource.PlayOneShot(spraySound);
                    }                    
                }
            }             
    }
}
