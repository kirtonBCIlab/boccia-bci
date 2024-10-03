using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using BCIEssentials.StimulusObjects;
using BCIEssentials.StimulusEffects;


public class FanGenerator : MonoBehaviour
{
    [Header("Fan Parameters")]
    public float theta;         // Angle in degrees
    public float columnSpacing; // Spacing between columns
    public float rowSpacing;    // Spacing between rows;

    private int _maxColumns = 7;        // Max number of columns 
    private int _maxRows = 7;           // Max number of rows

    [SerializeField]
    private float _r1;          // Inner radius
    public float R1
    {
        get { return _r1; }
        set { _r1 = Mathf.Clamp(value, 0, R2); }
    }

    [SerializeField]
    private float _r2;          // Outer radius
    public float R2
    {
        get { return _r2; }
        set { _r2 = Mathf.Clamp(value, R1, float.MaxValue); }
    }

    [SerializeField]
    private int _nColumns;        // Number of columns
    public int NColumns
    {
        get { return _nColumns; }
        set { _nColumns = Mathf.Clamp(value, 1, _maxColumns); }
    }

    [SerializeField]
    private int _nRows;           // Number of rows
    public int NRows
    {
        get { return _nRows; }
        set { _nRows = Mathf.Clamp(value, 1, _maxRows); }
    }

    [SerializeField]
    private int _lowElevationLimit; // Lower elevation limit
    public int LowElevationLimit
    {
        get { return _lowElevationLimit; }
        set { _lowElevationLimit = Mathf.Clamp(value, 0, HighElevationLimit); }
    }

    [SerializeField]
    private int _highElevationLimit; // Higher elevation limit
    public int HighElevationLimit
    {
        get { return _highElevationLimit; }
        set { _highElevationLimit = Mathf.Clamp(value, 0, LowElevationLimit); }
    }

    [Header("SPO settings")]
    public Color flashOnColor = Color.red;
    public Color flashOffColor = Color.white;

    public void GenerateFanShape()
    {
        GameObject fan = gameObject;
        
        float angleStep = theta / NColumns;
        float radiusStep = (R2 - R1) / NRows;

        // Only have spacing when there are more than 1 column or row
        if (NColumns > 1) { angleStep = (theta - (NColumns - 1) * columnSpacing) / NColumns; }
        if (NRows > 1) { radiusStep = (R2 - R1 - (NRows - 1) * rowSpacing) / NRows; }

        // Create the fan segments
        int segmentID = 0;
        for (int i = 0; i < NColumns; i++)
        {
            float startAngle = i * (angleStep + columnSpacing);
            float endAngle = startAngle + angleStep;

            for (int j = 0; j < NRows; j++)
            {
                float innerRadius = R1 + j * (radiusStep + rowSpacing);
                float outerRadius = innerRadius + radiusStep;

                CreateFanSegment(fan, startAngle, endAngle, innerRadius, outerRadius, segmentID);
                segmentID++;
            }
        }
    }

    private void CreateFanSegment(GameObject fan, float startAngle, float endAngle, float innerRadius, float outerRadius, int segmentID)
    {
        GameObject segment = new GameObject("FanSegment");
        segment.transform.SetParent(fan.transform);
        segment.tag = "BCI";

        MeshFilter meshFilter = segment.AddComponent<MeshFilter>();       
        MeshRenderer meshRenderer = segment.AddComponent<MeshRenderer>();

        Mesh mesh = new();
        meshFilter.mesh = mesh;

        int segments = 100; // Number of segments to approximate the arc
        int verticesCount = (segments + 1) * 2;
        Vector3[] vertices = new Vector3[verticesCount];
        int[] triangles = new int[segments * 6];

        float angleStep = (endAngle - startAngle) / segments;

        // Get the parent's values
        Quaternion parentRotation = fan.transform.rotation;
        Vector3 parentPosition = fan.transform.position;

        for (int i = 0; i <= segments; i++)
        {
            float angle = startAngle + i * angleStep;
            float rad = Mathf.Deg2Rad * angle;

            // Define vertices relative to the local origin
            Vector3 innerVertex = new(Mathf.Cos(rad) * innerRadius, Mathf.Sin(rad) * innerRadius, 0);
            Vector3 outerVertex = new(Mathf.Cos(rad) * outerRadius, Mathf.Sin(rad) * outerRadius, 0);

            vertices[i] = parentRotation * innerVertex + parentPosition;
            vertices[i + segments + 1] = parentRotation * outerVertex + parentPosition;

            if (i < segments)
            {
                int start = i * 6;
                triangles[start] = i;
                triangles[start + 1] = i + 1;
                triangles[start + 2] = i + segments + 1;

                triangles[start + 3] = i + 1;
                triangles[start + 4] = i + segments + 2;
                triangles[start + 5] = i + segments + 1;
            }
        }

        // Ensure normals are recalculated
        mesh.vertices = vertices;
        mesh.triangles = triangles;
        mesh.RecalculateNormals();

        // Check if the mesh bounds are correct
        mesh.RecalculateBounds();

        // Add Flashing effects component
        // TODO: We need something to add the color of the SPO segment here
        ColorFlashEffect colorFlashEffect = segment.AddComponent<ColorFlashEffect>();
        // colorFlashEffect.OnColor = flashOnColor;
        // colorFlashEffect.OffColor = flashOffColor;          

        // Add SPO component
        SPO spo = segment.AddComponent<SPO>();
        spo.ObjectID = segmentID;
        spo.Selectable = true;

        spo.StartStimulusEvent.AddListener(() => segment.GetComponent<ColorFlashEffect>().SetOn());
        spo.StopStimulusEvent.AddListener(() => segment.GetComponent<ColorFlashEffect>().SetOff());
        spo.OnSelectedEvent.AddListener(() => segment.GetComponent<SPO>().StopStimulus());
        spo.OnSelectedEvent.AddListener(() => segment.GetComponent<ColorFlashEffect>().Play()); 
    }

    // public void MakeFanSegmentsInteractable()
    // {
    //     foreach (Transform child in transform)
    //     {
    //         if (child != null && child.name == "FanSegment")
    //         {
    //             // Add a collider to make segment clickable
    //             MeshCollider meshCollider = child.AddComponent<MeshCollider>();
    //             meshCollider.sharedMesh = child.GetComponent<MeshFilter>().mesh;
    //             meshCollider.enabled = true;
                
    //             // Add compontent to handle click events
    //             FanClicker fanClicker = child.gameObject.AddComponent<FanClicker>();
    //             fanClicker.fanGenerator = this;
    //         }
    //     }
    // }

    public void DestroyFanSegments()
    {
        List<Transform> children = new List<Transform>();
        foreach (Transform child in transform)
        {
            if (child != null && child.name == "FanSegment")
            {
                children.Add(child);
            }
        }

        foreach (Transform child in children)
        {
            if (child != null) // Ensure the child is not null
            {
                Destroy(child.gameObject);
            }
        }
    }

    private void OnValidate()
    {
        NColumns = _nColumns;
        NRows = _nRows;
        R1 = _r1;
        R2 = _r2;
    }
}