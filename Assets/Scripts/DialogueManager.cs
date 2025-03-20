using System.Diagnostics;
using UnityEngine;
using UnityEngine.UIElements;

public class DialogueManager : MonoBehaviour
{
    public UIDocument dialogueUI;
    public UIDocument shopUI;

    private VisualElement _root;

    private UnityEngine.UIElements.Label _dialogueLabel;
    private UnityEngine.UIElements.Label buyLabel;
    private UnityEngine.UIElements.Label sellLabel;

    private Color selectedColor = new Color(1.0f, 0.39f, 0.39f, 100f / 255f); // #70FFC7 (100 opacity)
    private Color unselectedColor = new Color(0.44f, 0.44f, 0.44f, 212f / 255f); // #717171 (212 opacity)

    private enum Selection 
    { 
        Buy, 
        Sell 
    }
    private Selection currentSelection = Selection.Buy;

    private bool shopOpened = false;

    // Reference to ShopManager (to toggle Buy/Sell mode)
    public ShopManager shopManager;

    private void Start()
    {
        _root = dialogueUI.rootVisualElement;

        //ShopManager shopScript = FindFirstObjectByType<ShopManager>();

        //if (shopScript != null)
        //{
        //    shopScript.gameObject.SetActive(false); // Disable its GameObject
        //}
        //else
        //{
        //    UnityEngine.Debug.LogError("ShopManager script is missing in the scene!");
        //}

        // Ensure Shop UI is hidden at the start
        if (shopUI != null)
            shopUI.rootVisualElement.style.display = DisplayStyle.None;

        // Ensure Dialogue UI is visible at the start
        dialogueUI.rootVisualElement.style.display = DisplayStyle.Flex;

        _dialogueLabel = _root.Q<UnityEngine.UIElements.Label>("StartingTxt");
        buyLabel = _root.Q<UnityEngine.UIElements.Label>("BuyTxt");
        sellLabel = _root.Q<UnityEngine.UIElements.Label>("SellTxt");

        // Start the dialogue with the options immediately
        StartDialogue("Ah-ha! A customer!\nWhat would you like to do today?aaaa");

        // Register keyboard input for the dialogue
        _root.RegisterCallback<KeyUpEvent>(OnKeyUp);
    }


    private void StartDialogue(string message)
    {
        _dialogueLabel.text = message;
        _root.style.display = DisplayStyle.Flex; // Show the dialogue UI
    }

    private void OnKeyUp(KeyUpEvent e)
    {
        if (!shopOpened)
        {
            // W key - move up (Buy option)
            if (e.keyCode == KeyCode.W)
            {
                currentSelection = Selection.Buy;
                UpdateSelection();
            }
            // S key - move down (Sell option)
            else if (e.keyCode == KeyCode.S)
            {
                currentSelection = Selection.Sell;
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
            buyLabel.style.color = selectedColor;
            sellLabel.style.color = unselectedColor;
        }
        else
        {
            buyLabel.style.color = unselectedColor;
            sellLabel.style.color = selectedColor;
        }
    }

    void OpenShop()
    {
        if (shopUI != null)
        {
            shopUI.rootVisualElement.style.display = DisplayStyle.Flex; // Show Shop UI
        }

        _root.style.display = DisplayStyle.None; // Hide Dialogue UI
        shopOpened = true; // Prevent further navigation
    }
}
