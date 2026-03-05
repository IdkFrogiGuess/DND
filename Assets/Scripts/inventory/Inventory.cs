using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class Inventory : MonoBehaviour
{
    //Items 
    public ItemSO woodItem;
    public ItemSO axeItem;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    public GameObject hotBarObj;
    public GameObject inventorySlotparent;

    public Image dragIcon;

    private List<Slot> inventorySlots = new List<Slot>();
    private List<Slot> hotBarSlots = new List<Slot>();
    private List<Slot> allSlots = new List<Slot>();

    private Slot draggedSlot = null;
    private bool isDragging = false;


    private void Awake()
    {
        inventorySlots.AddRange(inventorySlotparent.GetComponentsInChildren<Slot>());
        hotBarSlots.AddRange(hotBarObj.GetComponentsInChildren<Slot>());

        allSlots.AddRange(inventorySlots);
        allSlots.AddRange(hotBarSlots);
    }
    public void Pickup(InputAction.CallbackContext context)
    {
        AddItem(woodItem, 3);
    }

    void Update()
    {

    }
    public void AddItem(ItemSO itemToAdd, int amount)
    {
        int remaining = amount;

        foreach (Slot slot in allSlots)
        {
            if (slot.hasItem() && slot.GetItem() == itemToAdd)
            {
                int currentAmount = slot.GetAmount();
                int maxStack = itemToAdd.maxStackSize;

                if (currentAmount < maxStack)
                {
                    int spaceLeft = maxStack - currentAmount;
                    int amountToAdd = Mathf.Min(spaceLeft, remaining);

                    slot.SetItem(itemToAdd, currentAmount + amountToAdd);
                    remaining -= amountToAdd;

                    if (remaining <= 0)
                        return;

                }

            }
        }
        foreach (Slot slot in allSlots)
        {
            if (!slot.hasItem())
            {
                int amountToPlace = Mathf.Min(itemToAdd.maxStackSize, remaining);
                slot.SetItem(itemToAdd, amountToPlace);
                remaining -= amountToPlace;

                if (remaining <= 0)
                    return;

                if (remaining > 0)
                {
                    Debug.LogWarning("Inventory is full, cant add" + remaining + "of" + itemToAdd.itemName);
                }
            }
        }
    }

    public void StartDrag(InputAction.CallbackContext context)
    {
        Slot hovered = GetHoveredSlot();
        if (hovered != null && hovered.hasItem())
        {
            draggedSlot = hovered;
            isDragging = true;
            dragIcon.enabled = true;

            dragIcon.sprite = hovered.GetItem().itemIcon;
            dragIcon.color = new Color(1, 1, 1, 0.5f);
        }
    }

   // public void EndDrag(InputAction.CallbackContext context)
   // {
   //     Slot slot = GetHoveredSlot();

    //    if(hovered != null)
//{
     //       HandleDrop(draggedSlot, hovered);

    //        dragIcon.enabled = false;
  //      }
        
 //   }

    private Slot GetHoveredSlot()
    {
        foreach (Slot s in allSlots)
        {
            if (s.hovering)
            {
                return s;
            }
        }
        return null;
    }
}




