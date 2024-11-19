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

    private Renderer _renderer;
    private Coroutine _effectRoutine;

    private void Awake()
    {
        _renderer = GetComponent<Renderer>();
        if (_renderer == null)
        {
            Debug.LogWarning($"No Renderer component found for {name}");
            Destroy(this);
            return;
        }
        if (_renderer.material == null)
        {
            Debug.LogWarning($"No material assigned to renderer component on {name}.");
            Destroy(this);
            return;
        }

        AssignMaterialColor(StartOn ? OnColor : OffColor);
    }

    public override void SetOn()
    {
        AssignMaterialColor(OnColor);
        IsOn = true;
    }

    public override void SetOff()
    {
        AssignMaterialColor(OffColor);
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

    private void AssignMaterialColor(Color color)
    {
        _renderer.material.color = color;
    }
}
