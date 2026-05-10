using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SelectedItemTracker : MonoBehaviour
{
    void OnEnable()
    {
        Slot.OnAnySlotLeftClicked += HandleSlotClicked;
    }

    void OnDisable()
    {
        Slot.OnAnySlotLeftClicked -= HandleSlotClicked;
    }

    private void HandleSlotClicked(Item item)
    {
        SelectedItemBus.Set(item);
    }
}
