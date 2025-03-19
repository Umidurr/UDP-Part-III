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

    [TextArea] public string description; // New: Item Description
    public int stock; // New: Stock Count
    public List<UserType> allowedUsers; // New: Who can buy it
}
