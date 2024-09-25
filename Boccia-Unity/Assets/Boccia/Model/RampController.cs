
// Interface for an object that manages a ramp (simulated or hardware)
public interface RampController
{
    public event System.Action RampChanged;

    public float Rotation { get; }
    public float Elevation { get; }

    public void RotateBy(float degrees);
    public void ElevateBy(float elevation);

    public void ResetRampPosition();

    // add remaining methods like calibration, test, reset
}

