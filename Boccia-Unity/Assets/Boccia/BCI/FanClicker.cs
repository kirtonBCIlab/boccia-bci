using UnityEngine;
using UnityEngine.EventSystems;
using BCIEssentials.StimulusObjects;
using Unity.VisualScripting;
using System.Runtime.CompilerServices;

public class FanClicker : MonoBehaviour, IPointerClickHandler
{
    private FanGenerator fanGenerator;
    private FanPresenter fanPresenter;
    
    private void Start()
    {
        fanGenerator = GetComponentInParent<FanGenerator>();
        fanPresenter = GetComponentInParent<FanPresenter>();
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        OnFanSegmentClick(transform);
    }

    /// <summary>
    /// Adds colliders and click event handlers to individual fan segments
    /// </summary>
    public void MakeFanSegmentsInteractable()
    {
        foreach (Transform child in transform)
        {
            if (child != null && child.name == "FanSegment")
            {
                // Add a collider to make segment clickable
                MeshCollider meshCollider = child.AddComponent<MeshCollider>();
                meshCollider.sharedMesh = child.GetComponent<MeshFilter>().mesh;
                meshCollider.enabled = true;
                
                // Add compontent to handle click events
                FanClicker fanClicker = child.gameObject.AddComponent<FanClicker>();
            }
        }
    }

    private void OnFanSegmentClick(Transform segment)
    {
        SPO spo = segment.GetComponent<SPO>();
        int segmentID = spo.ObjectID;
        int columnIndex = fanGenerator.NColumns - 1 - (segmentID / fanGenerator.NRows);
        int rowIndex = fanGenerator.NRows - 1 - (segmentID % fanGenerator.NRows);
        // Debug.Log("Fan segment clicked: " + segmentID);

        // Compute exact rotation angle and elevation based on clicked segmentID
        float rotationAngle = 0f;
        float elevation = 0f;
        
        if (fanGenerator.NColumns > 1)
        {
            rotationAngle = - fanGenerator.theta / 2 + columnIndex * (fanGenerator.theta / (fanGenerator.NColumns - 1));
        }

        if (fanGenerator.NRows > 1)
        {
            elevation = fanGenerator.HighElevationLimit - rowIndex * ((fanGenerator.HighElevationLimit - fanGenerator.LowElevationLimit) / (fanGenerator.NRows - 1));
        }

        // Round down to nearest integer
        int roundedRotationAngle = Mathf.FloorToInt(rotationAngle);
        int roundedElevation = Mathf.FloorToInt(elevation);

        // Call the appropriate movement based on the positioning mode
        switch (fanPresenter.positioningMode)
        {
            case FanPresenter.PositioningMode.CenterToRails:
                fanPresenter.Rotateby(rotationAngle);
                fanPresenter.ElevateBy(elevation);
                break;
            case FanPresenter.PositioningMode.CenterToBase:
                fanPresenter.RotateTo(rotationAngle);
                fanPresenter.ElevateTo(elevation);
                break;
            default:
                break;
        }

    }
}