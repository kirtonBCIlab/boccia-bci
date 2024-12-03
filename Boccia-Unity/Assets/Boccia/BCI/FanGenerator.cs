using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using BCIEssentials.StimulusObjects;
using FanNamespace;
using TMPro;
using UnityEngine.Rendering;



public class FanGenerator : MonoBehaviour
{
    public Material material;
    public Color colour = Color.grey;

    public GameObject fanAnnotations;

    public void GenerateFanShape(FanSettings fanSettings)
    {        
        float angleStep = fanSettings.Theta / fanSettings.NColumns;
        float radiusStep = (fanSettings.OuterRadius - fanSettings.InnerRadius) / fanSettings.NRows;

        // Only have spacing when there are more than 1 column or row
        if (fanSettings.NColumns > 1) { angleStep = (fanSettings.Theta - (fanSettings.NColumns - 1) * fanSettings.columnSpacing) / fanSettings.NColumns; }
        if (fanSettings.NRows > 1) { radiusStep = (fanSettings.OuterRadius - fanSettings.InnerRadius - (fanSettings.NRows - 1) * fanSettings.rowSpacing) / fanSettings.NRows; }

        // Create the fan segments
        for (int i = 0; i < fanSettings.NColumns; i++)
        {
            float startAngle = i * (angleStep + fanSettings.columnSpacing);
            float endAngle = startAngle + angleStep;

            for (int j = 0; j < fanSettings.NRows; j++)
            {
                float innerRadius = fanSettings.InnerRadius + j * (radiusStep + fanSettings.rowSpacing);
                float outerRadius = innerRadius + radiusStep;

                CreateFanSegment(startAngle, endAngle, innerRadius, outerRadius);
            }
        }
    }

    public void CreateFanSegment(float startAngle, float endAngle, float innerRadius, float outerRadius)
    {
        int segments = 100; // Number of segments to approximate the arc
        Mesh fanMesh = GenerateFanMesh(startAngle, endAngle, innerRadius, outerRadius, segments);
        CreateMeshObject("FanSegment", fanMesh);
    }

   public void GenerateBackButton(FanSettings fanSettings, BackButtonPositioningMode positionMode)
   {
        // If no backbutton, skip method
        if (positionMode == BackButtonPositioningMode.None) return;

        float startAngle = 0;
        float endAngle = fanSettings.BackButtonWidth / fanSettings.OuterRadius * Mathf.Rad2Deg; // Calculate the end angle for the back button
        int segments = 10; // Number of segments to approximate the arc
        Mesh fanMesh = GenerateFanMesh(startAngle, endAngle, fanSettings.InnerRadius, fanSettings.OuterRadius, segments);

        // Position the back button based on the BackButtonPositioningMode
        float rotationOffset = 0;
        switch (positionMode)
        {
            case BackButtonPositioningMode.Left:
                rotationOffset = fanSettings.columnSpacing + fanSettings.Theta;
                break;
            case BackButtonPositioningMode.Right:
                rotationOffset = -(fanSettings.columnSpacing + endAngle);
                break;
        }

        CreateMeshObject("BackButton", fanMesh, rotationOffset);
    }

    public void GenerateDropButton(FanSettings fanSettings)
    {
        int segments = 10; // Number of segments to approximate the arc
        Mesh fanMesh = GenerateFanMesh(0, fanSettings.Theta, fanSettings.InnerRadius - fanSettings.DropButtonHeight, fanSettings.InnerRadius - fanSettings.rowSpacing, segments);
        CreateMeshObject("DropButton", fanMesh);
    }

    public void DestroyFanSegments()
    {
        foreach (Transform child in transform)
        {
            Destroy(child.gameObject);
        }
    }

    private GameObject CreateMeshObject(string objectName, Mesh generatedMesh, float eulerRotation = 0)
    {
        GameObject meshObject = new(objectName);
        meshObject.transform.SetParent(transform);
        meshObject.transform.localPosition = Vector3.zero;
        meshObject.transform.localEulerAngles = new Vector3(0, 0, eulerRotation);
        meshObject.transform.localScale = Vector3.one;

        MeshFilter meshFilter = meshObject.AddComponent<MeshFilter>();
        meshFilter.mesh = generatedMesh;

        MeshRenderer meshRenderer = meshObject.AddComponent<MeshRenderer>();
        meshRenderer.material = material;
        meshRenderer.material.color = colour;

        return meshObject;
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

    public void GenerateFanAnnotations(FanSettings fanSettings, float currentRotation, float currentElevation, BackButtonPositioningMode backButtonPositioningMode)
    {   
        // Annotation for the start angle
        float startAngle = 0;
        float startRad = Mathf.Deg2Rad * startAngle;
        // Debug.Log("~~~~Generating new fan with an elevation of " + currentElevation + " and a rotation of " + currentRotation);
        Vector3 startPosition = new(Mathf.Cos(startRad) * fanSettings.OuterRadius, Mathf.Sin(startRad) * fanSettings.OuterRadius, 0);
        CreateTextAnnotation
        (
            startPosition,
            rotationAngle: 0,
            (currentRotation + fanSettings.Theta/2).ToString("F1") + "°",
            TextAlignmentOptions.BottomRight,
            annotationFontSize: fanSettings.annotationFontSize
        );

        // Annotation for the end angle
        float endAngle = fanSettings.Theta;
        float endRad = Mathf.Deg2Rad * endAngle;        
        Vector3 endPosition = new (Mathf.Cos(endRad) * fanSettings.OuterRadius, Mathf.Sin(endRad) * fanSettings.OuterRadius, 0);
        CreateTextAnnotation
        (
            position: endPosition,
            rotationAngle: fanSettings.Theta,
            (currentRotation - fanSettings.Theta/2).ToString("F1") + "°",
            TextAlignmentOptions.BottomLeft,
            annotationFontSize: fanSettings.annotationFontSize
        );

        // Place height anotations according to back button position 
        TextAlignmentOptions lowLimitPosition;
        TextAlignmentOptions highLimitPosition;
        Vector3 elevationLowPositionOffset;
        Vector3 elevationHighPositionOffset;
        float elevationRotationOffset;        
        switch (backButtonPositioningMode)
        {
            case BackButtonPositioningMode.Right:
                elevationLowPositionOffset = new(Mathf.Cos(endRad) * fanSettings.InnerRadius, Mathf.Sin(endRad) * fanSettings.InnerRadius + 0.05f, 0);
                elevationHighPositionOffset = new(Mathf.Cos(endRad) * fanSettings.OuterRadius, Mathf.Sin(endRad) * fanSettings.OuterRadius + 0.05f, 0);
                elevationRotationOffset = fanSettings.Theta;
                lowLimitPosition = TextAlignmentOptions.BottomRight;
                highLimitPosition = TextAlignmentOptions.TopRight;
                break;
            default:
                elevationLowPositionOffset = new (fanSettings.InnerRadius, -0.05f, 0);
                elevationHighPositionOffset = new(fanSettings.OuterRadius, -0.05f, 0);
                elevationRotationOffset = 0;
                lowLimitPosition = TextAlignmentOptions.BottomLeft;
                highLimitPosition = TextAlignmentOptions.TopLeft;
                break;
        }

        // Annotation for the low elevation limit
        CreateTextAnnotation
        (
            position: elevationLowPositionOffset,
            rotationAngle: elevationRotationOffset,
            text: (currentElevation - fanSettings.ElevationRange/2).ToString() + "%",
            textAlignment: lowLimitPosition,
            annotationFontSize: fanSettings.annotationFontSize
        );

        // Annotation for the high elevation limit
        CreateTextAnnotation
        (
            position: elevationHighPositionOffset,
            rotationAngle: elevationRotationOffset,
            text: (currentElevation + fanSettings.ElevationRange/2).ToString() + "%",
            textAlignment: highLimitPosition,
            annotationFontSize: fanSettings.annotationFontSize
        );
    }
    
    private void CreateTextAnnotation(Vector3 position, float rotationAngle, string text, TextAlignmentOptions textAlignment, float annotationFontSize)
    {
        GameObject textObject = Instantiate(fanAnnotations, transform);
        textObject.name = "TextAnnotation";

        textObject.transform.SetLocalPositionAndRotation
        (
            position,
            Quaternion.Euler(0, 0, -90 + rotationAngle)
        );

        // Add and configure RectTransform
        RectTransform rectTransform = textObject.GetComponent<RectTransform>();
        switch (textAlignment)
        {
            case TextAlignmentOptions.BottomLeft:
                rectTransform.pivot = new Vector2(0, 0);
                rectTransform.anchorMin = new Vector2(0, 0);
                rectTransform.anchorMax = new Vector2(0, 0);
                break;
            case TextAlignmentOptions.BottomRight:
                rectTransform.pivot = new Vector2(1, 0);
                rectTransform.anchorMin = new Vector2(1, 0);
                rectTransform.anchorMax = new Vector2(1, 0);
                break;
            case TextAlignmentOptions.TopLeft:
                rectTransform.pivot = new Vector2(0, 1);
                rectTransform.anchorMin = new Vector2(0, 1);
                rectTransform.anchorMax = new Vector2(0, 1);
                break;
            case TextAlignmentOptions.TopRight:
                rectTransform.pivot = new Vector2(1, 1);
                rectTransform.anchorMin = new Vector2(1, 1);
                rectTransform.anchorMax = new Vector2(1, 1);
                break;
            // Add other cases if needed
            default:
                break;
        }
        
        TextMeshPro textMeshProComponent = textObject.GetComponent<TextMeshPro>();
        if (textMeshProComponent != null)
        {
            textMeshProComponent.text = text;
            textMeshProComponent.fontSize = annotationFontSize;
            textMeshProComponent.alignment = textAlignment;
        }
    }
}