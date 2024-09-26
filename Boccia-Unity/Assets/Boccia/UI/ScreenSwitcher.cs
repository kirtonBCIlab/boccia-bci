using System.Collections;
using System.Collections.Generic;
using UnityEngine;


// Displays the screen indicated by BocciaModel.CurrentScreen
//
public class ScreenSwitcher : MonoBehaviour
{
    public Camera ScreenCamera;
    public float CameraDistance = 600.0f;
    public float CameraSpeed = 5.0f;

    private Vector3 targetPosition;
    private Quaternion targetRotation;

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
        screenList = new List<GameObject>
        {
            StartMenu,
            PlayMenu
        };
    }

    private void NavigationChanged()
    {
        // TODO - consider replacing with a dictionary that maps screen enum to screen game object
        switch (model.CurrentScreen)
        {
            case BocciaScreen.PlayMenu:
                PanCameraToScreen(PlayMenu);
                break;

            // For now just switch back to start menu to show switchign works
            default:
                PanCameraToScreen(StartMenu);
                break;
        }
    }

    // Pans the camera to show the new screen, hides all others
    private void PanCameraToScreen(GameObject screenToShow)
    {
        screenToShow.SetActive(true);
        PanCameraTo(screenToShow);

        foreach (var screen in screenList)
        {
            if (screen != screenToShow)
            {
                screen.SetActive(false);
            }
        }
    }

    private void PanCameraTo(GameObject screenToShow)
    {
        // Calculate target camera pose based on screen's postion and direction in world space
        targetPosition = screenToShow.transform.position + screenToShow.transform.forward * CameraDistance * -1.0f;
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

}
