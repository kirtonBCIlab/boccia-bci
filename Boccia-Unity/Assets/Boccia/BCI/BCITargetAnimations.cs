using UnityEngine;
using BCIEssentials.StimulusObjects;
using Unity.VisualScripting;


public class BCITargetAnimations : MonoBehaviour
{
    public TrainTargetAnimations trainTargetAnimation;
    private readonly Color targetColor = Color.yellow;  // Color for the color change animation
    private readonly float scalingFactor = 1.4f;        // Scaling factor for the size change animation

    private SPO spo;
    private Renderer objectRenderer;
    private Color originalColor;

    private BocciaModel _model;

    private void Awake()
    {
        spo = GetComponent<SPO>();
        objectRenderer = GetComponent<Renderer>();
        // Get fan segment color
        if (objectRenderer != null)
        {
            originalColor = objectRenderer.material.color;
        }
        // Get UI element color
        else if (TryGetComponent<UnityEngine.UI.Image>(out var image))
        {
            originalColor = image.color;
        }
    }

    private void OnEnable()
    {
        if (spo != null)
        {
            spo.StartTrainingStimulusEvent.AddListener(OnTrainingStart);
            spo.StopTrainingStimulusEvent.AddListener(OnTrainingStop);
        }
    }

    private void OnDisable()
    {
        if (spo != null)
        {
            spo.StartTrainingStimulusEvent.RemoveListener(OnTrainingStart);
            spo.StopTrainingStimulusEvent.RemoveListener(OnTrainingStop);
        }
    }

    private void OnTrainingStart()
    {
        switch (trainTargetAnimation)
        {
            case TrainTargetAnimations.None:
                break;
            case TrainTargetAnimations.SizeChange:
                StartSizeChange();
                break;
            case TrainTargetAnimations.ColorChange:
                StartColorChange();
                break;
        }
    }

    private void OnTrainingStop()
    {
        switch (trainTargetAnimation)
        {
            case TrainTargetAnimations.None:
                break;
            case TrainTargetAnimations.SizeChange:
                StopSizeChange();
                break;
            case TrainTargetAnimations.ColorChange:
                StopColorChange();
                break;
        }
        
    }

    private void StartSizeChange()
    {
        AdjustSize(scalingFactor);
    }

    private void StopSizeChange()
    {
        AdjustSize(1 / scalingFactor);
    }

    private void StartColorChange()
    {
        AdjustColor(targetColor);
    }

    private void StopColorChange()
    {
        AdjustColor(originalColor);
    }

    private void AdjustSize(float factor)
    {
        if (TryGetComponent<RectTransform>(out var rt))
        {
            // Remember old pivot in world space
            Vector3 oldPivotWorldPos = rt.TransformPoint(rt.pivot);

            // Adjust scale
            Vector3 currentScale = rt.localScale;
            rt.localScale = currentScale * factor;

            // Re-center
            Vector3 newPivotWorldPos = rt.TransformPoint(rt.pivot);
            rt.position += oldPivotWorldPos - newPivotWorldPos;
        }
        else
        {
            if (!TryGetComponent<MeshRenderer>(out var meshRenderer)) return;

            // Remember old center in world space
            Vector3 oldCenter = meshRenderer.bounds.center;

            // Adjust scale
            Vector3 currentScale = transform.localScale;
            transform.localScale = currentScale * factor;

            // Re-center
            Vector3 newCenter = meshRenderer.bounds.center;
            transform.position += oldCenter - newCenter;
        }
    }

    private void AdjustColor(Color newColor)
    {
        if (objectRenderer != null)
        {
            objectRenderer.material.color = newColor;
        }
        else
        {
            // Optionally handle UI elements
            if (TryGetComponent<UnityEngine.UI.Image>(out var image))
            {
                image.color = newColor;
            }
        }
    }    
}
