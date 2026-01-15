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

    private float _rotation;
    public float Rotation 
    { 
        get { return _rotation; } 
        set { _rotation = Math.Clamp(value, _model.RampSettings.RotationLimitMin, _model.RampSettings.RotationLimitMax); }
    }

    private float _elevation;
    public float Elevation
    { 
        get {return _elevation; }
        set { _elevation = Math.Clamp(value, _model.RampSettings.ElevationLimitMin, _model.RampSettings.ElevationLimitMax); }
    }

    public bool IsBarOpen { get; private set;}
    public bool IsMoving { get; set; }
    public bool IsSweeping { get; set; }

    public SimulatedRamp()
    {
        _model = BocciaModel.Instance;

        Rotation = _model.RampSettings.RotationOrigin;
        Elevation = _model.RampSettings.ElevationOrigin;
        IsBarOpen = false; // Initialize the bar state as closed
        IsMoving = false;
        IsSweeping = false;
    }

    public void RotateBy(float degrees)
    {
        // Relative rotation
        Rotation += degrees;
        //Debug.Log($"Simulated rotation by: {Rotation}");
        SendChangeEvent();
    }

    public void RotateTo(float degrees)
    {
        // Absolute rotation
        Rotation = degrees;
        //Debug.Log($"Simulated rotation to: {Rotation}");
        SendChangeEvent();
    }

    public void SetRotation(float degrees)
    {
        // Same as RotateTo, for consistency with the interface
        Rotation = degrees;
        SendChangeEvent();
    }

    public void ElevateBy(float elevation)
    {
        // Relative elevation
        Elevation += elevation;
        //Debug.Log($"Simulated elevation by: {Elevation}");
        SendChangeEvent();
    }

    public void ElevateTo(float elevation)
    {
        // Absolute elevation
        Elevation = elevation;
        // Debug.Log($"Simulated elevation to: {Elevation}");
        SendChangeEvent();
    }

    public void SetElevation(float elevation)
    {
        // Same as ElevateTo, for consistency with the interface
        Elevation = elevation;
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
