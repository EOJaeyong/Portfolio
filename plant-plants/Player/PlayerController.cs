using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Scripting.APIUpdating;
using UnityEngine.XR;
using UnityEngine.UI;
using TMPro;

public class PlayerController : MonoBehaviour
{
    [SerializeField]
    public Transform characterBody;
    [SerializeField]
    public Transform cameraArm;

    Animator animator;

    public bool lockCursorOnStart = true;

    //추후에 아이콘 등으로 대체
    public Image water; //물주는 중 
    public Image seed; //씨앗뿌리는 중 
    public Image plant; //괭이, 수확 중

    public GameObject SprayerPrefab; // 분무기 오브젝트
    public bool holdingSprayer = false; // 분무기를 들고 있는지 여부
    private GameObject heldSprayer;   // 들고 있는 분무기 오브젝트


    public GameObject ShovelPrefab;  // 괭이 오브젝트
    public bool holdingShoveler = false; // 괭이를 들고 있는지 여부
    private GameObject heldShovel;   // 들고 있는 괭이 오브젝트


    public GameObject SeedPrefab; // seed_a 프리팹
    private GameObject heldSeed; // 들고 있는 씨앗 오브젝트

    public GameObject SeedPrefab2; // 두 번째 식물의 씨앗 프리팹
    private GameObject heldSeed2; // 들고 있는 두 번째 식물의 씨앗 오브젝트
    public bool holdingSeed2 = false; // 두 번째 식물의 씨앗을 들고 있는지 여부

    public GameObject SeedPrefab3; // 세 번째 식물의 씨앗 프리팹
    private GameObject heldSeed3; // 들고 있는 세 번째 식물의 씨앗 오브젝트
    public bool holdingSeed3 = false; // 세 번째 식물의 씨앗을 들고 있는지 여부

    private GameObject heldItem; // 현재 들고 있는 아이템
    public Transform hand; // 플레이어의 손 위치

    public bool holdingSeed = false; // 씨앗을 들고 있는지 여부

    public delegate void SprayAction(bool holdingSprayer); // 분무기를 들거나 내려놓을 때 호출될 델리게이트
    public static event SprayAction OnSprayerToggled;

    public delegate void ShovelAction(bool holdingSprayer); //괭이를 들거나 내려놓을 때 호출될 델리게이트
    public static event ShovelAction OnShovelerToggled;

    private GameObject currentSprayer;
    // 정적으로 접근 가능한 PlayerController 인스턴스

    public static PlayerController Instance { get; private set; }

    private bool isAnimating = false; //애니메이션 중인지 여부를 나타내는 변수

    public delegate void ToolStateChanged(); //이벤트 발생을 위해 추가
    public static event ToolStateChanged OnToolStateChanged;

    public AudioSource audioSource;     // AudioSource 컴포넌트
    public AudioClip plantSound;      // 경고음 오디오 클립




    void Start()
    {
      
        // 'Hand_R' 오브젝트를 찾아서 hand 변수에 할당
        hand = transform.Find("Hand");

        //// 캐릭터에 붙어있는 애니메이터 컴포넌트를 가져와서 저장. 
        animator = characterBody.GetComponent<Animator>();

        // hand 오브젝트의 부모를 캐릭터의 Transform으로 설정
        hand.SetParent(characterBody);

        if (lockCursorOnStart)
            SetCursorLocked(true);

    }
    void SetCursorLocked(bool locked)
    {
        if (locked)
        {
            // Locked: 커서를 화면 중앙에 고정 + 숨김
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }
        else
        {
            // Confined: 창 밖으로 못 나가게(스탠드얼론에서 확실), 에디터에선 Locked 해제용
            // 에디터에서 완전한 Confined가 제한적일 수 있어요.
            Cursor.lockState = CursorLockMode.Confined;
            Cursor.visible = true;
        }
    }
    void Update()
    {
        LookAround(); // 시야를 회전시키는 함수 (카메라 조작 등)
        Move();       // 플레이어 이동 처리

        Action();     // 일반적인 동작 실행

        // 분무기 토글
        if (Input.GetKeyDown(KeyCode.Alpha2))
        {
            ToggleSprayer();

            // 분무기를 손에 들고 있을 때 다른 도구들을 손에서 제거
            if (holdingSprayer)
            {
                if (holdingShoveler)
                {
                    ToggleShoveler();
                }
                if (holdingSeed)
                {
                    TogglePlantSeed();
                }
                if (holdingSeed2)
                {
                    TogglePlantSeed2();
                }
                if (holdingSeed3)
                {
                    TogglePlantSeed3();
                }
            }
        }

        // 괭이 토글
        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            ToggleShoveler();

            // 괭이를 손에 들고 있을 때 다른 도구들을 손에서 제거
            if (holdingShoveler)
            {
                if (holdingSprayer)
                {
                    ToggleSprayer();
                }
                if (holdingSeed)
                {
                    TogglePlantSeed();
                }
                if (holdingSeed2)
                {
                    TogglePlantSeed2();
                }
                if (holdingSeed3)
                {
                    TogglePlantSeed3();
                }
            }
        }

        // 씨앗 심기 토글
        if (Input.GetKeyDown(KeyCode.Alpha3))
        {
            TogglePlantSeed();
            SeedPrefab.transform.GetComponent<Collider>().enabled = false;

            // 씨앗을 손에 들고 있을 때 다른 도구들을 손에서 제거
            if (holdingSeed)
            {
                if (holdingSprayer)
                {
                    ToggleSprayer();
                }
                if (holdingShoveler)
                {
                    ToggleShoveler();
                }
                if (holdingSeed2)
                {
                    TogglePlantSeed2();
                }
                if (holdingSeed3)
                {
                    TogglePlantSeed3();
                }
            }
        }

        // 씨앗2 심기 토글
        if (Input.GetKeyDown(KeyCode.Alpha4))
        {
            TogglePlantSeed2();
            SeedPrefab2.transform.GetComponent<Collider>().enabled = false;

            // 씨앗2를 손에 들고 있을 때 다른 도구들을 손에서 제거
            if (holdingSeed2)
            {
                if (holdingSprayer)
                {
                    ToggleSprayer();
                }
                if (holdingShoveler)
                {
                    ToggleShoveler();
                }
                if (holdingSeed)
                {
                    TogglePlantSeed();
                }
                if (holdingSeed3)
                {
                    TogglePlantSeed3();
                }
            }
        }

        // 씨앗3 심기 토글
        if (Input.GetKeyDown(KeyCode.Alpha5))
        {
            TogglePlantSeed3();

            SeedPrefab3.transform.GetComponent<Collider>().enabled = false;

            // 씨앗3을 손에 들고 있을 때 다른 도구들을 손에서 제거
            if (holdingSeed3)
            {
                if (holdingSprayer)
                {
                    ToggleSprayer();
                }
                if (holdingShoveler)
                {
                    ToggleShoveler();
                }
                if (holdingSeed)
                {
                    TogglePlantSeed();
                }
                if (holdingSeed2)
                {
                    TogglePlantSeed2();
                }
            }
        }

        // 마우스 우클릭으로 씨앗 심기
        if (Input.GetMouseButtonDown(1))
        {
            PlantSeed();
        }
      
       OnToolStateChanged += HandleActions;
    }
    void LateUpdate() //Update에서 호출하면 플레이어가 카메라가 바라보는 방향 안 봄
    {
        LookAround(); //회전 메서드
    }
    void PlantSeed()
    {
        // 씨앗을 들고 있고, 마우스가 밭을 클릭한 경우에만 실행
        if ( holdingSeed || holdingSeed2 || holdingSeed3  )
        {
            // 마우스로 클릭된 지점에서 ray를 발사하여 충돌한 객체 정보를 가져옴
            RaycastHit hit;
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            

            if (Physics.Raycast(ray, out hit))
            {
                if ( hit.collider.gameObject.CompareTag("FarmField")  
                    && hit.collider.GetComponent<FarmFieldInteraction>().IsDigged)
                {
                    
                    GameObject plantImage;
                    if (holdingSeed)
                    {
                        // 씨앗이 심어지면 콜라이더 활성화
                        SeedPrefab.transform.GetComponent<Collider>().enabled = true;

                        plantImage = Instantiate(SeedPrefab, hit.point, Quaternion.identity);
                    }
                    if (holdingSeed2)
                    {
                        SeedPrefab2.transform.GetComponent<Collider>().enabled = true;

                        plantImage = Instantiate(SeedPrefab2, hit.point, Quaternion.identity);
                    }
                    if (holdingSeed3)
                    {
                        SeedPrefab3.transform.GetComponent<Collider>().enabled = true;

                        plantImage = Instantiate(SeedPrefab3, hit.point, Quaternion.identity);
                    }
                    
                    audioSource.PlayOneShot(plantSound);
                    animator.SetBool("isAction", true);

                }
                else
                {
                    Debug.Log(hit.collider.gameObject.name);
                }
            }
            else
            {
                animator.SetBool("isAction", false); // 추가: 애니메이션을 비활성화하는 부분
       
            }

            // 충돌한 오브젝트가 밭인지 확인
            //if (hit.collider.CompareTag("FarmField"))
            //{
            //    // 밭의 위치에 이미지를 붙임
            //    GameObject plantImage = Instantiate(SeedPrefab, hit.point, Quaternion.identity);
            //    Debug.Log("씨앗을 밭에 심음");

            //    animator.SetBool("isAction", true);
            //}
            //else
            //{
            //    animator.SetBool("isAction", false); // 추가: 애니메이션을 비활성화하는 부분
            //}
        }

    }

    public void TogglePlantSeed3()
    {

       // holdingSeed3 = !holdingSeed3; // 씨앗을 들거나 내려놓음
        holdingSeed3 = !holdingSeed3;
        // 씨앗3 상태 변경 이벤트 발생
        OnToolStateChanged?.Invoke(); //오브젝트 비활성화될 때 Action애니메이션 재생 안되게 하는 코드
        
        if (holdingSeed3)
        {
            // 씨앗을 플레이어의 손 위치로 이동시킴
            SeedPrefab3.transform.position = hand.position;
            heldSeed3 = Instantiate(SeedPrefab3, hand.position, Quaternion.identity);
            heldSeed3.transform.parent = hand; // 씨앗을 손의 자식으로 설정

            holdingSeed3 = true;
            Debug.Log("씨앗 들기");

            seed.gameObject.SetActive(true); //씨앗 활성화
        }
        else
        {
            // 씨앗을 플레이어 앞에 떠 있도록 위치 조정
            SeedPrefab3.transform.position = transform.position + transform.forward * 2f; // 예시로 캐릭터 앞으로 2m 이동
            SeedPrefab3.transform.parent = null; // 씨앗의 부모를 해제하여 씬의 루트에 배치
            holdingSeed3 = false;

            // 씨앗 비활성화
            Destroy(heldSeed3);
            heldSeed3.SetActive(false);
            Debug.Log("씨앗 내려놓기");

            seed.gameObject.SetActive(false); //씨앗 비활성화
        }
    }
    public void TogglePlantSeed2()
    {
        holdingSeed2 = !holdingSeed2;
        //씨앗2 상태 변경 이벤트 발생 
        OnToolStateChanged?.Invoke(); //오브젝트 비활성화될 때 Action애니메이션 재생 안되게 하는 코드
        //holdingSeed2 = !holdingSeed2; // 씨앗을 들거나 내려놓음

        if (holdingSeed2)
        {
            // 씨앗을 플레이어의 손 위치로 이동시킴
            SeedPrefab2.transform.position = hand.position;
            heldSeed2 = Instantiate(SeedPrefab2, hand.position, Quaternion.identity);
            heldSeed2.transform.parent = hand; // 씨앗을 손의 자식으로 설정

            holdingSeed2 = true;
            SeedPrefab3.transform.GetComponent<Collider>().enabled = false;
            Debug.Log("씨앗 들기");

            seed.gameObject.SetActive(true); //씨앗 활성화
        }
        else
        {
            // 씨앗을 플레이어 앞에 떠 있도록 위치 조정
            SeedPrefab2.transform.position = transform.position + transform.forward * 2f; // 예시로 캐릭터 앞으로 2m 이동
            SeedPrefab2.transform.parent = null; // 씨앗의 부모를 해제하여 씬의 루트에 배치
            holdingSeed2 = false;

            // 씨앗 비활성화
            Destroy(heldSeed2);
            heldSeed2.SetActive(false);
            SeedPrefab2.transform.GetComponent<Collider>().enabled = false;
            Debug.Log("씨앗 내려놓기");

            seed.gameObject.SetActive(false); //씨앗 비활성화
        }
    }
    public void TogglePlantSeed()
    {
        holdingSeed = !holdingSeed;
        //씨앗1 상태 변경 이벤트 발생
        OnToolStateChanged?.Invoke(); //오브젝트 비활성화될 때 Action애니메이션 재생 안되게 하는 코드

        //holdingSeed = !holdingSeed; // 씨앗을 들거나 내려놓음

        if (holdingSeed)
        {
            // 씨앗을 플레이어의 손 위치로 이동시킴
            SeedPrefab.transform.position = hand.position;
            heldSeed = Instantiate(SeedPrefab, hand.position, Quaternion.identity);
            heldSeed.transform.parent = hand; // 씨앗을 손의 자식으로 설정

            holdingSeed = true;
            Debug.Log("씨앗 들기");

            seed.gameObject.SetActive(true); //씨앗 활성화
        }
        else
        {
            // 씨앗을 플레이어 앞에 떠 있도록 위치 조정
            SeedPrefab.transform.position = transform.position + transform.forward * 2f; // 예시로 캐릭터 앞으로 2m 이동
            SeedPrefab.transform.parent = null; // 씨앗의 부모를 해제하여 씬의 루트에 배치
            holdingSeed = false;

            // 씨앗 비활성화
            Destroy(heldSeed);
            heldSeed.SetActive(false);
            SeedPrefab.transform.GetComponent<Collider>().enabled = false;
            Debug.Log("씨앗 내려놓기");

            seed.gameObject.SetActive(false); //씨앗 비활성화
        }
    }

    public void ToggleShoveler()
    {
        holdingShoveler = !holdingShoveler;
        //괭이 상태 변경 이벤트 발생
        OnToolStateChanged?.Invoke(); //오브젝트 비활성화될 때 Action애니메이션 재생 안되게 하는 코드
        //holdingShoveler = !holdingShoveler;

        if (holdingShoveler)
        {

            // 괭이를 플레이어의 손 위치로 이동시킴
            heldShovel = Instantiate(ShovelPrefab, hand.position, Quaternion.identity);
            heldShovel.transform.parent = hand; // 괭이를 손에 부모로 설정
            // 괭이의 로테이션을 조정하여 원하는 방향으로 회전시킴
            heldShovel.transform.localRotation = Quaternion.Euler(0f, 0f, 0f);
            holdingShoveler = true;
            Debug.Log("괭이 장착");

            plant.gameObject.SetActive(true); //괭이 활성화

            //// 괭이를 플레이어의 손 위치로 이동시킴
            //ShovelPrefab.transform.position = hand.position;
            //ShovelPrefab.transform.parent = hand; // 괭이를 손에 부모로 설정
            //holdingShoveler = true;
            //Debug.Log("괭이 장착");

            //plant.gameObject.SetActive(true); //괭이 활성화
        }
        else
        {
            // 괭이를 플레이어 앞에 떠 있도록 위치 조정
            Destroy(heldShovel); // 괭이 오브젝트를 제거
            holdingShoveler = false;
            Debug.Log("괭이 해제");

            plant.gameObject.SetActive(false); //괭이 비활성화

            //// 괭이를 플레이어 앞에 떠 있도록 위치 조정
            //ShovelPrefab.transform.position = transform.position + transform.forward * 2f; // 예시로 캐릭터 앞으로 2m 이동
            //ShovelPrefab.transform.parent = null; // 괭이의 부모를 해제하여 씬의 루트에 배치
            //holdingShoveler = false;
            //Debug.Log("괭이 해제");

            //plant.gameObject.SetActive(false); //괭이 비활성화
        }
        // 이벤트 발생
        OnShovelerToggled?.Invoke(holdingShoveler);
    }

    public void ToggleSprayer()
    {
        holdingSprayer = !holdingSprayer;
        //분무기 상태 변경 이벤트 발생
        OnToolStateChanged?.Invoke(); //오브젝트 비활성화될 때 Action애니메이션 재생 안되게 하는 코드
        //holdingSprayer = !holdingSprayer; // 분무기를 들거나 내려놓음

        if (holdingSprayer)
        {

            // 괭이를 플레이어의 손 위치로 이동시킴
            heldSprayer = Instantiate(SprayerPrefab, hand.position, Quaternion.identity);
            heldSprayer.transform.parent = hand; // 괭이를 손에 부모로 설정
            // 괭이의 로테이션을 조정하여 원하는 방향으로 회전시킴
            heldSprayer.transform.localRotation = Quaternion.Euler(0f, 0f, 0f);
            holdingSprayer = true;
            Debug.Log("분무기 장착");
            //// 분무기를 플레이어의 손 위치로 이동시킴
            //SprayerPrefab.transform.position = hand.position;
            //SprayerPrefab.transform.parent = hand; // 분무기를 손에 부모로 설정
            //holdingSprayer = true;
            //Debug.Log("분무기 장착");

            water.gameObject.SetActive(true); //물뿌리는 활성화
        }

        else
        {
            // 분무기를 플레이어 앞에 떠 있도록 위치 조정
            Destroy(heldSprayer); // 괭이 오브젝트를 제거
            holdingSprayer = false;
            Debug.Log("분무기 해제");
            water.gameObject.SetActive(false); //물뿌리는 비활성화
        }
        // 이벤트 발생
        OnSprayerToggled?.Invoke(holdingSprayer);
    }

    private void Move()
    {
        // Input.GetAxis로 가로 세로 이동 입력값을 가져옴.
        Vector2 moveInput = new Vector2(Input.GetAxis("Horizontal"), Input.GetAxis("Vertical"));
        // 이동 여부
        bool isMove = moveInput.magnitude != 0;

        // ismove가 발생하면 걷는 애니메이션 작동 ismove가 발생하지 않는 다면 대기 애니메이션 작동
        animator.SetBool("isWalk", isMove);

        if (isMove)
        {
            // 평면화, 정규화 
            Vector3 lookForward = new Vector3(cameraArm.forward.x, 0f, cameraArm.forward.z).normalized;
            Vector3 lookRight = new Vector3(cameraArm.right.x, 0f, cameraArm.right.z).normalized;
            // 바라보고 있는 방향으로 이동
            Vector3 moveDir = lookForward * moveInput.y + lookRight * moveInput.x;

            // 캐릭터가 이동할 때 카메라가 바라보는 방향으로 바라보게 함.
            characterBody.forward = lookForward;
            // 캐릭터 이동
            transform.position += moveDir * Time.deltaTime * 5f;

            animator.SetBool("isWalk", true);
            animator.SetBool("isRun", false);

        }
    }

    // 마우스 움직임에 따라서 카메라 회전.
    private void LookAround()
    {
        // 이전 위치에서 얼마나 움직였는지 알게 해주고 mouseDelta 변수에 저장
        Vector2 mouseDelta = new Vector2(Input.GetAxis("Mouse X"), Input.GetAxis("Mouse Y"));
        // 카메라 암의 회전 값을 오일러 값으로 변환.
        Vector3 camAngle = cameraArm.rotation.eulerAngles;
        // -mouseDelta.y를 해서 y축이 위로 움직이면 피치값을 감소시키고 y축이 아래로 움직이면 피치값을 상승시킴
        float x = camAngle.x - mouseDelta.y;

        // 카메라가 뒤집어 지는 것을 방지.
        // 카메라 상하 움직임 제한.
        // x가 180도 보다 작은 경우는 위로 회전 하는 경우.
        // 0도가 아닌 -1도를 최저점 으로 하는 이유는 0도로 하면 카메라가 수평면 이하로 내려가지 않는 문제가 발생.
        if (x < 180f)
        {
            x = Mathf.Clamp(x, -1f, 70f);
        }

        else
        {
            // x값이 180보다 큰 경우는 아래로 회전 하는 경우.
            x = Mathf.Clamp(x, 335f, 361f);
        }

        cameraArm.rotation = Quaternion.Euler(x, camAngle.y + mouseDelta.x, camAngle.z);
        characterBody.rotation = Quaternion.Euler(0f, cameraArm.rotation.eulerAngles.y, 0f);

    }

    public void Action()
    {
        if (Input.GetKey(KeyCode.Y))
        {
            animator.SetBool("isAction", true);
        }
        else
        {
            animator.SetBool("isAction", false); // 추가: 애니메이션을 비활성화하는 부분
        }

    }

    void HandleActions() //들고 있는 오브젝트에 따라 맞는 코루틴 수행
    {
        // 분무기, 괭이, 씨앗 상태에 따라 처리할 작업 수행
        if (holdingSprayer)
        {
            // 분무기
            if (water != null)
            {
                StartCoroutine(PlayActionAnimation("Action", water));
            }
        }
        else if (holdingShoveler)
        {
            // 괭이
            if (plant != null)
            {
                StartCoroutine(PlayActionAnimation("Action", plant));
            }
        }
        else if (holdingSeed || holdingSeed2 || holdingSeed3)
        {
            // 씨앗
            if (seed != null)
            {
                StartCoroutine(PlayActionAnimation("Action", seed));
            }
        }
    }
    IEnumerator PlayActionAnimation(string animationName, Image actionImage)
    {
        isAnimating = true;
        animator.SetTrigger("isAction");

        if (actionImage != null)
        {
            actionImage.gameObject.SetActive(true);
            yield return new WaitForSeconds(1.6f);
            actionImage.gameObject.SetActive(false);
        }

        isAnimating = false;

        // 애니메이션이 끝난 후 Idle 상태로 돌아가기
        animator.Play("Idle");
    }
}