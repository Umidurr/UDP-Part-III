using System.Collections.Generic;
using UnityEngine;

public class PlayerInventory : MonoBehaviour
{
    public int money = 100000; // Player's starting money
    public int totalSpace = 20;

    // Unity does NOT support serializing Dictionaries in the Inspector, so we use a List
    public List<InventoryItem> ownedItemsList = new List<InventoryItem>();

    // Dictionary for fast lookup, but NOT serialized
    private Dictionary<ShopItem, InventoryItem> ownedItems = new Dictionary<ShopItem, InventoryItem>();

    private void Awake()
    {
        // Sync list to dictionary at start
        SyncListToDictionary();
    }

    // Synchronizes the List and Dictionary on Awake/Start.
    private void SyncListToDictionary()
    {
        ownedItems.Clear();
        foreach (var inventoryItem in ownedItemsList)
        {
            if (inventoryItem.item != null)
            {
                ownedItems[inventoryItem.item] = inventoryItem;
            }
        }
    }

    // Add an item to the inventory, increasing quantity if already owned.
    public void AddItem(ShopItem item)
    {
        if (ownedItems.ContainsKey(item))
        {
            ownedItems[item].quantity++;
        }
        else
        {
            InventoryItem newItem = new InventoryItem { item = item, quantity = 1 };
            ownedItems[item] = newItem;
            ownedItemsList.Add(newItem); // Keep the List in sync
        }
    }

    // Remove an item, deleting it from inventory if quantity reaches 0.
    public void RemoveItem(ShopItem item)
    {
        if (ownedItems.ContainsKey(item))
        {
            ownedItems[item].quantity--;
            if (ownedItems[item].quantity <= 0)
            {
                ownedItemsList.Remove(ownedItems[item]); // Remove from List
                ownedItems.Remove(item); // Remove from Dictionary
            }
        }
    }

    // Get how many of an item the player owns.
    public int GetOwnedAmount(ShopItem item)
    {
        return ownedItems.ContainsKey(item) ? ownedItems[item].quantity : 0;
    }

    // Check if the player has an item.
    public bool HasItem(ShopItem item)
    {
        return ownedItems.ContainsKey(item) && ownedItems[item].quantity > 0;
    }

    // Get total used inventory space
    public int GetUsedSpace()
    {
        int usedSpace = 0;

        // Iterate through owned items and sum up their quantities
        foreach (KeyValuePair<ShopItem, InventoryItem> entry in ownedItems)
        {
            usedSpace += entry.Value.quantity; // Correctly accessing the quantity
        }

        return usedSpace;
    }

    public int GetRemainingSpace()
    {
        return totalSpace - GetUsedSpace();
    }


}
