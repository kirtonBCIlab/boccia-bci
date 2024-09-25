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
                MoveCamera(new Vector3(849, 255, -184), new Vector3(7, 4, 0));
                break;
            
            default:
                StartMenu.SetActive(true);
                PlayMenu.SetActive(false);
                MoveCamera(new Vector3(121, 138, 31), new Vector3(0, 18, 0));
                break;
        }
    }

    private void MoveCamera(Vector3 position, Vector3 rotation)
    {
        ScreenCamera.transform.position = position;
        ScreenCamera.transform.rotation = Quaternion.Euler(rotation);
    }
}
