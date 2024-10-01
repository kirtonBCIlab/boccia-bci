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

    private int _maxColumns = 7;    // Max number of columns 
    private int _maxRows = 7;       // Max number of rows

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

    [Header("SPO settings")]
    public Color flashOnColor = Color.red;
    public Color flashOffColor = Color.white;

    public void GenerateFanShape()
    {
        GameObject fan = gameObject;
        fan.transform.rotation = Quaternion.identity;   // Reset rotation to default

        float angleStep = theta / NColumns;
        float radiusStep = (R2 - R1) / NRows;

        // Only have spacing when there are more than 1 column or row
        if (NColumns > 1) { angleStep = (theta - (NColumns - 1) * columnSpacing) / NColumns; }
        if (NRows > 1) { radiusStep = (R2 - R1 - (NRows - 1) * rowSpacing) / NRows; }

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

        // Rotate object on Z to align straight
        Vector3 currentRotation = fan.transform.rotation.eulerAngles;
        fan.transform.rotation = Quaternion.Euler(currentRotation.x, currentRotation.y, 90 - (theta / 2));
    }

    private void CreateFanSegment(GameObject fan, float startAngle, float endAngle, float innerRadius, float outerRadius, int segmentID)
    {
        GameObject segment = new GameObject("FanSegment");
        segment.transform.SetParent(fan.transform);
        segment.tag = "BCI";

        MeshFilter meshFilter = segment.AddComponent<MeshFilter>();
        MeshRenderer meshRenderer = segment.AddComponent<MeshRenderer>();

        Mesh mesh = new Mesh();
        meshFilter.mesh = mesh;

        int segments = 100; // Number of segments to approximate the arc
        int verticesCount = (segments + 1) * 2;
        Vector3[] vertices = new Vector3[verticesCount];
        int[] triangles = new int[segments * 6];

        float angleStep = (endAngle - startAngle) / segments;

        for (int i = 0; i <= segments; i++)
        {
            float angle = startAngle + i * angleStep;
            float rad = Mathf.Deg2Rad * angle;

            vertices[i] = new Vector3(Mathf.Cos(rad) * innerRadius, Mathf.Sin(rad) * innerRadius, 0);
            vertices[i + segments + 1] = new Vector3(Mathf.Cos(rad) * outerRadius, Mathf.Sin(rad) * outerRadius, 0);

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

    public void DestroyFanSegments()
    {
        foreach (Transform child in transform)
        {
            if (child.name == "FanSegment")
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