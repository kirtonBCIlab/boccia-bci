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

    public float Elevation { get; private set; }

    public bool IsBarOpen { get; private set;}
    public bool IsMoving { get; set; }

    public SimulatedRamp()
    {
        _model = BocciaModel.Instance;

        Rotation = _model.RampSettings.RotationOrigin;
        Elevation = _model.RampSettings.ElevationOrigin;
        IsBarOpen = false; // Initialize the bar state as closed
        IsMoving = false;
    }

    public void RotateBy(float degrees)
    {
        // Clamped to Min/Max Rotation
        Rotation = Mathf.Clamp(Rotation+degrees, _model.RampSettings.RotationLimitMin, _model.RampSettings.RotationLimitMax);
        // Debug.Log($"Simulated rotation by: {Rotation}");
        SendChangeEvent();
    }

    public void RotateTo(float degrees)
    {
        // Clamped to Min/Max Rotation
        Rotation = Mathf.Clamp(degrees, _model.RampSettings.RotationLimitMin, _model.RampSettings.RotationLimitMax);
        // Debug.Log($"Simulated rotation to: {Rotation}");
        SendChangeEvent();
    }

    public void ElevateBy(float height)
    {
        // Clamped to Min/Max Elevation
        Elevation = Mathf.Clamp(Elevation + height, _model.RampSettings.ElevationLimitMin, _model.RampSettings.ElevationLimitMax);
        // Debug.Log($"Simulated elevation by: {Elevation}");
        SendChangeEvent();
    }

    public void ElevateTo(float height)
    {
        // Clamped to Min/Max Elevation
        Elevation = Mathf.Clamp(height, _model.RampSettings.ElevationLimitMin, _model.RampSettings.ElevationLimitMax);
        // Debug.Log($"Simulated elevation to: {Elevation}");
        SendChangeEvent();
    }

    public void ResetRampPosition()
    {
        Rotation = _model.RampSettings.RotationOrigin;
        Elevation = _model.RampSettings.ElevationOrigin;
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
