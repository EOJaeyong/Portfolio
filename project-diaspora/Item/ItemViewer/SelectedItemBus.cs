using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SelectedItemBus
{
    public static Item Current { get; private set; }

    public static void Set(Item item)
    {
        Current = item;
        Debug.Log($"[SelectedItemBus] Current = {(item != null ? item.itemName : "NULL")}");
    }

    public static void Clear()
    {
        Current = null;
        Debug.Log("[SelectedItemBus] Cleared");
    }

}
