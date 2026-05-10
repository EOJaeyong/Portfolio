using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
public class ItemDetailsPanel : MonoBehaviour
{
    [Header("Big Slot UI (Frame + Icon)")]
    [SerializeField] private Image frameImage;   // 인벤토리 칸 프레임
    [SerializeField] private Image iconImage;    // 프레임 안에 들어갈 아이콘

    [Header("Texts")]
    [SerializeField] private TextMeshProUGUI nameText;
    [SerializeField] private TextMeshProUGUI typeText;
    [SerializeField] private TextMeshProUGUI descText;

    [Header("Empty Icon (optional)")]
    [SerializeField] private Sprite emptyIconSprite; // 빈 상태일 때 아이콘

    private void OnEnable()
    {
        
        Slot.OnAnySlotLeftClicked += Show;
    }
    private void OnDisable()
    {
        Slot.OnAnySlotLeftClicked -= Show;
    }

    private void Awake()
    {
        
        if (frameImage) frameImage.raycastTarget = false;

      
        if (iconImage)
        {
            iconImage.preserveAspect = true;
            iconImage.raycastTarget = false;
        }

        ResetUI();
    }

    private void ResetUI()
    {
        // 프레임은 그대로 두고, 아이콘/텍스트만 초기화
        if (iconImage)
        {
            if (emptyIconSprite != null)
            {
                iconImage.enabled = true;
                iconImage.sprite = emptyIconSprite;
                iconImage.color = Color.white;
            }
            else
            {
                iconImage.enabled = false; // 빈 스프라이트가 없다면 감춤
            }
        }

        SelectedItemBus.Clear();
        if (nameText) nameText.text = "";
        if (typeText) typeText.text = "";
        if (descText) descText.text = "";
    }

    public void Clear() => ResetUI();

    // 슬롯에서 좌클릭된 아이템으로 갱신
    public void Show(Item item)
    {
        if (item == null)
        {
            ResetUI();
            return;
        }

        // 아이콘만 교체
        if (iconImage)
        {
            if (item.itemImage != null)
            {
                iconImage.enabled = true;
                iconImage.sprite = item.itemImage;
                iconImage.color = Color.white;
            }
            else
            {
                if (emptyIconSprite != null)
                {
                    iconImage.enabled = true;
                    iconImage.sprite = emptyIconSprite;
                    iconImage.color = Color.white;
                }
                else iconImage.enabled = false;
            }
        }

    
        if (nameText) nameText.text = item.itemName;
        if (typeText) typeText.text = ToKoreanType(item.itemType);
        // 유물 아이템 description사용. description이 없는 일반 아이템일 경우 itemTale사용
        string description = item.artifactData != null ? item.artifactData.description : item.itemTale;
        if (descText) descText.text = description;
    }

    private string ToKoreanType(Item.ItemType t)
    {
        switch (t)
        {
            case Item.ItemType.Equipment: return "장비";
            case Item.ItemType.Used: return "소비";
            case Item.ItemType.Ingredient: return "재료";
            case Item.ItemType.ETC: return "기타";
            case Item.ItemType.ResearchLog: return "연구 로그";
            case Item.ItemType.Spoils: return "전리품";
            case Item.ItemType.Blueprint: return "청사진";
            case Item.ItemType.TaleObject: return "유물";
            default: return t.ToString();
        }
    }
}
