using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using FanNamespace;
using UnityEngine.UI;



public class FanPresenter : MonoBehaviour
{
    public FanGenerator fanGenerator;
    public FanInteractions fanInteractions;

    [Header("Positioning")]
    public FanPositioningMode positioningMode;
    public BackButtonPositioningMode backButtonPositioningMode;

    [Header("Fan screen")]
    [Tooltip("The screen that this fan is associated with")]
    public BocciaScreen fanTypeScreen;
    

    private BocciaModel _model;
    private Quaternion _originalRotation;
    private FanGenerator _previousFan;
    private FanGenerator _currentFan;
    
    // Start is called before the first frame update
    void Start()
    {
        _model = BocciaModel.Instance; 
        _model.NavigationChanged += NavigationChanged;

        _originalRotation = transform.rotation;   
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
        float zOffset = (180 - fanGenerator.Theta)/2 - shaftOrientation;
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
    private void CenterToBase()
    {
        float zOffset = (180 - fanGenerator.Theta)/2;
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
        // Enable the fan creation only if it matched the current screen
        if (fanTypeScreen == _model.CurrentScreen) 
        { 
            GenerateFanWorkflow();
        }
        else
        {
            fanGenerator.DestroyFanSegments();
        }
    }

    public void GenerateFanWorkflow()
    {
        StartCoroutine(GenerateFanCoroutine());
    }

    private IEnumerator GenerateFanCoroutine()
    {
        
        fanGenerator.DestroyFanSegments();

        // Force a frame to force fan segments destruction complete before generating the fan shape
        yield return null;

        // Reset to original rotation to avoid cumulative effects
        CenterToOrigin();

        // Get positioning mode and apply the corresponding offset    
        switch (positioningMode)
        {
            case FanPositioningMode.CenterToRails:
                CenterToRails();
                fanGenerator.GenerateFanShape();
                fanGenerator.GenerateBackButton(backButtonPositioningMode);
                fanGenerator.GenerateDropButton();
                fanInteractions.MakeFanSegmentsInteractable();
                fanGenerator.GenerateFanAnnotations(_model.RampRotation, _model.RampElevation, backButtonPositioningMode);
                break;
            case FanPositioningMode.CenterToBase:
                CenterToBase();
                fanGenerator.ElevationRange = 100f;
                backButtonPositioningMode = BackButtonPositioningMode.Left;
                fanGenerator.GenerateFanShape();
                fanGenerator.GenerateDropButton();
                fanInteractions.MakeFanSegmentsInteractable();
                fanGenerator.GenerateFanAnnotations(0, 50f, backButtonPositioningMode);

                // Change settings so that next fan is 
                // positioningMode = FanPositioningMode.CenterToRails;
                break;
            case FanPositioningMode.None:
                fanGenerator.GenerateFanShape();
                break;
        }

        // Save current fan for future reference
        _currentFan = fanGenerator;
    }
}
