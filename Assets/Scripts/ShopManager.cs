using System.Collections.Generic;
// using System.Reflection.Emit;
using UnityEngine;
using UnityEngine.UIElements;

public class ShopManager : MonoBehaviour
{
    // UI Document assigned in the Inspector
    public UIDocument doc;

    // Root UI element
    private VisualElement _root;

    // UI Elements
    private Label _playerMoneyLabel;
    private VisualElement _itemListContainer;

    // Buttons
    private Button _buyButton;
    private Button _sellButton;

    // Player inventory reference
    public Inventory playerInventory;

    // List of shop items
    public List<ShopItem> shopItems;

    // Selected Item
    private ShopItem _selectedItem;

    // Awake: Ensure correct screen resolution
    private void Awake()
    {
        // Set screen resolution (SNES-style resolution @ 4x)
        Screen.SetResolution(1280, 960, false);
    }

    // Start: Initialize the shop UI
    private void Start()
    {
        // Retrieve the root UI element
        _root = doc.rootVisualElement;

        // Find UI elements using their UXML names
        _playerMoneyLabel = _root.Q<Label>("MoneyLabel");
        _itemListContainer = _root.Q<VisualElement>("ITEMS");

        // Find and assign buttons
        _buyButton = _root.Q<Button>("BuyButton");
        _sellButton = _root.Q<Button>("SellButton");

        // Register button events
        //_buyButton.clicked += () => BuyItem();
        //_sellButton.clicked += () => SellItem();

        // Register keyboard shortcuts
        _root.RegisterCallback<KeyUpEvent>(OnKeyUp);

        // Allow the root UI element to be focusable
        _root.focusable = true;
        _root.Focus();

        // Initialize the shop
        PopulateShop();
        UpdateMoneyDisplay();
    }

    // Populate the shop item list
    private void PopulateShop()
    {
        // Loop through available item slots (assuming you have exactly 6)
        for (int i = 0; i < 6; i++)
        {
            // Ensure we don't exceed available shop items
            if (i >= shopItems.Count)
                break;

            // Get the item slot by name (Item1, Item2, ..., Item6)
            VisualElement itemSlot = _root.Q<VisualElement>($"Item{i + 1}");

            if (itemSlot != null)
            {
                // Find child elements within the item slot
                Label itemName = itemSlot.Q<Label>($"ItemName{i + 1}");
                Label itemCost = itemSlot.Q<Label>($"ItemCost{i + 1}");
                VisualElement itemPic = itemSlot.Q<VisualElement>($"ItemPic{i + 1}");

                // Update the UI with shop item data
                ShopItem item = shopItems[i];

                if (itemName != null) itemName.text = item.itemName;
                if (itemCost != null) itemCost.text = item.isPurchasable ? $"${item.buyPrice}" : "$ -";

                // Assuming itemPic is meant to display an image, set its background
                if (itemPic != null && item.itemIcon != null)
                {
                    itemPic.style.backgroundImage = new StyleBackground(item.itemIcon);
                }
            }
        }
    }



    // Select an item from the list
    private void SelectItem(ShopItem item)
    {
        _selectedItem = item;
        Debug.Log("Selected: " + item.itemName);
    }

    // Buy an item
    private void BuyItem()
    {
        if (_selectedItem == null) return;

        if (_selectedItem.isPurchasable && playerInventory.money >= _selectedItem.buyPrice)
        {
            playerInventory.money -= _selectedItem.buyPrice;
            playerInventory.AddItem(_selectedItem);
            UpdateMoneyDisplay();
            Debug.Log("Bought: " + _selectedItem.itemName);
        }
    }

    // Sell an item
    private void SellItem()
    {
        if (_selectedItem == null) return;

        if (playerInventory.HasItem(_selectedItem))
        {
            playerInventory.money += _selectedItem.sellPrice;
            playerInventory.RemoveItem(_selectedItem);
            UpdateMoneyDisplay();
            Debug.Log("Sold: " + _selectedItem.itemName);
        }
    }

    // Update the UI display of player money
    private void UpdateMoneyDisplay()
    {
        _playerMoneyLabel.text = "$" + playerInventory.money;
    }

    // Handle keyboard shortcuts
    private void OnKeyUp(KeyUpEvent e)
    {
        if (e.keyCode == KeyCode.B) // Press 'B' to Buy
        {
            BuyItem();
        }
        else if (e.keyCode == KeyCode.S) // Press 'S' to Sell
        {
            SellItem();
        }
    }
}
