using UnityEngine;
using UnityEngine.UIElements;

public class DialogueManager : MonoBehaviour
{
    public UIDocument doc;
    private VisualElement _root;
    private UnityEngine.UIElements.Label _dialogueLabel;
    private Button _continueButton;
    private int selectedOption = 0; // 0: Buy, 1: Sell
    private bool isDialogueActive = false;

    // Reference to ShopManager (to toggle Buy/Sell mode)
    public ShopManager shopManager;

    private void Start()
{
    // Check if doc or shopManager is null
    if (doc == null)
    {
        Debug.LogError("UIDocument reference is missing!");
    }
    if (shopManager == null)
    {
        Debug.LogError("ShopManager reference is missing!");
    }

    _root = doc.rootVisualElement;

    // Check if _root was initialized
    if (_root == null)
    {
        Debug.LogError("Root VisualElement is null!");
    }

    _dialogueLabel = _root.Q<UnityEngine.UIElements.Label>("DialogueLabel");
    _continueButton = _root.Q<Button>("ContinueButton");

    // Start the dialogue with the options immediately
    StartDialogue("Ah-ha! A customer! Here to spend your hard earned coins or looking to make some back?");

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
                selectedOption = 0;
                UpdateDialogueSelection();
            }
            // S key - move down (Sell option)
            else if (e.keyCode == KeyCode.S)
            {
                selectedOption = 1;
                UpdateDialogueSelection();
            }
            // L key - confirm the selection (Buy/Sell)
            else if (e.keyCode == KeyCode.L)
            {
                ConfirmDialogueSelection();
            }
        }
    }

    private void UpdateDialogueSelection()
    {
        // Update the UI to show which option is selected
        if (selectedOption == 0)
        {
            _dialogueLabel.text = "You selected: Buy\nPress L to confirm.";
        }
        else if (selectedOption == 1)
        {
            _dialogueLabel.text = "You selected: Sell\nPress L to confirm.";
        }
    }

    private void ConfirmDialogueSelection()
    {
        // Hide the dialogue UI
        _root.style.display = DisplayStyle.None;
        isDialogueActive = false;

        // Trigger Buy/Sell mode in the ShopManager based on the selection
        if (selectedOption == 0)
        {
            shopManager.ToggleBuySellMode(); // Set to Buy mode
        }
        else if (selectedOption == 1)
        {
            shopManager.ToggleBuySellMode(); // Set to Sell mode
        }

        // Optionally, you can also add logic to reset the UI or add transitions before the shop UI opens.
    }
}
