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
        Elevation = 0.0f;
    }

    public void RotateLeft()
    {
        RotateRamp(-30.0f);
    }

    public void RotateRight()
    {
        RotateRamp(30.0f);
    }

    // This task will produce the desired change in rotation over a few steps
    private async void RotateRamp(float rotationDegrees)
    {
        float startRotation = Rotation;
        float rotationStep = Math.Sign(rotationDegrees) * 1.0f;
        while(Math.Abs(Rotation - startRotation) <= Math.Abs(rotationDegrees))
        {
            await Task.Delay(100);
            Rotation += rotationStep;
        }
    }
}
