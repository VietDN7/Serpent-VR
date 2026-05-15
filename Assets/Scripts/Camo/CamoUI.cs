using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;


// Most of the functionality code for this was lifted from @Code_Weaver from this forum post
//          https://discussions.unity.com/t/xr-hands-how-to-listen-for-menu-button/908146/25

// For the love of everything that is worth living for, why is assigning an action to a button
// using OpenXR Interactables the most CONVOLUTED DAMN THING EVER?! IT'S CRIMINAL ! ! !
public class CamoUI : MonoBehaviour
{
   


    [SerializeField] 
    InputActionAsset controls;
    public GameObject parentHand;
    private GameObject theMenu;
    private InputAction camoMenuButton;
    
    //public GameObject menuCanvas;

    bool menuEnabled;

    void Awake()
    {
        var gameplayActionMap = controls.FindActionMap("XRI Left Interaction");
        camoMenuButton = gameplayActionMap.FindAction("CamoMenuButton");
        gameplayActionMap.Enable();

        theMenu = parentHand.transform.GetChild(0).gameObject;
        menuEnabled = theMenu.activeSelf;
    }

    void OnEnable()
    {
        camoMenuButton.performed += OnMenuPressed;
        
    }

    void OnDisable()
    {
        camoMenuButton.performed -= OnMenuPressed;
    }

    void UpdateMenuState()
    {
        menuEnabled = theMenu.activeSelf;
    }

    private void OnMenuPressed(InputAction.CallbackContext pleaseWork)
    {
        //print("THIS BUTTONS!");
        if (menuEnabled == false)
        {
            theMenu.SetActive(true);
        }
        else
        {
            theMenu.SetActive(false);
        }
        UpdateMenuState(); // GOD. FINALLY.
        
    }
    
}
