using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using BCIEssentials.StimulusObjects;
using FanNamespace;



public class FanGenerator : MonoBehaviour
{
    [Header("Fan Parameters")]
    public float columnSpacing; // Spacing between columns
    public float rowSpacing;    // Spacing between rows;

    private int _maxColumns = 7;            // Max number of columns 
    private int _maxRows = 7;               // Max number of rows
    private int _minRadiusDifference = 1;   // Minimum difference between inner and outer radius

    [SerializeField]
    private float _theta;               // Angle in degrees
    public float Theta
    {
        get { return _theta; }
        set { _theta = Mathf.Clamp(value, 5, 180); }
    }

    [SerializeField]
    private float _outerRadius;          // Outer radius
    public float OuterRadius
    {
        get { return _outerRadius; }
        set 
        {
            _outerRadius = Mathf.Clamp(value, _minRadiusDifference, float.MaxValue); 
            _innerRadius = Mathf.Clamp(_innerRadius, 0, _outerRadius - _minRadiusDifference);
        }
    }

    [SerializeField]
    private float _innerRadius;          // Inner radius
    public float InnerRadius
    {
        get { return _innerRadius; }
        set { _innerRadius = Mathf.Clamp(value, 0, OuterRadius-_minRadiusDifference); }
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
        set 
        { _lowElevationLimit = Mathf.Clamp(value, 0, _highElevationLimit-1); }
    }

    [SerializeField]
    private int _highElevationLimit; // Higher elevation limit
    public int HighElevationLimit
    {
        get { return _highElevationLimit; }
        set 
        {
            _lowElevationLimit = Mathf.Clamp(_lowElevationLimit, 0, _highElevationLimit-1);
            _highElevationLimit = Mathf.Clamp(value, _lowElevationLimit+1, 100);
        }
    }

    [Header("Additional Button Parameters")]
    [SerializeField]
    private float _backButtonWidth; // Width of the back button
    public float BackButtonWidth
    {
        get { return _backButtonWidth; }
        set { _backButtonWidth = Mathf.Clamp(value, 1f, float.MaxValue); }
    }

    [SerializeField]
    private float _dropButtonHeight; // Width of the back button
    public float DropButtonHeight
    {
        get { return _dropButtonHeight; }
        set { _dropButtonHeight = Mathf.Clamp(value, 0.5f, float.MaxValue); }
    }

    public void GenerateFanShape()
    {
        GameObject fan = gameObject;
        
        float angleStep = Theta / NColumns;
        float radiusStep = (OuterRadius - InnerRadius) / NRows;

        // Only have spacing when there are more than 1 column or row
        if (NColumns > 1) { angleStep = (Theta - (NColumns - 1) * columnSpacing) / NColumns; }
        if (NRows > 1) { radiusStep = (OuterRadius - InnerRadius - (NRows - 1) * rowSpacing) / NRows; }

        // Create the fan segments
        for (int i = 0; i < NColumns; i++)
        {
            float startAngle = i * (angleStep + columnSpacing);
            float endAngle = startAngle + angleStep;

            for (int j = 0; j < NRows; j++)
            {
                float innerRadius = InnerRadius + j * (radiusStep + rowSpacing);
                float outerRadius = innerRadius + radiusStep;

                CreateFanSegment(fan, startAngle, endAngle, innerRadius, outerRadius);
            }
        }
    }

    public void CreateFanSegment(GameObject fan, float startAngle, float endAngle, float innerRadius, float outerRadius)
    {
        GameObject segment = new("FanSegment");
        
        segment.transform.SetParent(fan.transform);

        MeshFilter meshFilter = segment.AddComponent<MeshFilter>();
        MeshRenderer meshRenderer = segment.AddComponent<MeshRenderer>();

        int segments = 100; // Number of segments to approximate the arc
        meshFilter.mesh = GenerateFanMesh(startAngle, endAngle, innerRadius, outerRadius, segments);

        segment.transform.SetLocalPositionAndRotation
        (
            new Vector3(segment.transform.localPosition.x, segment.transform.localPosition.y, 0),
            Quaternion.Euler(0, 0, 0)
        );
    }

   public void GenerateBackButton(BackButtonPositioningMode positionMode)
   {
        GameObject backButton = new("BackButton");
        backButton.transform.SetParent(gameObject.transform);

        MeshFilter meshFilter = backButton.AddComponent<MeshFilter>();
        MeshRenderer meshRenderer = backButton.AddComponent<MeshRenderer>();
        
        float startAngle = 0;
        float endAngle = BackButtonWidth / OuterRadius * Mathf.Rad2Deg; // Calculate the end angle for the back button
        int segments = 10; // Number of segments to approximate the arc
        meshFilter.mesh = GenerateFanMesh(startAngle, endAngle, InnerRadius, OuterRadius, segments);

        // Position the back button based on the BackButtonPositioningMode
        float rotationOffset = 0;
        switch (positionMode)
        {
            case BackButtonPositioningMode.Left:
                rotationOffset = columnSpacing + Theta;
                break;
            case BackButtonPositioningMode.Right:
                rotationOffset = -(columnSpacing + endAngle);
                break;
        }
        
        backButton.transform.SetLocalPositionAndRotation
        (
            new Vector3(backButton.transform.localPosition.x, backButton.transform.localPosition.y, 0),
            Quaternion.Euler(0, 0, rotationOffset)
        );
    }

    public void GenerateDropButton()
    {
        GameObject dropButton = new("DropButton");
        dropButton.transform.SetParent(gameObject.transform);

        MeshFilter meshFilter = dropButton.AddComponent<MeshFilter>();
        MeshRenderer meshRenderer = dropButton.AddComponent<MeshRenderer>();
        
        int segments = 50; // Number of segments to approximate the arc
        meshFilter.mesh = GenerateFanMesh(0, Theta, InnerRadius-DropButtonHeight, InnerRadius-rowSpacing, segments);

        dropButton.transform.SetLocalPositionAndRotation
        (
            new Vector3(dropButton.transform.localPosition.x, dropButton.transform.localPosition.y, 0),
            Quaternion.Euler(0, 0, 0)
        );
    }

    public void DestroyFanSegments()
    {
        foreach (Transform child in transform)
        {
            Destroy(child.gameObject);
        }
    }

    private Mesh GenerateFanMesh(float startAngle, float endAngle, float innerRadius, float outerRadius, int segments)
    {
        Mesh mesh = new();
        
        int verticesCount = (segments + 1) * 2;
        Vector3[] vertices = new Vector3[verticesCount];
        int[] triangles = new int[segments * 6];

        float angleStep = (endAngle - startAngle) / segments;

        for (int i = 0; i <= segments; i++)
        {
            float angle = startAngle + i * angleStep;
            float rad = Mathf.Deg2Rad * angle;

            // Define vertices relative to the local origin
            Vector3 innerVertex = new(Mathf.Cos(rad) * innerRadius, Mathf.Sin(rad) * innerRadius, 0);
            Vector3 outerVertex = new(Mathf.Cos(rad) * outerRadius, Mathf.Sin(rad) * outerRadius, 0);

            vertices[i] = innerVertex;
            vertices[i + segments + 1] = outerVertex;

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

        mesh.vertices = vertices;
        mesh.triangles = triangles;
        mesh.RecalculateNormals();
        mesh.RecalculateBounds();

        return mesh;
    }

    private void GenerateFanAnnotations()
    {

    }

    private void OnValidate()
    {
        Theta = _theta;
        NColumns = _nColumns;
        NRows = _nRows;
        InnerRadius = _innerRadius;
        OuterRadius = _outerRadius;
        BackButtonWidth = _backButtonWidth;
        DropButtonHeight = _dropButtonHeight;
        LowElevationLimit = _lowElevationLimit;
        HighElevationLimit = _highElevationLimit;
    }
}