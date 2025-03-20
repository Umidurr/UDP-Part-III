using UnityEngine;
using UnityEngine.UIElements;

public class DialogueManager : MonoBehaviour
{
    public UIDocument doc;
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

    private Button _continueButton;

    private int selectedOption = 0; // 0: Buy, 1: Sell
    private bool isDialogueActive = false;

    // Reference to ShopManager (to toggle Buy/Sell mode)
    //public ShopManager shopManager;

    private void Start()
    {
        // Check if doc or shopManager is null
        if (doc == null)
        {
            Debug.LogError("UIDocument reference is missing!");
        }
        //if (shopManager == null)
        //{
        //    Debug.LogError("ShopManager reference is missing!");
        //}

        _root = doc.rootVisualElement;

        // Check if _root was initialized
        if (_root == null)
        {
            Debug.LogError("Root VisualElement is null!");
        }

        _dialogueLabel = _root.Q<UnityEngine.UIElements.Label>("StartingTxt");
        buyLabel = _root.Q<UnityEngine.UIElements.Label>("BuyTxt");
        sellLabel = _root.Q<UnityEngine.UIElements.Label>("SellTxt");

        //_continueButton = _root.Q<Button>("ContinueButton");

        // Start the dialogue with the options immediately
        StartDialogue("Ah-ha! A customer!\nWhat would you like to do today?aaaa");

        // Register keyboard input for the dialogue
        _root.RegisterCallback<KeyUpEvent>(OnKeyUp);
    }


    private void StartDialogue(string message)
    {
        _dialogueLabel.text = message;
        _root.style.display = DisplayStyle.Flex; // Show the dialogue UI
        isDialogueActive = true;
    }

    private void OnKeyUp(KeyUpEvent e)
    {
        if (isDialogueActive)
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

            //// L key - confirm the selection (Buy/Sell)
            //else if (e.keyCode == KeyCode.L)
            //{
            //    //ConfirmDialogueSelection();
            //}
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

    //private void ConfirmDialogueSelection()
    //{
    //    // Hide the dialogue UI
    //    _root.style.display = DisplayStyle.None;
    //    isDialogueActive = false;

    //    // Trigger Buy/Sell mode in the ShopManager based on the selection
    //    if (selectedOption == 0)
    //    {
    //        shopManager.ToggleBuySellMode(); // Set to Buy mode
    //    }
    //    else if (selectedOption == 1)
    //    {
    //        shopManager.ToggleBuySellMode(); // Set to Sell mode
    //    }

    //    // Optionally, you can also add logic to reset the UI or add transitions before the shop UI opens.
    //}
}
