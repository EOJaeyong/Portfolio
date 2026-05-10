using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName ="New Item", menuName = "New Item/item")]
public class Item : ScriptableObject
{
    public string itemName; // 아이템의 이름
    public ItemType itemType; // 아이템의 유형
    public Sprite itemImage; // 아이템의 이미지
    public GameObject itemPrefab; //아이템의 프리팹
    public GameObject dropPrefab;   // 아이템 드롭 시 사용할 드롭 프리팹
    public float itemValue;     // 아이템의 가치 (단위 : $)
    public string itemTale;     // 아이템 스토리
    public ArtifactData artifactData; // 유물 아이템일 경우 


    public string weaponType;

    public enum ItemType
    {
        Equipment,
        Used,
        Ingredient,
        ETC,
        ResearchLog,
        Spoils,
        Blueprint,
        TaleObject
    }
}
