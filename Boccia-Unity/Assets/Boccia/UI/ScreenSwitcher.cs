using System.Collections;
using System.Collections.Generic;
using UnityEngine;


// Displays the screen indicated by BocciaModel.CurrentScreen
//
public class ScreenSwitcher : MonoBehaviour
{
    public Camera ScreenCamera;
    public Camera VirtualPlayCamera;
    public float CameraDistance = 600.0f;
    public float RampViewCameraDistance = 5.0f; // Custom distance so the camera will view the ramp
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
            VirtualPlayScreen
        };
    }

    private void NavigationChanged()
    {
        // TODO - consider replacing with a dictionary that maps screen enum to screen game object
        switch (model.CurrentScreen)
        {
            case BocciaScreen.PlayMenu:
                PanCameraToScreen(PlayMenu, RampViewCameraDistance);
                EnableVirtualPlayCamera(false);
                break;
                
            case BocciaScreen.HamburgerMenu:
                PanCameraToScreen(HamburgerMenuOptions, RampViewCameraDistance);
                EnableVirtualPlayCamera(false);
                break;

            case BocciaScreen.TrainingScreen:
                PanCameraToScreen(TrainingScreen, RampViewCameraDistance);
                EnableVirtualPlayCamera(false);
                break;

            case BocciaScreen.VirtualPlay:
                PanCameraToScreen(VirtualPlayScreen, RampViewCameraDistance);
                EnableVirtualPlayCamera(true);
                break;

            case BocciaScreen.GameOptions:
                PanCameraToScreen(GameOptionsMenu, RampViewCameraDistance);
                EnableVirtualPlayCamera(false);
                break;

            case BocciaScreen.BciOptions:
                PanCameraToScreen(BciOptionsMenu, RampViewCameraDistance);
                EnableVirtualPlayCamera(false);
                break;
            
            // For now just switch back to start menu to show switching works
            default:
                PanCameraToScreen(StartMenu, CameraDistance);
                EnableVirtualPlayCamera(false);
                break;
        }
    }

    // Pans the camera to show the new screen, hides all others
    private void PanCameraToScreen(GameObject screenToShow, float distance)
    {
        screenToShow.SetActive(true);
        PanCameraTo(screenToShow, distance);

        foreach (var screen in screenList)
        {
            if (screen != screenToShow)
            {
                screen.SetActive(false);
            }
        }
    }

    private void PanCameraTo(GameObject screenToShow, float distance)
    {
        // Calculate target camera pose based on screen's postion and direction in world space
        targetPosition = screenToShow.transform.position + screenToShow.transform.forward * distance * -1.0f;
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

    // Helper method to toggle the virtual play camera on or off
    // VirtualPlayCamera should only be enabled for the virtual play screen
    private void EnableVirtualPlayCamera(bool isEnabled)
    {
        VirtualPlayCamera.gameObject.SetActive(isEnabled);
    }

}
