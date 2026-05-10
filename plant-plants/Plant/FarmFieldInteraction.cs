using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class FarmFieldInteraction : MonoBehaviour
{
    public Image plant;

    private PlayerController playerController;
    public float shovelRange = 100f;

    public Texture2D newTexture; // 갈린 부분을 나타내는 새로운 텍스처

    private Texture2D originalTexture; // 기존 텍스처를 저장할 변수
    private Renderer renderer; // 렌더러 컴포넌트를 저장할 변수

    public Texture hoeTexture; // 갈린 밭을 나타내는 텍스처

    public GameObject seedPrefab; // 씨앗 프리팹

    public bool digground = false;

    private int growthLevel = 0; // 식물 성장 수준

    public GameObject smallPlantPrefab; // 작은 식물 모델 프리팹
    public GameObject mediumPlantPrefab; // 중간 식물 모델 프리팹
    public GameObject largePlantPrefab; // 큰 식물 모델 프리팹

    private GameObject currentPlantModel; // 현재 식물 모델

    public bool holdingSprayer = false; // 분무기를 들고 있는지 여부를 나타내는 변수

    public GameObject Sprayer; // 분무기 오브젝트
    public Transform hand; // 플레이어의 손 위치

    [SerializeField] private bool plantOnField = false; // 밭에 식물이 있는지 여부를 나타내는 변수

    public AudioSource audioSource;     // AudioSource 컴포넌트
    public AudioClip digSound;      //  오디오 클립
    private void Start()
    {
        // 렌더러 컴포넌트 가져오기
        renderer = GetComponent<Renderer>();

        // 기존 텍스처를 저장
        originalTexture = (Texture2D)renderer.material.mainTexture;

        // 씨앗 프리팹을 할당
        seedPrefab = Resources.Load<GameObject>("Seed");

        playerController = FindObjectOfType<PlayerController>();

    }

    private void Update()
    {
        DigGround();
    }

    void PlantSeed(Vector3 position)
    {
        // 씨앗 프리팹을 인스턴스화하고 심을 위치에 배치
        //Instantiate(seedPrefab, position, Quaternion.identity);
    }

    void PlantSeedposition(Vector3 position)
    {
        // 씨앗 프리팹을 인스턴스화하고 심을 위치에 배치
        Instantiate(seedPrefab, position, Quaternion.identity);
    }

    [SerializeField] bool isDigged = false;
    public bool IsDigged { get { return isDigged; } }

    void DigGround()
    {
        // 마우스 왼쪽 버튼 클릭 시
        if (playerController.holdingShoveler && Input.GetMouseButtonDown(0))
        {
            // 마우스로 클릭된 지점에서 ray를 발사하여 충돌한 객체 정보를 가져옴
            RaycastHit hit;

            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

            if (Physics.Raycast(ray, out hit, shovelRange))
            {
                // 충돌한 오브젝트가 밭 자기 자신인지 확인하여 바꿈 // 태그를 this.gameObject로 변경
                if (hit.collider.gameObject == this.gameObject)
                {
                    Debug.Log("감지 완료");

                    if (!IsPlantOnField())
                    {
                        plant.gameObject.SetActive(true);
                      
                        // 충돌한 오브젝트의 렌더러 컴포넌트 가져오기
                        Renderer hitRenderer = hit.collider.GetComponent<Renderer>();
                        // 충돌한 오브젝트의 텍스처 변경
                        hitRenderer.material.mainTexture = newTexture;
                        isDigged = true;
                        audioSource.PlayOneShot(digSound);

                        plantOnField = true; // 식물이 심어진 상태로 변경
                    }
                    else
                    {
                        Debug.Log("이미 식물이 심어져 있습니다.");
                    }
                }
            }
        }
    }

    private bool IsPlantOnField()
    {
        GameObject[] plants = GameObject.FindGameObjectsWithTag("Plant");

        foreach (GameObject plant in plants)
        {
            if (plant.transform.parent == transform)
            {
                return true;
            }
        }

        return false;
    }
    private void Dig()
    {
        // 갈린 부분을 나타내는 새로운 텍스처로 변경
        renderer.material.mainTexture = newTexture;
    }

    private void RestoreOriginalTexture()
    {
        // 기존 텍스처로 복원
        renderer.material.mainTexture = originalTexture;
    }

    void IncreaseGrowth()
    {
        // 현재 성장 수준을 증가시킴
        growthLevel++;
        Debug.Log("식물의 성장이 증가했습니다. 현재 성장 수준: " + growthLevel);
    }



    public void ToggleSprayer()
    {
        holdingSprayer = !holdingSprayer;

        // 분무기를 들거나 내려놓았을 때 분무기 위치를 조정
        if (holdingSprayer)
        {
            // 분무기를 플레이어의 손 위치로 이동
            Sprayer.transform.position = hand.position;
            Sprayer.transform.parent = hand; // 분무기를 손에 부모로 설정
            Debug.Log("분무기를 들었습니다.");
        }
        else
        {
            // 분무기를 플레이어 앞에 떠 있도록 위치 조정
            Sprayer.transform.position = transform.position + transform.forward * 2f;
            Sprayer.transform.parent = null; // 분무기의 부모를 해제하여 씬의 루트에 배치
            Debug.Log("분무기를 내려놓았습니다.");
        }
    }

    //아이콘 사라지게 하는 코루틴
    IEnumerator DisableObjectAfterDelay(float delay)
    {
        // 지정된 초 동안 기다림
        yield return new WaitForSeconds(delay);

        // 오브젝트를 비활성화
        plant.gameObject.SetActive(false);
    }

    bool IsPlantOnField(GameObject field)
    {
        GameObject[] plants = GameObject.FindGameObjectsWithTag("Plant");
        foreach (GameObject plant in plants)
        {
            // 밭과 식물의 위치가 같은지 확인
            if (plant.transform.position == field.transform.position)
            {
                return true;
            }
        }

        return false;
    }
}

