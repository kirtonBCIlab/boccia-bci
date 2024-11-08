using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using FanNamespace;
using UnityEngine.UI;



public class FanPresenter : MonoBehaviour
{
    [Header("Scripts")]
    public FanGenerator fanGenerator;
    public FanInteractions fanInteractions;

    [Header("Positioning")]
    public FanPositioningMode positioningMode;
    public BackButtonPositioningMode backButtonPositioningMode;

    [Header("Fan settings")]
    [SerializeField]
    private FanSettings _fineFan;

    [SerializeField]
    private FanSettings _coarseFan;

    [Header("Fan screen")]
    [Tooltip("The screen that this fan is associated with")]
    public BocciaScreen fanTypeScreen;
    
    private BocciaModel _model;
    private Quaternion _originalRotation;
    private BocciaGameMode _lastPlayMode;
    
    // Start is called before the first frame update
    void Start()
    {
        _model = BocciaModel.Instance; 
        _model.NavigationChanged += NavigationChanged;
        _model.WasChanged += UpdateFineFan;

        _originalRotation = transform.rotation;  

        UpdateFineFan(); // Update fine fan
    }

    /// <summary>
    /// Center the fan to the rails of the ramp. The fan will be generated
    /// as stated in `CenterToOrigin`, and then an offset of half the angle
    /// Theta + the angle between the rails and their starting position
    /// (i.e., with 0 deg over the middle leg od the ramp) will be applied
    /// in the counter-clockwise direction.
    /// </summary>
    private void CenterToRails()
    {
        float shaftOrientation = _model.GetRampOrientation();
        float zOffset = (180 - _fineFan.Theta)/2 - shaftOrientation;
        Quaternion newRotation = Quaternion.Euler(
            _originalRotation.eulerAngles.x,
            _originalRotation.eulerAngles.y,
            _originalRotation.eulerAngles.z + zOffset
            );
        transform.localRotation = newRotation;
    }

    /// <summary>
    /// Center the fan to the base of the ramp. The fan will be generated
    /// as stated in `CenterToOrigin`, and then an offset of half the angle
    /// Theta will be applied in the counter-clockwise direction.
    /// </summary>
    private void CenterNorth()
    {
        float zOffset = (180 - _coarseFan.Theta)/2;
        Quaternion newRotation = Quaternion.Euler(
            _originalRotation.eulerAngles.x,
            _originalRotation.eulerAngles.y,
            _originalRotation.eulerAngles.z + zOffset
        );
        transform.localRotation = newRotation;
    }

    /// <summary>
    /// Center the fan to the Game Options Menu. The fine fan is oriented to 
    /// the vertical axis of the game options menu, and offset by half of the
    /// fineFan theta so it is centered
    /// </summary>
    private void CenterGameOptionsMenu()
    {
        // float _originalRotation = transform.localRotation;
        float zOffset = (180 - _fineFan.Theta)/2;// - _originalRotation;
        Quaternion newRotation = Quaternion.Euler(
            _originalRotation.eulerAngles.x,
            _originalRotation.eulerAngles.y,
            _originalRotation.eulerAngles.z + zOffset
            );
        transform.localRotation = newRotation;
    }

    /// <summary>
    /// Reset the fan to the original rotation. The fan will be generated
    /// in the XY plane, counting the degrees in counter clock mode from
    /// the +X axis.
    /// </summary>
    private void CenterToOrigin()
    {
        transform.localRotation = _originalRotation;
    }

    
    private void NavigationChanged()
    {
        ResetFanWhenPlayModeChanges();
        DisplayFanOnCorrespondingScreen();
    }

    private void UpdateFineFan()
    {
        // If the model changed and fineFan exists, update it
        if (_fineFan != null)
        {
            _fineFan.Theta = _model.GameOptions.RotationRange;
            _fineFan.NColumns = _model.GameOptions.RotationPrecision;
            _fineFan.NRows = _model.GameOptions.ElevationPrecision;
            _fineFan.ElevationRange = _model.GameOptions.ElevationRange;
        }
    }

    public void GenerateFanWorkflow()
    {
        // For generating fan on GameOptionsMenu, run it serially, not as a coroutine
        // Otherwise, the coroutine will try to run before GameOptionsMenu is active, due to the way navigation and camera are handled, which will result in the coroutine failing for BocciaScreen.GameOptions
        if (fanTypeScreen == BocciaScreen.GameOptions)
        {
                // Debug.Log("Generating fan for GameOptionsMenu");
                fanGenerator.DestroyFanSegments();
                CenterToOrigin();
                CenterGameOptionsMenu();
                fanGenerator.GenerateFanShape(_fineFan);
        }
        // For all other cases, generate fan with a coroutine
        else
        {
            StartCoroutine(GenerateFanCoroutine());
        }
    }

    private IEnumerator GenerateFanCoroutine()
    {
        fanGenerator.DestroyFanSegments();

        // Force a frame to force fan segments destruction complete before generating the fan shape
        yield return null;

        // If the ramp is moving, wait for it to stop before generating the fan
        while (_model.IsRampMoving)
        {
            yield return null;
        }

        // Reset to original rotation to avoid cumulative effects
        CenterToOrigin();

        // Get positioning mode and apply the corresponding offset    
        switch (positioningMode)
        {
            case FanPositioningMode.CenterToRails:
                CenterToRails();
                fanGenerator.GenerateFanShape(_fineFan);
                fanGenerator.GenerateBackButton(_fineFan, backButtonPositioningMode);
                fanGenerator.GenerateDropButton(_fineFan);
                fanInteractions.MakeFanSegmentsInteractable(_fineFan);
                fanGenerator.GenerateFanAnnotations(_fineFan, _model.RampRotation, _model.RampElevation, backButtonPositioningMode);
                break;
            case FanPositioningMode.CenterToBase:
                CenterNorth();
                fanGenerator.GenerateFanShape(_coarseFan);
                fanGenerator.GenerateBackButton(_coarseFan, BackButtonPositioningMode.None);
                fanGenerator.GenerateDropButton(_coarseFan);
                fanInteractions.MakeFanSegmentsInteractable(_coarseFan);
                fanGenerator.GenerateFanAnnotations(_coarseFan, 0, _coarseFan.ElevationRange/2, backButtonPositioningMode);

                // Change settings so that next fan is 
                // positioningMode = FanPositioningMode.CenterToRails;
                break;
            case FanPositioningMode.CenterNorth:
                CenterNorth();
                fanGenerator.GenerateFanShape(_coarseFan);
                fanGenerator.GenerateBackButton(_coarseFan, backButtonPositioningMode);
                fanGenerator.GenerateDropButton(_coarseFan);
                fanInteractions.MakeFanSegmentsInteractable(_coarseFan);
                break;
            case FanPositioningMode.None:
                fanGenerator.GenerateFanShape(_coarseFan);
                break;
        }
    }

    /// <summary>
    ///  Resets the fan to generate a coarse fan if the gameMode changes
    /// </summary>
    private void ResetFanWhenPlayModeChanges()
    {
        BocciaGameMode currentPlayMode = _model.GameMode;
        bool currentlyInPlayMode = (currentPlayMode == BocciaGameMode.Virtual) || (currentPlayMode == BocciaGameMode.Play);

        if (currentlyInPlayMode && _lastPlayMode != currentPlayMode)
        {
            positioningMode = FanPositioningMode.CenterToBase;
            _lastPlayMode = currentPlayMode;
        }
    }

    /// <summary>
    /// Displays the fan only on the the corresponding screen, as set in the inspector
    /// </summary>
    private void DisplayFanOnCorrespondingScreen()
    {
        if (fanTypeScreen == _model.CurrentScreen) 
        { 
            GenerateFanWorkflow();
        }
        else
        {
            fanGenerator.DestroyFanSegments();
        }
    }
}
