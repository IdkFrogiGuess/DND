using UnityEngine;
using TMPro;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class Slot : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
   public bool hovering;

    private ItemSO heldItem;
    private int itemAmount;

        private Image iconImage;
    private TextMeshProUGUI amountText;


    private void Awake()
    {
        iconImage = transform.GetChild(0).GetComponent<Image>();
        amountText = transform.GetChild(1).GetComponent<TextMeshProUGUI>();
    }

    public ItemSO GetItem()
    {
        return heldItem;
    }

    public int GetAmount()
    {
        return itemAmount;
    }

    public void SetItem(ItemSO item, int amount = 1)
    {
        heldItem = item;
        itemAmount = amount;

        UpdateSlot();
    }
    public void UpdateSlot()
    {
        if (heldItem != null)
        {
            iconImage.sprite = heldItem.itemIcon;
            iconImage.enabled = true;
            amountText.text = itemAmount.ToString();
        }
        else
        {
            iconImage.sprite = null;
            iconImage.enabled = false;
            amountText.text = "";
        }
    }

    public int AddAmount(int amounttoAdd)
    {
        itemAmount += amounttoAdd;
        UpdateSlot();
        return itemAmount;
    }

    public int RemoveAmount(int amountToRemove)
    {
        itemAmount -= amountToRemove;
        if (itemAmount <= 0)
        {
            ClearSlot();
           
        }
        else
        {
            UpdateSlot();
        }

        return itemAmount;


    }
    public void ClearSlot()
    {
        heldItem = null;
        itemAmount = 0;
        UpdateSlot();
    }

    public bool hasItem()
    {
        return heldItem != null;
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
       hovering = true;
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        hovering = false;
    }
}
