using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UIElements;


// Simulate a ramp.  This uses Tasks instead of MonoBehavior.Update() to be more like a
// hardware ramp where updates may be driven by serial port events (independent of Unity)
public class SimulatedRamp: RampController
{
    public float Rotation { get; private set; }
    public float Elevation { get; private set; }

    public SimulatedRamp()
    {
        Rotation = 0.0f;
        Elevation = 50.0f;
    }

    public void RotateLeft()
    {
        RotateRamp(-6.0f);
    }

    public void RotateRight()
    {
        RotateRamp(6.0f);
    }

    public void MoveUp()
    {
        ChangeElevation(1.0f);
    }

    public void MoveDown()
    {
        ChangeElevation(-1.0f);
    }

    private async void RotateRamp(float rotationDegrees)
    {
        Rotation += rotationDegrees;
    }

    private async void ChangeElevation(float elevationPercent)
    {
        Elevation += elevationPercent;
    }

    public async void ResetRampPosition()
    {
        Rotation = 0.0f;
        Elevation = 50.0f;
    }
}
