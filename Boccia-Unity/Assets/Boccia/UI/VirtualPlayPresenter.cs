using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using BCIEssentials.StimulusObjects;
using BCIEssentials.StimulusEffects;


// This is an example UI presenter.  This could be broken into several smaller scripts
// that each reference their own local UI element.  Typically there's interaction between
// UI elements, so can be more convenient to keep the logic in a single presenter.
public class VirtualPlayPresenter : MonoBehaviour
{
    // TODO - This is a dummy example, replace with real ramp prefab object, etc
    public Button rotateLeftButton;
    public Button rotateRightButton;
    public Button moveUpButton;
    public Button moveDownButton;
    public Button resetRampButton;
    public Button resetBallButton;
    public Button dropBallButton;
    public Button colorButton;
    public Button randomJackButton;
    private List<Button> virtualPlayButtons;


    public Button toggleCameraButton;
    public Camera VirtualPlayCamera;
    private bool isCourtViewOn;

    private BocciaModel model;


    void Start()
    {
        // cache model and subscribe for changed event
        model = BocciaModel.Instance;
        model.WasChanged += ModelChanged;
        model.NavigationChanged += NavigationChanged;

        // Create a list of the virtual play buttons
        // Do not include the camera toggle button
        virtualPlayButtons = new List<Button>()
        {
            resetRampButton,
            resetBallButton,
            colorButton,
            randomJackButton,
        };

        // Add listeners to Virtual Play buttons
        AddListenersToVirtualPlayButtons();

        // Connect the camera toggle button
        toggleCameraButton.onClick.AddListener(ToggleCamera);

        // Connect testing buttons
        ConnectTestingButtons();
    }

    private void AddListenersToVirtualPlayButtons()
    {
        AddListenerToButton(resetRampButton, ResetRamp);
        AddListenerToButton(resetBallButton, model.ResetVirtualBalls);
        AddListenerToButton(colorButton, model.RandomBallColor);
        AddListenerToButton(randomJackButton, model.RandomJackBall);
    }

    private void AddListenerToButton(Button button, UnityEngine.Events.UnityAction action)
    {
        button.onClick.AddListener(action);
        SPO buttonSPO = button.GetComponent<SPO>();
        if (buttonSPO != null)
        {
            buttonSPO.OnSelected.AddListener(() => button.GetComponent<SPO>().StopStimulus());
            buttonSPO.OnSelected.AddListener(() => action());
        }
    }

    private void ConnectTestingButtons()
    {
        rotateLeftButton.onClick.AddListener(RotateLeft);
        rotateRightButton.onClick.AddListener(RotateRight);
        moveUpButton.onClick.AddListener(MoveUp);
        moveDownButton.onClick.AddListener(MoveDown);
        dropBallButton.onClick.AddListener(model.DropBall);
    }


    void OnDisable()
    {
        model.WasChanged -= ModelChanged;
    }

    void Update()
    {
    }

    private void ModelChanged()
    {
        // For a presenter, this is usually used to refresh the UI to reflect the
        // state of the model (UI does not store state, it just provides a way
        // to view and change it)
    }

    // Somehow we'll have to figure out the amount of rotation by looking at what SPO was clicked
    // Perhaps the SPO onCLick can provide the value that's initialized when the fan is created?
    // Temporary hard-coded values for incrementing
    private void RotateRight()
    {
        model.RotateBy(6.0f);
    }

    private void RotateLeft()
    {
        model.RotateBy(-6.0f);
    }

    private void MoveUp()
    {
        model.ElevateBy(1.0f);
    }

    private void MoveDown()
    {
        model.ElevateBy(-1.0f);
    }

    private void ResetRamp()
    {
        model.ResetRampPosition();
        model.ResetFanWhenRampResets();
    }

    private void ToggleCamera()
    {
        // Check if the camera is enabled, if it is disable it
        // If it is disabled, enable it
        if (VirtualPlayCamera.gameObject.activeSelf)
        {
            // Disable camera
            VirtualPlayCamera.gameObject.SetActive(false);
            isCourtViewOn = false;
        }

        else
        {
            // Enable camera
            VirtualPlayCamera.gameObject.SetActive(true);
            isCourtViewOn = true;
        }
    }

    private void ToggleVirtualPlayButtons(bool isInteractable)
    {
        virtualPlayButtons.ForEach(colorButton => colorButton.interactable = isInteractable);
    }

    private void ToggleFan(bool isInteractable)
    {
        GameObject[] fanSegments = GameObject.FindGameObjectsWithTag("BCI");
        foreach (GameObject fanSegment in fanSegments)
        {
            MeshCollider segmentCollider = fanSegment.GetComponent<MeshCollider>();
            if (segmentCollider != null)
            {
                segmentCollider.enabled = isInteractable;
            }
        }
    }

    private void NavigationChanged()
    {
        if (model.CurrentScreen != BocciaScreen.VirtualPlay)
        {
            // Disable court view camera if it is on
            if (VirtualPlayCamera.gameObject.activeSelf)
            {
                VirtualPlayCamera.gameObject.SetActive(false);
            }
        }

        if (model.CurrentScreen == BocciaScreen.VirtualPlay)
        {
            // Check if the court view was on before
            if (isCourtViewOn)
            {
                // Re-enable court view camera
                VirtualPlayCamera.gameObject.SetActive(true);
            }
        }
    }
}
