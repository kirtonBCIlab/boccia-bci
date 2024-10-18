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

    private FanGenerator _fanGenerator;
    private FanPresenter _fanPresenter;

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
    public void MakeFanSegmentsInteractable()
    {
        _fanGenerator = GetComponentInParent<FanGenerator>();
        _fanPresenter = GetComponentInParent<FanPresenter>();
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
        SPO spo = segment.GetComponent<SPO>();
        int segmentID = spo.ObjectID;
        int nfanSegments = _fanGenerator.NColumns * _fanGenerator.NRows;

        if (segmentID >= 0 && segmentID < nfanSegments)
        {
            OnFanSegmentClick(segmentID);
        }
        else if (segmentID == nfanSegments)
        {
            _fanGenerator.DestroyFanSegments();
        }
        else if (segmentID == nfanSegments + 1)
        {
            _model.DropBall();
        }

    }

    private void OnFanSegmentClick(int segmentID)
    {
        int columnIndex = _fanGenerator.NColumns - 1 - (segmentID / _fanGenerator.NRows);
        int rowIndex = _fanGenerator.NRows - 1 - (segmentID % _fanGenerator.NRows);

        // Initialize values to starting position of the ramp
        float rotationAngle = 0f;
        float elevation = 50f;

        // Calculate the rotation angle and elevation based on the number of columns and rows        
        if (_fanGenerator.NColumns > 1)
        {
            rotationAngle = - _fanGenerator.Theta / 2 + columnIndex * (_fanGenerator.Theta / (_fanGenerator.NColumns - 1));
        }

        if (_fanGenerator.NRows > 1)
        {
            elevation = _fanGenerator.ElevationRange/2 - rowIndex * (_fanGenerator.ElevationRange / (_fanGenerator.NRows - 1));
        }

        // Round down to nearest integer
        // Might be needed when serial communication is enabled
        // int roundedRotationAngle = Mathf.FloorToInt(rotationAngle);
        // int roundedElevation = Mathf.FloorToInt(elevation);

        // Call the appropriate movement based on the positioning mode
        switch (_fanPresenter.positioningMode)
        {
            case FanPositioningMode.CenterToRails:
                _model.RotateBy(rotationAngle);
                _model.ElevateBy(elevation);
                break;
            case FanPositioningMode.CenterToBase:
                _model.RotateTo(rotationAngle);
                _model.ElevateTo(elevation + 50f); // Add an offset of 50%, since 50% is the starting point of the elevation range
                break;
            default:
                break;
        }
    }
}