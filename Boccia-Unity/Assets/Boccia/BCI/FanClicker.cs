using UnityEngine;
using UnityEngine.EventSystems;
using BCIEssentials.StimulusObjects;
using Unity.VisualScripting;

public class FanClicker : MonoBehaviour, IPointerClickHandler
{
    private FanGenerator fanGenerator;
    
    private void Start()
    {
        fanGenerator = GetComponentInParent<FanGenerator>();
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        Debug.Log("Fan clicked" + eventData.pointerCurrentRaycast.gameObject.name);
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
                // fanClicker.fanGenerator = this.fanGenerator;
            }
        }
    }

    private void OnFanSegmentClick(Transform segment)
    {
        SPO spo = segment.GetComponent<SPO>();
        int segmentID = spo.ObjectID;
        int columnIndex = segmentID % fanGenerator.NColumns;
        int rowIndex = segmentID / fanGenerator.NColumns;

        float rotationAngle = -fanGenerator.theta / 2 + columnIndex * (fanGenerator.theta / (fanGenerator.NColumns - 1));
        float elevation = fanGenerator.LowElevationLimit + rowIndex * ((fanGenerator.HighElevationLimit - fanGenerator.LowElevationLimit) / (fanGenerator.NRows - 1));

        Debug.Log("Button clicked for segment " + segmentID);
        RotateFanSegment(segmentID, rotationAngle);
        ElevateFanSegment(segmentID, elevation);
    }

    private void RotateFanSegment(int segmentID, float angle)
    {
        Debug.Log("Rotating segment " + segmentID + " by " + angle);
    }

    private void ElevateFanSegment(int segmentID, float elevation)
    {
        Debug.Log("Elevating segment " + segmentID + " by " + elevation);
    }
}