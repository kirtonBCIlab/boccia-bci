using UnityEngine;
using Unity.VisualScripting;
using UnityEngine.EventSystems;
using BCIEssentials.StimulusObjects;
using BCIEssentials.StimulusEffects;
using FanNamespace;

public class FanInteractions : MonoBehaviour, IPointerClickHandler
{
    [Header("SPO settings")]
    public Color flashOnColor = Color.red;
    public Color flashOffColor = Color.white;

    private FanPresenter _fanPresenter;
    private FanSettings _fanSettings;

    private BocciaModel _model;
    
    private void Start()
    {
        _model = BocciaModel.Instance;
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        OnSegmentClick(eventData.pointerCurrentRaycast.gameObject.transform);
    }

    /// <summary>
    /// Adds colliders and click event handlers to individual fan segments
    /// </summary>
    public void MakeFanSegmentsInteractable(FanSettings fanSettings)
    {
        _fanPresenter = GetComponentInParent<FanPresenter>();
        _fanSettings = fanSettings;

        int segmentID = 0;
        foreach (Transform child in transform)
        {
            // Change layer to the interacaable layer
            child.gameObject.layer = gameObject.layer;

            // Add a collider to make segment clickable
            MeshCollider meshCollider = child.AddComponent<MeshCollider>();
            meshCollider.sharedMesh = child.GetComponent<MeshFilter>().mesh;
            meshCollider.enabled = true;

            // Add Flashing effects component
            // TODO: We need something to add the color of the SPO segment here
            ColorFlashEffect colorFlashEffect = child.AddComponent<ColorFlashEffect>();
            // colorFlashEffect.OnColor = flashOnColor;
            // colorFlashEffect.OffColor = flashOffColor;

            // Add SPO component to make segment selectable with BCI
            child.tag = "BCI";
            SPO spo = child.AddComponent<SPO>();
            spo.ObjectID = segmentID;
            spo.Selectable = true;

            spo.StartStimulusEvent.AddListener(() => child.GetComponent<ColorFlashEffect>().SetOn());
            spo.StopStimulusEvent.AddListener(() => child.GetComponent<ColorFlashEffect>().SetOff());
            spo.OnSelectedEvent.AddListener(() => child.GetComponent<SPO>().StopStimulus());
            spo.OnSelectedEvent.AddListener(() => child.GetComponent<ColorFlashEffect>().Play());                

            segmentID++;
        }
    }

    private void OnSegmentClick(Transform segment)
    {
        string segmentName = segment.name;
        switch (segmentName)
        {
            case "FanSegment":
                SPO spo = segment.GetComponent<SPO>();
                int segmentID = spo.ObjectID;
                OnFanSegmentClick(segmentID);

                // Handle which fan to draw next based on the positioning mode
                switch (_fanPresenter.positioningMode)
                {
                    case FanPositioningMode.CenterToBase:
                        _fanPresenter.positioningMode = FanPositioningMode.CenterToRails;
                        _fanPresenter.GenerateFanWorkflow();
                        break;
                    case FanPositioningMode.CenterToRails:
                        _fanPresenter.positioningMode = FanPositioningMode.CenterToRails;
                        _fanPresenter.GenerateFanWorkflow();
                        break;
                }
                break;
            case "BackButton":
                if (_fanPresenter.positioningMode == FanPositioningMode.CenterToRails)
                {
                    _fanPresenter.positioningMode = FanPositioningMode.CenterToBase;
                    _fanPresenter.GenerateFanWorkflow();
                }
                break;
            case "DropButton":
                // Only drop ball in play mode
                if (_fanPresenter.positioningMode == FanPositioningMode.CenterToRails || _fanPresenter.positioningMode == FanPositioningMode.CenterToBase)
                {
                    _model.DropBall();
                }
                break;
            default:
                break;
        }
    }

    private void OnFanSegmentClick(int segmentID)
    {
        // Debug.Log($"Segment ID: {segmentID}");
        int columnIndex = _fanSettings.NColumns - 1 - (segmentID / _fanSettings.NRows);
        int rowIndex = _fanSettings.NRows - 1 - (segmentID % _fanSettings.NRows);

        // Initialize values to starting position of the ramp
        float rotationAngle = 0f;
        float elevation = 50f;

        // Calculate the rotation angle and elevation based on the number of columns and rows        
        if (_fanSettings.NColumns > 1)
        {
            rotationAngle = - _fanSettings.Theta / 2 + columnIndex * (_fanSettings.Theta / (_fanSettings.NColumns - 1));
        }

        if (_fanSettings.NRows > 1)
        {
            elevation = _fanSettings.ElevationRange/2 - rowIndex * (_fanSettings.ElevationRange / (_fanSettings.NRows - 1));
        }

        // Round down to nearest integer
        // Might be needed when serial communication is enabled
        // int roundedRotationAngle = Mathf.FloorToInt(rotationAngle);
        // int roundedElevation = Mathf.FloorToInt(elevation);

        // Call the appropriate movement based on the positioning mode
        switch (_fanPresenter.positioningMode)
        {
            case FanPositioningMode.CenterToRails:
                // Debug.Log($"Relative Rotation: {rotationAngle}, Elevation: {elevation}");
                _model.RotateBy(rotationAngle);
                _model.ElevateBy(elevation);
                break;
            case FanPositioningMode.CenterToBase:
                // Debug.Log($"Absolute Rotation: {rotationAngle}, Elevation: {elevation}");
                _model.RotateTo(rotationAngle);
                _model.ElevateTo(elevation + 50f); // Add an offset of 50%, since 50% is the starting point of the elevation range
                break;
            default:
                break;
        }
    }
}