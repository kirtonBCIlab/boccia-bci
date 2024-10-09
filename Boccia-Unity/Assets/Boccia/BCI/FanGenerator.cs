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
        set { _theta = Mathf.Clamp(value, 1, 180); }
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
        set { _lowElevationLimit = Mathf.Clamp(value, 0, HighElevationLimit); }
    }

    [SerializeField]
    private int _highElevationLimit; // Higher elevation limit
    public int HighElevationLimit
    {
        get { return _highElevationLimit; }
        set { _highElevationLimit = Mathf.Clamp(value, 0, LowElevationLimit); }
    }

    [Header("Back Button Parameters")]
    [SerializeField]
    private float _backButtonWidth; // Width of the back button
    public float BackButtonWidth
    {
        get { return _backButtonWidth; }
        set { _backButtonWidth = Mathf.Clamp(value, 1f, float.MaxValue); }
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

    private void CreateFanSegment(GameObject fan, float startAngle, float endAngle, float innerRadius, float outerRadius)
    {
        GameObject segment = new GameObject("FanSegment");
        segment.transform.SetParent(fan.transform);
        
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
    }

    public void DestroyFanSegments()
    {
        foreach (Transform child in transform)
        {
            if (child.name == "FanSegment" || child.name == "BackButton") 
            {
                Destroy(child.gameObject);
            }
        }
    }

    public void GenerateBackButton(BackButtonPositioningMode positionMode)
    {
        GameObject backButton = new("BackButton");
        backButton.transform.SetParent(transform);

        MeshFilter meshFilter = backButton.AddComponent<MeshFilter>();
        MeshRenderer meshRenderer = backButton.AddComponent<MeshRenderer>();

        Mesh mesh = new();
        meshFilter.mesh = mesh;

        Vector3[] vertices = new Vector3[4];
        int[] triangles = new int[6];

        float halfWidth = BackButtonWidth / 2;

        vertices[0] = new Vector3(InnerRadius, 0, 0);
        vertices[1] = new Vector3(OuterRadius, 0, 0);
        vertices[2] = new Vector3(OuterRadius * Mathf.Cos(Theta), OuterRadius * Mathf.Sin(Theta), 0);
        vertices[3] = new Vector3(InnerRadius * Mathf.Cos(Theta), InnerRadius * Mathf.Sin(Theta), 0);

        triangles[0] = 0;
        triangles[1] = 1;
        triangles[2] = 2;

        triangles[3] = 0;
        triangles[4] = 2;
        triangles[5] = 3;

        mesh.vertices = vertices;
        mesh.triangles = triangles;
        mesh.RecalculateNormals();
        mesh.RecalculateBounds();

        // Position the back button based on the BackButtonPositioningMode
        float fanWidth = CalculateFanWidth();
        float offsetX = positionMode == BackButtonPositioningMode.Right ? fanWidth : -fanWidth;
        backButton.transform.SetLocalPositionAndRotation
        (
            new Vector3(offsetX, 0, 0),
            Quaternion.Euler(180, 0, positionMode == BackButtonPositioningMode.Right ? +Theta/2 : -Theta/2)
        );
    }

    private float CalculateFanWidth()
    {
        return OuterRadius * Mathf.Sin(Mathf.Deg2Rad * Theta / 2);
    }

    private void OnValidate()
    {
        Theta = _theta;
        NColumns = _nColumns;
        NRows = _nRows;
        InnerRadius = _innerRadius;
        OuterRadius = _outerRadius;
        BackButtonWidth = _backButtonWidth;
    }
}