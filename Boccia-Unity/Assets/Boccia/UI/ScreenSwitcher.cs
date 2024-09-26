using System.Collections;
using System.Collections.Generic;
using UnityEngine;


// Displays the screen indicated by BocciaModel.CurrentScreen
//
public class ScreenSwitcher : MonoBehaviour
{
    public Camera ScreenCamera;
    public GameObject StartMenu;
    public GameObject PlayMenu;

    private BocciaModel model;


    // Start is called before the first frame update
    void Start()
    {
        // cache model and subscribe for changed event
        model = BocciaModel.Instance;
        BocciaModel.NavigationChanged += NavigationChanged;

        // initialize to model
        NavigationChanged();
    }

    void NavigationChanged()
    {
        switch (model.CurrentScreen)
        {
            // TODO - make a helper function hide all but given screen and move camera to it
            case BocciaScreen.PlayMenu:
                StartMenu.SetActive(false);
                PlayMenu.SetActive(true); 
                // MoveCamera(StartMenu.transform);
                break;
            
            default:
                StartMenu.SetActive(true);
                PlayMenu.SetActive(false);
                // MoveCamera(PlayMenu.transform);
                break;
        }
    }

    // private void MoveCamera(Transform screenTransform)
    // {
        // TODO - calculate a camera position by projecting a point away from the prefab's normal?
        // Or, just provide a way to set the pose manually?
        // ScreenCamera.transform.position = screenTransform.position;
        // ScreenCamera.transform.rotation = screenTransform.rotation;
    // }
}
