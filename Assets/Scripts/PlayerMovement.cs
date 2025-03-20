using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    public float speed = 3.0f; // Movement speed
    public float minY = -2.94f; // Minimum Y position
    public float maxY = -0.95f; // Maximum Y position

    public SpriteRenderer spriteRenderer; // Reference to the Sprite Renderer
    public Sprite spriteFacingDown; 
    public Sprite spriteFacingUp;  

    private void Update()
    {
        Vector3 moveDirection = Vector3.zero;

        if (Input.GetKey(KeyCode.W)) 
        {
            moveDirection.y = 1;
            spriteRenderer.sprite = spriteFacingUp; // Change to back sprite
        }
        else if (Input.GetKey(KeyCode.S)) 
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
