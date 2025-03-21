using System.Diagnostics;
using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    public float speed = 3.0f; // Movement speed
    public float minY = -2.94f; // Minimum Y position
    public float maxY = -0.95f; // Maximum Y position

    public SpriteRenderer spriteRenderer; // Reference to the Sprite Renderer
    public Sprite spriteFacingDown; 
    public Sprite spriteFacingUp;

    public DialogueManager dialogueManager; // Reference to the Dialogue Manager

    private void start()
    {
        // Ensure dialogueManager is assigned
        if (dialogueManager == null)
        {
            dialogueManager = FindFirstObjectByType<DialogueManager>();
            if (dialogueManager == null)
            {
                UnityEngine.Debug.LogError("PlayerMovement: dialogueManager is missing in the scene!");
                return; // Prevent further execution if missing
            }
        }
    }

    private void Update()
    {
        // Prevent movement if dialogue is active
        if (dialogueManager.isDialogueActive) return;

        Vector3 moveDirection = Vector3.zero;

        if (Input.GetKey(KeyCode.W))
        {
            moveDirection.y = 1;
            spriteRenderer.sprite = spriteFacingUp; // Change to back sprite
        }
        
        if (Input.GetKey(KeyCode.S))
        {
            moveDirection.y = -1;
            spriteRenderer.sprite = spriteFacingDown; // Change to front sprite
        }

        transform.position += moveDirection * speed * Time.deltaTime; 

        // Restrict movement within min and max Y values
        float clampedY = Mathf.Clamp(transform.position.y, minY, maxY);
        transform.position = new Vector3(transform.position.x, clampedY, transform.position.z);
    }
}
