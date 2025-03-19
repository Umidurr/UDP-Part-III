/* 
UIDemo
Demonstrates options for handling user input with 
user interfaces designed with Unity UI Toolkit.

Copyright 2023 John M. Quick
*/

using UnityEngine;
using UnityEngine.UIElements;

public class UIDemo : MonoBehaviour {
    
    //UI document
    //assigned in Inspector
    public UIDocument doc;

    //UI doc root
    private VisualElement rootVisualElement;

    private Camera mainCamera;

    //awake
    private void Awake() {

        /*
        We want to enforce the screen resolution 
        on startup to prevent problems with the 
        window in the executable build.
        */

        //screen dimensions, in pixels
        //SNES resolution @4x
        int screenW = 1280;
        int screenH = 960;
        bool isFullScreen = false;

        //force default screen resolution
        Screen.SetResolution(screenW, screenH, isFullScreen);
    }

    //start
    void Start() {

        /*
        The UI Toolkit is managed in code via a UI Document.
        Every UI Document has a root, which can be accessed 
        in code. From there, a query system allows us to 
        access each individual UI element. Further, we can 
        register callback functions that execute whenever a 
        specific user interaction occurs on a UI element.
        */

        mainCamera = Camera.main;

        //retrieve UI doc root
        rootVisualElement = doc.rootVisualElement;

        /* 
        This is an example of event-based input handling using
        callbacks. This is currently Unity's preferred method
        for handling user input with UI Toolkit.

        Some UI objects are focusable by default, like buttons,
        while others are not, like any VisualElement that may
        serve a wide variety of purposes.

        Focusing is more obvious in a UI that uses touch or 
        mouse input. If input happens "on" a UI object, such as
        a user touching a button, then we make sure to handle it.

        However, this demo uses distinct key controls. Similar
        to many, but not all, game controller UIs, the exact
        spatial location of the input is not important. What
        matters is that a specific input command is received.

        Therefore, we focus the entire container object and
        then differentiate the inputs as they arrive to ensure
        the correct result is achieved.
        */

        //allow visual element to be focused
        rootVisualElement.focusable = true;

        //focus visual element
        rootVisualElement.Focus();
    }

    //update
    private void Update() 
    {
        if (mainCamera != null && rootVisualElement != null)
        {
            // Convert world position to screen position
            Vector2 screenPosition = mainCamera.WorldToScreenPoint(mainCamera.transform.position);

            // Apply screen position to UI Toolkit element
            rootVisualElement.style.position = Position.Absolute;
            //rootVisualElement.style.left = screenPosition.x;
            //rootVisualElement.style.top = Screen.height - screenPosition.y; // Flip for UI coordinates
        }
    }
}