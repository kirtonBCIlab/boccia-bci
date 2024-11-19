using BCIEssentials.StimulusEffects;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ColourFlashStimulusEffect : StimulusEffect
{
    [Header("Flash Settings")]
    [Tooltip("Material Color to assign while flashing is on")]
    public Color OnColor = Color.red;

    [Tooltip("Material Color to assign while flashing is off")]
    public Color OffColor = Color.white;

    public bool StartOn;

    [Header("Selection Flash Settings")]
    [Min(0)]
    public float SelectionFlashDuration = 0.2f;

    [Min(1)]
    public int SelectionFlashCount = 3;

    public bool IsPlaying => _effectRoutine != null;

    private Renderer _materialRenderer;
    private CanvasRenderer _canvasRenderer;
    private System.Action<Color> SetColor;

    private Coroutine _effectRoutine;

    private void Awake()
    {
        _materialRenderer = GetComponent<Renderer>();
        _canvasRenderer = GetComponent<CanvasRenderer>();
        if (_materialRenderer == null && _canvasRenderer == null)
        {
            Debug.LogWarning($"No Renderer component found for {name}");
            Destroy(this);
            return;
        }
        if (_materialRenderer != null)
        {
            SetColor = AssignMaterialColor;
            if (_materialRenderer.material == null)
            {
                Debug.LogWarning($"No material assigned to renderer component on {name}.");
                Destroy(this);
                return;
            }
        }
        else
            SetColor = AssignCanvasColor;

        SetColor(StartOn ? OnColor : OffColor);
    }

    public override void SetOn()
    {
        SetColor(OnColor);
        IsOn = true;
    }

    public override void SetOff()
    {
        SetColor(OffColor);
        IsOn = false;
    }

    public void Play()
    {
        Stop();
        _effectRoutine = StartCoroutine(RunEffect());
    }

    private void Stop()
    {
        if (!IsPlaying)
            return;

        SetOff();
        StopCoroutine(_effectRoutine);
        _effectRoutine = null;
    }

    private IEnumerator RunEffect()
    {
        for (var i = 0; i < SelectionFlashCount; i++)
        {
            SetOn();
            yield return new WaitForSecondsRealtime(SelectionFlashDuration);

            SetOff();
            yield return new WaitForSecondsRealtime(SelectionFlashDuration);
        }

        SetOff();
        _effectRoutine = null;
    }

    void AssignMaterialColor(Color color)
    {
        _materialRenderer.material.color = color;
    }

    void AssignCanvasColor(Color color)
    {
        _canvasRenderer.SetColor(color);
    }
}
