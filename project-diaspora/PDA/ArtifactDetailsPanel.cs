using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ArtifactDetailsPanel : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Image icon;                    // 검은 네모 위에 놓일 아이콘
    [SerializeField] private TextMeshProUGUI nameText;
    [SerializeField] private TextMeshProUGUI descText;
    [SerializeField] private TextMeshProUGUI scanDescText;

    [Header("Icon Fit")]
    [Tooltip("아이콘이 컨테이너 경계에서 가질 여백(px)")]
    [SerializeField] private float iconPadding = 8f;
    [Tooltip("아이콘 비율 유지")]
    [SerializeField] private bool preserveAspect = true;

    void Awake()
    {
        if (!icon) return;

        // 아이콘은 기본 비표시(획득/클릭 시에만 표시)
        icon.enabled = false;
        icon.preserveAspect = preserveAspect;

        // 아이콘 Rect를 부모(검은 네모 컨테이너) 안에 스트레치하고 패딩 적용
        var r = icon.rectTransform;
        r.anchorMin = Vector2.zero;   // (0,0)
        r.anchorMax = Vector2.one;    // (1,1)
        r.pivot = new Vector2(0.5f, 0.5f);
        ApplyIconPadding();
    }

    void OnRectTransformDimensionsChange()
    {
        // 패널 크기 변화 시 아이콘 패딩 재적용
        ApplyIconPadding();
    }

    public void Show(Sprite s, string name, string desc, string scanText, bool scanned)
    {
        // 텍스트
        if (nameText) nameText.text = name;
        if (descText) descText.text = desc;
        if (scanDescText) scanDescText.text = scanned
            ? (string.IsNullOrWhiteSpace(scanText) ? "???" : scanText)
            : "???";

        // 아이콘 표시/숨김 + 컨테이너 내에 맞추기
        if (icon)
        {
            icon.sprite = s;
            icon.enabled = (s != null);     // 아이콘이 있을 때만 보이도록
            icon.preserveAspect = preserveAspect;
            ApplyIconPadding();             // 컨테이너 경계 내로 강제
        }
    }

    public void Clear()
    {
        if (icon)
        {
            icon.sprite = null;
            icon.enabled = false;           // 빈 상태에선 숨김
        }
        if (nameText) nameText.text = "";
        if (descText) descText.text = "";
        if (scanDescText) scanDescText.text = "";
    }

    private void ApplyIconPadding()
    {
        if (!icon) return;
        var r = icon.rectTransform;
        // 컨테이너(검은 네모)의 내부로 스트레치된 상태에서 패딩(좌/하 +, 우/상 -)
        r.offsetMin = new Vector2(iconPadding, iconPadding);
        r.offsetMax = new Vector2(-iconPadding, -iconPadding);
    }
}
