using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using FanNamespace;


public class FanPresenter : MonoBehaviour
{
    public FanGenerator fanGenerator;
    public FanInteractions fanInteractions;

    public enum FanPositioningMode
    {
        None,
        CenterToRails,
        CenterToBase
    }

    [Header("Positioning")]
    public FanPositioningMode positioningMode;
    public BackButtonPositioningMode backButtonPositioningMode;

    private BocciaModel _model;
    private Quaternion _originalRotation;
    
    // Start is called before the first frame update
    void Start()
    {
        _model = BocciaModel.Instance; 
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
        Debug.Log("Shaft orientation: " + shaftOrientation);
        float zOffset = (180 - fanGenerator.Theta)/2 - shaftOrientation;
        Quaternion newRotation = Quaternion.Euler(
            _originalRotation.eulerAngles.x,
            _originalRotation.eulerAngles.y,
            _originalRotation.eulerAngles.z + zOffset
            );
        transform.rotation = newRotation;
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
        transform.rotation = newRotation;
    }

    /// <summary>
    /// Reset the fan to the original rotation. The fan will be generated
    /// in the XY plane, counting the degrees in counter clock mode from
    /// the +X axis.
    /// </summary>
    private void CenterToOrigin()
    {
        transform.rotation = _originalRotation;
    }

    public void GenerateFan()
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
                fanInteractions.MakeFanSegmentsInteractable();
                fanGenerator.GenerateBackButton(backButtonPositioningMode);
                break;
            case FanPositioningMode.CenterToBase:
                CenterToBase();
                fanGenerator.GenerateFanShape();
                fanInteractions.MakeFanSegmentsInteractable();
                break;
            case FanPositioningMode.None:
                fanGenerator.GenerateFanShape();
                break;
        }
    }
}
