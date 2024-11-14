using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UIElements;


// Simulate a ramp.  This uses Tasks instead of MonoBehavior.Update() to be more like a
// hardware ramp where updates may be driven by serial port events (independent of Unity)
public class SimulatedRamp : RampController
{
    public event System.Action RampChanged;

    private BocciaModel _model;

    public float Rotation { get; private set; }
    private float _originRotation;
    private float _minRotation;
    private float _maxRotation;

    public float Elevation { get; private set; }
    private float _originElevation;
    private float _minElevation;
    private float _maxElevation;

    public bool IsBarOpen { get; private set;}
    public bool IsMoving { get; set; }

    public SimulatedRamp()
    {
        _model = BocciaModel.Instance;

        InitializeRampSettings();

        Rotation = _originRotation;
        Elevation = _originElevation;
        IsBarOpen = false; // Initialize the bar state as closed
        IsMoving = false;
    }

    // Initialize the parameters pulled from RampSettings()
    private void InitializeRampSettings()
    {
        _originElevation = _model.RampSettings.ElevationOrigin;
        _originRotation = _model.RampSettings.RotationOrigin;

        _minElevation = _model.RampSettings.ElevationLimitMin;
        _maxElevation = _model.RampSettings.ElevationLimitMax;

        _minRotation = _model.RampSettings.RotationLimitMin;
        _maxRotation = _model.RampSettings.RotationLimitMax;
    }

    public void RotateBy(float degrees)
    {
        // Clamped to Min/Max Rotation
        Rotation = Mathf.Clamp(Rotation+degrees, _minRotation, _maxRotation);
        // Debug.Log($"Simulated rotation by: {Rotation}");
        SendChangeEvent();
    }

    public void RotateTo(float degrees)
    {
        // Clamped to Min/Max Rotation
        Rotation = Mathf.Clamp(degrees, _minRotation, _maxRotation);
        // Debug.Log($"Simulated rotation to: {Rotation}");
        SendChangeEvent();
    }

    public void ElevateBy(float height)
    {
        // Clamped to Min/Max Elevation
        Elevation = Mathf.Clamp(Elevation + height, _minElevation, _maxElevation);
        // Debug.Log($"Simulated elevation by: {Elevation}");
        SendChangeEvent();
    }

    public void ElevateTo(float height)
    {
        // Clamped to Min/Max Elevation
        Elevation = Mathf.Clamp(height, _minElevation, _maxElevation);
        // Debug.Log($"Simulated elevation to: {Elevation}");
        SendChangeEvent();
    }

    public void ResetRampPosition()
    {
        Rotation = _originRotation;
        Elevation = _originElevation;
        SendChangeEvent();
    }

    public void DropBall()
    {
        IsBarOpen = true; // Toggle bar state
        SendChangeEvent();
    }

    public void ResetBar()
    {
        IsBarOpen = false;
        SendChangeEvent();
    }

    private void SendChangeEvent()
    {
        RampChanged?.Invoke();
    }
}
