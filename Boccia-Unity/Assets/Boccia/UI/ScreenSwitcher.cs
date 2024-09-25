using System.Collections;
using System.Collections.Generic;
using UnityEngine;


// Displays the screen indicated by BocciaModel.CurrentScreen
//
public class ScreenSwitcher : MonoBehaviour
{
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
            case BocciaScreen.StartMenu:
                StartMenu.SetActive(true);
                PlayMenu.SetActive(false);
                break;

            case BocciaScreen.PlayMenu:
                StartMenu.SetActive(false);
                PlayMenu.SetActive(true);
                break;
            
            default:
                StartMenu.SetActive(true);
                PlayMenu.SetActive(false);
                break;
        }
    }
}
