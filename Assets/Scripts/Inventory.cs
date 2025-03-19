using System.Collections.Generic;
using UnityEngine;

public class PlayerInventory : MonoBehaviour
{
    public int money = 100000; // Player's starting money

    // Unity does NOT support serializing Dictionaries in the Inspector, so we use a List
    public List<InventoryItem> ownedItemsList = new List<InventoryItem>();

    // Dictionary for fast lookup, but NOT serialized
    private Dictionary<ShopItem, InventoryItem> ownedItems = new Dictionary<ShopItem, InventoryItem>();

    private void Awake()
    {
        // Sync list to dictionary at start
        SyncListToDictionary();
    }

    /// <summary>
    /// Synchronizes the List and Dictionary on Awake/Start.
    /// </summary>
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

    /// <summary>
    /// Add an item to the inventory, increasing quantity if already owned.
    /// </summary>
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

    /// <summary>
    /// Remove an item, deleting it from inventory if quantity reaches 0.
    /// </summary>
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

    /// <summary>
    /// Get how many of an item the player owns.
    /// </summary>
    public int GetOwnedAmount(ShopItem item)
    {
        return ownedItems.ContainsKey(item) ? ownedItems[item].quantity : 0;
    }

    /// <summary>
    /// Check if the player has an item.
    /// </summary>
    public bool HasItem(ShopItem item)
    {
        return ownedItems.ContainsKey(item) && ownedItems[item].quantity > 0;
    }
}
