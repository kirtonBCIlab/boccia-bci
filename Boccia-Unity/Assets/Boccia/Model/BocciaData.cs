using System;
using UnityEngine;

public enum BocciaBciParadigm
{
    P300,
}

public enum BocciaAnimation
{
    Default,
}

// BocciaData contains state used by BocciaModel.  This is just a dumb container
// that's easy to serialize to/from disk.  Busines logic should go in the model.
[System.Serializable]
public class BocciaData
{
    public bool WasInitialized;

    // Game
    public Color BallColor;
    public float ElevationPrecision;
    public float ElevationRange;
    public float ElevationSpeed;
    public float RotationPrecision;
    public float RotationRange;
    public float RotationSpeed;

    // hotkeys?

    // BCI
    public BocciaBciParadigm Paradigm;
    public int NumFlashes;
    public int NumTrainingWindows;
    public BocciaAnimation TargetAnimation;
    public bool ShamFeedback;
    public Double StimulusOnDuration;
    public Double StimulusOffDuration;

    // bci testing?

    // Hardware
    public string SerialPortName;
}

