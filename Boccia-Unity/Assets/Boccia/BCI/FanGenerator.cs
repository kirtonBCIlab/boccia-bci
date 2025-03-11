using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using BCIEssentials.StimulusObjects;
using BCIEssentials.StimulusEffects;
using FanNamespace;
using TMPro;
using UnityEngine.Rendering;



public class FanGenerator : MonoBehaviour
{
    public Material material;
    public Color colour = Color.grey;

    public GameObject fanAnnotations;

    [Header("Stimulus Settings")]
    public Sprite faceSprite;
    [SerializeField]
    [Tooltip("Set this to the Shaft Adapter GameObject for Play and Virtual Play fans")]
    private GameObject faceSpriteRotationCorrector;
    private GameObject spriteObject;
    private BocciaStimulusType _stimulusType;

    private FanPositioningMode _fanPositioningMode;

    private BocciaModel _model;

    void Start()
    {
        _model = BocciaModel.Instance;
    }

    public void SetFanPositioningMode(FanPositioningMode fanPositioningMode)
    {
        _fanPositioningMode = fanPositioningMode;
    }

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

                CreateFanSegment(angleStep, startAngle, endAngle, innerRadius, outerRadius);
            }
        }
    }

    public void CreateFanSegment(float angleStep, float startAngle, float endAngle, float innerRadius, float outerRadius)
    {
        int segments = 100; // Number of segments to approximate the arc
        Mesh fanMesh = GenerateFanMesh(startAngle, endAngle, innerRadius, outerRadius, segments);
        GameObject fanSegment = CreateMeshObject("FanSegment", fanMesh);
        Vector3 fanSegmentMidpoint = CalculateSegmentMidpoint(startAngle, endAngle, innerRadius, outerRadius);

        Vector3 spriteScale = new Vector3(0.05f, 0.05f, 0.05f);
        CreateSegmentSprite(fanSegment, fanSegmentMidpoint, spriteScale);
    }

   public void GenerateBackButton(FanSettings fanSettings, BackButtonPositioningMode positionMode)
   {
        // If using separate Back button, skip method
        if (_model.P300Settings.SeparateButtons) return;
        
        // If no backbutton (i.e. for the coarse fan), skip method
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

        GameObject backButton = CreateMeshObject("BackButton", fanMesh, rotationOffset);
        Vector3 backButtonMidpoint = CalculateSegmentMidpoint(startAngle, endAngle, fanSettings.InnerRadius, fanSettings.OuterRadius); 

        if (IsFaceSpriteStimulus())
        {
            Vector3 spriteScale = new Vector3(0.1f, 0.1f, 0.1f);
            CreateSegmentSprite(backButton, backButtonMidpoint, spriteScale);
        }
    }

    public void GenerateDropButton(FanSettings fanSettings)
    {
        // If using separate Drop button, skip method
        if (_model.P300Settings.SeparateButtons) return;

        float innerRadius = fanSettings.InnerRadius - fanSettings.DropButtonHeight;
        float outerRadius = fanSettings.InnerRadius - fanSettings.rowSpacing;
        int segments = 10; // Number of segments to approximate the arc
        Mesh fanMesh = GenerateFanMesh(0, fanSettings.Theta, innerRadius, outerRadius, segments);
        GameObject dropButton = CreateMeshObject("DropButton", fanMesh);
        Vector3 dropButtonMidpoint = CalculateSegmentMidpoint(0, fanSettings.Theta, innerRadius, outerRadius);

        if (IsFaceSpriteStimulus())
        {
            Vector3 spriteScale = new Vector3(0.05f, 0.05f, 0.05f);
            CreateSegmentSprite(dropButton, dropButtonMidpoint, spriteScale);
        }
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
        Vector2[] uv = new Vector2[verticesCount]; // UVs for each vertex
        int[] triangles = new int[segments * 6];
        float angleStep = (endAngle - startAngle) / segments;

        // Calculate the center of the segment
        Vector3 meshCenter = CalculateSegmentMidpoint(startAngle, endAngle, innerRadius, outerRadius);

        for (int i = 0; i <= segments; i++)
        {
            float angle = startAngle + i * angleStep;
            float rad = Mathf.Deg2Rad * angle;

            // Define vertices relative to the local origin
            Vector3 innerVertex = new(Mathf.Cos(rad) * innerRadius, Mathf.Sin(rad) * innerRadius, 0);
            Vector3 outerVertex = new(Mathf.Cos(rad) * outerRadius, Mathf.Sin(rad) * outerRadius, 0);

            vertices[i] = innerVertex;
            vertices[i + segments + 1] = outerVertex;

            // Calculate UV coordinates (used for the gradient shader)
            Vector2 segmentCenter = new Vector2(meshCenter.x, meshCenter.y);
            Vector2 innerUV = new Vector2(innerVertex.x, innerVertex.y) - segmentCenter;
            Vector2 outerUV = new Vector2(outerVertex.x, outerVertex.y) - segmentCenter;

            // Normalize UV
            uv[i] = innerUV * 0.5f + new Vector2(0.5f, 0.5f);
            uv[i + segments + 1] = outerUV * 0.5f + new Vector2(0.5f, 0.5f);

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
        mesh.uv = uv;
        mesh.triangles = triangles;
        mesh.RecalculateNormals();
        mesh.RecalculateBounds();

        return mesh;
    }

    public void GenerateFanAnnotations(FanSettings fanSettings, float currentRotation, float currentElevation, BackButtonPositioningMode backButtonPositioningMode, FanPositioningMode fanPositioningMode)
    {   
        // Get scaled font size for Fine Fan based on Fine Fan theta
        float scaledFontSize = ScaleAnnotationSize(fanSettings, fanPositioningMode);

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
            annotationFontSize: scaledFontSize
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
            annotationFontSize: scaledFontSize
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


    // This scales the font size so we can see the annotations (for now)
    private float ScaleAnnotationSize(FanSettings fanSettings, FanPositioningMode fanPositioningMode)
    {
        if (fanPositioningMode == FanPositioningMode.CenterToBase)
        {
            return fanSettings.annotationFontSize;
        }

        float baseFontSize = fanSettings.annotationFontSize;
        float theta = fanSettings.Theta;

        if (theta > 21)
        {
            return baseFontSize;
        }

        if (15 < theta && theta <= 21)
        {
            float scaledFontSize = baseFontSize * 0.8f;
            return scaledFontSize;
        }

        else if (12 < theta && theta <= 15)
        {
            float scaledFontSize = baseFontSize * 0.6f;
            return scaledFontSize;
        }

        else 
        {
            float scaledFontSize = baseFontSize * 0.5f;
            Debug.Log($"Scaled font size: {scaledFontSize}");
            return scaledFontSize;
        }
    }

    public void CreateSegmentSprite(GameObject segment, Vector3 segmentMidPoint, Vector3 scale)
    {
        // Create GameObject for the face sprite as a child of the fan segment
        spriteObject = new GameObject("FaceSprite");
        spriteObject.transform.SetParent(segment.transform);
        SpriteRenderer spriteRenderer = spriteObject.AddComponent<SpriteRenderer>();
        spriteRenderer.sprite = faceSprite;

        // Set the transform of the sprite object based on the segment mid point
        Vector3 spriteObjectPosition = new Vector3(segmentMidPoint.x, segmentMidPoint.y, -0.01f);
        spriteObject.transform.localPosition = spriteObjectPosition;
        spriteObject.transform.localScale = scale;

        // Set the rotation of the sprite object
        Quaternion spriteRotation;
        if (_fanPositioningMode == FanPositioningMode.CenterToRails)
        {
            spriteRotation = Quaternion.Euler(-90, faceSpriteRotationCorrector.transform.eulerAngles.y, 0);
        }
        else
        {
            spriteRotation = Quaternion.Euler(90, 0, 0);
        }

        spriteObject.transform.rotation = spriteRotation;

        // Disable sprite initially
        spriteObject.SetActive(false);
    }

    private bool IsFaceSpriteStimulus()
    {
        if (_model.GameMode == BocciaGameMode.Train)
        {
            _stimulusType = _model.P300Settings.Train.StimulusType;
        }
        else if (_model.GameMode == BocciaGameMode.Play || _model.GameMode == BocciaGameMode.Virtual)
        {
            _stimulusType = _model.P300Settings.Test.StimulusType;
        }

        if (_stimulusType == BocciaStimulusType.FaceSprite)
        {
            return true;
        }

        return false;
    }

    private Vector3 CalculateSegmentMidpoint(float startAngle, float endAngle, float innerRadius, float outerRadius)
    {
        float midAngle = (startAngle + endAngle) / 2f;
        float midRadius = (innerRadius + outerRadius) / 2f;
        float midX = midRadius * Mathf.Cos(midAngle * Mathf.Deg2Rad);
        float midY = midRadius * Mathf.Sin(midAngle * Mathf.Deg2Rad);
        Vector3 segmentMidpoint = new Vector3(midX, midY, 0f);

        return segmentMidpoint;
    }
}   