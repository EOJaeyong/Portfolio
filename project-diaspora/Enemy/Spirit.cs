using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class Spirit : MonoBehaviour
{
    [Header("플레이어 설정")]
    public Transform player; // 플레이어의 위치
    public Transform playerView; // 플레이어의 시선

    [Header("감지 및 행동 설정")]
    public float detectionRange = 10.0f; // 플레이어를 감지할 기본 거리
    public float chaseDetectionRange = 15.0f; // 추격을 시작할 거리
    public float chaseThreshold = 5.0f; // 시선 유지 시간이 이 값 이상일 때 추격 시작

    [Header("강제 추격 설정")]
    public float forcedChaseDuration = 10.0f; // 강제 추격 지속 시간

    [Header("장애물 설정")]
    public LayerMask obstacleMask; // 장애물 설정을 위한 레이어

    [Header("이동 속도 및 도망 설정")]
    public float moveSpeed = 3.5f; // 배회 시 이동 속도
    public float chaseSpeed = 5.5f; // 추격 시 속도
    public float angularSpeed = 120f; // 회전 속도
    public float fleeDistance = 5.0f; // 도망 거리

    [Header("시선 판별 추가 설정")]
    public float minLookDistance = 2.0f; // 가까이서 시선 판별 시작 거리
    public float angleThreshold = 30f; // 시선 판별 시 최대 각도

    [Header("Wander Settings")]
    public float wanderRadius = 10f; // 배회 반경
    public float wanderInterval = 5f; // 배회 지점 변경 간격

    [Header("사운드 설정")]
    public float soundDetectionRange = 20f; // 플레이어가 이 범위 내에 있을 때만 사운드를 출력
    public AudioSource audioSource; // 인스펙터에서 오디오 소스를 직접 지정해야 함

    private NavMeshAgent agent; // 네이브메시 에이전트 (이동을 위한)
    private Animator anim; // 애니메이터 컴포넌트 (애니메이션 제어)
    private Camera playerCamera; // 플레이어 카메라

    private float lookTimer = 0f; // 시선 유지 시간
    private bool forcedChase = false; // 강제 추격 상태 플래그
    private float forcedChaseTimer = 0f; // 강제 추격 경과 시간
    private float wanderTimer; // 배회 타이머

    void Start()
    {
        anim = GetComponent<Animator>(); // 애니메이터 컴포넌트 할당
        agent = GetComponent<NavMeshAgent>(); // 네이브메시 에이전트 할당
        if (agent != null)
        {
            agent.speed = moveSpeed; // 이동 속도 설정
            agent.angularSpeed = angularSpeed; // 회전 속도 설정
            agent.updateRotation = true; // 회전 업데이트 설정
        }

        wanderTimer = wanderInterval; // 배회 타이머 초기화

        if (player == null)
        {
            var go = GameObject.FindGameObjectWithTag("Player"); // 플레이어 찾기
            if (go != null) player = go.transform; // 플레이어 할당
        }

        if (playerView == null && Camera.main != null)
        {
            playerView = Camera.main.transform; // 기본 카메라 할당
        }
        if (playerView != null)
            playerCamera = playerView.GetComponent<Camera>(); // 카메라 컴포넌트 할당

        // 오디오소스 null 체크 - 없으면 경고 출력
        if (audioSource == null)
        {
            Debug.LogWarning("[Spirit] AudioSource가 지정되지 않았습니다. 사운드는 재생되지 않습니다.");
        }
    }

    void Update()
    {
        if (player == null) return; // 플레이어가 없으면 종료

        Character characterComp = player.GetComponent<Character>();
        if (characterComp != null && characterComp.isDead)
        {
            // 네비메시 멈추기
            if (agent != null)
            {
                agent.isStopped = true;
            }
               
            // 애니메이션 멈추기
            if (anim != null)
            {
                anim.SetBool("isMoving", false);
                anim.enabled = false;
            }
             
            return;
        }

        float distToPlayer = Vector3.Distance(transform.position, player.position); // 플레이어와의 거리 계산

        // 오디오 출력 조건 처리
        if (audioSource != null)
        {
            if (distToPlayer <= soundDetectionRange) // 플레이어가 소리 범위 내에 있으면
            {
                if (!audioSource.isPlaying) // 소리가 재생되지 않으면
                    audioSource.Play(); // 소리 재생
            }
            else
            {
                if (audioSource.isPlaying) // 소리가 재생 중이면
                    audioSource.Stop(); // 소리 정지
            }
        }

        bool isOnScreen = false, isVisible = false; // 화면 내 여부, 시야 내 여부
        if (playerCamera != null)
        {
            Vector3 vp = playerCamera.WorldToViewportPoint(transform.position); // 월드 좌표를 뷰포트 좌표로 변환
            isOnScreen = vp.z > 0 && vp.x > 0 && vp.x < 1 && vp.y > 0 && vp.y < 1; // 화면 내 여부 체크
            RaycastHit hit;
            Vector3 dir = (transform.position - playerView.position).normalized;
            if (Physics.Raycast(playerView.position, dir, out hit, detectionRange, obstacleMask)) // 장애물 체크
                isVisible = (hit.transform == transform); // 시야 내 여부 체크
            else
                isVisible = true;
        }

        // 시선 판별
        bool isLooking;
        if (distToPlayer < minLookDistance)
        {
            float angle = Vector3.Angle(playerView.forward, (transform.position - player.position));
            isLooking = (angle < angleThreshold) && isOnScreen && isVisible; // 각도와 화면 내 여부, 시야 내 여부
        }
        else
        {
            isLooking = isOnScreen && isVisible; // 시선 내에서 장애물 없음
        }

        if (isLooking) lookTimer += Time.deltaTime; // 시선이 유지되면 타이머 증가
        else lookTimer = 0f; // 시선이 끊기면 타이머 초기화

        bool shouldChase = false;
        bool shouldFlee = false;

        // 강제 추격 상태 처리
        if (forcedChase)
        {
            forcedChaseTimer += Time.deltaTime; // 강제 추격 시간 증가
            shouldChase = true; // 추격 상태
            if (forcedChaseTimer >= forcedChaseDuration) // 강제 추격 시간이 지나면 해제
            {
                forcedChase = false;
                forcedChaseTimer = 0f;
                lookTimer = 0f;
            }
        }
        else if (isLooking && lookTimer < chaseThreshold && distToPlayer <= detectionRange) // 시선 유지 시간이 짧고 거리가 가까울 때 도망
        {
            shouldFlee = true;
        }
        else if (isLooking && lookTimer >= chaseThreshold) // 시선 유지 시간이 길어지면 강제 추격
        {
            forcedChase = true;
            forcedChaseTimer = 0f;
            shouldChase = true;
        }
        else if (distToPlayer <= chaseDetectionRange) // 추격 거리 내에 있으면 추격
        {
            shouldChase = true;
        }

        string currState = shouldChase ? "추격(Chase)" : shouldFlee ? "도망(Flee)" : "배회(Wander)"; // 상태 로그 출력
       // Debug.Log($"Spirit 상태: {currState}, 거리: {distToPlayer:F2}, isLooking: {isLooking}, lookTimer: {lookTimer:F2}");

        // 도망/추격 상태가 아니면 배회
        if (!shouldChase && !shouldFlee)
        {
            anim.speed = 1f;
            agent.speed = moveSpeed;
            wanderTimer -= Time.deltaTime; // 배회 타이머 감소
            if (wanderTimer <= 0f || agent.remainingDistance <= agent.stoppingDistance) // 배회 지점 변경
            {
                Vector3 rnd = RandomNavSphere(transform.position, wanderRadius, NavMesh.AllAreas);
                agent.SetDestination(rnd); // 새로운 배회 지점 설정
                wanderTimer = wanderInterval; // 타이머 초기화
            }
            anim.SetBool("isMoving", agent.velocity.sqrMagnitude > 0.1f); // 이동 애니메이션 처리
            return;
        }

        // 추격/도망 상태에 따른 속도 및 거리 설정
        float effectiveRange = shouldChase ? chaseDetectionRange : detectionRange;
        float effectiveSpeed = shouldChase ? chaseSpeed : moveSpeed;
        agent.speed = effectiveSpeed;

        // 플레이어가 범위 밖이면 이동 취소
        if (distToPlayer > effectiveRange)
        {
            agent.ResetPath();
            anim.SetBool("isMoving", false);
            anim.speed = 0;
            return;
        }
        else
        {
            anim.speed = 1;
            anim.SetBool("isMoving", true); // 이동 애니메이션 처리
        }

        // 도망 상태일 때
        if (shouldFlee)
        {
            Vector3 fleeDir = (transform.position - player.position).normalized; // 도망 방향 계산
            Vector3 fd = transform.position + fleeDir * fleeDistance;
            NavMeshHit nh;
            if (NavMesh.SamplePosition(fd, out nh, 2f, NavMesh.AllAreas)) // 도망 경로 설정
                fd = nh.position;
            agent.SetDestination(fd);
        }
        // 추격 상태일 때
        else if (shouldChase)
        {
            Vector3 dest = player.position; // 플레이어를 추적
            NavMeshHit nh;
            if (NavMesh.SamplePosition(dest, out nh, 2f, NavMesh.AllAreas)) // 목적지 확인 후 이동
                dest = nh.position;
            agent.SetDestination(dest);
        }
    }

    // 랜덤 배회 지점 계산
    public static Vector3 RandomNavSphere(Vector3 origin, float dist, int areaMask)
    {
        Vector3 rnd = Random.insideUnitSphere * dist + origin;
        NavMeshHit hit;
        NavMesh.SamplePosition(rnd, out hit, dist, areaMask);
        return hit.position;
    }
}
