using System.Collections.Generic;
using UnityEngine;

public enum UserType
{
    Randi,
    Purim,
    Popoi,
}

[CreateAssetMenu(fileName = "NewItem", menuName = "Shop/Item")]
public class ShopItem : ScriptableObject
{
    public string itemName;
    public Sprite itemIcon;
    public int buyPrice;
    public int sellPrice;
    public bool isPurchasable;
    public int stock; // Current stock
    public int initialStock; // Store original stock

    [TextArea] public string description; // New: Item Description
    public List<UserType> allowedUsers; // New: Who can buy it

    private void OnEnable()
    {
        // Ensure initialStock is set only once when the ScriptableObject is loaded
        if (initialStock == 0)
        {
            initialStock = stock;
        }
    }

    public void ResetStock()
    {
        stock = initialStock; // Reset stock when entering the game
    }
}
