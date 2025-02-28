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

    private int _segmentID;
    
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
            // Change layer to the interacable layer
            child.gameObject.layer = gameObject.layer;

            // Add a collider to make segment clickable
            MeshCollider meshCollider = child.AddComponent<MeshCollider>();
            meshCollider.sharedMesh = child.GetComponent<MeshFilter>().mesh;
            meshCollider.enabled = true;

            // Add Flashing effects component
            // TODO: We need something to add the color of the SPO segment here
            FanSegmentColorFlashEffect fanSegmentColorFlashEffect = child.AddComponent<FanSegmentColorFlashEffect>();

            // Add SPO component to make segment selectable with BCI
            child.tag = "BCI";
            SPO spo = child.AddComponent<SPO>();
            spo.ObjectID = -100;
            spo.Selectable = true;

            spo.StartStimulusEvent.AddListener(() => child.GetComponent<FanSegmentColorFlashEffect>().SetOn());
            spo.StopStimulusEvent.AddListener(() => child.GetComponent<FanSegmentColorFlashEffect>().SetOff());

            spo.OnSelectedEvent.AddListener(() => child.GetComponent<SPO>().StopStimulus());
            spo.OnSelectedEvent.AddListener(() => child.GetComponent<FanSegmentColorFlashEffect>().Play());            
            spo.OnSelectedEvent.AddListener(() => OnSegmentClick(child.transform));  

            // Add BCITargetAnimations component to change color when training starts
            BCITargetAnimations bciTargetAnimations = child.gameObject.AddComponent<BCITargetAnimations>();
            bciTargetAnimations.bocciaAnimation = _model.P300Settings.Train.TargetAnimation;

            FanSegmentIdentifier segmentIdentifier = child.AddComponent<FanSegmentIdentifier>();
            segmentIdentifier.SegmentID = segmentID;
            segmentID++;
        }
    }

    private void OnSegmentClick(Transform segment)
    {
        if (Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift))
        {
            // Store the segment as the target element
            int segmentID = segment.GetComponent<FanSegmentIdentifier>().SegmentID;
            _model.SetTargetElement(segmentID);
            return;
        }

        string segmentName = segment.name;
        switch (segmentName)
        {
            case "FanSegment":
                FanSegmentIdentifier identifier = segment.GetComponent<FanSegmentIdentifier>();
                int segmentID = identifier.SegmentID;
                OnFanSegmentClick(segmentID);

                // Handle which fan to draw next based on the positioning mode
                switch (_fanPresenter.positioningMode)
                {
                    case FanPositioningMode.CenterToBase:
                        _fanPresenter.positioningMode = FanPositioningMode.CenterToRails;
                        _fanPresenter.GenerateFanWorkflow();
                        _model.FanTypeChanged();
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
        float rotationAngle = _model.RampSettings.RotationOrigin;
        float elevation = _model.RampSettings.ElevationOrigin;

        float threshold = 1e-5f;

        // Calculate the rotation angle and elevation based on the number of columns and rows       
        if (_fanSettings.NColumns > 1)
        {
            float segmentAngle = _fanSettings.Theta / _fanSettings.NColumns;
            // Calculate rotation angle and add half of the segment angle to center the ramp in the segment
            rotationAngle = (- _fanSettings.Theta / 2) + (columnIndex * segmentAngle) + (segmentAngle / 2);
            
            // Avoid unnecessarily small numbers due to rounding
            if (Mathf.Abs(rotationAngle) < threshold)
            {
                rotationAngle = 0f;
            }
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