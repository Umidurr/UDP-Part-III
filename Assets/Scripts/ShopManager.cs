using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection.Emit;
using UnityEngine;
using UnityEngine.UIElements;

public class ShopManager : MonoBehaviour
{
    // UI Document assigned in the Inspector
    public UIDocument doc;

    // Root UI element
    private VisualElement _root;

    // UI Elements
    private UnityEngine.UIElements.Label _playerMoneyLabel;
    private VisualElement _itemListContainer;

    // Buttons
    private Button _buyButton;
    private Button _sellButton;
    private Button _rotateButton; 

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
        _playerMoneyLabel = _root.Q<UnityEngine.UIElements.Label>("MoneyLabel");
        _itemListContainer = _root.Q<VisualElement>("ITEMS");

        // Find and assign buttons
        _buyButton = _root.Q<Button>("BuyButton");
        _sellButton = _root.Q<Button>("SellButton");

        // Register button events
        //_buyButton.clicked += () => BuyItem();
        //_sellButton.clicked += () => SellItem();

        // Initialize UI Elements for item details
        UnityEngine.UIElements.Label itemDesc = _root.Q<UnityEngine.UIElements.Label>("ItemDesc");
        UnityEngine.UIElements.Label itemStock = _root.Q<UnityEngine.UIElements.Label>("StockAmt");

        if (itemDesc != null) itemDesc.text = ""; // Ensure it's empty on start
        if (itemStock != null) itemStock.text = ""; // Hide stock count initially

        // Find the arrow-down element
        var arrowDown = _root.Q<VisualElement>("ArrowDown");
        if (arrowDown != null)
        {
            arrowDown.RegisterCallback<ClickEvent>(evt => RotateShopItems());
        }

        // Register keyboard shortcuts
        _root.RegisterCallback<KeyUpEvent>(OnKeyUp);

        // Allow the root UI element to be focusable
        _root.focusable = true;
        _root.Focus();

        // Initialize the shop
        PopulateShop();
        UpdateMoneyDisplay();

        PrintAllUIElements(); // This will list every element in the UI

        // ** Add Header Selection and Hover Effects**
        SetupHeaders();
    }

    private void PrintAllUIElements()
    {
        UnityEngine.Debug.Log("Printing all UI elements in the document:");

        foreach (var child in _root.Children())
        {
            UnityEngine.Debug.Log("Found element: " + child.name);
        }
    }


    // Populate the shop item list
    private void PopulateShop()
    {
        for (int i = 0; i < 6; i++)
        {
            if (i >= shopItems.Count) break;

            VisualElement itemSlot = _root.Q<VisualElement>($"Item{i + 1}");

            if (itemSlot != null)
            {
                UnityEngine.UIElements.Label itemName = itemSlot.Q<UnityEngine.UIElements.Label>($"ItemName{i + 1}");
                UnityEngine.UIElements.Label itemCost = itemSlot.Q<UnityEngine.UIElements.Label>($"ItemCost{i + 1}");
                VisualElement itemPic = itemSlot.Q<VisualElement>($"ItemPic{i + 1}");

                ShopItem item = shopItems[i];

                if (itemName != null) itemName.text = item.itemName;
                if (itemCost != null) itemCost.text = item.isPurchasable ? $"{item.buyPrice}" : "-";

                if (itemPic != null && item.itemIcon != null)
                {
                    itemPic.style.backgroundImage = new StyleBackground(item.itemIcon);
                }

                // Set default background color (Nothing - 717171, 212 opacity)
                itemSlot.style.backgroundColor = new StyleColor(new Color(0.44f, 0.44f, 0.44f, 212f / 255f));

                // Set Restricted color (FF6464, 100 opacity) if item is not purchasable
                if (!item.isPurchasable)
                {
                    itemSlot.style.backgroundColor = new StyleColor(new Color(1.0f, 0.39f, 0.39f, 100f / 255f)); // FF6464, 100 opacity
                }

                // Apply hover effect (Hovered -> 70FFC7, 100 opacity)
                itemSlot.RegisterCallback<MouseEnterEvent>(evt =>
                {
                    itemSlot.style.backgroundColor = new StyleColor(new Color(0.44f, 1.0f, 0.78f, 1.0f)); // 70FFC7, 100 opacity
                });

                // Restore previous state on exit
                itemSlot.RegisterCallback<MouseLeaveEvent>(evt =>
                {
                    if (!item.isPurchasable)
                    {
                        itemSlot.style.backgroundColor = new StyleColor(new Color(1.0f, 0.39f, 0.39f, 100f / 255f)); // FF6464, 100 opacity
                    }
                    else
                    {
                        itemSlot.style.backgroundColor = new StyleColor(new Color(0.44f, 0.44f, 0.44f, 212f / 255f)); // 717171, 212 opacity
                    }
                });

                // Attach Click Event to Select the Item
                itemSlot.RegisterCallback<ClickEvent>(evt => SelectItem(item));
            }
        }
    }

    //private void PopulateShop()
    //{
    //    for (int i = 0; i < 6; i++)
    //    {
    //        if (i >= shopItems.Count) break;

    //        VisualElement itemSlot = _root.Q<VisualElement>($"Item{i + 1}");

    //        if (itemSlot != null)
    //        {
    //            UnityEngine.UIElements.Label itemName = itemSlot.Q<UnityEngine.UIElements.Label>($"ItemName{i + 1}");
    //            UnityEngine.UIElements.Label itemCost = itemSlot.Q<UnityEngine.UIElements.Label>($"ItemCost{i + 1}");
    //            VisualElement itemPic = itemSlot.Q<VisualElement>($"ItemPic{i + 1}");

    //            ShopItem item = shopItems[i];

    //            if (itemName != null) itemName.text = item.itemName;
    //            if (itemCost != null) itemCost.text = item.isPurchasable ? $"{item.buyPrice}" : " -";

    //            // Apply red overlay if not purchasable
    //            itemSlot.style.backgroundColor = new StyleColor(item.isPurchasable
    //                ? new Color(0.44f, 0.44f, 0.44f, 212f / 255f) // Grey
    //                : new Color(1.0f, 0.39f, 0.39f, 1.0f)); // Red

    //            if (itemPic != null && item.itemIcon != null)
    //            {
    //                itemPic.style.backgroundImage = new StyleBackground(item.itemIcon);
    //            }

    //            // **NEW: Attach Click Event to Select the Item**
    //            itemSlot.RegisterCallback<ClickEvent>(evt => SelectItem(item));
    //        }
    //    }
    //}



    private void RotateShopItems()
    {
        if (shopItems.Count <= 6) return; // No need to rotate if there are only 6 or fewer items

        // Remove the first item and push the rest forward
        ShopItem firstItem = shopItems[0];
        shopItems.RemoveAt(0);
        shopItems.Add(firstItem); // Move the first item to the end

        // Re-populate the UI with the updated list
        PopulateShop();
    }


    // Select an item from the list
    private void SelectItem(ShopItem item)
    {
        _selectedItem = item;

        // Find UI Elements for description, stock, and users
        UnityEngine.UIElements.Label itemDesc = _root.Q<UnityEngine.UIElements.Label>("ItemDesc");
        UnityEngine.UIElements.Label itemStock = _root.Q<UnityEngine.UIElements.Label>("StockAmt");

        // Show the selected item's details
        if (itemDesc != null) itemDesc.text = item.description;
        if (itemStock != null) itemStock.text = $"Stock: {item.stock}";

        // Update user usability visuals
        VisualElement randiIcon = _root.Q<VisualElement>("Randi");
        VisualElement purimIcon = _root.Q<VisualElement>("Purim");
        VisualElement popoiIcon = _root.Q<VisualElement>("Popoi");

        if (randiIcon != null)
            randiIcon.style.opacity = item.allowedUsers.Contains(UserType.Randi) ? 1f : 0f;
        if (purimIcon != null)
            purimIcon.style.opacity = item.allowedUsers.Contains(UserType.Purim) ? 1f : 0f;
        if (popoiIcon != null)
            popoiIcon.style.opacity = item.allowedUsers.Contains(UserType.Popoi) ? 1f : 0f;
    }

    // Set up headers
    private void SetupHeaders()
    {
        VisualElement itemsHeader = _root.Q<VisualElement>("ITEMS");
        VisualElement equipmentsHeader = _root.Q<VisualElement>("EQUIPMENTS");

        if (itemsHeader == null || equipmentsHeader == null)
        {
            UnityEngine.Debug.LogError("ERROR: ITEMS or EQUIPMENTS header NOT FOUND in UI!");
            return;
        }

        UnityEngine.Debug.Log("Headers found: ITEMS and EQUIPMENTS");

        // **Force them to accept input**
        itemsHeader.pickingMode = PickingMode.Position;
        equipmentsHeader.pickingMode = PickingMode.Position;

        // **Add Debugs for Hover**
        itemsHeader.RegisterCallback<MouseEnterEvent>(evt =>
        {
            UnityEngine.Debug.Log("Hovering over ITEMS tab");
            itemsHeader.style.backgroundColor = new StyleColor(new Color(0.29f, 0.66f, 0.82f, 200f / 255f)); // 4AA8D1
        });

        equipmentsHeader.RegisterCallback<MouseEnterEvent>(evt =>
        {
            UnityEngine.Debug.Log("Hovering over EQUIPMENTS tab");
            equipmentsHeader.style.backgroundColor = new StyleColor(new Color(0.29f, 0.66f, 0.82f, 200f / 255f)); // 4AA8D1
        });

        // **Restore inactive state when not hovered**
        itemsHeader.RegisterCallback<MouseLeaveEvent>(evt =>
        {
            UnityEngine.Debug.Log("Stopped hovering over ITEMS tab");
            if (!itemsHeader.ClassListContains("active"))
            {
                itemsHeader.style.backgroundColor = new StyleColor(new Color(0.44f, 0.44f, 0.44f, 212f / 255f)); // 717171
            }
        });

        equipmentsHeader.RegisterCallback<MouseLeaveEvent>(evt =>
        {
            UnityEngine.Debug.Log("Stopped hovering over EQUIPMENTS tab");
            if (!equipmentsHeader.ClassListContains("active"))
            {
                equipmentsHeader.style.backgroundColor = new StyleColor(new Color(0.44f, 0.44f, 0.44f, 212f / 255f)); // 717171
            }
        });

        // **Add Debugs for Clicks**
        itemsHeader.RegisterCallback<ClickEvent>(evt =>
        {
            UnityEngine.Debug.Log("ITEMS tab clicked");
            SetActiveTab(itemsHeader, equipmentsHeader);
        });

        equipmentsHeader.RegisterCallback<ClickEvent>(evt =>
        {
            UnityEngine.Debug.Log("EQUIPMENTS tab clicked");
            SetActiveTab(equipmentsHeader, itemsHeader);
        });
    }

    // Set an active tab
    private void SetActiveTab(VisualElement selectedTab, VisualElement otherTab)
    {
        // Active Tab Color (4AA8D1, 200 opacity)
        selectedTab.style.backgroundColor = new StyleColor(new Color(0.29f, 0.66f, 0.82f, 200f / 255f)); // 4AA8D1, 200 opacity
        selectedTab.AddToClassList("active");

        // Inactive Tab Color (717171, 212 opacity)
        otherTab.style.backgroundColor = new StyleColor(new Color(0.44f, 0.44f, 0.44f, 212f / 255f)); // 717171, 212 opacity
        otherTab.RemoveFromClassList("active");
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
            UnityEngine.Debug.LogError("Bought: " + _selectedItem.itemName);
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
            UnityEngine.Debug.LogError("Sold: " + _selectedItem.itemName);
        }
    }

    // Update the UI display of player money
    private void UpdateMoneyDisplay()
    {
        //_playerMoneyLabel.text = "$" + playerInventory.money;
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
