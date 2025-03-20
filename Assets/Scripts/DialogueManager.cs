using System.Diagnostics;
using System.Security.Cryptography;
using UnityEngine;
using UnityEngine.UIElements;

public class DialogueManager : MonoBehaviour
{
    public UIDocument dialogueUI;
    public UIDocument shopUI;

    public Transform player;    // Reference to the player
    public Transform merchant;  // Reference to the merchant
    public float interactionDistance = 1.5f; // Distance required to interact

    private VisualElement _root;

    private UnityEngine.UIElements.Label _dialogueLabel;
    private UnityEngine.UIElements.Label buyLabel;
    private UnityEngine.UIElements.Label sellLabel;

    private Color selectedColor = new Color(0.44f, 1.0f, 0.78f, 100f / 255f); // #70FFC7 (100 opacity)
    private Color unselectedColor = new Color(0.44f, 0.44f, 0.44f, 212f / 255f); // #717171 (212 opacity)

    private enum Selection { Buy, Sell }
    private Selection currentSelection = Selection.Buy;

    public ShopManager shopManager;

    private bool isDialogueActive = false; // Track if dialogue is currently shown

    private void Start()
    {
        _root = dialogueUI.rootVisualElement;

        // Ensure Shop UI is hidden at the start
        if (shopUI != null)
            shopUI.rootVisualElement.style.display = DisplayStyle.None;

        // Ensure Dialogue UI is hidden at the start
        dialogueUI.rootVisualElement.style.display = DisplayStyle.None;

        _dialogueLabel = _root.Q<UnityEngine.UIElements.Label>("StartingTxt");
        buyLabel = _root.Q<UnityEngine.UIElements.Label>("BuyTxt");
        sellLabel = _root.Q<UnityEngine.UIElements.Label>("SellTxt");

        // Ensure shopManager is assigned
        if (shopManager == null)
        {
            shopManager = FindFirstObjectByType<ShopManager>();
            if (shopManager == null)
            {
                UnityEngine.Debug.LogError("DialogueManager: shopManager is missing in the scene!");
                return; // Prevent further execution if missing
            }
        }

        // Register keyboard input for the dialogue
        _root.RegisterCallback<KeyUpEvent>(OnKeyUp);

        // Allow the root UI element to be focusable
        _root.focusable = true;
        _root.Focus();
    }

    private void Update()
    {
        // Check if player is close enough to interact
        if (!isDialogueActive && Vector2.Distance(player.position, merchant.position) < interactionDistance)
        {
            if (Input.GetKeyDown(KeyCode.L))
            {
                ShowDialogue();
            }
        }
    }

    private void ShowDialogue()
    {
        dialogueUI.rootVisualElement.style.display = DisplayStyle.Flex; // Show Dialogue UI
        isDialogueActive = true;

        _dialogueLabel.text = "Ah-ha! A customer!\nWhat would you like to do today?";

        // Allow the root UI element to be focusable
        _root.focusable = true;
        _root.Focus();
    }

    private void OnKeyUp(KeyUpEvent e)
    {
        if (isDialogueActive) // Only allow input if dialogue is open
        {
            // W key - move up (Buy option)
            if (e.keyCode == KeyCode.W)
            {
                currentSelection = Selection.Buy;
                shopManager.ToggleBuySellMode(false);
                UpdateSelection();
            }
            // S key - move down (Sell option)
            else if (e.keyCode == KeyCode.S)
            {
                currentSelection = Selection.Sell;
                shopManager.ToggleBuySellMode(true);
                UpdateSelection();
            }
            // L key - confirm the selection (Buy/Sell)
            else if (e.keyCode == KeyCode.L)
            {
                OpenShop();
            }
        }
    }

    void UpdateSelection()
    {
        if (currentSelection == Selection.Buy)
        {
            buyLabel.style.backgroundColor = selectedColor;
            sellLabel.style.backgroundColor = unselectedColor;
        }
        else
        {
            buyLabel.style.backgroundColor = unselectedColor;
            sellLabel.style.backgroundColor = selectedColor;
        }
    }

    void OpenShop()
    {
        if (shopUI != null)
        {
            shopUI.rootVisualElement.style.display = DisplayStyle.Flex; // Show Shop UI
        }

        _root.style.display = DisplayStyle.None; // Hide Dialogue UI
        isDialogueActive = false; // Reset dialogue state
    }
}
