using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class CoilHead : MonoBehaviour
{
    [Header("플레이어 설정")]
    [Tooltip("플레이어 오브젝트의 Transform을 연결.")]
    public Transform player;

    [Tooltip("플레이어가 보는 방향(예: 카메라)의 Transform을 연결")]
    public Transform playerView;

    [Header("추적 판단 기준")]
    [Tooltip("플레이어가 코일 헤드를 바라보고 있다고 판단할 때의 기준 값 (0~1 사이). 값이 높으면 플레이어가 조금만 봐도 '바라보고 있다'고 인식.")]
    public float faceThreshold = 0.7f;

    [Header("플레이어 감지 거리")]
    [Tooltip("코일 헤드가 플레이어를 감지할 수 있는 최대 거리.")]
    public float detectionRange = 10.0f;

    [Header("장애물 설정")]
    [Tooltip("Raycast(레이를 쏘아서 확인할 때)에서 확인할 장애물의 레이어. 바닥은 포함 시키면 안 됨.")]
    public LayerMask obstacleMask;

    [Header("이동 속도 설정")]
    [Tooltip("코일헤드의 이동 속도를 조절합니다.")]
    public float moveSpeed = 3.5f;

    [Header("대기 상태 사운드")]
    [Tooltip("대기 상태에 진입할 때 재생할 오디오 클립.")]
    public AudioClip idleSound;

    private NavMeshAgent agent;
    private Animator anim;
    private Camera playerCamera;
    private AudioSource audioSource;
    private bool hasPlayedIdleSound = false;

    public Character playerCharacter;


    void Awake()
    {
        anim = GetComponent<Animator>();

        if (player == null)
        {
            GameObject playerObj = GameObject.FindGameObjectWithTag("PlayerBody");
            if (playerObj != null)
            {
                player = playerObj.transform;
            }
            else
            {
                Debug.LogWarning("태그 'Player'를 가진 오브젝트를 찾을 수 없습니다.");
            }
        }

        if (player != null)
            playerCharacter = player.GetComponent<Character>(); // 한 번만 GetComponent

        if (playerView == null)
        {
            if (Camera.main != null)
            {
                playerView = Camera.main.transform;
            }
            else
            {
                Debug.LogWarning("메인 카메라를 찾을 수 없습니다.");
            }
        }

        // playerView에 연결된 Camera 컴포넌트 캐싱
        if (playerView != null)
        {
            playerCamera = playerView.GetComponent<Camera>();
        }

        // AudioSource 컴포넌트 연결 또는 생성
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }

    }

    IEnumerator Start()
    {
        if (player != null)
        {
            playerCharacter = player.GetComponent<Character>();
        }


        agent = GetComponent<NavMeshAgent>();
        // 이동 속도 설정
        if (agent != null)
        {
            agent.speed = moveSpeed;
        }
        while (player == null)
        {
            GameObject playerObj = GameObject.FindGameObjectWithTag("PlayerBody");
            if (playerObj != null)
            {
                player = playerObj.transform;
                playerView = Camera.main != null ? Camera.main.transform : player;
                if (playerView != null)
                {
                    playerCamera = playerView.GetComponent<Camera>();
                }
            }
            yield return null;
        }

        if (agent == null)
        {
            Debug.LogError("NavMeshAgent 컴포넌트가 없습니다.");
        }
    }

    void Update()
    {

        // moveSpeed가 변경되었을 경우를 위해 매 프레임 업데이트
        if (agent != null)
        {
            agent.speed = moveSpeed;
        }

        float distance = Vector3.Distance(transform.position, player.position);
        if (distance > detectionRange)
        {
            agent.ResetPath();
            agent.velocity = Vector3.zero; // 잔여 속도 제거
            agent.isStopped = true;          // 에이전트를 정지 상태로 전환
            anim.speed = 0; // 애니메이션 정지
            anim.SetBool("isMoving", false);

            // 대기 상태일 때 소리 1회만 재생
            if (!hasPlayedIdleSound && idleSound != null)
            {
                audioSource.PlayOneShot(idleSound);
                hasPlayedIdleSound = true;
            }
            return;
        }
        else
        {
            agent.isStopped = false;         // 이동 재개 시 에이전트 활성화
        }

        bool isOnScreen = false;
        bool isVisible = false;
       
        if (playerCamera != null)
        {
          
            // 화면 내 위치를 확인.
            // viewportPos.x와 .y는 화면에서의 상대적 위치를 0~1 사이의 값으로 알려줌.
            // (0,0) = 좌측 하단, (1,1) = 우측 상단
            // viewportPos.z는 카메라 앞으로부터의 거리를 나타내고, 양수(>0)이면 카메라 앞쪽, 음수(<0)이면 뒤쪽.
            Vector3 viewportPos = playerCamera.WorldToViewportPoint(transform.position);
            isOnScreen = viewportPos.z > 0 && viewportPos.x > 0 && viewportPos.x < 1 && viewportPos.y > 0 && viewportPos.y < 1;

            // 카메라에서 코일 헤드 방향으로 Raycast하여 장애물에 의해 가려졌는지 체크
            RaycastHit hit;
            Vector3 direction = (transform.position - playerView.position).normalized;
            if (Physics.Raycast(playerView.position, direction, out hit, detectionRange, obstacleMask))
            {
                isVisible = (hit.transform == transform);
            }
            else
            {
                isVisible = true;
            }
        }

        // 플레이어의 화면 내에 코일 헤드가 실제로 보인다면 추적하지 않음.
        if (isOnScreen && isVisible)
        {
            agent.ResetPath();
            agent.velocity = Vector3.zero;
            agent.isStopped = true;
            anim.SetBool("isMoving", false);
            anim.speed = 0;

            if (!hasPlayedIdleSound && idleSound != null)
            {
                audioSource.PlayOneShot(idleSound);
                hasPlayedIdleSound = true;
            }
        }
        else
        {
            agent.isStopped = false;
            agent.SetDestination(player.position);
            anim.SetBool("isMoving", true);
            anim.speed = 1;

            // 추적 상태로 전환되었으므로 다시 소리 재생 가능하게
            hasPlayedIdleSound = false;
        }
    }
}
