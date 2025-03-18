using System.Collections.Generic;
using UnityEngine;

public class Inventory : MonoBehaviour
{
    public int money = 10000;
    public List<ShopItem> ownedItems = new List<ShopItem>();

    public void AddItem(ShopItem item)
    {
        ownedItems.Add(item);
    }

    public void RemoveItem(ShopItem item)
    {
        ownedItems.Remove(item);
    }

    public bool HasItem(ShopItem item)
    {
        return ownedItems.Contains(item);
    }
}