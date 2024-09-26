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
    private List<GameObject> screenList;


    // Start is called before the first frame update
    void Start()
    {
        // cache model and subscribe for changed event
        model = BocciaModel.Instance;
        BocciaModel.NavigationChanged += NavigationChanged;

        // initialize to model
        InitializeScreens();
        NavigationChanged();
    }

    private void InitializeScreens()
    {
        screenList = new List<GameObject>();
        screenList.Add(StartMenu);
        screenList.Add(PlayMenu);
    }

    private void NavigationChanged()
    {
        switch (model.CurrentScreen)
        {
            case BocciaScreen.PlayMenu:
                ShowScreen(PlayMenu);
                break;
            
            default:
                ShowScreen(StartMenu);
                break;
        }
    }

    // Hides every screen except screenToShow
    private void ShowScreen(GameObject screenToShow)
    {
        foreach (var screen in screenList)
        {
            if(screen == screenToShow)
            {
                screen.SetActive(true);
            }
            else
            {
                screen.SetActive(false);
            }
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
