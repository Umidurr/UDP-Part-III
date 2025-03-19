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
    public PlayerInventory playerInventory;

    // List of shop items
    public List<ShopItem> shopItems;

    // List of equipment items
    public List<ShopItem> equipmentItems;

    // Selected Item
    private ShopItem _selectedItem;
    private VisualElement _selectedItemSlot; // Stores the currently selected UI element

    // Awake: Ensure correct screen resolution
    private void Awake()
    {
        // Set screen resolution (SNES-style resolution @ 4x)
        Screen.SetResolution(1280, 960, false);
    }
    private void PopulateShop()
    {
        PopulateShop(shopItems); // Default to shopItems if no list is provided
    }

    // Start: Initialize the shop UI
    private void Start()
    {
        // Retrieve the root UI element
        _root = doc.rootVisualElement;

        // Ensure playerInventory is assigned
        if (playerInventory == null)
        {
            playerInventory = FindFirstObjectByType<PlayerInventory>();
            if (playerInventory == null)
            {
                UnityEngine.Debug.LogError("ShopManager: PlayerInventory is missing in the scene!");
                return; // Prevent further execution if missing
            }
        }

        // ** Reset stock when entering the game **
        ResetStockForAllItems();

        // Find UI elements using their UXML names
        _playerMoneyLabel = _root.Q<UnityEngine.UIElements.Label>("MoneyTxt");
        _itemListContainer = _root.Q<VisualElement>("ITEMS");

        // Find and assign buttons
        // Find and assign Buy button from UI
        VisualElement buyButton = _root.Q<VisualElement>("BUYSELL");
        if (buyButton != null)
        {
            buyButton.RegisterCallback<ClickEvent>(evt => BuyItem());
        }
        else
        {
            UnityEngine.Debug.LogError("Buy button not found in UI!");
        }

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
        UpdateInventoryDisplay(); // Ensure inventory space is updated

        // ** Add Header Selection and Hover Effects**
        SetupHeaders();
    }


    // Populate the shop item list
    private void PopulateShop(List<ShopItem> itemList)
    {
        for (int i = 0; i < 6; i++)
        {
            if (i >= itemList.Count) break;

            VisualElement itemSlot = _root.Q<VisualElement>($"Item{i + 1}");

            if (itemSlot != null)
            {
                UnityEngine.UIElements.Label itemName = itemSlot.Q<UnityEngine.UIElements.Label>($"ItemName{i + 1}");
                UnityEngine.UIElements.Label itemCost = itemSlot.Q<UnityEngine.UIElements.Label>($"ItemCost{i + 1}");
                VisualElement itemPic = itemSlot.Q<VisualElement>($"ItemPic{i + 1}");

                ShopItem item = itemList[i];

                if (itemName != null) itemName.text = item.itemName;
                if (itemCost != null) itemCost.text = item.isPurchasable ? $"{item.buyPrice}" : "-";

                if (itemPic != null && item.itemIcon != null)
                {
                    itemPic.style.backgroundImage = new StyleBackground(item.itemIcon);
                }

                // Set default background color (Grey - 717171, 212 opacity)
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

    private void RotateShopItems()
    {
        // Check which tab is currently active
        VisualElement itemsHeader = _root.Q<VisualElement>("ITEMS");
        VisualElement equipmentHeader = _root.Q<VisualElement>("EQUIPMENTS");

        if (itemsHeader.ClassListContains("active"))
        {
            // Only rotate if there are more than 6 items
            if (shopItems.Count > 6)
            {
                ShopItem firstItem = shopItems[0];
                shopItems.RemoveAt(0);
                shopItems.Add(firstItem);

                PopulateShop(shopItems); // Re-populate with rotated list
            }
        }
        else if (equipmentHeader.ClassListContains("active"))
        {
            // Only rotate if there are more than 6 equipment items
            if (equipmentItems.Count > 6)
            {
                ShopItem firstItem = equipmentItems[0];
                equipmentItems.RemoveAt(0);
                equipmentItems.Add(firstItem);

                PopulateShop(equipmentItems); // Re-populate with rotated list
            }
        }
    }



    // Select an item from the list
    private void SelectItem(ShopItem item)
    {
        _selectedItem = item;

        // Find UI Elements for description, stock, and users
        UnityEngine.UIElements.Label itemDesc = _root.Q<UnityEngine.UIElements.Label>("ItemDesc");
        UnityEngine.UIElements.Label itemStock = _root.Q<UnityEngine.UIElements.Label>("StockAmt");
        UnityEngine.UIElements.Label ownedAmount = _root.Q<UnityEngine.UIElements.Label>("OwnedAmt");
        UnityEngine.UIElements.Label moneyTxt = _root.Q<UnityEngine.UIElements.Label>("MoneyTxt");
        UnityEngine.UIElements.Label totalSpaceLabel = _root.Q<UnityEngine.UIElements.Label>("TotalSpace");
        UnityEngine.UIElements.Label spaceAmtLabel = _root.Q<UnityEngine.UIElements.Label>("SpaceAmt");

        // Find UI Elements for amount selection
        UnityEngine.UIElements.Label amtLabel = _root.Q<UnityEngine.UIElements.Label>("Amt");
        VisualElement leftArrow = _root.Q<VisualElement>("LeftArrow");
        VisualElement rightArrow = _root.Q<VisualElement>("RightArrow");

        // Set initial amount
        int currentAmount = 1;

        // If item is restricted, set to 00 and disable arrows
        if (!item.isPurchasable)
        {
            amtLabel.text = "00";
            leftArrow.SetEnabled(false);
            rightArrow.SetEnabled(false);
            return;
        }

        // Ensure arrows are enabled if item is purchasable
        leftArrow.SetEnabled(true);
        rightArrow.SetEnabled(true);

        // Set default value to 01
        amtLabel.text = "01";

        // Register Click Events for Left and Right Arrows
        leftArrow.RegisterCallback<ClickEvent>(evt =>
        {
            if (currentAmount == 1)
            {
                currentAmount = 99; // Loop to 99 if at 01
            }
            else
            {
                currentAmount--;
            }
            amtLabel.text = currentAmount.ToString("D2"); // Format as 2-digit
        });

        rightArrow.RegisterCallback<ClickEvent>(evt =>
        {
            if (currentAmount == 99)
            {
                currentAmount = 1; // Reset to 01 if at 99
            }
            else
            {
                currentAmount++;
            }
            amtLabel.text = currentAmount.ToString("D2"); // Format as 2-digit
        });

        // Show the selected item's details
        if (itemDesc != null) itemDesc.text = item.description;
        if (itemStock != null) itemStock.text = $"{item.stock}";

        // Retrieve owned amount from PlayerInventory
        int playerOwned = playerInventory.GetOwnedAmount(item);
        if (ownedAmount != null) ownedAmount.text = $"{playerOwned}";
        if (ownedAmount == null)
        {
            UnityEngine.Debug.LogError("No owned.");
        }

        // Update money display
        if (moneyTxt != null) moneyTxt.text = $"{playerInventory.money}";

        // Update inventory space display
        if (totalSpaceLabel != null) totalSpaceLabel.text = $"/{playerInventory.totalSpace}";
        if (spaceAmtLabel != null) spaceAmtLabel.text = $"{playerInventory.GetUsedSpace()}";

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

    private void SetupHeaders()
    {
        // Get headers as VisualElements
        VisualElement itemsHeader = _root.Q<VisualElement>("ITEMS");
        VisualElement equipmentHeader = _root.Q<VisualElement>("EQUIPMENTS");

        // Get background covers
        VisualElement itemBackCover = _root.Q<VisualElement>("ItemBackCover");
        VisualElement eqBackCover = _root.Q<VisualElement>("EQBackCover");

        // Get child elements that might be intercepting input
        VisualElement itemText = _root.Q<VisualElement>("ItemTxt");
        VisualElement eqText = _root.Q<VisualElement>("EqTxt");
        VisualElement itemRect = _root.Q<VisualElement>("ItemRect");
        VisualElement eqRect = _root.Q<VisualElement>("EQRect");

        if (itemsHeader != null && equipmentHeader != null && itemBackCover != null && eqBackCover != null)
        {
            // Ensure ITEMS and EQUIPMENTS receive clicks
            itemsHeader.pickingMode = PickingMode.Position;
            equipmentHeader.pickingMode = PickingMode.Position;

            // Prevent child elements from blocking clicks
            if (itemText != null) itemText.pickingMode = PickingMode.Ignore;
            if (eqText != null) eqText.pickingMode = PickingMode.Ignore;
            if (itemRect != null) itemRect.pickingMode = PickingMode.Ignore;
            if (eqRect != null) eqRect.pickingMode = PickingMode.Ignore;

            // Fix potential overlapping issues
            itemsHeader.BringToFront(); // Ensures ITEMS is on top
            equipmentHeader.BringToFront();

            // Default states: ITEMS active, EQUIPMENTS inactive
            itemBackCover.style.backgroundColor = new StyleColor(new Color(0.29f, 0.66f, 0.82f, 200f / 255f)); // Active 4AA8D1
            eqBackCover.style.backgroundColor = new StyleColor(new Color(0.44f, 0.44f, 0.44f, 212f / 255f)); // Inactive 717171

            // Hover effect: Light version of active color
            itemsHeader.RegisterCallback<MouseEnterEvent>(evt =>
            {
                if (!itemsHeader.ClassListContains("active"))
                    itemBackCover.style.backgroundColor = new StyleColor(new Color(0.29f, 0.66f, 0.82f, 150f / 255f)); // Lighter 4AA8D1
            });

            itemsHeader.RegisterCallback<MouseLeaveEvent>(evt =>
            {
                if (!itemsHeader.ClassListContains("active"))
                    itemBackCover.style.backgroundColor = new StyleColor(new Color(0.44f, 0.44f, 0.44f, 212f / 255f)); // Reset to 717171
            });

            equipmentHeader.RegisterCallback<MouseEnterEvent>(evt =>
            {
                if (!equipmentHeader.ClassListContains("active"))
                    eqBackCover.style.backgroundColor = new StyleColor(new Color(0.29f, 0.66f, 0.82f, 150f / 255f)); // Lighter 4AA8D1
            });

            equipmentHeader.RegisterCallback<MouseLeaveEvent>(evt =>
            {
                if (!equipmentHeader.ClassListContains("active"))
                    eqBackCover.style.backgroundColor = new StyleColor(new Color(0.44f, 0.44f, 0.44f, 212f / 255f)); // Reset to 717171
            });

            // Click event to switch active tab
            equipmentHeader.RegisterCallback<ClickEvent>(evt => SetActiveTab(equipmentHeader, itemsHeader, eqBackCover, itemBackCover));
            itemsHeader.RegisterCallback<ClickEvent>(evt => SetActiveTab(itemsHeader, equipmentHeader, itemBackCover, eqBackCover));

            // Mark ITEMS as active initially & populate shop with default items
            itemsHeader.AddToClassList("active");
            PopulateShop(shopItems);
        }
    }



    private void SetActiveTab(VisualElement selectedTab, VisualElement otherTab, VisualElement activeBackCover, VisualElement inactiveBackCover)
    {
        // Set selected tab active
        selectedTab.AddToClassList("active");
        otherTab.RemoveFromClassList("active");

        // Active background color (4AA8D1)
        activeBackCover.style.backgroundColor = new StyleColor(new Color(0.29f, 0.66f, 0.82f, 200f / 255f));

        // Inactive background color (717171)
        inactiveBackCover.style.backgroundColor = new StyleColor(new Color(0.44f, 0.44f, 0.44f, 212f / 255f));

        // Determine which list to display
        if (selectedTab.name == "EQUIPMENTS")
        {
            PopulateShop(equipmentItems); // Show equipment items
        }
        else
        {
            PopulateShop(shopItems); // Show default shop items
        }
    }


    // Buy an item
    private void BuyItem()
    {
        if (_selectedItem == null) return;

        // Find UI Elements for space and stock
        UnityEngine.UIElements.Label amtLabel = _root.Q<UnityEngine.UIElements.Label>("Amt");
        int amountToBuy = int.Parse(amtLabel.text); // Convert amount label to int

        // Check if item is purchasable
        if (!_selectedItem.isPurchasable)
        {
            UnityEngine.Debug.LogError("This item cannot be purchased!");
            return;
        }

        // Check stock availability
        if (_selectedItem.stock == 0)
        {
            UnityEngine.Debug.LogError("Out of stock! Cannot purchase.");
            return;
        }

        // Ensure we do not buy more than available stock
        if (amountToBuy > _selectedItem.stock)
        {
            UnityEngine.Debug.LogError("Not enough stock available for this purchase!");
            return;
        }

        // Check inventory space
        int remainingSpace = playerInventory.GetRemainingSpace();
        if (amountToBuy > remainingSpace)
        {
            UnityEngine.Debug.LogError("Not enough space in inventory!");
            return;
        }

        // Restrict based on allowed users (Equipment Only)
        if (_root.Q<VisualElement>("EQUIPMENTS").ClassListContains("active"))
        {
            UserType currentPlayer = playerInventory.GetCurrentPlayer();
            if (!_selectedItem.allowedUsers.Contains(currentPlayer))
            {
                UnityEngine.Debug.LogError("You are not allowed to use this equipment!");
                return;
            }
        }

        // Check if player has enough money
        int totalCost = _selectedItem.buyPrice * amountToBuy;
        if (playerInventory.money < totalCost)
        {
            UnityEngine.Debug.LogError("Not enough money!");
            return;
        }

        // Process purchase
        playerInventory.money -= totalCost;
        playerInventory.AddItem(_selectedItem, amountToBuy);
        _selectedItem.stock -= amountToBuy;

        // Update UI elements
        UpdateMoneyDisplay();
        UpdateInventoryDisplay();
        SelectItem(_selectedItem); // Refresh UI to show new stock count

        UnityEngine.Debug.Log($"Purchased {amountToBuy}x {_selectedItem.itemName}");
    }



    // Sell an item
    private void SellItem()
    {
        if (_selectedItem == null) return; // No item selected

        // Get UI elements
        UnityEngine.UIElements.Label ownedAmountLabel = _root.Q<UnityEngine.UIElements.Label>("OwnedAmt");
        UnityEngine.UIElements.Label amtLabel = _root.Q<UnityEngine.UIElements.Label>("Amt");

        // Get the selected amount from the UI (convert from string to int)
        int sellAmount = int.Parse(amtLabel.text);

        // Get the player's owned quantity from inventory
        int ownedQuantity = playerInventory.GetOwnedAmount(_selectedItem);

        // Prevent selling if the player doesn't own enough
        if (ownedQuantity < sellAmount || sellAmount <= 0)
        {
            UnityEngine.Debug.LogWarning($"Cannot sell {sellAmount}x {_selectedItem.itemName}. You only own {ownedQuantity}.");
            return;
        }

        // Calculate total sell price
        int totalSellPrice = _selectedItem.sellPrice * sellAmount;

        // Add money to player's inventory
        playerInventory.money += totalSellPrice;

        // Remove the sold amount from inventory
        for (int i = 0; i < sellAmount; i++)
        {
            playerInventory.RemoveItem(_selectedItem);
        }

        // Update UI
        UpdateMoneyDisplay();
        UpdateInventoryDisplay();

        // Get the new owned quantity after selling
        int newOwnedQuantity = playerInventory.GetOwnedAmount(_selectedItem);

        // Update the OwnedAmt label in real-time
        if (ownedAmountLabel != null)
        {
            ownedAmountLabel.text = $"{newOwnedQuantity}";
        }

        // Do **NOT** reset `Amt` — keep it the same
        UnityEngine.Debug.Log($"Sold {sellAmount}x {_selectedItem.itemName} for {totalSellPrice}. Remaining: {newOwnedQuantity}");
    }




    // Update the UI display of player money
    private void UpdateMoneyDisplay()
    {
        _playerMoneyLabel.text = $"{playerInventory.money}";
    }

    // Ensure inventory space updates whenever the shop opens or items change
    private void UpdateInventoryDisplay()
    {
        UnityEngine.UIElements.Label totalSpaceLabel = _root.Q<UnityEngine.UIElements.Label>("TotalSpace");
        UnityEngine.UIElements.Label spaceAmtLabel = _root.Q<UnityEngine.UIElements.Label>("SpaceAmt");

        if (totalSpaceLabel != null) totalSpaceLabel.text = $"/{playerInventory.totalSpace}";
        if (spaceAmtLabel != null) spaceAmtLabel.text = $"{playerInventory.GetUsedSpace()}";
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

    // ** New Function to Reset Stock **
    private void ResetStockForAllItems()
    {
        foreach (ShopItem item in shopItems)
        {
            item.ResetStock();
        }

        foreach (ShopItem item in equipmentItems)
        {
            item.ResetStock();
        }
    }
}

