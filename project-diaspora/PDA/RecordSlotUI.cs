using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class RecordSlotUI : MonoBehaviour
{
    [SerializeField] private Image iconImage;       // 아이콘(또는 ?)
    [SerializeField] private Button button;         // 클릭 시 상세 표시
    [SerializeField] private GameObject lockOverlay;// 잠금 시 오버레이(선택)

    // 현재 슬롯에 들어있는 기록 ID. 빈 슬롯은 -1.
    public int RecordId { get; private set; } = -1;

    private System.Action<int> onClick;

    public void Init(System.Action<int> clickHandler)
    {
        onClick = clickHandler;
        if (button)
        {
            button.onClick.RemoveAllListeners();
            button.onClick.AddListener(() => { if (RecordId >= 0) onClick?.Invoke(RecordId); });
        }
        if (iconImage) iconImage.raycastTarget = false; // 불필요 레이캐스트 차단
    }

    // 빈 슬롯('?' 상태)으로 세팅
    public void ShowQuestion(Sprite questionSprite)
    {
        RecordId = -1;
        if (iconImage) iconImage.sprite = questionSprite;
        if (button) button.interactable = false;
        if (lockOverlay) lockOverlay.SetActive(true);
    }

    // 해금된 기록을 표시
    public void ShowRecord(RecordData data)
    {
        RecordId = data.id;
        if (iconImage) iconImage.sprite = data.icon;
        if (button) button.interactable = true;
        if (lockOverlay) lockOverlay.SetActive(false);
    }
}
