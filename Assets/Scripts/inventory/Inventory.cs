using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class Inventory : MonoBehaviour
{
    //Items 
    public ItemSO woodItem;
    public ItemSO axeItem;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    public GameObject hotBarObj;
    public GameObject inventorySlotparent;

    private List<Slot> inventorySlots = new List<Slot>();
    private List<Slot> hotBarSlots = new List<Slot>();
    private List<Slot> allSlots = new List<Slot>();

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
            if(slot.hasItem() && slot.GetItem() == itemToAdd)
            {
                int currentAmount = slot.GetAmount();
                int maxStack = itemToAdd.maxStackSize;

                if (currentAmount < maxStack)
                {
                    int spaceLeft = maxStack - currentAmount;
                    int amountToAdd = Mathf.Min(spaceLeft, remaining);

                    slot.SetItem(itemToAdd, currentAmount + amountToAdd);
                    remaining -= amountToAdd;

                    if(remaining <= 0)
                        return;

                }

            }
        }
        foreach(Slot slot in allSlots)
        {
            if (!slot.hasItem())
            {
                int amountToPlace = Mathf.Min(itemToAdd.maxStackSize, remaining);
                slot.SetItem(itemToAdd, amountToPlace);
                remaining -= amountToPlace;

                if (remaining <= 0)
                    return;

                if(remaining > 0)
                {
                    Debug.LogWarning("Inventory is full, cant add" + remaining + "of" + itemToAdd.itemName);
                }
            }
        }
    }
}
