using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class View3DButtonRT : MonoBehaviour
{
    // 버튼 OnClick에서 호출
    public void OnClick_View3D()
    {
        // 현재 선택된 아이템 
        var current = SelectedItemBus.Current;
        if (current == null)
        {
            Debug.LogWarning("[View3DButtonRT] 현재 선택된 아이템이 없습니다. 슬롯을 먼저 클릭하세요.");
            return;
        }

        // 보기용 프리팹 우선 → 없으면 dropPrefab 사용
        GameObject prefab = current.itemPrefab != null ? current.itemPrefab : current.dropPrefab;
        if (prefab == null)
        {
            Debug.LogWarning($"[View3DButtonRT] '{current.itemName}'에 사용할 프리팹이 없습니다. (itemPrefab/dropPrefab)");
            return;
        }

        // 뷰어 인스턴스 확인
        if (Item3DViewerRT.Instance == null)
        {
            Debug.LogWarning("[View3DButtonRT] Item3DViewerRT.Instance가 없습니다. 씬에 배치하세요.");
            return;
        }

        // 뷰어 실행
        Item3DViewerRT.Instance.Show(prefab);
    }
}
