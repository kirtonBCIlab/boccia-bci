using UnityEngine;
using Unity.VisualScripting;
using UnityEngine.EventSystems;
using BCIEssentials.StimulusObjects;
using BCIEssentials.StimulusEffects;

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
        OnFanSegmentClick(eventData.pointerCurrentRaycast.gameObject.transform);
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
            if (child != null && child.name == "FanSegment")
            {
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
    }

    private void OnFanSegmentClick(Transform segment)
    {        
        SPO spo = segment.GetComponent<SPO>();
        int segmentID = spo.ObjectID;
        int columnIndex = _fanGenerator.NColumns - 1 - (segmentID / _fanGenerator.NRows);
        int rowIndex = _fanGenerator.NRows - 1 - (segmentID % _fanGenerator.NRows);

        // Compute exact rotation angle and elevation based on clicked segmentID
        float rotationAngle = 0f;
        float elevation = 0f;
        
        if (_fanGenerator.NColumns > 1)
        {
            rotationAngle = - _fanGenerator.Theta / 2 + columnIndex * (_fanGenerator.Theta / (_fanGenerator.NColumns - 1));
        }

        if (_fanGenerator.NRows > 1)
        {
            elevation = _fanGenerator.HighElevationLimit - rowIndex * ((_fanGenerator.HighElevationLimit - _fanGenerator.LowElevationLimit) / (_fanGenerator.NRows - 1));
        }

        // Round down to nearest integer
        int roundedRotationAngle = Mathf.FloorToInt(rotationAngle);
        int roundedElevation = Mathf.FloorToInt(elevation);

        // Call the appropriate movement based on the positioning mode
        switch (_fanPresenter.positioningMode)
        {
            case FanPresenter.FanPositioningMode.CenterToRails:
                Rotateby(rotationAngle);
                ElevateBy(elevation);
                break;
            case FanPresenter.FanPositioningMode.CenterToBase:
                RotateTo(rotationAngle);
                ElevateTo(elevation);
                break;
            default:
                break;
        }
    }

    // Ramp movement functions
    public void Rotateby(float degrees) 
    { 
        // Debug.Log("Rotating by " + degrees);
        _model.RotateBy(degrees); 
    }

    public void RotateTo(float angle) 
    {
        // Debug.Log("Rotating to " + angle);
        _model.RotateTo(angle); 
    }
    
    public void ElevateBy(float elevation) 
    { 
        // Debug.Log("Elevating by " + elevation);
        _model.ElevateBy(elevation); 
    }

    public void ElevateTo(float elevation) 
    {
        // Debug.Log("Elevating to " + elevation);
        _model.ElevateTo(elevation);
    }
}