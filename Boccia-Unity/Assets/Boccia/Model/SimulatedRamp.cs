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

    private float _minRotation = -85.0f;
    private float _maxRotation = 85.0f;
    private float _rotation;
    public float Rotation 
    { 
        get { return _rotation; } 
        set { _rotation = Math.Clamp(value, _minRotation, _maxRotation); }
    }

    private float _maxElevation = 100.0f;
    private float _minElevation = 0.0f;
    private float _elevation;
    public float Elevation
    { 
        get {return _elevation; }
        set { _elevation = Math.Clamp(value, _minElevation, _maxElevation); }
    }

    public bool IsBarOpen { get; private set;}
    public bool IsMoving { get; set; }

    public SimulatedRamp()
    {
        Rotation = 0.0f;
        Elevation = 50.0f;
        IsBarOpen = false; // Initialize the bar state as closed
        IsMoving = false;
    }

    public void RotateBy(float degrees)
    {
        Rotation += degrees;
        //Debug.Log($"Simulated rotation by: {Rotation}");
        SendChangeEvent();
    }

    public void RotateTo(float degrees)
    {
        Rotation = degrees;
        //Debug.Log($"Simulated rotation to: {Rotation}");
        SendChangeEvent();
    }

    public void ElevateBy(float elevation)
    {
        Elevation += elevation;
        //Debug.Log($"Simulated elevation by: {Elevation}");
        SendChangeEvent();
    }

    public void ElevateTo(float elevation)
    {
        Elevation = elevation;
        // Debug.Log($"Simulated elevation to: {Elevation}");
        SendChangeEvent();
    }

    public void ResetRampPosition()
    {
        Rotation = 0.0f;
        Elevation = 50.0f;
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
