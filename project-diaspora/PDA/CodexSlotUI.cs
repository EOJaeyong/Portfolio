using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
public class CodexSlotUI : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] private Image iconImage;     // Child 아이콘
    [SerializeField] private Button button;       // Slot 버튼(배경)
    [SerializeField] private GameObject lockOverlay; // 잠금 오버레이(옵션)

    [Header("Icon Fit")]
    [Tooltip("아이콘 가장자리 여백(px)")]
    [SerializeField] private float iconPadding = 8f;
    [Tooltip("아이콘 비율 유지하며 슬롯 안에 맞추기")]
    [SerializeField] private bool preserveAspect = true;

    // 현재 슬롯에 등록된 유물 ID. 빈 슬롯은 -1.
    public int ArtifactId { get; private set; } = -1;

    // 외부에서 클릭 처리기를 주입받음
    private System.Action<int> onClick;

    void Awake()
    {
        if (iconImage)
        {
            iconImage.raycastTarget = false;
            // 아이콘 Rect를 부모 슬롯 안으로 스트레치 + 패딩 적용
            var r = iconImage.rectTransform;
            r.anchorMin = Vector2.zero;
            r.anchorMax = Vector2.one;
            r.pivot = new Vector2(0.5f, 0.5f);
            ApplyIconPadding();
            iconImage.preserveAspect = preserveAspect;
            // 기본은 숨김(빈 슬롯에서 버튼 배경만 보이게)
            iconImage.enabled = false;
        }
        if (button) button.onClick.AddListener(HandleClick);
    }

    void OnRectTransformDimensionsChange()
    {
        // 슬롯 크기가 바뀌면 아이콘도 다시 맞춘다
        ApplyIconPadding();
    }

    public void Init(System.Action<int> clickHandler)
    {
        onClick = clickHandler;
    }

    // 빈 슬롯(‘?’ 대신 배경만 보여줌)
    public void ShowQuestion(Sprite questionSprite)
    {
        ArtifactId = -1;

        if (iconImage)
        {
            iconImage.sprite = questionSprite;      // '?' 스프라이트로 설정
            iconImage.enabled = (questionSprite != null); // 스프라이트가 있다면 활성화

            // '?' 이미지도 아이콘과 동일하게 비율 유지 및 패딩 적용
            iconImage.preserveAspect = preserveAspect;
            ApplyIconPadding();
        }

        if (button) button.interactable = false;
        if (lockOverlay) lockOverlay.SetActive(true);
    }

    // 유물로 세팅(아이콘을 슬롯 안에 자동 맞춤)
    public void ShowArtifact(ArtifactData data)
    {
        ArtifactId = data.id;

        if (iconImage)
        {
            iconImage.sprite = data.icon;
            iconImage.enabled = true;               // 아이콘 표시
            iconImage.preserveAspect = preserveAspect;
            ApplyIconPadding();                     // 현재 슬롯 크기에 맞게 패딩 적용
        }

        if (button) button.interactable = true;
        if (lockOverlay) lockOverlay.SetActive(false);
    }

    private void HandleClick()
    {
        if (ArtifactId >= 0) onClick?.Invoke(ArtifactId);
    }

    private void ApplyIconPadding()
    {
        if (!iconImage) return;
        var r = iconImage.rectTransform;
        // offsetMin(x,y) = 좌/하단 +padding,  offsetMax(x,y) = 우/상단 -padding
        r.offsetMin = new Vector2(iconPadding, iconPadding);
        r.offsetMax = new Vector2(-iconPadding, -iconPadding);
    }
}
