using UnityEngine;
using BCIEssentials.StimulusObjects;

public class BCITargetAnimations : MonoBehaviour
{
    private SPO spo;
    private Renderer objectRenderer;
    private Color originalColor;

    private void Awake()
    {
        spo = GetComponent<SPO>();
        objectRenderer = GetComponent<Renderer>();
        if (objectRenderer != null)
        {
            originalColor = objectRenderer.material.color;
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
        if (objectRenderer != null)
        {
            objectRenderer.material.color = Color.yellow;
        }
    }

    private void OnTrainingStop()
    {
        if (objectRenderer != null)
        {
            objectRenderer.material.color = originalColor;
        }
    }
}
