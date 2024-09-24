
// Interface for an object that manages a ramp (simulated or hardware)
public interface RampController
{
    public float Rotation { get; }
    public float Elevation { get; }

    public void RotateBy(float degrees);
    public void ElevateBy(float elevation);

    // add remaining methods like calibration, test, reset
}

