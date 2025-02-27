using System.Collections;
using System.Collections.Generic;
using UnityEngine;


// Displays the screen indicated by BocciaModel.CurrentScreen
//
public class ScreenSwitcher : MonoBehaviour
{
    public Camera ScreenCamera;
    public float CameraDistance = 600.0f;
    public float ScreenDistance = 5.0f; // Distance to view screens
    public float RampViewDistance = 3.3f; // Distance to place the camera behind the ramp
    public float RampCameraHeight = 2.3f; // Y-position of the camera when looking at the ramp
    public float CameraSpeed = 5.0f;

    private Vector3 targetPosition;
    private Quaternion targetRotation;

    public GameObject StartMenu;
    public GameObject PlayMenu;
    public GameObject HamburgerMenuOptions;
    public GameObject GameOptionsMenu;
    public GameObject BciOptionsMenu;
    public GameObject VirtualPlayScreen;
    public GameObject TrainingScreen;
    public GameObject RampSetupMenu;
    public GameObject PlayScreen;

    private BocciaModel model;
    private List<GameObject> screenList;


    // Start is called before the first frame update
    void Start()
    {
        // cache model and subscribe for changed event
        model = BocciaModel.Instance;
        model.NavigationChanged += NavigationChanged;

        // initialize to model
        InitializeScreens();
        NavigationChanged();
    }

    void OnDisable()
    {
        model.NavigationChanged -= NavigationChanged;
    }


    private void InitializeScreens()
    {
        screenList = new List<GameObject>
        {
            StartMenu,
            PlayMenu,
            HamburgerMenuOptions,
            GameOptionsMenu,
            TrainingScreen,
            BciOptionsMenu,
            VirtualPlayScreen,
            RampSetupMenu,
            PlayScreen
        };
    }

    private void NavigationChanged()
    {
        // TODO - consider replacing with a dictionary that maps screen enum to screen game object
        switch (model.CurrentScreen)
        {
            case BocciaScreen.PlayMenu:
                PanCameraToScreen(PlayMenu, ScreenDistance);
                break;
                
            case BocciaScreen.HamburgerMenu:
                PanCameraToScreen(HamburgerMenuOptions, ScreenDistance);
                break;

            case BocciaScreen.TrainingScreen:
                PanCameraToScreen(TrainingScreen, ScreenDistance);
                break;

            case BocciaScreen.RampSetup:
                // First show the play menu since ramp setup displays there
                PanCameraToScreen(PlayMenu, ScreenDistance);
                ShowRampSetupMenu(true);
                break;

            case BocciaScreen.Play:
                PanCameraToScreen(PlayScreen, RampViewDistance);
                break;

            case BocciaScreen.VirtualPlay:
                PanCameraToScreen(VirtualPlayScreen, RampViewDistance);
                break;

            case BocciaScreen.GameOptions:
                PanCameraToScreen(GameOptionsMenu, ScreenDistance);
                break;

            case BocciaScreen.BciOptions:
                PanCameraToScreen(BciOptionsMenu, ScreenDistance);
                break;
            
            // For now just switch back to start menu to show switching works
            default:
                PanCameraToScreen(StartMenu, CameraDistance);
                break;
        }

        // Maybe this call is expensive, could look into calling only on the appropriate screens
        model.SetRampControllerBasedOnMode();
    }

    // Pans the camera to show the new screen, hides all others
    private void PanCameraToScreen(GameObject screenToShow, float distance)
    {
        screenToShow.SetActive(true);

        foreach (var screen in screenList)
        {
            if (screen != screenToShow)
            {
                screen.SetActive(false);
            }
        }

        PanCameraTo(screenToShow, distance);
    }

    private void PanCameraTo(GameObject screenToShow, float distance)
    {
        // Calculate target camera pose based on screen's postion and direction in world space
        targetPosition = screenToShow.transform.position + screenToShow.transform.forward * distance * -1.0f;

        if (IsRampView())
        {
            targetPosition.y = RampCameraHeight;
        }

        targetRotation = screenToShow.transform.rotation;

        // Spawn two routines to move the camera to the new pose.  Use private members for target
        // just in case this is called before previous coroutine completed, that way they move to same target.
        StartCoroutine(PositionCamera());
        StartCoroutine(RotateCamera());
    }

    private IEnumerator PositionCamera()
    {
        Vector3 currentPosition = ScreenCamera.transform.position;
        while (Vector3.Distance(ScreenCamera.transform.position, targetPosition) > 0.01f)
        {
            currentPosition = Vector3.Lerp(currentPosition, targetPosition, CameraSpeed * Time.deltaTime);
            ScreenCamera.transform.position = currentPosition;
            yield return null;
        }
        ScreenCamera.transform.position = targetPosition;
    }

    private IEnumerator RotateCamera()
    {
        Quaternion currentRotation = ScreenCamera.transform.rotation;
        while (Quaternion.Angle(currentRotation, targetRotation) > 0.01f)
        {
            currentRotation = Quaternion.Lerp(currentRotation, targetRotation, CameraSpeed * Time.deltaTime);
            ScreenCamera.transform.rotation = currentRotation;
            yield return null;
        }
        ScreenCamera.transform.rotation = targetRotation;
    }

    private void ShowRampSetupMenu(bool isActive)
    {
        RampSetupMenu.SetActive(isActive);
    }

    private bool IsRampView()
    {
        return (model.CurrentScreen == BocciaScreen.Play || model.CurrentScreen == BocciaScreen.VirtualPlay);
    }
}
