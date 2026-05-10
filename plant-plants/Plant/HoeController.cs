using UnityEngine;

public class HoeController : MonoBehaviour
{
    public float interactionRange = 100f; // 상호작용 범위
    public LayerMask groundLayer; // 땅 레이어
    public GameObject plantPrefab; // 심을 식물 프리팹

    void Update()
    {
        // 마우스 오른쪽 버튼을 눌렀을 때
        if (Input.GetMouseButtonDown(1))
        {
            // 주변에 있는 땅을 검사하여 괭이 사용 가능한지 확인
            RaycastHit hit;
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(ray, out hit, interactionRange, groundLayer))
            {
                // 땅을 팔 수 있는 위치에서 식물 심기 시도
                PlantAt(hit.point);
            }
        }
    } 

    void PlantAt(Vector3 position)
    {
        // 땅에 식물을 심을 위치에 새로운 식물 생성
        GameObject newPlant = Instantiate(plantPrefab, position, Quaternion.identity);
        // 식물 오브젝트에 대한 추가 설정 가능

        Debug.Log("식물을 심었습니다!");
    }
}